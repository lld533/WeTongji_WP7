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


namespace WeTongji
{
    public partial class MainPage : PhoneApplicationPage
    {
        #region [Fields]
        private static Boolean isResourceLoaded = false;

        private int refreshCounter = 0;

        private int refreshNewsCounter = 0;

        #endregion

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            RefreshNewsCompleted += RefreshCampusInfoImages;
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
                    }
#else
                    if (db.DatabaseExists())
                    {
                        db.DeleteDatabase();
                    }
                    db.CreateDatabase();
#endif
                    RefreshAll();
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
            get { return this.ApplicationBar.Buttons[0] as ApplicationBarIconButton; }
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
                    OnRefreshNewsCompleted();
                    --RefreshCounter;
                }
            }
        }

        #endregion

        #region [Event Handlers]

        private event EventHandler RefreshNewsCompleted;

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
            var p = PanoramaItem_PeopleOfWeek.DataContext as PersonExt;

            if (p != null)
                this.NavigationService.Navigate(new Uri(String.Format("/Pages/PeopleOfWeek.xaml?q={0}", p.Id), UriKind.RelativeOrAbsolute));
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

        #endregion

        #region [Api]

        /// <summary>
        /// LogOn demo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogOn_Click(object sender, EventArgs e)
        {
            Button_Login.IsEnabled = false;
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
                        Border_SignedIn.Visibility = Visibility.Visible;
                        TextBox_Id.Text = String.Empty;
                        PasswordBox_Password.Password = String.Empty;

                        //...Todo @_@ Refresh app bar
                    });


                    WeTongji.Business.Global.Instance.UpdateSettings(arg.Result.User.UID, pw, arg.Result.Session);

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
                        Button_Login.IsEnabled = true;

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
                    });
                };

            client.Execute(req);
        }

        #region [Refresh related functions]

        private void RefreshAll()
        {
            refreshCounter = 0;

            //RefreshActivityList();
            //RefreshPeopleOfWeek();
            //RefreshCampusInfo();
        }

        #region [Activities]

        private void RefreshActivityList()
        {
            WTDispatcher.Instance.Do(() =>
            {
                ActivitiesGetRequest<ActivitiesGetResponse> req = new ActivitiesGetRequest<ActivitiesGetResponse>();
                WTDefaultClient<ActivitiesGetResponse> client = new WTDefaultClient<ActivitiesGetResponse>();

                #region [Subscribe event handler]

                #region [Execute completed]
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
                            }
                            else
                                itemInDB.SetObject(item);
                        }

                        //...Todo @_@ Update NextPager;

                        db.SubmitChanges();
                    }

                    this.Dispatcher.BeginInvoke(() =>
                    {
                        using (var db = WTShareDataContext.ShareDB)
                        {
                            //...Todo @_@ Take SORTing into consideration
                            var q = from ActivityExt a in db.Activities
                                    orderby a.Id descending
                                    select a;
                            ListBox_Activity.ItemsSource = new ObservableCollection<ActivityExt>(q);
                        }

                    });

                    --refreshCounter;
                };
                #endregion

                #region [Execute Failed]
                client.ExecuteFailed += (o, arg) =>
                {
                    //...Do Nothing if failed
                    Debug.WriteLine(arg.Error);

                    --refreshCounter;
                };
                #endregion

                #endregion

                ++refreshCounter;
                client.Execute(req);
            });
        }

        #endregion

        #region [People of Week]
        private void RefreshPeopleOfWeek()
        {
            WTDispatcher.Instance.Do(() =>
            {
                PeopleGetRequest<PeopleGetResponse> req = new PeopleGetRequest<PeopleGetResponse>();
                WTDefaultClient<PeopleGetResponse> client = new WTDefaultClient<PeopleGetResponse>();

                //...Update Activities
                client.ExecuteCompleted += (o, arg) =>
                {
                    PersonExt p = null;

                    using (var db = WTShareDataContext.ShareDB)
                    {
                        int count = arg.Result.People.Count();

                        for (int i = 0; i < count; ++i)
                        {
                            var item = arg.Result.People.ElementAt(i);
                            var itemInDB = db.People.Where((a) => a.Id == item.Id).FirstOrDefault();

                            //...There is no such item
                            if (itemInDB == null)
                            {
                                var tmp = new PersonExt();
                                tmp.SetObject(item);

                                db.People.InsertOnSubmit(tmp);
                            }
                            //...Already in DB
                            else
                            {
                                itemInDB.Favorite = item.Favorite;
                                itemInDB.Like = item.Like;
                                itemInDB.Read = item.Read;

                                //...Todo @_@ Update CanFavorite, CanLike, etc.
                                // if user signed in.
                            }
                        }

                        //...Todo @_@ Update NextPager;

                        db.SubmitChanges();
                    }

                    using (var db = WTShareDataContext.ShareDB)
                    {
                        //...Query the latest person of week
                        var q = from PersonExt person in db.People
                                orderby person.NO descending
                                select person;

                        p = q.FirstOrDefault();
                    }

                    if (p != null)
                        UpdatePeopleOfWeekImages(p);

                    this.Dispatcher.BeginInvoke(() =>
                    {
                        using (var db = WTShareDataContext.ShareDB)
                        {
                            //...Update UI
                            if (p != null)
                            {
                                ScrollViewer_PeopleOfWeek.Visibility = Visibility.Visible;
                                PanoramaItem_PeopleOfWeek.DataContext = p;
                            }
                        }
                    });

                    --RefreshCounter;
                };

                client.ExecuteFailed += (o, arg) =>
                {
                    //...Do Nothing if failed
                    Debug.WriteLine(arg.Error);

                    --RefreshCounter;
                };

                ++RefreshCounter;
                client.Execute(req);
            });
        }

        private void UpdatePeopleOfWeekImages(PersonExt person)
        {
            if (null == person)
                throw new ArgumentNullException("person");

            #region [Update Avatar]

            if (!person.Avatar.EndsWith("missing.png")
                && !String.IsNullOrEmpty(person.AvatarGuid)
                && !person.AvatarExists())
            {
                var client = new WTDownloadImageClient();

                client.DownloadImageStarted += (obj, arg) =>
                    {
                        Debug.WriteLine("Download person avatar started: {0}", arg.Url);
                    };

                client.DownloadImageCompleted += (obj, arg) =>
                    {
                        Debug.WriteLine("Download person avatar completed: {0}", arg.Url);

                        person.SaveAvatar(arg.ImageStream);

                        this.Dispatcher.BeginInvoke(() =>
                        {
                            try
                            {
                                (PanoramaItem_PeopleOfWeek.DataContext as PersonExt).SendPropertyChanged("AvatarImageBrush");
                            }
                            catch { }
                        });
                    };

                client.DownloadImageFailed += (obj, arg) =>
                {
                    Debug.WriteLine("Download person avatar FAILED: {0}\nErr:{1}", arg.Url, arg.Error);
                };

                client.Execute(person.Avatar);
            }

            #endregion

            #region [Update First Image]

            var images = person.GetImages();
            if (images.Count > 0 && !person.ImageExists())
            {
                var kv = images.First();
                var client = new WTDownloadImageClient();

                client.DownloadImageStarted += (obj, arg) =>
                {
                    Debug.WriteLine("Download person's first image started: {0}", arg.Url);
                };

                client.DownloadImageCompleted += (obj, arg) =>
                {
                    Debug.WriteLine("Download person's first image completed: {0}", arg.Url);

                    person.SaveImage(arg.ImageStream);

                    this.Dispatcher.BeginInvoke(() =>
                    {
                        (PanoramaItem_PeopleOfWeek.DataContext as PersonExt).SendPropertyChanged("FirstImageBrush");
                    });
                };

                client.DownloadImageFailed += (obj, arg) =>
                {
                    Debug.WriteLine("Download person's first image FAILED: {0}\nErr:{1}", arg.Url, arg.Error);
                };

                client.Execute(kv.Key);
            }

            #endregion
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

        private void RefreshCampusInfoImages(object sender, EventArgs e)
        {
            WTDispatcher.Instance.Do(() =>
            {
                SchoolNewsExt sn = null;
                AroundExt an = null;
                ForStaffExt fs = null;
                ClubNewsExt cn = null;

                using (var db = WTShareDataContext.ShareDB)
                {
                    var q = (from SchoolNewsExt news in db.SchoolNewsTable.ToArray()
                             where !String.IsNullOrEmpty(news.ImageExtList)
                             select news);
                    sn = q.LastOrDefault();
                }

                using (var db = WTShareDataContext.ShareDB)
                {
                    var q = (from AroundExt news in db.AroundTable.ToArray()
                             where !String.IsNullOrEmpty(news.ImageExtList)
                             select news);
                    an = q.LastOrDefault();
                }

                using (var db = WTShareDataContext.ShareDB)
                {
                    var q = (from ForStaffExt news in db.ForStaffTable.ToArray()
                             where !String.IsNullOrEmpty(news.ImageExtList)
                             select news);
                    fs = q.LastOrDefault();
                }

                using (var db = WTShareDataContext.ShareDB)
                {
                    var q = (from ClubNewsExt news in db.ClubNewsTable.ToArray()
                             where !String.IsNullOrEmpty(news.ImageExtList)
                             select news);
                    cn = q.LastOrDefault();
                }

                this.Dispatcher.BeginInvoke(() =>
                {
                    Button_TongjiNews.DataContext = sn;
                    Button_AroundNews.DataContext = an;
                    Button_OfficialNotes.DataContext = fs;
                    Button_ClubNews.DataContext = cn;
                });

                #region [Download Images]

                #region [School News]

                if (sn != null && !sn.ImageExists())
                {
                    WTDownloadImageClient c = new WTDownloadImageClient();

                    c.DownloadImageStarted += (obj, arg) =>
                    {
                        Debug.WriteLine("Download 1st image of school news started: {0}", arg.Url);
                    };

                    c.DownloadImageFailed += (obj, arg) =>
                    {
                        Debug.WriteLine("Download 1st image of school news FAILED: {0}\nError: {1}", arg.Url, arg.Error);
                    };

                    c.DownloadImageCompleted += (obj, arg) =>
                    {
                        Debug.WriteLine("Download 1st image of school news completed: {0}", arg.Url);

                        sn.SaveImage(arg.ImageStream);

                        this.Dispatcher.BeginInvoke(() =>
                        {
                            sn.SendPropertyChanged("FirstImageBrush");
                        });
                    };

                    c.Execute(sn.GetImagesURL().First());
                }
                #endregion

                #region [Around News]

                if (an != null && !an.ImageExists())
                {
                    WTDownloadImageClient c = new WTDownloadImageClient();

                    c.DownloadImageStarted += (obj, arg) =>
                    {
                        Debug.WriteLine("Download 1st image of around news started: {0}", arg.Url);
                    };

                    c.DownloadImageFailed += (obj, arg) =>
                    {
                        Debug.WriteLine("Download 1st image of around news FAILED: {0}\nError: {1}", arg.Url, arg.Error);
                    };

                    c.DownloadImageCompleted += (obj, arg) =>
                    {
                        Debug.WriteLine("Download 1st image of around news completed: {0}", arg.Url);

                        an.SaveImage(arg.ImageStream);

                        this.Dispatcher.BeginInvoke(() =>
                        {
                            an.SendPropertyChanged("FirstImageBrush");
                        });
                    };

                    c.Execute(an.GetImagesURL().First());
                }

                #endregion

                #region [Official Notes]

                if (fs != null && !fs.ImageExists())
                {
                    WTDownloadImageClient c = new WTDownloadImageClient();

                    c.DownloadImageStarted += (obj, arg) =>
                    {
                        Debug.WriteLine("Download 1st image of official note started: {0}", arg.Url);
                    };

                    c.DownloadImageFailed += (obj, arg) =>
                    {
                        Debug.WriteLine("Download 1st image of official note FAILED: {0}\nError: {1}", arg.Url, arg.Error);
                    };

                    c.DownloadImageCompleted += (obj, arg) =>
                    {
                        Debug.WriteLine("Download 1st image of official note completed: {0}", arg.Url);

                        fs.SaveImage(arg.ImageStream);

                        this.Dispatcher.BeginInvoke(() =>
                        {
                            fs.SendPropertyChanged("FirstImageBrush");
                        });
                    };

                    c.Execute(fs.GetImagesURL().First());
                }

                #endregion

                #region [Club News]

                if (cn != null && !cn.ImageExists())
                {
                    WTDownloadImageClient c = new WTDownloadImageClient();

                    c.DownloadImageStarted += (obj, arg) =>
                    {
                        Debug.WriteLine("Download 1st image of club news started: {0}", arg.Url);
                    };

                    c.DownloadImageFailed += (obj, arg) =>
                    {
                        Debug.WriteLine("Download 1st image of club news FAILED: {0}\nError: {1}", arg.Url, arg.Error);
                    };

                    c.DownloadImageCompleted += (obj, arg) =>
                    {
                        Debug.WriteLine("Download 1st image of club news completed: {0}", arg.Url);

                        cn.SaveImage(arg.ImageStream);

                        this.Dispatcher.BeginInvoke(() =>
                        {
                            cn.SendPropertyChanged("FirstImageBrush");
                        });
                    };

                    c.Execute(cn.GetImagesURL().First());
                }

                #endregion

                #endregion
            });
        }

        private void OnRefreshNewsCompleted()
        {
            var handler = RefreshNewsCompleted;
            if (handler != null)
                handler(this, EventArgs.Empty);
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
                //...Todo @_@ Take SORTing into consideration
                activitySrc = new ObservableCollection<ActivityExt>(activityArr.Reverse());
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
                    ListBox_Activity.ItemsSource = activitySrc;
                    ListBox_Activity.Visibility = Visibility.Visible;
                }

                if (personSrc != null)
                {
                    ScrollViewer_PeopleOfWeek.Visibility = Visibility.Visible;
                    PanoramaItem_PeopleOfWeek.DataContext = personSrc;
                }

                Button_TongjiNews.DataContext = snSrc;
                Button_AroundNews.DataContext = anSrc;
                Button_OfficialNotes.DataContext = fsSrc;
                Button_ClubNews.DataContext = cnSrc;
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
                            catch (System.Exception ex)
                            {

                            }

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
    }
}