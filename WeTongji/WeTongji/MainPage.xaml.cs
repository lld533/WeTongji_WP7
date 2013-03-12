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
using System.Threading;
using WeTongji.Utility;


namespace WeTongji
{
    public partial class MainPage : PhoneApplicationPage
    {
        #region [Fields]
        private static Boolean isResourceLoaded = false;

        private int refreshCounter = 0;

        private int refreshNewsCounter = 0;

        private ActivitySortMethod activitySortMethod = ActivitySortMethod.kByCreationTime;

        private Boolean hasLoadMoreActivities = false;

        #endregion

        #region

        private Thread PersonThread;
        private Thread ActivityThread;

        #endregion

        // Constructor
        public MainPage()
        {
            PersonThread = new Thread(new ThreadStart(RefreshPeopleOfWeek))
            {
                IsBackground = true,
                Name = "RefreshPeopleOfWeek"
            };

            ActivityThread = new Thread(new ThreadStart(RefreshExistingExpiredActivities))
            {
                IsBackground = true,
                Name = "RefreshActivitiesForTheFirstTime"
            };

            InitializeComponent();
        }

        #region [Overridden]

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (Global.Instance.Settings.HintOnExit)
            {
                var result = MessageBox.Show("你确认要退出微同济吗？", "", MessageBoxButton.OKCancel);
                if (MessageBoxResult.Cancel == result)
                {
                    e.Cancel = true;
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New)
            {
                using (var db = WTShareDataContext.ShareDB)
                {
#if true
                    if (!db.DatabaseExists())
                    {
                        db.CreateDatabase();
                    }
#else
                    if (db.DatabaseExists())
                    {
                        db.DeleteDatabase();
                    }
                    db.CreateDatabase();
#endif
                }

                if (!isResourceLoaded)
                {
                    var thread = new System.Threading.Thread(new ThreadStart(LoadDataFromDatabase))
                    {
                        IsBackground = true,
                        Name = "LoadDataFromDatabase"
                    };

                    thread.Start();
                }
            }
        }

        #endregion

        #region [Properties]

        private ApplicationBarIconButton Button_Login
        {
            get
            {
                if (Panorama_Core.SelectedIndex == 0 && Border_SignedOut.Visibility == Visibility.Visible)
                    return this.ApplicationBar.Buttons[0] as ApplicationBarIconButton;
                return null;
            }
        }

        private int RefreshCounter
        {
            get { return refreshCounter; }
            set
            {
                if (value != refreshCounter)
                {
                    refreshCounter = value;

                    this.Dispatcher.BeginInvoke(() =>
                    {
                        if (refreshCounter == 0)
                        {
                            ProgressBarPopup.Instance.Close();
                        }
                        else
                        {
                            ProgressBarPopup.Instance.Open();
                        }
                    });
                }
            }
        }

        private int RefreshNewsCounter
        {
            get
            {
                return refreshNewsCounter;
            }
            set
            {
                if (value < 0 || value == RefreshNewsCounter)
                    return;

                if (0 == RefreshNewsCounter)
                {
                    ++RefreshCounter;
                }

                refreshNewsCounter = value;

                if (RefreshNewsCounter == 0)
                {
                    --RefreshCounter;
                }
            }
        }

        private UserExt UserSource
        {
            get { return Border_SignedIn.DataContext as UserExt; }
            set { }
        }

        private CalendarNode CalendarNodeSource
        {
            get { return Button_Alarm.DataContext as CalendarNode; }
            set { }
        }

        private ObservableCollection<ActivityExt> ActivityListSource
        {
            get
            {
                if (null == ListBox_Activity.ItemsSource)
                    return null;
                return ListBox_Activity.ItemsSource as ObservableCollection<ActivityExt>;
            }
            set
            {
                ListBox_Activity.ItemsSource = value;

                if (value != null && value.Count > 0)
                {
                    ListBox_Activity.Visibility = Visibility.Visible;
                }
                else
                {
                    ListBox_Activity.Visibility = Visibility.Collapsed;
                }
            }
        }

        private ForStaffExt OfficialNoteSource
        {
            get
            {
                return Button_OfficialNotes.DataContext == null ? null : Button_OfficialNotes.DataContext as ForStaffExt;
            }
            set
            {
                Button_OfficialNotes.DataContext = value;

                if (value != null)
                {
                    var imgUrls = value.GetImagesURL();
                    if (imgUrls != null && imgUrls.Count() > 0 && !value.ImageExists())
                    {
                        var url = imgUrls.First();
                        var client = new WTDownloadImageClient();
                        client.DownloadImageCompleted += (o, e) =>
                        {
                            this.Dispatcher.BeginInvoke(() =>
                                   {
                                       if (OfficialNoteSource != null)
                                       {
                                           OfficialNoteSource.SaveImage(e.ImageStream);

                                           OfficialNoteSource.SendPropertyChanged("FirstImageBrush");

                                       }
                                   });
                        };
                        client.Execute(url);
                    }
                }
            }
        }

        private ClubNewsExt ClubNewsSource
        {
            get
            {
                return Button_ClubNews.DataContext == null ? null : Button_ClubNews.DataContext as ClubNewsExt;
            }
            set
            {
                Button_ClubNews.DataContext = value;

                if (value != null)
                {
                    var imgUrls = value.GetImagesURL();
                    if (imgUrls != null && imgUrls.Count() > 0 && !value.ImageExists())
                    {
                        var url = imgUrls.First();
                        var client = new WTDownloadImageClient();
                        client.DownloadImageCompleted += (o, e) =>
                        {
                            this.Dispatcher.BeginInvoke(() =>
                                {
                                    if (ClubNewsSource != null)
                                    {
                                        ClubNewsSource.SaveImage(e.ImageStream);

                                        ClubNewsSource.SendPropertyChanged("FirstImageBrush");
                                    }
                                });
                        };
                        client.Execute(url);
                    }
                }
            }
        }

        private SchoolNewsExt TongjiNewsSource
        {
            get
            {
                return Button_TongjiNews.DataContext == null ? null : Button_TongjiNews.DataContext as SchoolNewsExt;
            }
            set
            {
                Button_TongjiNews.DataContext = value;

                if (value != null)
                {
                    var imgUrls = value.GetImagesURL();
                    if (imgUrls != null && imgUrls.Count() > 0 && !value.ImageExists())
                    {
                        var url = imgUrls.First();
                        var client = new WTDownloadImageClient();

                        client.DownloadImageCompleted += (o, e) =>
                            {
                                this.Dispatcher.BeginInvoke(() =>
                                    {
                                        if (TongjiNewsSource != null)
                                        {
                                            TongjiNewsSource.SaveImage(e.ImageStream);

                                            TongjiNewsSource.SendPropertyChanged("FirstImageBrush");
                                        }
                                    });
                            };

                        client.Execute(url);
                    }
                }
            }
        }

        private AroundExt AroundNewsSource
        {
            get
            {
                return Button_AroundNews.DataContext == null ? null : Button_AroundNews.DataContext as AroundExt;
            }
            set
            {
                Button_AroundNews.DataContext = value;

                if (value != null && !value.IsTitleImageExists())
                {
                    var client = new WTDownloadImageClient();
                    client.DownloadImageCompleted += (o, e) =>
                    {
                        this.Dispatcher.BeginInvoke(() =>
                            {
                                if (AroundNewsSource != null)
                                {
                                    AroundNewsSource.SaveImage(e.ImageStream);

                                    AroundNewsSource.SendPropertyChanged("TitleImageBrush");
                                }
                            });
                    };
                    client.Execute(value.TitleImage);
                }
            }
        }

        private PersonExt PersonSource
        {
            get
            {
                return ScrollViewer_PeopleOfWeek.DataContext == null ? null : ScrollViewer_PeopleOfWeek.DataContext as PersonExt;
            }
            set
            {
                ScrollViewer_PeopleOfWeek.DataContext = value;

                if (value != null)
                {
                    ScrollViewer_PeopleOfWeek.Visibility = Visibility.Visible;

                    #region [Avatar]

                    var avatarClient = new WTDownloadImageClient();
                    avatarClient.DownloadImageCompleted += (o, e) =>
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            if (PersonSource != null)
                            {
                                PersonSource.SaveAvatar(e.ImageStream);
                                PersonSource.SendPropertyChanged("AvatarImageBrush");
                            }
                        });

                    };
                    avatarClient.Execute(value.Avatar);

                    #endregion

                    #region [First Image]

                    var images = value.GetImages();
                    if (images.Count > 0 && !value.ImageExists())
                    {
                        var client = new WTDownloadImageClient();
                        client.DownloadImageCompleted += (o, e) =>
                        {
                            this.Dispatcher.BeginInvoke(() =>
                            {
                                if (PersonSource != null)
                                {
                                    PersonSource.SaveImage(e.ImageStream);
                                    PersonSource.SendPropertyChanged("FirstImageBrush");
                                }
                            });

                        };
                        client.Execute(images.First().Key);
                    }
                    #endregion
                }
                else
                {
                    ScrollViewer_PeopleOfWeek.Visibility = Visibility.Collapsed;
                }
            }
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
            var src = PersonSource;

            if (src != null)
                this.NavigationService.Navigate(new Uri(String.Format("/Pages/PeopleOfWeek.xaml?q={0}", src.Id), UriKind.RelativeOrAbsolute));
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
            if (a.IsValid)
                this.NavigationService.Navigate(new Uri("/Pages/Activity.xaml?q=" + a.Id, UriKind.RelativeOrAbsolute));
        }

        private void NavToOfficialNotesList(Object sender, RoutedEventArgs e)
        {
            var ctrl = sender as Control;
            this.NavigationService.Navigate(new Uri("/Pages/CampusInfo.xaml?q=2", UriKind.RelativeOrAbsolute));
        }

        private void NavToClubNewsList(Object sender, RoutedEventArgs e)
        {
            var ctrl = sender as Control;
            this.NavigationService.Navigate(new Uri("/Pages/CampusInfo.xaml?q=3", UriKind.RelativeOrAbsolute));
        }

        private void NavToTongjiNewsList(Object sender, RoutedEventArgs e)
        {
            var ctrl = sender as Control;
            this.NavigationService.Navigate(new Uri("/Pages/CampusInfo.xaml?q=0", UriKind.RelativeOrAbsolute));
        }

        private void NavToAroundNewsList(Object sender, RoutedEventArgs e)
        {
            var ctrl = sender as Control;
            this.NavigationService.Navigate(new Uri("/Pages/CampusInfo.xaml?q=1", UriKind.RelativeOrAbsolute));
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

            if (TextBlock_NoArrangement.Visibility == Visibility.Collapsed)
            {
                this.NavigationService.Navigate(new Uri("/Pages/CourseDetail.xaml", UriKind.RelativeOrAbsolute));
            }
        }

        private void ViewMyAgenda(Object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Pages/MyAgenda.xaml", UriKind.RelativeOrAbsolute));
        }

        private void NavigateToSettings(Object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Pages/MySettings.xaml", UriKind.RelativeOrAbsolute));
        }

        private void NavigateToAbout(Object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Pages/About.xaml", UriKind.RelativeOrAbsolute));
        }

        private void NavToPeopleOfWeekList(Object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Pages/PeopleOfWeekList.xaml", UriKind.RelativeOrAbsolute));
        }

        #endregion

        #region [Api]

        /// <summary>
        /// LogOn demo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogOn_Click(object sender, EventArgs e)
        {
            try
            {
                Button_Login.IsEnabled = false;
            }
            catch { }

            this.TextBlock_Today.Text = DateTime.Now.Day.ToString();
            this.Focus();

            var no = this.TextBox_Id.Text;
            var pw = this.PasswordBox_Password.Password;


            var req = new UserLogOnRequest<UserLogOnResponse>();
            var client = new WTDefaultClient<UserLogOnResponse>();

            req.NO = no;
            req.Password = pw;

            try
            {
                req.Validate();
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    MessageBox.Show("学号必须由数字组成", "学号错误", MessageBoxButton.OK);

                    TextBox_Id.Focus();
                    TextBox_Id.SelectAll();
                    return;
                }
                else if (ex is ArgumentNullException)
                {
                    MessageBox.Show("密码不能少于6位", "密码错误", MessageBoxButton.OK);

                    PasswordBox_Password.Focus();
                    PasswordBox_Password.SelectAll();
                    return;
                }
            }

            client.ExecuteCompleted += (obj, arg) =>
                {
                    Debug.WriteLine("User signed in. StuNo:{0}, UID:{1}, Session:{2}", no, arg.Result.User.UID, arg.Result.Session);

                    this.Dispatcher.BeginInvoke(() =>
                    {
                        Border_SignedOut.Visibility = Visibility.Collapsed;
                        TextBox_Id.Text = String.Empty;
                        PasswordBox_Password.Password = String.Empty;

                        if ((this.ApplicationBar as ApplicationBar).Buttons.Count != 1)
                        {
                            (this.ApplicationBar as ApplicationBar).Buttons.Clear();

                            ApplicationBarIconButton button;
                            button = new ApplicationBarIconButton(new Uri("/icons/appbar.refresh.rest.png", UriKind.RelativeOrAbsolute)) { Text = "刷新" };
                            button.Click += RefreshButton_Click;
                            (this.ApplicationBar as ApplicationBar).Buttons.Add(button);

                            var mi = new ApplicationBarMenuItem() { Text = "注销" };
                            mi.Click += SignOut;
                            this.ApplicationBar.MenuItems.Add(mi);
                        }
                    });

                    Global.Instance.CurrentUserID = arg.Result.User.UID;
                    Global.Instance.UpdateSettings(arg.Result.User.UID, pw, arg.Result.Session);

                    UserExt targetUser = null;

                    #region [Handle database]

                    using (var db = new WeTongji.DataBase.WTUserDataContext(arg.Result.User.UID))
                    {
                        var userInfo = db.UserInfo.SingleOrDefault();

                        //...Create an instance if the user never signs in.
                        if (userInfo == null)
                        {
                            targetUser = new UserExt();
                            targetUser.SetObject(arg.Result.User);

                            db.UserInfo.InsertOnSubmit(targetUser);
                        }
                        //...Update user's info
                        else
                        {
                            userInfo.SetObject(arg.Result.User);
                            targetUser = userInfo;
                        }

                        db.SubmitChanges();
                    }

                    #endregion

                    #region [Update UI]

                    this.Dispatcher.BeginInvoke(() =>
                    {
                        using (var db = new WeTongji.DataBase.WTUserDataContext(arg.Result.User.UID))
                        {
                            Button_Favorite.DataContext = Button_PersonalProfile.DataContext = targetUser = db.UserInfo.SingleOrDefault();
                        }
                    });

                    #endregion

                    #region [Update Avatar]

                    if (targetUser != null && !targetUser.Avatar.EndsWith("missing.png") && !targetUser.AvatarImageExists())
                    {
                        var img_client = new WTDownloadImageClient();

                        img_client.DownloadImageStarted += (s, args) =>
                            {
                                Debug.WriteLine("Download user's avatar started: {0}", args.Url);
                            };
                        img_client.DownloadImageFailed += (s, args) =>
                            {
                                Debug.WriteLine("Download user's avatar failed: {0}\nError:{1}", args.Url, args.Error);
                            };
                        img_client.DownloadImageCompleted += (s, args) =>
                            {
                                Debug.WriteLine("Download user's avatar completed: {0}", args.Url);

                                targetUser.SaveAvatarImage(args.ImageStream);

                                this.Dispatcher.BeginInvoke(() =>
                                {
                                    targetUser.SendPropertyChanged("AvatarImageBrush");
                                });
                            };

                        img_client.Execute(targetUser.Avatar);
                    }

                    #endregion

                    #region [download courses, favorites and schedule in order]

                    DownloadCourses();

                    #endregion
                };

            client.ExecuteFailed += (obj, arg) =>
                {
                    Debug.WriteLine("Sign in failed. StuNo:{0}, Pw:{1}\nError:{2}", no, pw, arg.Error);

                    var err = arg.Error;

                    this.Dispatcher.BeginInvoke(() =>
                    {
                        try
                        {
                            Button_Login.IsEnabled = true;
                        }
                        catch { }

                        if (err is WTException)
                        {
                            var wte = err as WTException;
                            switch (wte.StatusCode.Id)
                            {
                                case WeTongji.Api.Util.Status.LoginTimeOut:
                                    MessageBox.Show("登录超时,请重试");
                                    return;
                                case WeTongji.Api.Util.Status.NoAccount:
                                    {
                                        MessageBox.Show("账号不存在,请重试");
                                        TextBox_Id.Focus();
                                        TextBox_Id.SelectAll();
                                    }
                                    return;
                                case WeTongji.Api.Util.Status.NotActivatedAccount:
                                    {
                                        MessageBox.Show("账号未激活,请重试");
                                        TextBox_Id.Focus();
                                        TextBox_Id.SelectAll();
                                    }
                                    return;
                                case WeTongji.Api.Util.Status.NotRegistered:
                                    {
                                        MessageBox.Show("用户没有注册,请重试");
                                        TextBox_Id.Focus();
                                        TextBox_Id.SelectAll();
                                    }
                                    return;
                                case WeTongji.Api.Util.Status.InvalidPassword:
                                    {
                                        MessageBox.Show("密码不符合要求,请重新输入");
                                        PasswordBox_Password.Focus();
                                        PasswordBox_Password.SelectAll();
                                    }
                                    return;
                                case WeTongji.Api.Util.Status.AccountPasswordDismatch:
                                    {
                                        MessageBox.Show("密码错误,请重新输入");
                                        PasswordBox_Password.Focus();
                                        PasswordBox_Password.SelectAll();
                                    }
                                    return;
                            }

                            MessageBox.Show("登录失败，请重试");
                        }
                        else if (err is System.Net.WebException)
                        {
                            MessageBox.Show("登录失败，请检查Wifi或网络连接后重试");
                            try
                            {
                                Button_Login.IsEnabled = true;
                            }
                            catch { }
                        }
                    });
                };

            client.Execute(req);
        }

        #region [Refresh related functions]

        #region [Activities]

        private void GetAllUnexpiredActivities()
        {
            int unexpirePageId = -1;
            Action updateUnExpireAction = null;

            #region [Core action]

            updateUnExpireAction = () =>
            {
                ActivitiesGetRequest<ActivitiesGetResponse> req = new ActivitiesGetRequest<ActivitiesGetResponse>();
                WTDefaultClient<ActivitiesGetResponse> client = new WTDefaultClient<ActivitiesGetResponse>();

                Debug.WriteLine("[updateExpireAction Thread]" + Thread.CurrentThread.GetHashCode());

                this.Dispatcher.BeginInvoke(() =>
                {
                    ProgressBarPopup.Instance.Open();
                });

                #region [Add additional parameter]

                if (unexpirePageId > 0)
                    req.SetAdditionalParameter(WTDefaultClient<ActivitiesGetResponse>.PAGE, unexpirePageId);

                req.Expire = false;

                #endregion

                #region [Subscribe event handler]

                #region [Execute completed]
                client.ExecuteCompleted += (o, arg) =>
                {
                    int count = arg.Result.Activities.Count();
                    ActivityExt[] responseActivities = new ActivityExt[count];

                    for (int i = 0; i < count; ++i)
                    {
                        using (var db = WTShareDataContext.ShareDB)
                        {
                            var item = arg.Result.Activities[i];
                            var itemInDB = db.Activities.Where((a) => a.Id == item.Id).FirstOrDefault();

                            //...There is no such item
                            if (itemInDB == null)
                            {
                                itemInDB = new ActivityExt();
                                itemInDB.SetObject(item);

                                db.Activities.InsertOnSubmit(itemInDB);
                            }
                            else
                            {
                                itemInDB.SetObject(item);
                            }

                            responseActivities[i] = itemInDB;
                            db.SubmitChanges();
                        }

                        unexpirePageId = arg.Result.NextPager;
                    }

                    this.Dispatcher.BeginInvoke(() =>
                    {
                        UpdateAcitivityList(responseActivities, arg.Result.NextPager > 0 ? true : false, false);
                    });

                    if (arg.Result.NextPager > 0)
                    {
                        updateUnExpireAction();
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            ProgressBarPopup.Instance.Close();
                        });
                    }
                };
                #endregion

                #region [Execute Failed]
                client.ExecuteFailed += (o, arg) =>
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        ProgressBarPopup.Instance.Close();
                    });
                };
                #endregion

                #endregion

                if (!String.IsNullOrEmpty(Global.Instance.CurrentUserID))
                    client.Execute(req, Global.Instance.Session, Global.Instance.Settings.UID);
                else
                    client.Execute(req);
            };

            #endregion

            updateUnExpireAction();
        }

        private void RefreshExistingExpiredActivities()
        {
            Action updateExpireAction = null;

            Global.Instance.ActivityPageId = -1;
            this.Dispatcher.BeginInvoke(() =>
            {
                var src = ActivityListSource;
                if (src != null && src.Count > 0 && !src.Last().IsValid)
                {
                    src.RemoveAt(src.Count - 1);
                }
            });

            #region [Core action]
            updateExpireAction = () =>
            {
                ActivitiesGetRequest<ActivitiesGetResponse> req = new ActivitiesGetRequest<ActivitiesGetResponse>();
                WTDefaultClient<ActivitiesGetResponse> client = new WTDefaultClient<ActivitiesGetResponse>();

                this.Dispatcher.BeginInvoke(() =>
                {
                    ProgressBarPopup.Instance.Open();
                });

                Debug.WriteLine("[updateExpireAction Thread]" + Thread.CurrentThread.GetHashCode());

                #region [Add additional parameter]

                if (Global.Instance.ActivityPageId > 0)
                    req.SetAdditionalParameter(WTDefaultClient<ActivitiesGetResponse>.PAGE, Global.Instance.ActivityPageId);

                req.Expire = true;

                #endregion

                #region [Subscribe event handler]

                #region [Execute completed]
                client.ExecuteCompleted += (o, arg) =>
                {
                    Boolean anyActivityExisting = false;
                    int count = arg.Result.Activities.Count();
                    ActivityExt[] responseActivities = new ActivityExt[count];

                    for (int i = 0; i < count; ++i)
                    {
                        using (var db = WTShareDataContext.ShareDB)
                        {
                            var item = arg.Result.Activities[i];
                            var itemInDB = db.Activities.Where((a) => a.Id == item.Id).FirstOrDefault();

                            //...There is no such item
                            if (itemInDB == null)
                            {
                                itemInDB = new ActivityExt();
                                itemInDB.SetObject(item);

                                db.Activities.InsertOnSubmit(itemInDB);
                            }
                            else
                            {
                                itemInDB.SetObject(item);
                                anyActivityExisting = true;
                            }

                            responseActivities[i] = itemInDB;
                            db.SubmitChanges();
                        }

                        Global.Instance.ActivityPageId = arg.Result.NextPager;
                    }


                    if (arg.Result.NextPager > 0 && anyActivityExisting)
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            UpdateAcitivityList(responseActivities, true, true);
                        });

                        updateExpireAction();
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            UpdateAcitivityList(responseActivities, false, arg.Result.NextPager <= 0 ? false : true);

                            ProgressBarPopup.Instance.Close();
                        });
                    }
                };
                #endregion

                #region [Execute Failed]
                client.ExecuteFailed += (o, arg) =>
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        ProgressBarPopup.Instance.Close();
                    });
                };
                #endregion

                #endregion

                if (!String.IsNullOrEmpty(Global.Instance.CurrentUserID))
                    client.Execute(req, Global.Instance.Session, Global.Instance.Settings.UID);
                else
                    client.Execute(req);
            };
            #endregion

            updateExpireAction();
        }

        private void LoadAnotherExpiredActivities()
        {
            if (Global.Instance.ActivityPageId == 0)
                return;

            ActivitiesGetRequest<ActivitiesGetResponse> req = new ActivitiesGetRequest<ActivitiesGetResponse>();
            WTDefaultClient<ActivitiesGetResponse> client = new WTDefaultClient<ActivitiesGetResponse>();

            Debug.WriteLine("[updateExpireAction Thread]" + Thread.CurrentThread.GetHashCode());

            #region [Add additional parameter]

            if (Global.Instance.ActivityPageId > 0)
                req.SetAdditionalParameter(WTDefaultClient<ActivitiesGetResponse>.PAGE, Global.Instance.ActivityPageId);

            req.IsAsc = false;
            req.Expire = true;

            #endregion

            #region [Subscribe event handler]

            #region [Execute completed]
            client.ExecuteCompleted += (o, arg) =>
            {
                int count = arg.Result.Activities.Count();
                ActivityExt[] responseActivities = new ActivityExt[count];

                for (int i = 0; i < count; ++i)
                {
                    using (var db = WTShareDataContext.ShareDB)
                    {
                        var item = arg.Result.Activities[i];
                        var itemInDB = db.Activities.Where((a) => a.Id == item.Id).FirstOrDefault();

                        //...There is no such item
                        if (itemInDB == null)
                        {
                            itemInDB = new ActivityExt();
                            itemInDB.SetObject(item);

                            db.Activities.InsertOnSubmit(itemInDB);
                        }
                        else
                        {
                            itemInDB.SetObject(item);
                        }

                        responseActivities[i] = itemInDB;
                        db.SubmitChanges();
                    }

                    Global.Instance.ActivityPageId = arg.Result.NextPager;
                }

                this.Dispatcher.BeginInvoke(() =>
                {
                    UpdateAcitivityList(responseActivities, false, arg.Result.NextPager <= 0 ? false : true);
                    ProgressBarPopup.Instance.Close();
                });
            };
            #endregion

            #region [Execute Failed]
            client.ExecuteFailed += (o, arg) =>
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    ProgressBarPopup.Instance.Close();
                });
            };
            #endregion

            #endregion

            this.Dispatcher.BeginInvoke(() =>
            {
                ProgressBarPopup.Instance.Open();
            });

            if (!String.IsNullOrEmpty(Global.Instance.CurrentUserID))
                client.Execute(req, Global.Instance.Session, Global.Instance.Settings.UID);
            else
                client.Execute(req);
        }

        #endregion

        #region [People of Week]

        /// <summary>
        /// Try to get the latest Person from the server.
        /// </summary>
        private void RefreshPeopleOfWeek()
        {
            PeopleGetRequest<PeopleGetResponse> req = new PeopleGetRequest<PeopleGetResponse>();
            WTDefaultClient<PeopleGetResponse> client = new WTDefaultClient<PeopleGetResponse>();

            this.Dispatcher.BeginInvoke(() =>
            {
                ProgressBarPopup.Instance.Open();
            });

            #region [Add handlers]

            client.ExecuteCompleted += (o, arg) =>
            {
                int count = arg.Result.People.Count();

                PersonExt[] people = new PersonExt[count];

                #region [Update database]

                for (int i = 0; i < count; ++i)
                {
                    var item = arg.Result.People.ElementAt(i);
                    using (var db = WTShareDataContext.ShareDB)
                    {
                        var itemInDB = db.People.Where((a) => a.Id == item.Id).FirstOrDefault();

                        //...New data
                        if (itemInDB == null)
                        {
                            itemInDB = new PersonExt();
                            itemInDB.SetObject(item);

                            db.People.InsertOnSubmit(itemInDB);
                        }
                        //...Already in DB
                        else
                        {
                            itemInDB.SetObject(item);
                        }

                        people[i] = itemInDB;

                        db.SubmitChanges();
                    }
                }

                #endregion

                Global.Instance.PersonPageId = arg.Result.NextPager;

                var p = people.OrderBy((person) => person.NO).Last();

                this.Dispatcher.BeginInvoke(() =>
                {
                    if (PersonSource == null || p.Id > PersonSource.Id)
                    {
                        PersonSource = p;
                    }

                    ProgressBarPopup.Instance.Close();
                });
            };

            client.ExecuteFailed += (o, arg) =>
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        ProgressBarPopup.Instance.Close();
                    });
                };

            #endregion

            client.Execute(req);
        }

        #endregion

        #region [Campus Info]

        private void RefreshCampusInfo()
        {
            WTDispatcher.Instance.Do(() =>
            {
                #region [School News]
                {
                    var req = new SchoolNewsGetListRequest<SchoolNewsGetListResponse>();
                    var client = new WTDefaultClient<SchoolNewsGetListResponse>();

                    #region [Subscribe event handlers]

                    client.ExecuteCompleted += (o, e) =>
                        {
                            using (var db = WTShareDataContext.ShareDB)
                            {
                                foreach (var item in e.Result.SchoolNews)
                                {
                                    SchoolNewsExt itemInDB = null;

                                    if (db.SchoolNewsTable != null && db.SchoolNewsTable.Count() > 0)
                                        db.SchoolNewsTable.Where((news) => news.Id == item.Id).SingleOrDefault();

                                    //...the item is not saved
                                    if (itemInDB == null)
                                    {
                                        var tmp = new SchoolNewsExt();
                                        tmp.SetObject(item);
                                        db.SchoolNewsTable.InsertOnSubmit(tmp);
                                    }
                                    //...update info if the item is kept in database
                                    else
                                    {
                                        itemInDB.SetObject(item);
                                    }
                                }

                                db.SubmitChanges();
                            }

                            this.Dispatcher.BeginInvoke(() =>
                            {
                                --RefreshNewsCounter;
                            });
                        };

                    client.ExecuteFailed += (o, e) =>
                        {
                            Debug.WriteLine("Fail to refresh school news.\nError:{0}", e.Error);

                            this.Dispatcher.BeginInvoke(() =>
                            {
                                --RefreshNewsCounter;
                            });
                        };

                    #endregion

                    this.Dispatcher.BeginInvoke(() =>
                    {
                        ++RefreshNewsCounter;
                    });
                    client.Execute(req);
                }

                #endregion

                #region [Around News]

                {
                    var req = new AroundsGetRequest<AroundsGetResponse>();
                    var client = new WTDefaultClient<AroundsGetResponse>();

                    #region [Subscribe event handlers]

                    client.ExecuteCompleted += (o, e) =>
                    {
                        using (var db = WTShareDataContext.ShareDB)
                        {
                            foreach (var item in e.Result.Arounds)
                            {
                                AroundExt itemInDB = null;

                                var table = db.GetTable<AroundExt>();

                                if (table != null && table.Count() > 0)
                                    table.Where((news) => news.Id == item.Id).SingleOrDefault();

                                //...the item is not saved
                                if (itemInDB == null)
                                {
                                    var tmp = new AroundExt();
                                    tmp.SetObject(item);
                                    table.InsertOnSubmit(tmp);
                                }
                                //...update info if the item is kept in database
                                else
                                {
                                    itemInDB.SetObject(item);
                                }
                            }

                            db.SubmitChanges();
                        }

                        this.Dispatcher.BeginInvoke(() =>
                        {
                            --RefreshNewsCounter;
                        });
                    };

                    client.ExecuteFailed += (o, e) =>
                    {
                        Debug.WriteLine("Fail to refresh around news.\nError:{0}", e.Error);

                        this.Dispatcher.BeginInvoke(() =>
                        {
                            --RefreshNewsCounter;
                        });
                    };

                    #endregion

                    this.Dispatcher.BeginInvoke(() =>
                    {
                        ++RefreshNewsCounter;
                    });
                    client.Execute(req);
                }

                #endregion

                #region [Official Notes]

                {
                    var req = new ForStaffsGetRequest<ForStaffsGetResponse>();
                    var client = new WTDefaultClient<ForStaffsGetResponse>();

                    #region [Subscribe event handlers]

                    client.ExecuteCompleted += (o, e) =>
                    {
                        using (var db = WTShareDataContext.ShareDB)
                        {
                            foreach (var item in e.Result.ForStaffs)
                            {
                                ForStaffExt itemInDB = null;

                                var table = db.GetTable<ForStaffExt>();

                                if (table != null && table.Count() > 0)
                                    table.Where((news) => news.Id == item.Id).SingleOrDefault();

                                //...the item is not saved
                                if (itemInDB == null)
                                {
                                    var tmp = new ForStaffExt();
                                    tmp.SetObject(item);
                                    table.InsertOnSubmit(tmp);
                                }
                                //...update info if the item is kept in database
                                else
                                {
                                    itemInDB.SetObject(item);
                                }
                            }

                            db.SubmitChanges();
                        }

                        this.Dispatcher.BeginInvoke(() =>
                        {
                            --RefreshNewsCounter;
                        });
                    };

                    client.ExecuteFailed += (o, e) =>
                    {
                        Debug.WriteLine("Fail to refresh official note.\nError:{0}", e.Error);

                        this.Dispatcher.BeginInvoke(() =>
                        {
                            --RefreshNewsCounter;
                        });
                    };

                    #endregion

                    this.Dispatcher.BeginInvoke(() =>
                    {
                        ++RefreshNewsCounter;
                    });
                    client.Execute(req);
                }

                #endregion

                #region [Club News]

                {
                    var req = new ClubNewsGetListRequest<ClubNewsGetListResponse>();
                    var client = new WTDefaultClient<ClubNewsGetListResponse>();

                    #region [Subscribe event handlers]

                    client.ExecuteCompleted += (o, e) =>
                    {
                        using (var db = WTShareDataContext.ShareDB)
                        {
                            foreach (var item in e.Result.ClubNews)
                            {
                                AroundExt itemInDB = null;

                                var table = db.GetTable<ClubNewsExt>();

                                if (table != null && table.Count() > 0)
                                    table.Where((news) => news.Id == item.Id).SingleOrDefault();

                                //...the item is not saved
                                if (itemInDB == null)
                                {
                                    var tmp = new ClubNewsExt();
                                    tmp.SetObject(item);
                                    table.InsertOnSubmit(tmp);
                                }
                                //...update info if the item is kept in database
                                else
                                {
                                    itemInDB.SetObject(item);
                                }
                            }

                            db.SubmitChanges();
                        }

                        this.Dispatcher.BeginInvoke(() =>
                        {
                            --RefreshNewsCounter;
                        });
                    };

                    client.ExecuteFailed += (o, e) =>
                    {
                        Debug.WriteLine("Fail to refresh club news.\nError:{0}", e.Error);

                        this.Dispatcher.BeginInvoke(() =>
                        {
                            --RefreshNewsCounter;
                        });
                    };

                    #endregion

                    this.Dispatcher.BeginInvoke(() =>
                    {
                        ++RefreshNewsCounter;
                    });
                    client.Execute(req);
                }

                #endregion
            });
        }

        #endregion

        #endregion

        #endregion

        #region [Visual]

        private void PasswordBox_Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdateLoginButton(null, null);
        }

        private void SignInControl_GotFocus(Object sender, RoutedEventArgs e)
        {
            ScrollViewer_SignOutRoot.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
        }

        private void SignInControl_LostFocus(Object sender, RoutedEventArgs e)
        {
            ScrollViewer_SignOutRoot.ScrollToVerticalOffset(0);
            ScrollViewer_SignOutRoot.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
        }

        private void UpdateLoginButton(object sender, TextChangedEventArgs e)
        {
            try
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
            catch { }
        }

        private void Panorama_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            var p = sender as Panorama;

            #region [App bar]
            if (p.SelectedIndex == 0 && Border_SignedOut.Visibility == Visibility.Visible)
            {
                (this.ApplicationBar as ApplicationBar).Buttons.Clear();

                ApplicationBarIconButton button;
                button = new ApplicationBarIconButton(new Uri("/icons/appbar.check.rest.png", UriKind.RelativeOrAbsolute)) { Text = "登录", IsEnabled = false };
                UpdateLoginButton(null, null);
                button.Click += LogOn_Click;
                (this.ApplicationBar as ApplicationBar).Buttons.Add(button);

                button = new ApplicationBarIconButton(new Uri("/icons/appbar.register.rest.png", UriKind.RelativeOrAbsolute)) { Text = "注册" };
                button.Click += NavigateToSignUp;
                (this.ApplicationBar as ApplicationBar).Buttons.Add(button);
            }
            else
            {
                if ((this.ApplicationBar as ApplicationBar).Buttons.Count != 1)
                {
                    (this.ApplicationBar as ApplicationBar).Buttons.Clear();

                    ApplicationBarIconButton button;
                    button = new ApplicationBarIconButton(new Uri("/icons/appbar.refresh.rest.png", UriKind.RelativeOrAbsolute)) { Text = "刷新" };
                    button.Click += RefreshButton_Click;
                    (this.ApplicationBar as ApplicationBar).Buttons.Add(button);
                }
            }

            #region [People of Week]

            if (e.AddedItems.Contains(Border_PeopleOfWeek))
            {
                this.ApplicationBar.Buttons.Clear();

                var src = PersonSource;

                if (src != null)
                {
                    ApplicationBarIconButton button;

                    button = new ApplicationBarIconButton(
                        new Uri(src.CanLike ? "/icons/appbar.like.rest.png" : "/icons/appbar.unlike.rest.png",
                            UriKind.RelativeOrAbsolute))
                        {
                            Text = "喜欢"
                        };
                    this.ApplicationBar.Buttons.Add(button);

                    button = new ApplicationBarIconButton(
                        new Uri(src.CanFavorite ? "/icons/appbar.favs.rest.png" : "/icons/appbar.unfavourite.rest.png",
                            UriKind.RelativeOrAbsolute))
                        {
                            Text = "收藏"
                        };
                    this.ApplicationBar.Buttons.Add(button);

                    button = new ApplicationBarIconButton(new Uri("/icons/appbar.person.list.rest.png", UriKind.RelativeOrAbsolute)) { Text = "查看历史" };
                    button.Click += NavToPeopleOfWeekList;
                    this.ApplicationBar.Buttons.Add(button);

                    button = new ApplicationBarIconButton(new Uri("/icons/appbar.refresh.rest.png", UriKind.RelativeOrAbsolute)) { Text = "刷新" };
                    button.Click += RefreshButton_Click;
                    this.ApplicationBar.Buttons.Add(button);
                }
            }

            #endregion

            if (e.AddedItems.Contains(Border_Activities))
            {
                Queue<Object> q = new Queue<Object>();

                foreach (var item in this.ApplicationBar.MenuItems)
                {
                    q.Enqueue(item);
                }

                this.ApplicationBar.MenuItems.Clear();

                ApplicationBarMenuItem mi;

                mi = new ApplicationBarMenuItem() { Text = "最近活动" };
                mi.Click += SortActivitiesCompareToNow;
                this.ApplicationBar.MenuItems.Insert(0, mi);

                mi = new ApplicationBarMenuItem() { Text = "最火活动" };
                mi.Click += SortActivitiesByScheduleNumber;
                this.ApplicationBar.MenuItems.Insert(0, mi);


                mi = new ApplicationBarMenuItem() { Text = "最新活动" };
                mi.Click += SortActivitiesByCreationTime;
                this.ApplicationBar.MenuItems.Insert(0, mi);

                while (q.Count > 0)
                {
                    this.ApplicationBar.MenuItems.Add(q.Dequeue());
                }
            }
            else if (e.RemovedItems.Contains(Border_Activities))
            {
                for (int i = 0; i < 3; ++i)
                {
                    this.ApplicationBar.MenuItems.RemoveAt(0);
                }
            }
            #endregion

            #region [Refresh for the first time or Call AutoRefresh]

            #region [Activities]

            if (p.SelectedIndex == 1)
            {
                if ((ActivityThread.ThreadState & ThreadState.Unstarted) == ThreadState.Unstarted)
                {
                    ActivityThread.Start();

                    var thread = new Thread(new ThreadStart(RefreshExistingExpiredActivities))
                    {
                        Name = "RefreshExistingExpiredActivities"
                    };
                    thread.Start();
                }
                else if (ActivityThread.ThreadState == ThreadState.Stopped && Global.Instance.Settings.AutoRefresh)
                {
                    ActivityThread = new Thread(new ThreadStart(LoadAnotherExpiredActivities))
                    {
                        IsBackground = true,
                        Name = "LoadAnotherExpiredActivities"
                    };

                    ActivityThread.Start();
                }
            }

            #endregion
            #region [People of Week]

            else if (p.SelectedIndex == 3)
            {
                //...Obliged to get the latest person for the first time since the App is launched
                if (Global.Instance.PersonPageId == -1)
                {
                    if ((PersonThread.ThreadState & ThreadState.Unstarted) == ThreadState.Unstarted)
                    {
                        PersonThread.Start();
                    }
                    else if (PersonThread.ThreadState == ThreadState.Stopped)
                    {
                        PersonThread = new Thread(new ThreadStart(RefreshPeopleOfWeek))
                        {
                            IsBackground = true,
                            Name = "RefreshPeopleOfWeek_AutoRefresh"
                        };

                        PersonThread.Start();
                    }
                }
                //...Auto refresh the latest person according to user's settings
                else if (Global.Instance.Settings.AutoRefresh)
                {
                    if (PersonThread.ThreadState != ThreadState.Running)
                    {
                        PersonThread = new Thread(new ThreadStart(RefreshPeopleOfWeek))
                        {
                            IsBackground = true,
                            Name = "RefreshPeopleOfWeek_AutoRefresh"
                        };

                        PersonThread.Start();
                    }
                }
            }

            #endregion

            #endregion
        }

        private void SendFeedback(Object sender, EventArgs e)
        {
            var task = new Microsoft.Phone.Tasks.EmailComposeTask();
            var version = AppVersion.Current;

            task.Body = String.Format("我正在{0} {1}上使用微同济Windows Phone v{2}，", Microsoft.Phone.Info.DeviceStatus.DeviceManufacturer, Microsoft.Phone.Info.DeviceStatus.DeviceName, version);
            task.Subject = String.Format("[用户反馈]微同济Windows Phone v{0}", version);
            task.To = "we@tongji.edu.cn";
            task.Show();
        }

        private void ShareToFriends(Object sender, EventArgs e)
        {
            var task = new Microsoft.Phone.Tasks.EmailComposeTask();
            var version = AppVersion.Current;

            task.Body = String.Format("我正在使用微同济-同济大学专属校园移动应用，帮助管理我的大学日程，推送校内校外的大小活动，不再错过任何一个精彩的活动，快点和我一起去下载(we.tongji.edu.cn)");
            task.Subject = String.Format("推荐使用WeTongji(Windows Phone版)", version);
            task.Show();
        }

        private void RefreshButton_Click(Object sender, EventArgs e)
        {

        }

        private void SignOut(Object sender, EventArgs e)
        {
            var result = MessageBox.Show("确定想要注销账号？", "账号注销", MessageBoxButton.OKCancel);
            if (MessageBoxResult.OK == result)
            {
                var req = new UserLogOffRequest<WTResponse>();
                var client = new WTDefaultClient<WTResponse>();

                Action a = () =>
                {
                    Global.Instance.CurrentUserID = String.Empty;
                    Global.Instance.CleanSettings();
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        #region [App bar]

                        this.ApplicationBar.MenuItems.RemoveAt(this.ApplicationBar.MenuItems.Count - 1);

                        this.ApplicationBar.Buttons.Clear();
                        ApplicationBarIconButton button;
                        button = new ApplicationBarIconButton(new Uri("/icons/appbar.check.rest.png", UriKind.RelativeOrAbsolute)) { Text = "登录", IsEnabled = false };
                        UpdateLoginButton(null, null);
                        button.Click += LogOn_Click;
                        (this.ApplicationBar as ApplicationBar).Buttons.Add(button);

                        button = new ApplicationBarIconButton(new Uri("/icons/appbar.register.rest.png", UriKind.RelativeOrAbsolute)) { Text = "注册" };
                        button.Click += NavigateToSignUp;
                        (this.ApplicationBar as ApplicationBar).Buttons.Add(button);

                        #endregion

                        #region [Switch to Border_SignOut]

                        Border_SignedIn.DataContext = Button_Alarm.DataContext = null;
                        Border_SignedOut.Visibility = Visibility.Visible;

                        #endregion

                        //...Clear global data
                    });
                };

                client.ExecuteCompleted += (obj, args) => { a.Invoke(); };
                client.ExecuteFailed += (obj, args) => { a.Invoke(); };

                client.Execute(req, Global.Instance.Session, Global.Instance.Settings.UID);
            }
        }

        #region [Activity]

        private void ReloadActivities()
        {
            Thread thread = null;

            switch (this.activitySortMethod)
            {
                case ActivitySortMethod.kByScheduleNumber:
                    thread = new Thread(new ThreadStart(SortActivitiesByScheduleNumberCore))
                    {
                        IsBackground = true,
                        Name = "SortActivitiesByScheduleNumberCore"
                    };
                    break;
                case ActivitySortMethod.kByCreationTime:
                    thread = new Thread(new ThreadStart(SortActivitiesByCreationTimeCore))
                    {
                        IsBackground = true,
                        Name = "SortActivitiesByCreationTimeCore"
                    };
                    break;
                case ActivitySortMethod.kCompareToNow:
                    thread = new Thread(new ThreadStart(SortActivitiesCompareToNowCore))
                    {
                        IsBackground = true,
                        Name = "SortActivitiesCompareToNowCore"
                    };
                    break;
            }

            thread.Start();
        }

        private void ResetListBoxActivityVerticalOffset()
        {
            var obj = ListBox_Activity as DependencyObject;
            try
            {
                while (obj != null)
                {
                    if (obj is ScrollViewer)
                    {
                        (obj as ScrollViewer).ScrollToVerticalOffset(0);
                        break;
                    }
                    else
                    {
                        obj = VisualTreeHelper.GetChild(obj, 0);
                    }
                }
            }
            catch { }
        }

        private void Button_LoadMoreActivities_Click(Object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            btn.IsHitTestVisible = false;
            btn.Content = "正在加载更多活动...";

            var thread = new Thread(new ThreadStart(LoadAnotherExpiredActivities))
            {
                IsBackground = true,
                Name = "Button_LoadMoreActivities_Click"
            };

            thread.Start();
        }

        /// <summary>
        /// Insert activities to or update activities in ListBox_Activity's ItemsSource
        /// </summary>
        /// <param name="activities">a range of activities to be insert or update in UI</param>
        /// <param name="willContinue">refresh will continue</param>
        /// <param name="hasMore">"Load more" should be insert to the end of the List</param>
        private void UpdateAcitivityList(IEnumerable<ActivityExt> activities, Boolean willContinue, Boolean hasMore)
        {
            var src = ActivityListSource;
            if (src == null || src.Count == 0)
            {
                ActivityListSource = new ObservableCollection<ActivityExt>(activities);

                if (!willContinue && hasMore)
                {
                    ActivityListSource.Add(ActivityExt.InvalidActivityExt);
                    this.hasLoadMoreActivities = true;
                }
                else
                {
                    this.hasLoadMoreActivities = false;
                }

                ListBox_Activity.Visibility = Visibility.Visible;
            }
            else
            {
                //...Remove "add more"
                if (!src.Last().IsValid)
                    src.RemoveAt(src.Count - 1);

                #region [Update or Insert activities]
                foreach (var a in activities)
                {
                    ActivityExt target = src.Where((item) => item.Id == a.Id).SingleOrDefault();
                    if (target != null)
                    {
                        if (target.Schedule != a.Schedule)
                        {
                            target.Schedule = a.Schedule;
                            target.SendPropertyChanged("Schedule");
                        }
                        if (target.Title != a.Title)
                        {
                            target.Title = a.Title;
                            target.SendPropertyChanged("Title");
                        }
                        if (target.Location != a.Location)
                        {
                            target.Location = a.Location;
                            target.SendPropertyChanged("Location");
                        }
                        if (target.Begin != a.Begin || target.End != a.End)
                        {
                            target.Begin = a.Begin;
                            target.End = a.End;
                            target.SendPropertyChanged("DisplayTime");
                        }
                    }
                    else
                    {
                        switch (this.activitySortMethod)
                        {
                            case ActivitySortMethod.kByCreationTime:
                                {
                                    var count = src.Where((item) => item.CreatedAt > a.CreatedAt).Count();
                                    src.Insert(count, a);
                                }
                                break;
                            case ActivitySortMethod.kByScheduleNumber:
                                {
                                    if (a.Begin > DateTime.Now)
                                    {
                                        var count = src.Where((item) => item.Schedule > a.Schedule).Count();
                                        src.Insert(count, a);
                                    }
                                }
                                break;
                            case ActivitySortMethod.kCompareToNow:
                                {
                                    if (a.Begin > DateTime.Now)
                                    {
                                        var count = src.Where((item) => item.Begin < a.Begin).Count();
                                        src.Insert(count, a);
                                    }
                                }
                                break;
                        }
                    }
                }
                #endregion

                if (!willContinue && hasMore)
                {
                    src.Add(ActivityExt.InvalidActivityExt);
                    this.hasLoadMoreActivities = true;
                }
                else
                {
                    this.hasLoadMoreActivities = false;
                }
            }

        }

        /// <summary>
        /// 最新, do not filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SortActivitiesByCreationTime(Object sender, EventArgs e)
        {
            if (this.activitySortMethod != ActivitySortMethod.kByCreationTime)
            {
                var thread = new Thread(new ThreadStart(SortActivitiesByCreationTimeCore))
                {
                    IsBackground = true,
                    Name = "SortActivitiesByCreationTimeCore"
                };

                thread.Start();
            }
            else
            {
                ResetListBoxActivityVerticalOffset();
            }
        }

        private void SortActivitiesByCreationTimeCore()
        {
            ActivityExt[] activities = null;
            ObservableCollection<ActivityExt> result = null;

            this.Dispatcher.BeginInvoke(() =>
            {
                ProgressBarPopup.Instance.Open();
            });

            using (var db = WTShareDataContext.ShareDB)
            {
                activities = db.Activities.ToArray();
            }

            if (null != activities)
            {
                var q = from ActivityExt a in activities
                        orderby a.CreatedAt descending
                        select a;
                result = new ObservableCollection<ActivityExt>(q);

                this.Dispatcher.BeginInvoke(() =>
                {
                    this.activitySortMethod = ActivitySortMethod.kByCreationTime;
                    if (this.hasLoadMoreActivities)
                        result.Add(ActivityExt.InvalidActivityExt);
                    
                    ActivityListSource = result;
                    ResetListBoxActivityVerticalOffset();
                    ProgressBarPopup.Instance.Close();
                });
            }
        }

        /// <summary>
        /// 最火,filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SortActivitiesByScheduleNumber(Object sender, EventArgs e)
        {
            if (this.activitySortMethod != ActivitySortMethod.kByScheduleNumber)
            {
                var thread = new Thread(new ThreadStart(SortActivitiesByScheduleNumberCore))
                {
                    IsBackground = true,
                    Name = "SortActivitiesByLikeNumberCore"
                };

                thread.Start();
            }
            else
            {
                ResetListBoxActivityVerticalOffset();
            }
        }

        private void SortActivitiesByScheduleNumberCore()
        {
            ActivityExt[] activities = null;
            ObservableCollection<ActivityExt> result = null;

            this.Dispatcher.BeginInvoke(() =>
            {
                ProgressBarPopup.Instance.Open();
            });

            using (var db = WTShareDataContext.ShareDB)
            {
                activities = db.Activities.ToArray();
            }

            if (null != activities)
            {
                var q = activities.Where((a) => a.Begin > DateTime.Now);

                if (q.Count() == 0)
                {
                    q = activities.OrderByDescending((a) => a.Schedule).ThenByDescending((a) => a.CreatedAt);
                }
                else
                {
                    q = q.OrderByDescending((a) => a.Schedule).ThenByDescending((a) => a.CreatedAt);
                }

                result = new ObservableCollection<ActivityExt>(q);

                this.Dispatcher.BeginInvoke(() =>
                {
                    this.activitySortMethod = ActivitySortMethod.kByScheduleNumber;
                    ActivityListSource = result;
                    ResetListBoxActivityVerticalOffset();
                    ProgressBarPopup.Instance.Close();
                });
            }
        }

        /// <summary>
        /// 最近,filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SortActivitiesCompareToNow(Object sender, EventArgs e)
        {
            if (this.activitySortMethod != ActivitySortMethod.kCompareToNow)
            {
                var thread = new Thread(new ThreadStart(SortActivitiesCompareToNowCore))
                {
                    IsBackground = true,
                    Name = "SortActivitiesCompareToNowCore"
                };

                thread.Start();
            }
            else
            {
                ResetListBoxActivityVerticalOffset();
            }
        }

        private void SortActivitiesCompareToNowCore()
        {
            ActivityExt[] activities = null;
            ObservableCollection<ActivityExt> result = null;

            this.Dispatcher.BeginInvoke(() =>
            {
                ProgressBarPopup.Instance.Open();
            });

            using (var db = WTShareDataContext.ShareDB)
            {
                activities = db.Activities.ToArray();
            }

            if (null != activities)
            {
                var q = activities.Where((a) => a.Begin > DateTime.Now);

                if (q.Count() == 0)
                {
                    q = activities.OrderByDescending((a) => a.Begin);
                    if (q.Count() > 5)
                    {
                        q = q.Take(5);
                    }
                }
                else
                {
                    q = q.OrderBy((a) => a.Begin);
                }

                result = new ObservableCollection<ActivityExt>(q);

                this.Dispatcher.BeginInvoke(() =>
                {
                    this.activitySortMethod = ActivitySortMethod.kCompareToNow;
                    ActivityListSource = result;
                    ResetListBoxActivityVerticalOffset();
                    ProgressBarPopup.Instance.Close();
                });
            }
        }

        #endregion

        #endregion

        #region [Data Operatings]

        private void LoadDataFromDatabase()
        {
            ActivityExt[] activityArr = null;
            ObservableCollection<ActivityExt> activitySrc = null;
            PersonExt personSrc = null;
            SchoolNewsExt snSrc = null;
            AroundExt anSrc = null;
            ForStaffExt fsSrc = null;
            ClubNewsExt cnSrc = null;

            Debug.WriteLine("Load data from db started.");

            //...Activity
            using (var db = WTShareDataContext.ShareDB)
            {
                if (db.Activities.Count() > 0)
                {
                    activityArr = db.Activities.ToArray();
                }
            }

            if (activityArr != null)
            {
                activitySrc = new ObservableCollection<ActivityExt>(activityArr.OrderByDescending((a) => a.CreatedAt));
            }


            //...PeopleOfWeek
            PersonExt[] personArr = null;
            using (var db = WTShareDataContext.ShareDB)
            {
                personArr = db.People.ToArray();
            }

            if (null != personArr)
                personSrc = personArr.LastOrDefault();

            //Campus Info
            {
                {
                    SchoolNewsExt[] sn = null;
                    using (var db = WTShareDataContext.ShareDB)
                    {
                        sn = db.SchoolNewsTable.ToArray();
                    }

                    if (sn != null)
                        //Button_TongjiNews.DataContext = sn.Where((news) => !String.IsNullOrEmpty(news.ImageExtList)).LastOrDefault();
                        snSrc = sn.Where((news) => !String.IsNullOrEmpty(news.ImageExtList)).LastOrDefault();
                }

                {
                    AroundExt[] an = null;
                    using (var db = WTShareDataContext.ShareDB)
                    {
                        an = db.AroundTable.ToArray();
                    }

                    if (an != null)
                        //Button_AroundNews.DataContext = an.Where((news) => !String.IsNullOrEmpty(news.ImageExtList)).LastOrDefault();
                        anSrc = an.Where((news) => !String.IsNullOrEmpty(news.ImageExtList)).LastOrDefault();
                }

                {
                    ForStaffExt[] fs = null;
                    using (var db = WTShareDataContext.ShareDB)
                    {
                        fs = db.ForStaffTable.ToArray();
                    }

                    if (fs != null)
                        //Button_OfficialNotes.DataContext = fs.Where((news) => !String.IsNullOrEmpty(news.ImageExtList)).LastOrDefault();
                        fsSrc = fs.Where((news) => !String.IsNullOrEmpty(news.ImageExtList)).LastOrDefault();
                }

                {
                    ClubNewsExt[] cn = null;
                    using (var db = WTShareDataContext.ShareDB)
                    {
                        cn = db.ClubNewsTable.ToArray();
                    }

                    if (cn != null)
                        //Button_ClubNews.DataContext = cn.Where((news) => !String.IsNullOrEmpty(news.ImageExtList)).LastOrDefault();
                        cnSrc = cn.Where((news) => !String.IsNullOrEmpty(news.ImageExtList)).LastOrDefault();
                }

            }

            this.Dispatcher.BeginInvoke(() =>
            {
                if (activitySrc != null && activitySrc.Count() > 0)
                {
                    ActivityListSource = activitySrc;
                    ListBox_Activity.Visibility = Visibility.Visible;
                }

                if (personSrc != null)
                {
                    ScrollViewer_PeopleOfWeek.Visibility = Visibility.Visible;
                    PersonSource = personSrc;
                }

                TongjiNewsSource = snSrc;
                AroundNewsSource = anSrc;
                OfficialNoteSource = fsSrc;
                ClubNewsSource = cnSrc;
            });
        }

        /// <summary>
        /// Update corresponding semester properties and delete existing courses of the response semester
        /// if the response semester exists in the database, then inserting response courses.
        /// Otherwise, create a new semester and the semester's courses from the response and store them
        /// in database.
        /// </summary>
        /// <remarks>
        /// This function is called when timetable has been downloaded completely.
        /// </remarks>
        /// <param name="param">TimetableGetResponse</param>
        private void OnDownloadCoursesCompleted(Object param)
        {
            var result = param as TimeTableGetResponse;

            if (result == null)
                throw new NotSupportedException("TimeTableGetResponse is expected");

            // target semester
            Semester semester = null;

            #region [Create or update semester in database]

            using (var db = new WTUserDataContext(Global.Instance.Settings.UID))
            {
                semester = db.Semesters.Where((sem) => sem.SchoolYearStartAt == result.SchoolYearStartAt
                    && sem.SchoolYearWeekCount == result.SchoolYearWeekCount).SingleOrDefault();
            }

            if (semester == null)
            {
                //...Create and store a new semester
                semester = new Semester()
                {
                    Id = Guid.NewGuid().ToString(),
                    SchoolYearStartAt = result.SchoolYearStartAt,
                    SchoolYearWeekCount = result.SchoolYearWeekCount,
                    SchoolYearCourseWeekCount = result.SchoolYearCourseWeekCount
                };

                using (var db = new WTUserDataContext(Global.Instance.Settings.UID))
                {
                    db.Semesters.InsertOnSubmit(semester);

                    db.SubmitChanges();
                }
            }
            else
            {
                CourseExt[] existingCoursesOfCurrentSemester = null;

                using (var db = new WTUserDataContext(Global.Instance.Settings.UID))
                {
                    existingCoursesOfCurrentSemester = db.Courses.Where((c) => c.SemesterGuid == semester.Id).ToArray();
                }

                if (existingCoursesOfCurrentSemester != null)
                {
                    foreach (var c in existingCoursesOfCurrentSemester)
                        using (var db = new WTUserDataContext(Global.Instance.Settings.UID))
                        {
                            try
                            {
                                db.Courses.Attach(c);
                            }
                            catch { }

                            db.Courses.DeleteOnSubmit(c);
                            db.SubmitChanges();
                        }
                }
            }

            #endregion

            #region [Inserting courses]

            int courseCount = result.Courses.Count();
            CourseExt[] courseExtArr = null;

            if (courseCount > 0)
            {
                courseExtArr = new CourseExt[courseCount];
                for (int i = 0; i < courseCount; ++i)
                {
                    courseExtArr[i] = new CourseExt()
                    {
                        Id = Guid.NewGuid().ToString(),
                        UID = Global.Instance.Settings.UID,
                        SemesterGuid = semester.Id
                    };
                    courseExtArr[i].SetObject(result.Courses[i]);
                }

                using (var db = new WTUserDataContext(Global.Instance.Settings.UID))
                {
                    db.Courses.InsertAllOnSubmit(courseExtArr);
                    db.SubmitChanges();
                }
            }
            #endregion

            DownloadFavorite();
        }

        private void OnDownloadFavoriteCompleted(Object param)
        {
            if (param == null)
                throw new ArgumentNullException();

            var args = param as WTExecuteCompletedEventArgs<FavoriteGetResponse>;
            if (args == null)
                throw new NotSupportedException();

            var sb = new StringBuilder();

            #region [Activity]

            {
                sb.Clear();

                foreach (var a in args.Result.Activities)
                {
                    ActivityExt itemInShareDB = null;

                    using (var db = WTShareDataContext.ShareDB)
                    {
                        itemInShareDB = db.Activities.Where((ac) => ac.Id == a.Id).SingleOrDefault();
                    }

                    #region [Handle Share DB]

                    //...Not stored in Share DB
                    if (itemInShareDB == null)
                    {
                        itemInShareDB = new ActivityExt();
                        itemInShareDB.SetObject(a);

                        using (var db = WTShareDataContext.ShareDB)
                        {
                            db.Activities.InsertOnSubmit(itemInShareDB);

                            db.SubmitChanges();
                        }
                    }
                    //...Stored in share DB
                    else
                    {
                        itemInShareDB.SetObject(a);
                        using (var db = WTShareDataContext.ShareDB)
                        {
                            db.Activities.Attach(itemInShareDB);

                            db.SubmitChanges();
                        }
                    }
                    #endregion

                    sb.AppendFormat("{0}_", a.Id);
                }

                using (var db = new WTUserDataContext(Global.Instance.Settings.UID))
                {
                    var favObj = db.Favorites.Where((fo) => fo.Id == (uint)FavoriteIndex.kActivity).Single();
                    favObj.Value = sb.ToString().TrimEnd('_');

                    db.SubmitChanges();
                }
            }

            #endregion

            #region [PeopleOfWeek]

            {
                sb.Clear();

                foreach (var p in args.Result.People)
                {
                    PersonExt item = null;

                    #region [Handle Share db]

                    using (var db = WTShareDataContext.ShareDB)
                    {
                        item = db.People.Where((people) => people.Id == p.Id).SingleOrDefault();
                    }

                    //...Not stored in Share DB
                    if (item == null)
                    {
                        item = new PersonExt();
                        item.SetObject(p);

                        using (var db = WTShareDataContext.ShareDB)
                        {
                            db.People.InsertOnSubmit(item);

                            db.SubmitChanges();
                        }
                    }
                    //...Stored in share DB
                    else
                    {
                        item.SetObject(p);
                        using (var db = WTShareDataContext.ShareDB)
                        {
                            db.People.Attach(item);

                            db.SubmitChanges();
                        }
                    }

                    #endregion

                    sb.AppendFormat("{0}_", p.Id);
                }

                using (var db = new WTUserDataContext(Global.Instance.Settings.UID))
                {
                    var favObj = db.Favorites.Where((fo) => fo.Id == (uint)FavoriteIndex.kPeopleOfWeek).Single();
                    favObj.Value = sb.ToString().TrimEnd('_');

                    db.SubmitChanges();
                }
            }

            #endregion

            #region [Campus Info]

            #region [School News]

            {
                sb.Clear();

                foreach (var news in args.Result.SchoolNews)
                {
                    SchoolNewsExt item = null;

                    #region [Handle Share db]

                    using (var db = WTShareDataContext.ShareDB)
                    {
                        item = db.SchoolNewsTable.Where((n) => n.Id == news.Id).SingleOrDefault();
                    }

                    //...Not stored in Share DB
                    if (item == null)
                    {
                        item = new SchoolNewsExt();
                        item.SetObject(news);

                        using (var db = WTShareDataContext.ShareDB)
                        {
                            db.SchoolNewsTable.InsertOnSubmit(item);

                            db.SubmitChanges();
                        }
                    }
                    //...Stored in share DB
                    else
                    {
                        item.SetObject(news);
                        using (var db = WTShareDataContext.ShareDB)
                        {
                            db.SchoolNewsTable.Attach(item);

                            db.SubmitChanges();
                        }
                    }

                    #endregion

                    sb.AppendFormat("{0}_", news.Id);
                }

                using (var db = new WTUserDataContext(Global.Instance.Settings.UID))
                {
                    var favObj = db.Favorites.Where((fo) => fo.Id == (uint)FavoriteIndex.kTongjiNews).Single();
                    favObj.Value = sb.ToString().TrimEnd('_');

                    db.SubmitChanges();
                }
            }

            #endregion

            #region [Around News]

            {
                sb.Clear();

                foreach (var news in args.Result.Arounds)
                {
                    AroundExt item = null;

                    #region [Handle Share db]

                    using (var db = WTShareDataContext.ShareDB)
                    {
                        item = db.AroundTable.Where((n) => n.Id == news.Id).SingleOrDefault();
                    }

                    //...Not stored in Share DB
                    if (item == null)
                    {
                        item = new AroundExt();
                        item.SetObject(news);

                        using (var db = WTShareDataContext.ShareDB)
                        {
                            db.AroundTable.InsertOnSubmit(item);

                            db.SubmitChanges();
                        }
                    }
                    //...Stored in share DB
                    else
                    {
                        item.SetObject(news);
                        using (var db = WTShareDataContext.ShareDB)
                        {
                            db.AroundTable.Attach(item);

                            db.SubmitChanges();
                        }
                    }

                    #endregion

                    sb.AppendFormat("{0}_", news.Id);
                }

                using (var db = new WTUserDataContext(Global.Instance.Settings.UID))
                {
                    var favObj = db.Favorites.Where((fo) => fo.Id == (uint)FavoriteIndex.kAroundNews).Single();
                    favObj.Value = sb.ToString().TrimEnd('_');

                    db.SubmitChanges();
                }
            }

            #endregion

            #region [Official Notes]

            {
                sb.Clear();

                foreach (var news in args.Result.ForStaffs)
                {
                    ForStaffExt item = null;

                    #region [Handle Share db]

                    using (var db = WTShareDataContext.ShareDB)
                    {
                        item = db.ForStaffTable.Where((n) => n.Id == news.Id).SingleOrDefault();
                    }

                    //...Not stored in Share DB
                    if (item == null)
                    {
                        item = new ForStaffExt();
                        item.SetObject(news);

                        using (var db = WTShareDataContext.ShareDB)
                        {
                            db.ForStaffTable.InsertOnSubmit(item);

                            db.SubmitChanges();
                        }
                    }
                    //...Stored in share DB
                    else
                    {
                        item.SetObject(news);
                        using (var db = WTShareDataContext.ShareDB)
                        {
                            db.ForStaffTable.Attach(item);

                            db.SubmitChanges();
                        }
                    }

                    #endregion

                    sb.AppendFormat("{0}_", news.Id);
                }

                using (var db = new WTUserDataContext(Global.Instance.Settings.UID))
                {
                    var favObj = db.Favorites.Where((fo) => fo.Id == (uint)FavoriteIndex.kOfficialNotes).Single();
                    favObj.Value = sb.ToString().TrimEnd('_');

                    db.SubmitChanges();
                }
            }

            #endregion

            #region [Club News]

            {
                sb.Clear();

                foreach (var news in args.Result.ClubNews)
                {
                    ClubNewsExt item = null;

                    #region [Handle Share db]

                    using (var db = WTShareDataContext.ShareDB)
                    {
                        item = db.ClubNewsTable.Where((n) => n.Id == news.Id).SingleOrDefault();
                    }

                    //...Not stored in Share DB
                    if (item == null)
                    {
                        item = new ClubNewsExt();
                        item.SetObject(news);

                        using (var db = WTShareDataContext.ShareDB)
                        {
                            db.ClubNewsTable.InsertOnSubmit(item);

                            db.SubmitChanges();
                        }
                    }
                    //...Stored in share DB
                    else
                    {
                        item.SetObject(news);
                        using (var db = WTShareDataContext.ShareDB)
                        {
                            db.ClubNewsTable.Attach(item);

                            db.SubmitChanges();
                        }
                    }

                    #endregion

                    sb.AppendFormat("{0}_", news.Id);
                }

                using (var db = new WTUserDataContext(Global.Instance.Settings.UID))
                {
                    var favObj = db.Favorites.Where((fo) => fo.Id == (uint)FavoriteIndex.kClubNews).Single();
                    favObj.Value = sb.ToString().TrimEnd('_');

                    db.SubmitChanges();
                }
            }

            #endregion

            #endregion

            Debug.WriteLine("Download favorite completed.");

            //...Update UI
            this.Dispatcher.BeginInvoke(() =>
            {
                var dc = Button_Favorite.DataContext as UserExt;
                if (dc != null)
                {
                    dc.SendPropertyChanged("FavoritesCount");
                }
            });

            DownloadSchedule();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param">WTExecuteCompletedEventArgs[ScheduleGetResponse]</param>
        private void OnDownloadScheduleCompleted(Object param)
        {
            if (param == null)
            {
                throw new ArgumentNullException();
            }

            var arg = param as WTExecuteCompletedEventArgs<ScheduleGetResponse>;
            if (arg == null)
                throw new NotSupportedException("WTExecuteCompletedEventArgs<ScheduleGetResponse> is expected.");

            #region [Store or update activities]

            foreach (var a in arg.Result.Activities)
            {
                ActivityExt itemInDB = null;
                using (var db = WTShareDataContext.ShareDB)
                {
                    itemInDB = db.Activities.Where((activity) => activity.Id == a.Id).SingleOrDefault();
                }

                if (itemInDB == null)
                {
                    var activityEx = new ActivityExt();
                    activityEx.SetObject(a);

                    using (var db = WTShareDataContext.ShareDB)
                    {
                        db.Activities.InsertOnSubmit(activityEx);
                        db.SubmitChanges();
                    }
                }
                else
                {
                    itemInDB.SetObject(a);

                    using (var db = WTShareDataContext.ShareDB)
                    {
                        db.Activities.Attach(itemInDB);
                        db.SubmitChanges();
                    }
                }
            }

            #endregion

            #region [Save exams]

            {
                var firstExam = arg.Result.Exams.FirstOrDefault();
                if (firstExam != null)
                {
                    int count = arg.Result.Exams.Count();
                    ExamExt[] examExtArr = new ExamExt[count];
                    Semester targetSemester = null;
                    Semester[] allSemesters = null;

                    for (int i = 0; i < count; ++i)
                    {
                        examExtArr[i] = new ExamExt();
                        examExtArr[i].SetObject(arg.Result.Exams.ElementAt(i));
                    }

                    using (var db = new WTUserDataContext(Global.Instance.Settings.UID))
                    {
                        allSemesters = db.Semesters.ToArray();
                    }

                    if (allSemesters != null)
                        foreach (var s in allSemesters)
                        {
                            if (s.IsInSemester(examExtArr[0]))
                            {
                                for (int i = 0; i < count; ++i)
                                {
                                    examExtArr[i].SemesterGuid = s.Id;
                                }

                                ExamExt[] previousData = null;
                                using (var db = new WTUserDataContext(Global.Instance.Settings.UID))
                                {
                                    previousData = db.Exams.Where((e) => e.SemesterGuid == s.Id).ToArray();
                                    db.Exams.DeleteAllOnSubmit(previousData);

                                    db.SubmitChanges();
                                }

                                targetSemester = s;

                                break;
                            }
                        }

                    if (targetSemester != null)
                    {

                        foreach (var e in examExtArr)
                        {
                            using (var db = new WTUserDataContext(Global.Instance.Settings.UID))
                            {
                                db.Exams.InsertOnSubmit(e);
                                db.SubmitChanges();
                            }
                        }
                    }
                }
            }

            #endregion

            Debug.WriteLine("Download timetable completed.");
        }

        private void DownloadCourses()
        {
            var tt_req = new TimeTableGetRequest<TimeTableGetResponse>();
            var tt_client = new WTDefaultClient<TimeTableGetResponse>();

            Debug.WriteLine("In download courses.");

            #region [Add handlers]

            tt_client.ExecuteFailed += (s, args) =>
            {
                Debug.WriteLine("Fail to get user's course.\nError:{0}", args.Error);
            };

            tt_client.ExecuteCompleted += (s, args) =>
            {
                var thread = new Thread(new ParameterizedThreadStart(OnDownloadCoursesCompleted))
                {
                    Name = "OnDownloadTimeTableCompleted",
                    IsBackground = true
                };
                thread.Start(args.Result);
            };

            #endregion

            tt_client.Execute(tt_req, Global.Instance.Session, Global.Instance.Settings.UID);
        }

        private void DownloadFavorite()
        {
            Debug.WriteLine("In download favorite.");

            var fav_req = new FavoriteGetRequest<FavoriteGetResponse>();
            var fav_client = new WTDefaultClient<FavoriteGetResponse>();


            #region [Add handlers]

            fav_client.ExecuteFailed += (s, args) =>
            {
                Debug.WriteLine("Fail to get user's favorite.\nError:{0}", args.Error);
            };

            fav_client.ExecuteCompleted += (s, args) =>
            {
                var thread = new Thread(new ParameterizedThreadStart(OnDownloadFavoriteCompleted))
                {
                    IsBackground = true,
                    Name = "OnDownloadFavoriteCompleted"
                };

                thread.Start(args);
            };

            #endregion

            fav_client.Execute(fav_req, Global.Instance.Session, Global.Instance.Settings.UID);

            //...Todo @_@ NextPager
        }

        private void DownloadSchedule()
        {
            Debug.WriteLine("In download schedule");

            var schedule_req = new ScheduleGetRequest<ScheduleGetResponse>();
            var schedule_client = new WTDefaultClient<ScheduleGetResponse>();

            schedule_req.Begin = DateTime.Now.Date;
            schedule_req.End = DateTime.Now.Date + TimeSpan.FromDays(1);

            #region [Add handlers]

            schedule_client.ExecuteFailed += (s, args) =>
            {
                Debug.WriteLine("Fail to get user's schedule.\n Error{0}", args.Error);
            };

            schedule_client.ExecuteCompleted += (s, args) =>
            {
                Debug.WriteLine("Get user's schedule completed.");

                OnDownloadScheduleCompleted(args);

                ComputeCalendar();
            };

            #endregion

            schedule_client.Execute(schedule_req, Global.Instance.Session, Global.Instance.Settings.UID);
        }

        private void ComputeCalendar()
        {
            Debug.WriteLine("Compute calendar");

            List<CalendarNode> list = new List<CalendarNode>();

            #region [Compute courses and exams]

            Semester[] allSemesters = null;
            Semester currentSemester = null;
            using (var db = new WTUserDataContext(Global.Instance.Settings.UID))
            {
                allSemesters = db.Semesters.ToArray();
            }

            if (allSemesters != null)
            {
                foreach (var s in allSemesters)
                {
                    if (s.SchoolYearStartAt <= DateTime.Now && s.SchoolYearEndAt >= DateTime.Now)
                    {
                        currentSemester = s;
                        break;
                    }
                }

                if (currentSemester != null)
                {
                    CourseExt[] courses = null;
                    ExamExt[] exams = null;

                    using (var db = new WTUserDataContext(Global.Instance.Settings.UID))
                    {
                        courses = db.Courses.Where((c) => c.SemesterGuid == currentSemester.Id).ToArray();
                        exams = db.Exams.Where((e) => e.SemesterGuid == currentSemester.Id).ToArray();
                    }

                    foreach (var c in courses)
                    {
                        var node = c.GetCalendarNode(currentSemester, DateTime.Now);
                        if (node != null)
                            list.Add(node);
                    }

                    foreach (var e in exams)
                    {
                        if (e.Begin.Date == DateTime.Now.Date)
                            list.Add(e.GetCalendarNode());
                    }
                }
            }

            #endregion

            #region [Compute Favorite activities]

            FavoriteObject fo = null;
            using (var db = new WTUserDataContext(Global.Instance.Settings.UID))
            {
                fo = db.Favorites.Where((o) => o.Id == (int)FavoriteIndex.kActivity).SingleOrDefault();
            }

            if (fo != null && !String.IsNullOrEmpty(fo.Value))
            {
                var strIdxArr = fo.Value.Split("_".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                int count = strIdxArr.Count();
                int[] intIdxArr = new int[count];

                for (int i = 0; i < count; ++i)
                {
                    Int32.TryParse(strIdxArr[i], out intIdxArr[i]);
                }

                foreach (var idx in intIdxArr)
                {
                    using (var db = WTShareDataContext.ShareDB)
                    {
                        var activity = db.Activities.Where((a) => a.Id == idx).SingleOrDefault();
                        if (activity != null && activity.Begin.Date == DateTime.Now.Date)
                        {
                            list.Add(activity.GetCalendarNode());
                        }
                    }
                }
            }

            #endregion

            var q = from node in list
                    where node > DateTime.Now
                    orderby node.Self ascending
                    select node;

            var selectedCalendarNode = q.FirstOrDefault();

            this.Dispatcher.BeginInvoke(() =>
            {
                if (selectedCalendarNode != null)
                {
                    Button_Alarm.DataContext = selectedCalendarNode;
                    TextBlock_NoArrangement.Visibility = Visibility.Collapsed;
                }
                else
                {
                    TextBlock_NoArrangement.Visibility = Visibility.Visible;
                }
            });

        }

        #endregion

        #endregion

        #region [Enum]

        public enum ActivitySortMethod
        {
            kByCreationTime,
            kByScheduleNumber,
            kCompareToNow
        }

        #endregion
    }
}