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

namespace WeTongji
{
    public partial class MyAgenda : PhoneApplicationPage
    {
        private int highlightIdx = -1;

        public MyAgenda()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New)
            {
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


                var thread = new Thread(new ThreadStart(LoadData))
                {
                    IsBackground = true,
                    Name = "LoadData"
                };

                ProgressBarPopup.Instance.Open();
                thread.Start();
            }
        }

        private void AppButton_Today_Clicked(Object sender, EventArgs e)
        {
            if (LongListSelector_Core.ItemsSource != null)
            {
                try
                {
                    var src = LongListSelector_Core.ItemsSource as List<Group<CalendarNode>>;
                    var group = src.Where((g) => g.Key == DateTime.Now.Date).SingleOrDefault();
                    if (group != null)
                    {
                        LongListSelector_Core.ScrollToGroup(group);
                    }
                }
                catch { }
            }
        }

        private void LoadData()
        {
            Debug.WriteLine("LoadData started.");

            var list = new List<CalendarNode>();
            CourseExt[] courses = null;
            ExamExt[] exams = null;
            Semester[] semesters = null;
            String serializedActivityIdArr = String.Empty;

            using (var db = new WTUserDataContext(Global.Instance.Settings.UID))
            {
                courses = db.Courses.ToArray();
                exams = db.Exams.ToArray();
                semesters = db.Semesters.ToArray();
                var activity = db.Favorites.Where((fo) => fo.Id == (int)FavoriteIndex.kActivity).SingleOrDefault();
                if (activity != null)
                    serializedActivityIdArr = activity.Value;
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

            if (exams != null)
            {
                foreach (var e in exams)
                {
                    list.Add(e.GetCalendarNode());
                }
            }

            if (!String.IsNullOrEmpty(serializedActivityIdArr))
            {
                var strIdArr = serializedActivityIdArr.Split("_".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                int count = strIdArr.Count();
                int id;
                ActivityExt activity;

                for (int i = 0; i < count; ++i)
                {
                    if (Int32.TryParse(strIdArr[i], out id))
                    {
                        using (var db = WTShareDataContext.ShareDB)
                        {
                            activity = db.Activities.Where((a) => a.Id == id).SingleOrDefault();
                        }

                        if (activity != null)
                        {
                            list.Add(activity.GetCalendarNode());
                        }
                    }

                }
            }

            if (list.Count > 0)
            {
                var calendarNodeByDay = (from CalendarNode node in list
                                         group node by node.BeginTime.Date into n
                                         orderby n.Key
                                         select new Group<CalendarNode>(n.Key, n)).ToList();

                CalendarNode nodeToScrollTo = null;
                var today = calendarNodeByDay.Where((node) => node.Key == DateTime.Now.Date).SingleOrDefault();

                //...No arrangement today
                if (today == null)
                {
                    int idx = calendarNodeByDay.Where((node) => node.Key < DateTime.Now).Count();
                    calendarNodeByDay.Insert(idx, new Group<CalendarNode>(DateTime.Now.Date, new CalendarNode[] { CalendarNode.NoArrangementNode }));
                    nodeToScrollTo = calendarNodeByDay.ElementAt(idx).Items.First();
                }
                else
                {
                    //...All arrangements today are expired
                    if (today.Last().BeginTime < DateTime.Now)
                    {
                        today.Items.Add(CalendarNode.NoArrangementNode);
                        nodeToScrollTo = today.Items.Last();
                    }
                    else
                    {
                        nodeToScrollTo = today.Items.Where((node) => node.BeginTime > DateTime.Now).SingleOrDefault();
                    }
                }

                this.Dispatcher.BeginInvoke(() =>
                {
                    TextBlock_Loading.Visibility = Visibility.Collapsed;
                    LongListSelector_Core.ItemsSource = calendarNodeByDay;

                    if (nodeToScrollTo != null)
                    {
                        LongListSelector_Core.UpdateLayout();
                        LongListSelector_Core.ScrollTo(nodeToScrollTo);
                        LongListSelector_Core.UpdateLayout();

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


                    (this.ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = true;

                    ProgressBarPopup.Instance.Close();
                });
            }

        }

        private void LongListtSelector_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            var lls = sender as LongListSelector;

            if (lls.SelectedItem == null)
                return;

            var item = lls.SelectedItem as CalendarNode;
            lls.SelectedItem = null;

            switch (item.NodeType)
            {
                case CalendarNodeType.kActivity:
                    this.NavigationService.Navigate(new Uri("/Pages/Activity.xaml?q=" + item.Id, UriKind.RelativeOrAbsolute));
                    return;
                case CalendarNodeType.kObligedCourse:
                case CalendarNodeType.kOptionalCourse:
                    this.NavigationService.Navigate(new Uri(String.Format("/Pages/CourseDetail.xaml?q={0}&d={1}",item.Id, item.BeginTime), UriKind.RelativeOrAbsolute));
                    return;
                case CalendarNodeType.kExam:
                    this.NavigationService.Navigate(new Uri(String.Format("/Pages/CourseDetail.xaml?q={0}", item.Id), UriKind.RelativeOrAbsolute));
                    return;
            }
        }


        #region [Class]

        public class Group<T> : IEnumerable<T> where T : CalendarNode
        {
            public Group(DateTime key, IEnumerable<T> items)
            {
                this.Key = key;
                this.Items = items.OrderBy((T) => T).ToList<T>();
            }

            public override bool Equals(object obj)
            {
                Group<T> that = obj as Group<T>;

                return (that != null) && (this.Key.Equals(that.Key));
            }

            public DateTime Key
            {
                get;
                set;
            }

            public IList<T> Items
            {
                get;
                set;
            }

            #region IEnumerable<T> Members

            public IEnumerator<T> GetEnumerator()
            {
                return this.Items.GetEnumerator();
            }

            #endregion

            #region IEnumerable Members

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.Items.GetEnumerator();
            }

            #endregion
        }


        #endregion
    }
}