using System;
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
using System.Globalization;
using WeTongji.Utility;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using System.Reflection;

namespace WeTongji
{
    public partial class MyAgenda : PhoneApplicationPage
    {
        #region [Fields]

        private DispatcherTimer refresh_dt = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };

        private Boolean registerVerticalScrollChanged = false;

        private event EventHandler VerticalScrollChanged;

        #endregion

        #region [Const]

        private const String SharedShellContentPath = "/Shared/ShellContent/";

        private const String StandardTileBackgroundImageName = SharedShellContentPath + "StandardTileBackgroundImage";

        private const String StandardTileBackgroundImageSearchPattern = StandardTileBackgroundImageName + ".*";

        #endregion

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

        #region [Override Navigation]

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            #region [Handles AppBar Buttons]

            #region [Today]

            //...Create AppButton_Today if it has not been created yet.
            if (this.ApplicationBar == null || this.ApplicationBar.Buttons.Count == 0)
            {
                var appBtn = new Microsoft.Phone.Shell.ApplicationBarIconButton()
                {
                    Text = StringLibrary.MyAgenda_AppBarTodayText,
                    IsEnabled = false
                };

                //...Todo @_@ Localizable
                if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "zh")
                    appBtn.IconUri = new Uri(String.Format("/icons/days_zh/{0}/{0}_{1}.png", DateTime.Now.Month, DateTime.Now.Day),
                            UriKind.RelativeOrAbsolute);
                else
                    appBtn.IconUri = new Uri(String.Format("/icons/days_en/{0}/{0}_{1}.png", DateTime.Now.Month, DateTime.Now.Day),
                        UriKind.RelativeOrAbsolute);

                appBtn.Click += AppButton_Today_Clicked;
                this.ApplicationBar = new Microsoft.Phone.Shell.ApplicationBar();
                this.ApplicationBar.Buttons.Add(appBtn);
            }
            //...Refresh the icon AppButton_Today if it has already been created.
            else
            {
                //...Todo @_@ Localizable
                if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "zh")
                    TodayButton.IconUri = new Uri(String.Format("/icons/days_zh/{0}/{0}_{1}.png", DateTime.Now.Month, DateTime.Now.Day),
                            UriKind.RelativeOrAbsolute);
                else
                    TodayButton.IconUri = new Uri(String.Format("/icons/days_en/{0}/{0}_{1}.png", DateTime.Now.Month, DateTime.Now.Day),
                        UriKind.RelativeOrAbsolute);

                TodayButton.IsEnabled = false;
            }

            #endregion

            #region [Pin to start]

            {
                var btn = new ApplicationBarIconButton();
                ShellTile TileToFind = ShellTile.ActiveTiles.SingleOrDefault(
                    x => x.NavigationUri.ToString().Contains("nav=MyAgenda") && x.NavigationUri.ToString().Contains("uid="));

                //...Create a new Pin/Unpin Button and insert it into AppBar
                if (this.ApplicationBar.Buttons.Count == 1)
                {
                    if (TileToFind == null)
                    {
                        btn.Text = StringLibrary.MyAgenda_AppBarPin;
                        btn.IconUri = new Uri("/icons/appbar.pin.png", UriKind.RelativeOrAbsolute);
                        btn.Click += AppBar_Pin_Clicked;
                    }
                    else
                    {
                        btn.Text = StringLibrary.MyAgenda_AppBarUnpin;
                        btn.IconUri = new Uri("/icons/appbar.unpin.png", UriKind.RelativeOrAbsolute);
                        btn.Click += AppBar_Unpin_Clicked;
                    }

                    this.ApplicationBar.Buttons.Add(btn);
                }
                //...Refresh Pin/Unpin Button state
                else if (this.ApplicationBar.Buttons.Count == 2)
                {
                    PinButton.Click -= AppBar_Unpin_Clicked;
                    PinButton.Click -= AppBar_Pin_Clicked;

                    if (TileToFind == null)
                    {
                        PinButton.Text = StringLibrary.MyAgenda_AppBarPin;
                        PinButton.IconUri = new Uri("/icons/appbar.pin.png", UriKind.RelativeOrAbsolute);
                        PinButton.Click += AppBar_Pin_Clicked;
                    }
                    else
                    {
                        PinButton.Text = StringLibrary.MyAgenda_AppBarUnpin;
                        PinButton.IconUri = new Uri("/icons/appbar.unpin.png", UriKind.RelativeOrAbsolute);

                        PinButton.Click += AppBar_Unpin_Clicked;
                    }
                }

            }

            #endregion

            #endregion

            Global.Instance.AgendaSourceStateChanged += this.AgendaSourceStateChangedHandler;

            if (String.IsNullOrEmpty(Global.Instance.CurrentUserID) && Global.Instance.CurrentAgendaSourceState == SourceState.NotSet)
            {
                Global.Instance.CurrentUserID = this.NavigationContext.QueryString["uid"];
                Global.Instance.StartSettingAgendaSource();

                var loadLocalAgendaThread = new Thread(new ThreadStart(LoadLocalAgenda));
                loadLocalAgendaThread.Start();
            }
            else
            {
                var thread = new Thread(new ThreadStart(LoadData));
                thread.Start();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            Global.Instance.AgendaSourceStateChanged -= this.AgendaSourceStateChangedHandler;
        }

        #endregion

        #region [Properties]

        private ApplicationBarIconButton TodayButton
        {
            get { return this.ApplicationBar.Buttons[0] as ApplicationBarIconButton; }
        }

        private ApplicationBarIconButton PinButton
        {
            get { return this.ApplicationBar.Buttons[1] as ApplicationBarIconButton; }
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

        #endregion

        #region [Event handlers]

        private void AgendaSourceStateChanged(Object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                var src = this.LongListSelector_Core.ItemsSource;
                LongListSelector_Core.ItemsSource = null;
                LongListSelector_Core.ItemsSource = src;

                var node = Global.Instance.AgendaSource.GetNextCalendarNode();
            });
        }

        private void AppButton_Today_Clicked(Object sender, EventArgs e)
        {
            if (LongListSelector_Core.ItemsSource != null)
            {
                try
                {
                    FlurryWP8SDK.Api.LogEvent(((int)FlurryWP8SDK.Models.EventName.ClickAppBarTodayButton).ToString());

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
                                LongListSelector_Core.UpdateLayout();

                                #region [Binding Top item with scrolling offset]

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

                                #endregion

                                if (nodeToScrollTo.IsNoArrangementNode)
                                {
                                    AppButton_Today_Clicked(this.ApplicationBar.Buttons[0], EventArgs.Empty);
                                }
                                else
                                {
                                    LongListSelector_Core.ScrollTo(nodeToScrollTo);
                                    LongListSelector_Core.UpdateLayout();

                                    #region [Play donate animation]
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
                                                Storyboard sb = itemLayoutRoot.Resources["Donate"] as Storyboard;
                                                sb.Begin();
                                                break;
                                            }
                                        }
                                    }
                                    #endregion
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

                            TodayButton.IsEnabled = true;
                            refresh_dt.Start();
                        });
                    }
                    break;
                case SourceState.NotSet:
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            LongListSelector_Core.ItemsSource = null;
                            TodayButton.IsEnabled = false;
                        });
                    }
                    break;
                case SourceState.Setting:
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            TextBlock_Loading.Visibility = Visibility.Visible;
                            TodayButton.IsEnabled = false;
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
                CurrentNode = Global.Instance.AgendaSource.GetNextCalendarNode();
                AgendaSourceStateChanged(Global.Instance, EventArgs.Empty);
            };

            //...date changed
            if (CurrentNode.IsNoArrangementNode && DateTime.Now.Date > CurrentNode.BeginTime.Date)
            {
                //...Todo @_@ Localizable
                if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "zh")
                    TodayButton.IconUri
                        = new Uri(String.Format("/icons/days_zh/{0}/{0}_{1}.png", DateTime.Now.Month, DateTime.Now.Day), UriKind.RelativeOrAbsolute);
                else
                    TodayButton.IconUri
                        = new Uri(String.Format("/icons/days_en/{0}/{0}_{1}.png", DateTime.Now.Month, DateTime.Now.Day), UriKind.RelativeOrAbsolute);

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

        private void AppBar_Pin_Clicked(Object sender, EventArgs e)
        {
            //...Source state has been set
            if (Global.Instance.CurrentAgendaSourceState != SourceState.Done)
                return;


            // Look to see if the tile already exists and if so, don't try to create again.
            ShellTile TileToFind = ShellTile.ActiveTiles.SingleOrDefault(
                    x => x.NavigationUri.ToString().Contains("nav=MyAgenda") && x.NavigationUri.ToString().Contains("uid="));

            // Create the tile if we didn't find it already exists.
            if (TileToFind == null && !String.IsNullOrEmpty(Global.Instance.CurrentUserID))
            {
                CalendarNode node = Global.Instance.AgendaSource.GetNextIconicTileCalendarNode();

                #region [Iconic tile for WP7.8 or WP8 users]
                //...Current OS platform supports wide tile
                if (WeTongji.Utility.PlatformVersionHelper.IsTargetVersion)
                {
                    //...Todo @_@ implement the wide tile
                    String IconicTileIconImagePathPattern = "/icons/tile/wp8/IconImage/{0}/{1}.png";
                    String IconicTileSmallIconImagePathPattern = "/icons/tile/wp8/IconImage/{0}/{1}.png";

                    var tileData = Mangopollo.Tiles.TilesCreator.CreateIconicTile
                        (
                            "WeTongji",
                            DateTime.Now.Day,
                            new Color(),
                            new Uri(String.Format(IconicTileIconImagePathPattern,
                                                CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "zh" ? "zh" : "en",
                                                DateTime.Now.Month),
                                    UriKind.RelativeOrAbsolute),
                            new Uri(String.Format(IconicTileSmallIconImagePathPattern,
                                                CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "zh" ? "zh" : "en",
                                                DateTime.Now.Month),
                                    UriKind.RelativeOrAbsolute),
                            node == null ? String.Empty : node.Title,
                            node == null ? String.Empty : node.IconicTileDisplayTime,
                            node == null ? String.Empty : node.Location
                        );

                    Type type = Type.GetType("Microsoft.Phone.Shell.ShellTile, Microsoft.Phone");
                    MethodInfo createmethod = type.GetMethod("Create", new[] { typeof(Uri), typeof(ShellTileData), typeof(bool) });

                    createmethod.Invoke(null, new Object[] 
                    {
                        new Uri("/Pages/MyAgenda.xaml?nav=MyAgenda&uid=" + Global.Instance.CurrentUserID, UriKind.Relative), 
                        tileData,
                        true
                    });

                }
                #endregion
                #region [Standard tile for WP7.5 users]
                //...Current OS platform does NOT support wide tile
                else
                {
                    Uri backgroundImageUri = new Uri("/icon_173_173.png", UriKind.RelativeOrAbsolute);

                    //...Current user's avatar is used as the background image
                    using (var db = new WTUserDataContext(Global.Instance.CurrentUserID))
                    {
                        var avatar = db.UserInfo.Single().Avatar;
                        if (!avatar.EndsWith("missing.png"))
                        {
                            var store = IsolatedStorageFile.GetUserStoreForApplication();
                            var files = store.GetFileNames(StandardTileBackgroundImageSearchPattern);
                            if (files != null)
                            {
                                foreach (var existingBackgroundImage in files)
                                {
                                    store.DeleteFile(SharedShellContentPath + existingBackgroundImage);
                                }
                            }

                            String ext = "." + avatar.GetImageFileExtension();

                            //...Copy current user's avatar to share folder so that the image could be retrieved by system.
                            if (store.FileExists(db.UserInfo.Single().AvatarGuid + ext))
                            {
                                store.CopyFile(db.UserInfo.Single().AvatarGuid + ext, StandardTileBackgroundImageName + ext);
                                backgroundImageUri = new Uri("isostore:" + StandardTileBackgroundImageName + ext, UriKind.Absolute);
                            }
                        }
                    }

                    //...Create tile data
                    StandardTileData NewTileData = new StandardTileData()
                    {
                        BackgroundImage = backgroundImageUri,
                        BackTitle = "WeTongji",
                        BackContent = (node == null) ? String.Empty : node.Title
                    };
                    ShellTile.Create(new Uri("/Pages/MyAgenda.xaml?nav=MyAgenda&uid=" + Global.Instance.CurrentUserID, UriKind.Relative), NewTileData);
                }
                #endregion

                PinButton.IconUri = new Uri("/icons/appbar.unpin.png", UriKind.RelativeOrAbsolute);
                PinButton.Text = StringLibrary.MyAgenda_AppBarUnpin;
            }
        }

        private void AppBar_Unpin_Clicked(Object sender, EventArgs e)
        {
            ShellTile TileToFind = ShellTile.ActiveTiles.SingleOrDefault(
                    x => x.NavigationUri.ToString().Contains("nav=MyAgenda") && x.NavigationUri.ToString().Contains("uid="));

            if (null != TileToFind)
            {
                TileToFind.Delete();

                PinButton.IconUri = new Uri("/icons/appbar.pin.png", UriKind.RelativeOrAbsolute);
                PinButton.Text = StringLibrary.MyAgenda_AppBarPin;

                var store = IsolatedStorageFile.GetUserStoreForApplication();
                var files = store.GetFileNames(StandardTileBackgroundImageSearchPattern);
                if (files != null)
                    foreach (var f in files)
                    {
                        store.DeleteFile(SharedShellContentPath + f);
                    }
            }
        }

        #endregion

        #region [Private functions]

        private void LoadData()
        {
            AgendaSourceStateChangedHandler(Global.Instance, EventArgs.Empty);
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
        }

        /// <summary>
        /// Load current user's course,exams and activities to construct Global agenda.
        /// </summary>
        /// <remarks>
        /// Pre-condition: !String.IsNullOrEmpty(Global.Instance.CurrentUserID)
        /// </remarks>
        private void LoadLocalAgenda()
        {
            if (String.IsNullOrEmpty(Global.Instance.CurrentUserID))
            {
                return;
            }

            List<CalendarNode> list = new List<CalendarNode>();
            CourseExt[] courses = null;
            ExamExt[] exams = null;
            Semester[] semesters = null;

            #region [Collect source]

            #region [Collect courses]
            using (var db = new WTUserDataContext(Global.Instance.CurrentUserID))
            {
                courses = db.Courses.ToArray();
                exams = db.Exams.ToArray();
                semesters = db.Semesters.ToArray();
            }

            if (courses != null && semesters != null)
            {
                foreach (var c in courses)
                {
                    var s = semesters.Where((semester) => semester.Id == c.SemesterGuid).SingleOrDefault();
                    if (s != null)
                    {
                        list.AddRange(c.GetCalendarNodes(s));
                    }
                }
            }
            #endregion

            #region [Collect exams]
            if (exams != null)
            {
                foreach (var e in exams)
                {
                    list.Add(e.GetCalendarNode());
                }
            }
            #endregion

            #region [Collect activities]

            try
            {
                ActivityExt[] allActivities = null;
                ItemId[] activityIds = null;

                using (var db = WTShareDataContext.ShareDB)
                {
                    allActivities = db.Activities.ToArray();
                }

                using (var db = new WTUserDataContext(Global.Instance.CurrentUserID))
                {
                    activityIds = db.ScheduledActivitiesId.ToArray();
                }

                foreach (var id in activityIds)
                {
                    var a = allActivities.Where(x => x.Id == id.Id).SingleOrDefault();
                    if (a != null)
                    {
                        list.Add(a.GetCalendarNode());
                        Global.Instance.ParticipatingActivitiesIdList.Add(id.Id);
                    }
                }
            }
            catch { }

            #endregion


            #endregion

            var result = (from CalendarNode node in list
                          group node by node.BeginTime.Date into n
                          orderby n.Key
                          select new CalendarGroup<CalendarNode>(n.Key, n)).ToList();

            Global.Instance.SetAgendaSource(result);
        }

        #endregion
    }
}