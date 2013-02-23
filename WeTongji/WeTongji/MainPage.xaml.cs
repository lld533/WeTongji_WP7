using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Diagnostics;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.Phone.Tasks;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Controls.Primitives;
using Microsoft.Phone.Shell;
using System.Windows.Navigation;
using WeTongji.DataBase;
using System.IO.IsolatedStorage;
using WeTongji.Api.Domain;
using System.Collections.ObjectModel;
using WeTongji.Business;
using WeTongji.Api.Request;
using WeTongji.Api.Response;
using WeTongji.Api;
using WeTongji.Pages;


namespace WeTongji
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            this.Loaded += (o, e) =>
                {
                    var store = IsolatedStorageFile.GetUserStoreForApplication();

                    using (var db = WTShareDataContext.ShareDB)
                    {
                        ListBox_Activity.ItemsSource = new ObservableCollection<ActivityExt>(db.Activities);
                    }
                };

        }

        #region [Overridden]

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ThemeManager.ToDarkTheme();

            if (e.NavigationMode == NavigationMode.New)
            {
                using (var db = WTShareDataContext.ShareDB)
                {
#if true
                    if (!db.DatabaseExists())
                    {
                        db.CreateDatabase();
                        Refresh();
                    }
#else
                    if (db.DatabaseExists())
                    {
                        db.DeleteDatabase();
                    }
                    db.CreateDatabase();
                    Refresh();
#endif
                }
            }           
        }

        #endregion

        #region [Properties]

        private ApplicationBarIconButton Button_Login
        {
            get { return this.ApplicationBar.Buttons[0] as ApplicationBarIconButton; }
        }

        #endregion

        #region [Functions]

        #region [Nav]

        private void MyButtonClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var str = button.Content as String;
            this.NavigationService.Navigate(new Uri("/Pages/" + str + ".xaml", UriKind.RelativeOrAbsolute));
        }

        void Open(object sender, RoutedEventArgs e)
        {
            (this.Resources["OpenPopup"] as Storyboard).Begin();
        }

        void Close(object sender, RoutedEventArgs e)
        {
            (this.Resources["ClosePopup"] as Storyboard).Begin();
        }

        void NavToPeopleOfWeek(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Pages/PeopleOfWeek.xaml", UriKind.RelativeOrAbsolute));
        }

        void NavToForgotPassword(Object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Pages/ForgotPassword.xaml", UriKind.RelativeOrAbsolute));
        }

        private void NavigateToSignUp(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Pages/TongjiMail.xaml", UriKind.RelativeOrAbsolute));
        }

        private void ListBox_Activity_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            var lb = sender as ListBox;

            if (lb.SelectedIndex == -1)
                return;

            var a = lb.SelectedItem as ActivityExt;
            lb.SelectedIndex = -1;
            this.NavigationService.Navigate(new Uri("/Pages/Activity.xaml?q=" + a.Id, UriKind.RelativeOrAbsolute));
        }

        private void NavToCampusInfo(Object sender, RoutedEventArgs e)
        {
            var ctrl = sender as Control;
            this.NavigationService.Navigate(new Uri("/Pages/CampusInfo.xaml?q=" + ctrl.DataContext.ToString(), UriKind.RelativeOrAbsolute));
        }

        private void ViewPersonalProfile(Object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Pages/PersonalProfile.xaml", UriKind.RelativeOrAbsolute));
        }

        private void ViewMyFavorite(Object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Pages/MyFavorite.xaml", UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// View the alarm item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewAlarmItem(Object sender, RoutedEventArgs e)
        {
            // Todo @_@ Check the type of the alarm item and get the destination page
            // In the demo, just navigate to CourseDetail page, i.e., activity is neglected.

            if (TextBlock_NoCourse.Visibility == Visibility.Collapsed)
            {
                this.NavigationService.Navigate(new Uri("/Pages/CourseDetail.xaml", UriKind.RelativeOrAbsolute));
            }
        }

        private void ViewMyAgenda(Object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Pages/MyAgenda.xaml", UriKind.RelativeOrAbsolute));
        }

        #endregion

        #region [Api]

        /// <summary>
        /// LogOn demo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogOn_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Refresh()
        {
            RefreshActivityList();
        }

        private void RefreshActivityList()
        {
            WTDispatcher.Instance.Do(() =>
            {
                ActivitiesGetRequest<ActivitiesGetResponse> req = new ActivitiesGetRequest<ActivitiesGetResponse>();
                WTDefaultClient<ActivitiesGetResponse> client = new WTDefaultClient<ActivitiesGetResponse>();

                //...Tell the user that the background thread starts to work
                this.Dispatcher.BeginInvoke(() =>
                {
                    ProgressBarPopup.Instance.Open();
                });

                //...Update Activities
                client.ExecuteCompleted += (o, arg) =>
                {
                    using (var db = WTShareDataContext.ShareDB)
                    {
                        int count = arg.Result.Activities.Count();
                        for (int i = 0; i < count; ++i)
                        {
                            var item = arg.Result.Activities.ElementAt(i);
                            var itemInDB = db.Activities.Where((a) => a.Id == item.Id).FirstOrDefault();

                            //...There is no such item
                            if (itemInDB == null)
                            {
                                var tmp = new ActivityExt();
                                tmp.SetObject(item);

                                db.Activities.InsertOnSubmit(tmp);

                                //list.Add(tmp);
                                this.Dispatcher.BeginInvoke(() =>
                                {
                                    (ListBox_Activity.ItemsSource as ObservableCollection<ActivityExt>).Add(tmp);
                                });
                            }
                            //...Already in DB
                            else
                            {
                                itemInDB.Favorite = item.Favorite;
                                itemInDB.Like = item.Like;
                                itemInDB.Schedule = item.Schedule;

                                //...Todo @_@ Update CanFavorite, CanSchedule, CanLike, etc.
                                // if user signed in.
                            }
                        }

                        //...Todo @_@ Update NextPager;


                        db.SubmitChanges();
                    }

                    this.Dispatcher.BeginInvoke(() =>
                    {
                        //...Tell the user that the background thread stops working.
                        ProgressBarPopup.Instance.Close();
                    });
                };

                client.ExecuteFailed += (o, arg) =>
                {
                    //...Do Nothing if failed
                    Debug.WriteLine(arg.Error);

                    this.Dispatcher.BeginInvoke(() =>
                    {
                        ProgressBarPopup.Instance.Close();
                    });
                };

                client.Execute(req);
            });
        }

        #endregion

        #region [Visual]

        private void PasswordBox_Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdateLoginButton(null, null);
        }

        private void UpdateLoginButton(object sender, TextChangedEventArgs e)
        {
            if (!String.IsNullOrEmpty(PasswordBox_Password.Password) && !String.IsNullOrEmpty(TextBox_Id.Text))
            {
                Button_Login.IsEnabled = true;
            }
            else
            {
                Button_Login.IsEnabled = false;
            }
        }

        private void Panorama_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
        }

        #endregion

        #endregion
    }
}