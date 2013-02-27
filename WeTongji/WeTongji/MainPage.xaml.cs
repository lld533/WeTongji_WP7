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

            this.Loaded += (o, e) =>
                {
                    if (!isResourceLoaded)
                    {
                        //...Activity
                        using (var db = WTShareDataContext.ShareDB)
                        {
                            if (db.Activities.Count() > 0)
                            {
                                //...Todo @_@ Take SORTing into consideration
                                var q = from ActivityExt a in db.Activities
                                        orderby a.Id descending
                                        select a;
                                ListBox_Activity.ItemsSource = new ObservableCollection<ActivityExt>(q);
                            }
                        }

                        //...PeopleOfWeek
                        using (var db = WTShareDataContext.ShareDB)
                        {
                            var q = from PersonExt person in db.People
                                    orderby person.NO descending
                                    select person;
                            var latestPerson = q.FirstOrDefault();
                            if (latestPerson != null)
                            {
                                StackPanel_PeopleOfWeek.Visibility = Visibility.Visible;
                                PanoramaItem_PeopleOfWeek.DataContext = latestPerson;
                            }
                        }

                        //Activity Info
                        {
                            {
                                SchoolNewsExt[] sn = null;
                                using (var db = WTShareDataContext.ShareDB)
                                {
                                    sn = db.SchoolNewsTable.ToArray();
                                }

                                if(sn!=null)
                                    Button_TongjiNews.DataContext = sn.Where((news) => !String.IsNullOrEmpty(news.ImageExtList)).LastOrDefault();
                            }

                            {
                                AroundExt[] an = null;
                                using (var db = WTShareDataContext.ShareDB)
                                {
                                    an = db.AroundTable.ToArray();
                                }

                                if (an != null)
                                    Button_AroundNews.DataContext = an.Where((news) => !String.IsNullOrEmpty(news.ImageExtList)).LastOrDefault();
                            }

                            {
                                ForStaffExt[] fs = null;
                                using (var db = WTShareDataContext.ShareDB)
                                {
                                    fs = db.ForStaffTable.ToArray();
                                }

                                if (fs != null)
                                    Button_OfficialNotes.DataContext = fs.Where((news) => !String.IsNullOrEmpty(news.ImageExtList)).LastOrDefault();
                            }

                            {
                                ClubNewsExt[] cn = null;
                                using (var db = WTShareDataContext.ShareDB)
                                {
                                    cn = db.ClubNewsTable.ToArray();
                                }

                                if (cn != null)
                                    Button_ClubNews.DataContext = cn.Where((news) => !String.IsNullOrEmpty(news.ImageExtList)).LastOrDefault();
                            }

                        }

                        isResourceLoaded = true;
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

        #region [Refresh related functions]

        private void RefreshAll()
        {
            refreshCounter = 0;

            RefreshActivityList();
            RefreshPeopleOfWeek();
            RefreshCampusInfo();
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
                                StackPanel_PeopleOfWeek.Visibility = Visibility.Visible;
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