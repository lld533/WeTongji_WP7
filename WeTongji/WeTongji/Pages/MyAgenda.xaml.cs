﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;
using System.Diagnostics;
using WeTongji.Api.Domain;
using WeTongji.DataBase;
using WeTongji.Business;
using System.Collections.ObjectModel;
using WeTongji.Pages;
using System.Threading;
using System.Windows.Media.Animation;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Threading;

namespace WeTongji
{
    public partial class MyAgenda : PhoneApplicationPage
    {
        public MyAgenda()
        {
            InitializeComponent();

            Global.Instance.AgendaSourceChanged += AgendaSourceStateChanged;
            refresh_dt.Tick += Refresh_Tick;

            VerticalScrollChanged += (o, e) =>
            {
                UpdateTopItem();
            };
        }

        private DispatcherTimer refresh_dt = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };

        private void AgendaSourceStateChanged(Object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                var src = this.LongListSelector_Core.ItemsSource;
                LongListSelector_Core.ItemsSource = null;
                LongListSelector_Core.ItemsSource = src;

                var node = Global.Instance.AgendaSource.GetNextCalendarNode();
                HighlightCurrentNode(node);
            });
        }

        private CalendarNode CurrentNode { get; set; }

        private CalendarGroup<CalendarNode> TopItemSource
        {
            get
            {
                return TextBlock_TopItemDate.DataContext == null ? null : TextBlock_TopItemDate.DataContext as CalendarGroup<CalendarNode>;
            }
            set
            {
                var oldValue = TopItemSource;
                TextBlock_TopItemDate.DataContext = value;

                //...Visible previous calendar node
                if (oldValue == null || oldValue.Key > value.Key)
                {
                    (this.Resources["VisiblePreviousCalendarNode"] as Storyboard).Begin();
                }
                //...Visible next calendar node
                else if (oldValue.Key < value.Key)
                {
                    (this.Resources["VisibleNextCalendarNode"] as Storyboard).Begin();
                }
            }
        }

        private event EventHandler VerticalScrollChanged;

        private Boolean registerVerticalScrollChanged = false;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //...Create AppButton_Today if it has not been created yet.
            if (this.ApplicationBar == null || this.ApplicationBar.Buttons.Count == 0)
            {
                var appBtn = new Microsoft.Phone.Shell.ApplicationBarIconButton(
                    new Uri(String.Format("/icons/days/{0}/{0}_{1}.png", DateTime.Now.Month, DateTime.Now.Day),
                        UriKind.RelativeOrAbsolute))
                {
                    Text = "今日",
                    IsEnabled = false
                };
                appBtn.Click += AppButton_Today_Clicked;
                this.ApplicationBar = new Microsoft.Phone.Shell.ApplicationBar();
                this.ApplicationBar.Buttons.Add(appBtn);
            }
            //...Refresh the icon AppButton_Today if it has already been created.
            else
            {
                var btn = this.ApplicationBar.Buttons[0] as Microsoft.Phone.Shell.ApplicationBarIconButton;
                btn.IconUri = new Uri(String.Format("/icons/days/{0}/{0}_{1}.png", DateTime.Now.Month, DateTime.Now.Day),
                        UriKind.RelativeOrAbsolute);
                btn.IsEnabled = false;
            }

            Global.Instance.AgendaSourceStateChanged += this.AgendaSourceStateChangedHandler;

            var thread = new Thread(new ThreadStart(TempLoadData)) { IsBackground = true };
            thread.Start();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            Global.Instance.AgendaSourceStateChanged -= this.AgendaSourceStateChangedHandler;
        }

        private void TempLoadData()
        {
            AgendaSourceStateChangedHandler(Global.Instance, EventArgs.Empty);
        }

        private void AppButton_Today_Clicked(Object sender, EventArgs e)
        {
            if (LongListSelector_Core.ItemsSource != null)
            {
                try
                {
                    var src = LongListSelector_Core.ItemsSource as List<CalendarGroup<CalendarNode>>;
                    var group = src.Where((g) => g.Key == DateTime.Now.Date).SingleOrDefault();
                    if (group != null)
                    {
                        LongListSelector_Core.ScrollToGroup(group);
                    }
                }
                catch { }
            }
        }

        private void AgendaSourceStateChangedHandler(Object sender, EventArgs e)
        {
            var global = sender as Global;

            switch (global.CurrentAgendaSourceState)
            {
                case SourceState.Done:
                    {
                        var nodeToScrollTo = global.AgendaSource.GetNextCalendarNode();

                        this.Dispatcher.BeginInvoke(() =>
                        {
                            CurrentNode = nodeToScrollTo;

                            #region [LongListSelector has no source]

                            if (LongListSelector_Core.ItemsSource == null)
                            {
                                TextBlock_Loading.Visibility = Visibility.Collapsed;
                                LongListSelector_Core.ItemsSource = global.AgendaSource;
                                {
                                    LongListSelector_Core.UpdateLayout();

                                    if (!registerVerticalScrollChanged)
                                    {
                                        DependencyObject depObj = LongListSelector_Core;

                                        while (!(depObj is ScrollViewer))
                                        {
                                            Debug.WriteLine(depObj.GetType());
                                            depObj = VisualTreeHelper.GetChild(depObj, 0);
                                        }

                                        this.SetBinding(DependencyProperty.RegisterAttached("VerticalOffset",

                                            typeof(double),

                                            this.GetType(),

                                            new PropertyMetadata((obj, arg) =>
                                            {

                                                if (VerticalScrollChanged != null)

                                                    VerticalScrollChanged(this, EventArgs.Empty);

                                            })),

                                        new Binding("VerticalOffset") { Source = depObj as ScrollViewer });

                                        registerVerticalScrollChanged = true;
                                    }
                                }

                                LongListSelector_Core.ScrollTo(nodeToScrollTo);
                                LongListSelector_Core.UpdateLayout();

                                {
                                    DependencyObject obj = LongListSelector_Core;
                                    while (!(obj is VirtualizingStackPanel))
                                    {
                                        obj = VisualTreeHelper.GetChild(obj, 0);
                                    }

                                    int count = VisualTreeHelper.GetChildrenCount(obj);

                                    for (int i = 0; i < count; ++i)
                                    {
                                        var tmp = VisualTreeHelper.GetChild(obj, i);

                                        var source = (tmp as Control).DataContext as LongListSelectorItem;
                                        if (source.Item == nodeToScrollTo)
                                        {
                                            obj = tmp;

                                            //...Get the layout root of the item
                                            while (!(obj is ContentPresenter))
                                            {
                                                obj = VisualTreeHelper.GetChild(obj, 0);
                                            }

                                            var itemLayoutRoot = VisualTreeHelper.GetChild(obj, 0) as Panel;
                                            Storyboard sb = null;
                                            if (nodeToScrollTo.IsNoArrangementNode)
                                                sb = itemLayoutRoot.Resources["GetHighlight_NoArrangement"] as Storyboard;
                                            else
                                                sb = itemLayoutRoot.Resources["GetHighlight"] as Storyboard;
                                            sb.Begin();
                                            break;
                                        }
                                    }
                                }
                            }
                            #endregion
                            #region [LongListSelector has source]
                            else
                            {
                                this.Dispatcher.BeginInvoke(() =>
                                {
                                    LongListSelector_Core.ItemsSource = global.AgendaSource;
                                });
                            }
                            #endregion

                            (this.ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = true;
                            refresh_dt.Start();
                        });
                    }
                    break;
                case SourceState.NotSet:
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            LongListSelector_Core.ItemsSource = null;
                            (this.ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = false;
                        });
                    }
                    break;
                case SourceState.Setting:
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            TextBlock_Loading.Visibility = Visibility.Visible;
                            (this.ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = false;
                        });
                    }
                    break;
            }

        }

        private void Refresh_Tick(Object sender, EventArgs e)
        {
            if (CurrentNode == null)
                return;

            Action core = () =>
            {
                Storyboard appear, disappear;
                appear = disappear = null;

                //...Get current node and begin disappear storyboard.
                #region [Disappear]
                {
                    DependencyObject obj = LongListSelector_Core;
                    while (!(obj is VirtualizingStackPanel))
                    {
                        obj = VisualTreeHelper.GetChild(obj, 0);
                    }

                    int count = VisualTreeHelper.GetChildrenCount(obj);

                    for (int i = 0; i < count; ++i)
                    {
                        var tmp = VisualTreeHelper.GetChild(obj, i);

                        var source = (tmp as Control).DataContext as LongListSelectorItem;
                        CalendarNode srcItem = source == null ? null : source.Item as CalendarNode;
                        if (srcItem != null && CurrentNode.CompareTo(srcItem) == 0)
                        {
                            obj = tmp;

                            //...Get the layout root of the item
                            while (!(obj is ContentPresenter))
                            {
                                obj = VisualTreeHelper.GetChild(obj, 0);
                            }

                            var itemLayoutRoot = VisualTreeHelper.GetChild(obj, 0) as Panel;
                            if (CurrentNode.IsNoArrangementNode)
                                disappear = itemLayoutRoot.Resources["LostHighlight_NoArrangement"] as Storyboard;
                            else
                                disappear = itemLayoutRoot.Resources["LostHighlight"] as Storyboard;
                            break;
                        }
                    }

                }
                #endregion

                //...Get the next node and try to begin appear storyboard
                #region [Appear]
                {
                    if (disappear != null)
                    {
                        EventHandler handler = null;
                        handler = (obj, arg) =>
                        {
                            try
                            {
                                CurrentNode = Global.Instance.AgendaSource.GetNextCalendarNode();
                                AgendaSourceStateChanged(Global.Instance, EventArgs.Empty);
                                HighlightCurrentNode(CurrentNode, true);
                                disappear.Completed -= handler;
                            }
                            catch { }
                        };

                        disappear.Completed += handler;
                        disappear.Begin();
                    }
                    else
                    {
                        CurrentNode = Global.Instance.AgendaSource.GetNextCalendarNode();
                        AgendaSourceStateChanged(Global.Instance, EventArgs.Empty);
                        HighlightCurrentNode(CurrentNode, true);
                    }
                }
                #endregion
            };

            //...date changed
            if (CurrentNode.IsNoArrangementNode && DateTime.Now.Date > CurrentNode.BeginTime.Date)
            {
                (this.ApplicationBar.Buttons[0] as ApplicationBarIconButton).IconUri
                    = new Uri(String.Format("/icons/days/{0}/{0}_{1}.png", DateTime.Now.Month, DateTime.Now.Day), UriKind.RelativeOrAbsolute);

                core();
            }
            //...Current node is expired
            else if (!CurrentNode.IsNoArrangementNode && DateTime.Now > CurrentNode.BeginTime)
            {
                core();
            }
        }

        private void LongListtSelector_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            var lls = sender as LongListSelector;

            if (lls.SelectedItem == null)
                return;

            var item = lls.SelectedItem as CalendarNode;
            lls.SelectedItem = null;

            if (!item.IsNoArrangementNode)
                switch (item.NodeType)
                {
                    case CalendarNodeType.kActivity:
                        this.NavigationService.Navigate(new Uri("/Pages/Activity.xaml?q=" + item.Id, UriKind.RelativeOrAbsolute));
                        return;
                    case CalendarNodeType.kObligedCourse:
                    case CalendarNodeType.kOptionalCourse:
                        this.NavigationService.Navigate(new Uri(String.Format("/Pages/CourseDetail.xaml?q={0}&d={1}", item.Id, item.BeginTime), UriKind.RelativeOrAbsolute));
                        return;
                    case CalendarNodeType.kExam:
                        this.NavigationService.Navigate(new Uri(String.Format("/Pages/CourseDetail.xaml?q={0}", item.Id), UriKind.RelativeOrAbsolute));
                        return;
                }
        }

        private void UpdateTopItem()
        {
            DependencyObject depObj = LongListSelector_Core;

            while (!(depObj is VirtualizingStackPanel))
            {
                depObj = VisualTreeHelper.GetChild(depObj, 0);
            }

            int count = VisualTreeHelper.GetChildrenCount(depObj);

            for (int i = 0; i < count; ++i)
            {
                var curGroup = VisualTreeHelper.GetChild(depObj, i) as Control;

                var pnt = curGroup.TransformToVisual(LongListSelector_Core).Transform(new Point());

                if (pnt.Y < curGroup.RenderSize.Height  //...At top
                    && pnt.Y >= 0   //...The node is fully visible
                    )
                {
                    if (curGroup.DataContext != null)
                    {
                        var src = (curGroup.DataContext as LongListSelectorItem).Item;

                        if (src is CalendarGroup<CalendarNode>)
                        {
                            TopItemSource = src as CalendarGroup<CalendarNode>;
                        }
                        else if (src is CalendarNode)
                        {
                            var groups = (LongListSelector_Core.ItemsSource as IEnumerable<CalendarGroup<CalendarNode>>);
                            if (groups != null)
                            {
                                var group = groups.Where((g) => g.Key == (src as CalendarNode).BeginTime.Date).SingleOrDefault();
                                if (group != null)
                                {
                                    TopItemSource = group;
                                }
                            }
                        }
                        else
                        {
                            //...Do nothing
                        }
                        break;
                    }
                }
            }

            if (CurrentNode != null)
                HighlightCurrentNode(CurrentNode);
        }

        private void HighlightCurrentNode(CalendarNode targetSource, bool playTransition = false)
        {
            DependencyObject obj = LongListSelector_Core;
            while (!(obj is VirtualizingStackPanel))
            {
                obj = VisualTreeHelper.GetChild(obj, 0);
            }

            int count = VisualTreeHelper.GetChildrenCount(obj);

            for (int i = 0; i < count; ++i)
            {
                var tmp = VisualTreeHelper.GetChild(obj, i);

                var source = (tmp as Control).DataContext as LongListSelectorItem;
                CalendarNode srcItem = source == null ? null : source.Item as CalendarNode;
                if (srcItem != null && targetSource.CompareTo(srcItem) == 0)
                {
                    obj = tmp;

                    //...Get the layout root of the item
                    while (!(obj is ContentPresenter))
                    {
                        obj = VisualTreeHelper.GetChild(obj, 0);
                    }

                    var itemLayoutRoot = VisualTreeHelper.GetChild(obj, 0) as Panel;
                    Storyboard sb = null;
                    if (targetSource.IsNoArrangementNode)
                    {
                        if (playTransition)
                            sb = itemLayoutRoot.Resources["GetHighlight_NoArrangement"] as Storyboard;
                        else
                            sb = itemLayoutRoot.Resources["RefreshHighlight_NoArrangement"] as Storyboard;
                    }
                    else
                    {
                        if (playTransition)
                            sb = itemLayoutRoot.Resources["GetHighlight"] as Storyboard;
                        else
                            sb = itemLayoutRoot.Resources["RefreshHighlight"] as Storyboard;
                    }
                    sb.Begin();
                    break;
                }
            }

        }
    }
}