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
using System.Windows.Navigation;
using WeTongji.Api.Domain;
using WeTongji.DataBase;
using WeTongji.Business;
using WeTongji.Api;
using System.Diagnostics;
using WeTongji.Pages;
using WeTongji.Utility;
using Microsoft.Phone.Shell;
using WeTongji.Api.Request;
using WeTongji.Api.Response;

namespace WeTongji
{
    public partial class PeopleOfWeek : PhoneApplicationPage
    {
        public PeopleOfWeek()
        {
            InitializeComponent();
        }
        
        #region [Overridden]

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var uri = e.Uri.ToString().TrimStart("/Pages/PeopleOfWeek.xaml?q=".ToCharArray());
            int idx;

            if (int.TryParse(uri, out idx))
            {
                var thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(LoadData));
                thread.Start(idx);
            }
        }

        private void LoadData(Object param)
        {
            int idx = (int)param;
            PersonExt p = null;

            this.Dispatcher.BeginInvoke(() =>
            {
                ProgressBarPopup.Instance.Open();
            });

            using (var db = WTShareDataContext.ShareDB)
            {
                p = db.People.Where((person) => person.Id == idx).SingleOrDefault();
            }

            if (p != null)
            {
                var images = p.GetImageExts();
                int count = images.Count();

                this.Dispatcher.BeginInvoke(() =>
                {
                    ProgressBarPopup.Instance.Close();

                    this.DataContext = p;

                    #region [Set description]

                    var tbs = p.Description.GetInlineCollection();

                    StackPanel_Description.Children.Clear();

                    var number = tbs.Count();
                    for (int i = 0; i < number; ++i)
                    {
                        var tb = tbs.ElementAt(i);
                        tb.Style = this.Resources["DescriptionTextBlockStyle"] as Style;
                        StackPanel_Description.Children.Add(tb);
                    }

                    #endregion

                    Pivot_Core.Visibility = Visibility.Visible;
                });

                #region [App bar buttons]

                if (!String.IsNullOrEmpty(Global.Instance.Settings.UID))
                {
                    using (var db = new WTUserDataContext(Global.Instance.Settings.UID))
                    {
                        var favObj = db.Favorites.Where((fo) => fo.Id == (int)FavoriteIndex.kPeopleOfWeek).SingleOrDefault();

                        if (favObj.Contains(p.Id))
                        {
                            p.CanFavorite = false;
                        }
                    }
                }

                this.Dispatcher.BeginInvoke(() =>
                {
                    InitAppBarButtons();
                });

                #endregion

                #region [Download Images]
                if (count > 0)
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        ListBox_Pic.ItemsSource = images;
                    });

                    for (int i = 0; i < count; ++i)
                    {
                        int j = i;
                        var img = images.ElementAt(i);
                        if (!img.Url.EndsWith("missing.png") && !p.ImageExists(i))
                        {
                            var client = new WTDownloadImageClient();

                            client.DownloadImageCompleted += (obj, arg) =>
                            {
                                this.Dispatcher.BeginInvoke(() =>
                                {
                                    if (j == 0)
                                        p.SendPropertyChanged("FirstImageBrush");

                                    img.SendPropertyChanged("ImageBrush");
                                });
                            };

                            client.Execute(img.Url, img.Id + "." + img.Url.GetImageFileExtension());
                        }
                    }
                }
                #endregion
            }
            else
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    ProgressBarPopup.Instance.Close();
                });
            }
        }

        private void InitAppBarButtons()
        {
            var person = this.DataContext as PersonExt;

            if (person == null)
                return;

            if (person.CanLike)
            {
                var btn = ApplicationBar.Buttons[0] as ApplicationBarIconButton;

                btn.IsEnabled = true;
                btn.Click += Button_Like_Clicked;
                btn.Click -= Button_Unlike_Clicked;
            }
            else
            {
                var btn = ApplicationBar.Buttons[0] as ApplicationBarIconButton;

                btn.IconUri = new Uri("/icons/appbar.unlike.rest.png", UriKind.RelativeOrAbsolute);
                btn.Text = "取消喜欢";
                btn.IsEnabled = true;
                btn.Click -= Button_Like_Clicked;
                btn.Click += Button_Unlike_Clicked;
            }

            if (person.CanFavorite)
            {
                var btn = ApplicationBar.Buttons[1] as ApplicationBarIconButton;

                btn.IsEnabled = true;
                btn.Click += Button_Favorite_Clicked;
                btn.Click -= Button_Unfavorite_Clicked;
            }
            else
            {
                var btn = ApplicationBar.Buttons[1] as ApplicationBarIconButton;

                btn.IconUri = new Uri("/icons/appbar.unfavourite.rest.png", UriKind.RelativeOrAbsolute);
                btn.Text = "取消收藏";
                btn.IsEnabled = true;
                btn.Click -= Button_Favorite_Clicked;
                btn.Click += Button_Unfavorite_Clicked;
            }
        }

        #endregion

        #region [App bar]

        private void Button_Like_Clicked(Object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(Global.Instance.CurrentUserID))
            {
                MessageBox.Show("请登录后再试", "提示", MessageBoxButton.OK);
            }
            else
            {
                (this.ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = false;

                var req = new PersonLikeRequest<WTResponse>();
                var client = new WTDefaultClient<WTResponse>();
                req.Id = (this.DataContext as PersonExt).Id;

                client.ExecuteCompleted += (obj, arg) =>
                {
                    PersonExt itemInDB = null;
                    using (var db = WTShareDataContext.ShareDB)
                    {
                        itemInDB = db.People.Where((person) => person.Id == req.Id).SingleOrDefault();

                        if (itemInDB != null)
                        {
                            ++itemInDB.Like;
                            itemInDB.CanLike = false;
                            db.SubmitChanges();
                        }
                    }

                    this.Dispatcher.BeginInvoke(() =>
                    {
                        var btn = ApplicationBar.Buttons[0] as ApplicationBarIconButton;

                        btn.IconUri = new Uri("/icons/appbar.unlike.rest.png", UriKind.RelativeOrAbsolute);
                        btn.Text = "取消喜欢";
                        btn.IsEnabled = true;
                        btn.Click -= Button_Like_Clicked;
                        btn.Click += Button_Unlike_Clicked;

                        var person = this.DataContext as PersonExt;

                        if (itemInDB != null)
                        {
                            person.Like = itemInDB.Like;
                            person.SendPropertyChanged("Like");

                            (this.Resources["IncreaseLikeNumberAnimation"] as Storyboard).Begin();
                        }

                        ProgressBarPopup.Instance.Close();
                    });
                };

                client.ExecuteFailed += (obj, arg) =>
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        (this.ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = true;
                        ProgressBarPopup.Instance.Close();
                    });

                    if (arg.Error is System.Net.WebException)
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            MessageBox.Show("操作失败，请检查Wifi或网络链接后重试", "提示", MessageBoxButton.OK);
                            (this.ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = true;
                        });
                    }
                    else if (arg.Error is WTException)
                    {
                        var err = arg.Error as WTException;

                        this.Dispatcher.BeginInvoke(() =>
                        {
                            switch (err.StatusCode.Id)
                            {
                                case Api.Util.Status.NoAuth:
                                    {
                                        MessageBox.Show("您是否在其他客户端中登录过？请重新登录", "提示", MessageBoxButton.OK);
                                    }
                                    break;
                                //...Todo @_@ Check other status code.
                            }
                        });
                    }
                    else
                    {
                        MessageBox.Show("操作失败，请重试", "提示", MessageBoxButton.OK);
                    }
                };

                ProgressBarPopup.Instance.Open();
                client.Execute(req, Global.Instance.Session, Global.Instance.Settings.UID);
            }
        }

        private void Button_Unlike_Clicked(Object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(Global.Instance.CurrentUserID))
            {
                MessageBox.Show("请登录后再试", "提示", MessageBoxButton.OK);
            }
            else
            {
                (this.ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = false;

                var req = new PersonUnLikeRequest<WTResponse>();
                var client = new WTDefaultClient<WTResponse>();
                req.Id = (this.DataContext as PersonExt).Id;

                client.ExecuteCompleted += (obj, arg) =>
                {
                    PersonExt itemInDB = null;

                    using (var db = WTShareDataContext.ShareDB)
                    {
                        itemInDB = db.People.Where((person) => person.Id == req.Id).SingleOrDefault();

                        if (itemInDB != null)
                        {
                            itemInDB.Like = Math.Max(0, itemInDB.Like - 1);
                            itemInDB.CanLike = true;
                            db.SubmitChanges();
                        }
                    }

                    this.Dispatcher.BeginInvoke(() =>
                    {
                        var btn = ApplicationBar.Buttons[0] as ApplicationBarIconButton;

                        btn.IconUri = new Uri("/icons/appbar.like.rest.png", UriKind.RelativeOrAbsolute);
                        btn.Text = "喜欢";
                        btn.IsEnabled = true;
                        btn.Click += Button_Like_Clicked;
                        btn.Click -= Button_Unlike_Clicked;

                        var person = this.DataContext as PersonExt;

                        if (itemInDB != null)
                        {
                            person.Like = itemInDB.Like;
                            person.SendPropertyChanged("Like");

                            (this.Resources["DecreaseLikeNumberAnimation"] as Storyboard).Begin();
                        }

                        ProgressBarPopup.Instance.Close();
                    });
                };

                client.ExecuteFailed += (obj, arg) =>
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        (this.ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = true;
                        ProgressBarPopup.Instance.Close();
                    });

                    if (arg.Error is System.Net.WebException)
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            MessageBox.Show("操作失败，请检查Wifi或网络链接后重试", "提示", MessageBoxButton.OK);
                            (this.ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = true;
                        });
                    }
                    else if (arg.Error is WTException)
                    {
                        var err = arg.Error as WTException;

                        this.Dispatcher.BeginInvoke(() =>
                        {
                            switch (err.StatusCode.Id)
                            {
                                case Api.Util.Status.NoAuth:
                                    {
                                        MessageBox.Show("您是否在其他客户端中登录过？请重新登录", "提示", MessageBoxButton.OK);
                                    }
                                    break;
                                //...Todo @_@ Check other status code.
                            }
                        });
                    }
                    else
                    {
                        MessageBox.Show("操作失败，请重试", "提示", MessageBoxButton.OK);
                    }
                };

                ProgressBarPopup.Instance.Open();
                client.Execute(req, Global.Instance.Session, Global.Instance.Settings.UID);
            }
        }

        private void Button_Favorite_Clicked(Object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(Global.Instance.CurrentUserID))
            {
                MessageBox.Show("请登录后再试", "提示", MessageBoxButton.OK);
                return;
            }
            else
            {
                var req = new PersonFavoriteRequest<WTResponse>();
                var client = new WTDefaultClient<WTResponse>();

                req.Id = (this.DataContext as PersonExt).Id;

                client.ExecuteCompleted += (obj, arg) =>
                {
                    PersonExt itemInDB = null;

                    if (!String.IsNullOrEmpty(Global.Instance.CurrentUserID))
                    {
                        using (var db = WTShareDataContext.ShareDB)
                        {
                            itemInDB = db.People.Where((person) => person.Id == req.Id).SingleOrDefault();

                            if (itemInDB != null)
                            {
                                ++itemInDB.Favorite;
                                itemInDB.CanFavorite = false;
                            }

                            db.SubmitChanges();
                        }

                        using (var db = new WTUserDataContext(Global.Instance.Settings.UID))
                        {
                            var fav = db.Favorites.Where((fo) => fo.Id == (int)FavoriteIndex.kPeopleOfWeek).SingleOrDefault();

                            fav.Add(req.Id);

                            db.SubmitChanges();
                        }

                    }

                    this.Dispatcher.BeginInvoke(() =>
                    {
                        var favBtn = this.ApplicationBar.Buttons[1] as ApplicationBarIconButton;
                        favBtn.IconUri = new Uri("/icons/appbar.unfavourite.rest.png", UriKind.RelativeOrAbsolute);
                        favBtn.Text = "取消收藏";
                        favBtn.Click -= Button_Favorite_Clicked;
                        favBtn.Click += Button_Unfavorite_Clicked;
                        favBtn.IsEnabled = true;

                        var person = this.DataContext as PersonExt;

                        if (itemInDB != null)
                        {
                            person.Favorite = itemInDB.Favorite;
                            person.SendPropertyChanged("Favorite");

                            (this.Resources["IncreaseFavoriteNumberAnimation"] as Storyboard).Begin();
                        }

                        ProgressBarPopup.Instance.Close();
                    });

                };

                client.ExecuteFailed += (obj, arg) =>
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        ProgressBarPopup.Instance.Close();
                        (this.ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = true;

                        if (arg.Error is System.Net.WebException)
                        {
                            MessageBox.Show("操作失败，请检查Wifi和网络链接后重试", "提示", MessageBoxButton.OK);
                        }
                        else if (arg.Error is WTException)
                        {
                            switch ((arg.Error as WTException).StatusCode.Id)
                            {
                                case Api.Util.Status.NoAuth:
                                    {
                                        MessageBox.Show("您是否在其他客户端中登录过？请重新登录", "提示", MessageBoxButton.OK);
                                    }
                                    break;
                                //...Todo @_@ Check other status code.
                            }
                        }
                    });
                };

                ProgressBarPopup.Instance.Open();
                (this.ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = false;
                client.Execute(req, Global.Instance.Session, Global.Instance.Settings.UID);
            }
        }

        private void Button_Unfavorite_Clicked(Object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(Global.Instance.CurrentUserID))
            {
                MessageBox.Show("请登录后再试", "提示", MessageBoxButton.OK);
                return;
            }
            else
            {
                var req = new PersonUnFavoriteRequest<WTResponse>();
                var client = new WTDefaultClient<WTResponse>();

                req.Id = (this.DataContext as PersonExt).Id;

                client.ExecuteCompleted += (obj, arg) =>
                {
                    PersonExt itemInDB = null;

                    if (!String.IsNullOrEmpty(Global.Instance.CurrentUserID))
                    {
                        using (var db = WTShareDataContext.ShareDB)
                        {
                            itemInDB = db.People.Where((news) => news.Id == req.Id).SingleOrDefault();

                            if (itemInDB != null)
                            {
                                itemInDB.Favorite = Math.Max(0, itemInDB.Favorite - 1);
                                itemInDB.CanFavorite = true;
                            }

                            db.SubmitChanges();
                        }

                        using (var db = new WTUserDataContext(Global.Instance.Settings.UID))
                        {
                            var fav = db.Favorites.Where((fo) => fo.Id == (int)FavoriteIndex.kPeopleOfWeek).SingleOrDefault();

                            fav.Remove(req.Id);

                            db.SubmitChanges();
                        }

                    }
                    
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        var favBtn = this.ApplicationBar.Buttons[1] as ApplicationBarIconButton;
                        favBtn.IconUri = new Uri("/icons/appbar.favs.rest.png", UriKind.RelativeOrAbsolute);
                        favBtn.Text = "收藏";
                        favBtn.Click += Button_Favorite_Clicked;
                        favBtn.Click -= Button_Unfavorite_Clicked;
                        favBtn.IsEnabled = true;

                        var person = this.DataContext as PersonExt;

                        if (itemInDB != null)
                        {
                            person.Favorite = itemInDB.Favorite;
                            person.SendPropertyChanged("Favorite");

                            (this.Resources["DecreaseFavoriteNumberAnimation"] as Storyboard).Begin();
                        }

                        ProgressBarPopup.Instance.Close();
                    });

                };

                client.ExecuteFailed += (obj, arg) =>
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        ProgressBarPopup.Instance.Close();
                        (this.ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = true;

                        if (arg.Error is System.Net.WebException)
                        {
                            MessageBox.Show("操作失败，请检查Wifi和网络链接后重试", "提示", MessageBoxButton.OK);
                        }
                        else if (arg.Error is WTException)
                        {
                            switch ((arg.Error as WTException).StatusCode.Id)
                            {
                                case Api.Util.Status.NoAuth:
                                    {
                                        MessageBox.Show("您是否在其他客户端中登录过？请重新登录", "提示", MessageBoxButton.OK);
                                    }
                                    break;
                                //...Todo @_@ Check other status code.
                            }
                        }
                    });
                };

                ProgressBarPopup.Instance.Open();
                (this.ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = false;
                client.Execute(req, Global.Instance.Session, Global.Instance.Settings.UID);
            }
        }

        #endregion

        #region [Refresh related functions]

        ///// <summary>
        ///// Get the latest data of current news from the server and refresh Like number on UI
        ///// </summary>
        ///// <param name="isLikeButtonClicked">
        ///// True if the function is called by clicking the Like button. 
        ///// False if the function is called by clicking the Unlike button.
        ///// </param>
        ///// <remarks>
        ///// If failed, "Like" +1 when Like button clicked while "Like" -1 when Unlike button clicked.
        ///// </remarks>
        //private void RefreshCurrentNewsOnLikeButtonClicked(int id, Boolean isLikeButtonClicked)
        //{
        //    var req = new Person<SchoolNewsGetResponse>();
        //    var client = new WTDefaultClient<SchoolNewsGetResponse>();

        //    req.Id = id;

        //    client.ExecuteCompleted += (obj, arg) =>
        //    {
        //        using (var db = WTShareDataContext.ShareDB)
        //        {
        //            var news = db.SchoolNewsTable.Where((n) => n.Id == req.Id).SingleOrDefault();

        //            if (news != null)
        //            {
        //                news.Like = arg.Result.SchoolNews.Like;
        //            }

        //            db.SubmitChanges();
        //        }

        //        if (isLikeButtonClicked)
        //        {
        //            this.Dispatcher.BeginInvoke(() =>
        //            {
        //                var sn = this.DataContext as SchoolNewsExt;

        //                if (sn.Like != arg.Result.SchoolNews.Like)
        //                {
        //                    int previousLikeValue = sn.Like;
        //                    sn.Like = arg.Result.SchoolNews.Like;
        //                    sn.SendPropertyChanged("Like");

        //                    //...Play animation
        //                    if (previousLikeValue < arg.Result.SchoolNews.Like)
        //                        (this.Resources["IncreaseLikeNumberAnimation"] as Storyboard).Begin();
        //                }
        //            });
        //        }
        //        else
        //        {
        //            this.Dispatcher.BeginInvoke(() =>
        //            {
        //                var sn = this.DataContext as SchoolNewsExt;
        //                int previousLikeValue = sn.Like;
        //                sn.Like = arg.Result.SchoolNews.Like;

        //                sn.SendPropertyChanged("Like");

        //                //...Play animation
        //                if (previousLikeValue > arg.Result.SchoolNews.Like)
        //                    (this.Resources["DecreaseLikeNumberAnimation"] as Storyboard).Begin();
        //            });
        //        }
        //    };
        //    client.ExecuteFailed += (obj, arg) =>
        //    {
        //        this.Dispatcher.BeginInvoke(() =>
        //        {
        //            var sn = this.DataContext as SchoolNewsExt;

        //            if (isLikeButtonClicked)
        //            {
        //                ++sn.Like;
        //                sn.SendPropertyChanged("Like");

        //                //...Play animation
        //                (this.Resources["IncreaseLikeNumberAnimation"] as Storyboard).Begin();
        //            }
        //            else
        //            {
        //                sn.Like = Math.Max(0, sn.Like - 1);
        //                sn.SendPropertyChanged("Like");

        //                (this.Resources["DecreaseLikeNumberAnimation"] as Storyboard).Begin();
        //            }
        //        });
        //    };

        //    client.Execute(req, Global.Instance.Session, Global.Instance.Settings.UID);
        //}

        ///// <summary>
        ///// Get the latest Favorite number of current news from the server when user click fav/unfav button
        ///// </summary>
        ///// <remarks>
        ///// If failed, "Favorite" +1 when Like button clicked while "Favorite" -1 when UnFavorite button clicked.
        ///// </remarks>
        //private void RefreshCurrentNewsOnFavoriteButtonClicked(int id, Boolean isFavoriteButtonClicked)
        //{
        //    var req = new SchoolNewsGetRequest<SchoolNewsGetResponse>();
        //    var client = new WTDefaultClient<SchoolNewsGetResponse>();

        //    req.Id = id;

        //    client.ExecuteCompleted += (obj, arg) =>
        //    {
        //        using (var db = WTShareDataContext.ShareDB)
        //        {
        //            var news = db.SchoolNewsTable.Where((n) => n.Id == req.Id).SingleOrDefault();

        //            if (news != null)
        //            {
        //                news.Favorite = arg.Result.SchoolNews.Favorite;
        //            }

        //            db.SubmitChanges();
        //        }

        //        this.Dispatcher.BeginInvoke(() =>
        //        {
        //            var sn = this.DataContext as SchoolNewsExt;

        //            int previousValue = sn.Favorite;
        //            sn.Favorite = arg.Result.SchoolNews.Favorite;
        //            sn.SendPropertyChanged("Favorite");

        //            if (isFavoriteButtonClicked)
        //            {
        //                if (previousValue < sn.Favorite)
        //                {
        //                    (this.Resources["IncreaseFavoriteNumberAnimation"] as Storyboard).Begin();
        //                }
        //            }
        //            else
        //            {
        //                if (previousValue > sn.Favorite)
        //                {
        //                    (this.Resources["DecreaseFavoriteNumberAnimation"] as Storyboard).Begin();
        //                }
        //            }
        //        });
        //    };

        //    client.ExecuteFailed += (obj, arg) =>
        //    {
        //        //...Refresh UI and do nothing with database
        //        this.Dispatcher.BeginInvoke(() =>
        //        {
        //            var sn = this.DataContext as SchoolNewsExt;

        //            if (isFavoriteButtonClicked)
        //            {
        //                sn.Favorite++;
        //                sn.SendPropertyChanged("Favorite");
        //                (this.Resources["IncreaseFavoriteNumberAnimation"] as Storyboard).Begin();
        //            }
        //            else
        //            {
        //                sn.Favorite = Math.Max(0, sn.Favorite - 1);
        //                sn.SendPropertyChanged("Favorite");
        //                (this.Resources["DecreaseFavoriteNumberAnimation"] as Storyboard).Begin();
        //            }
        //        });
        //    };

        //    client.Execute(req, Global.Instance.Session, Global.Instance.Settings.UID);
        //}

        ///// <summary>
        ///// Refresh "Read" component of current piece of Tongji News.
        ///// </summary>
        ///// <remarks>
        ///// This function should be executed in UI thread.
        ///// </remarks>
        //private void RefreshCurrentNewsOnRead(int id)
        //{
        //    var req = new SchoolNewsGetRequest<SchoolNewsGetResponse>();
        //    var client = new WTDefaultClient<SchoolNewsGetResponse>();

        //    req.Id = id;

        //    client.ExecuteCompleted += (obj, arg) =>
        //    {
        //        SchoolNewsExt itemInDB = null;
        //        using (var db = WTShareDataContext.ShareDB)
        //        {
        //            itemInDB = db.SchoolNewsTable.Where((n) => n.Id == req.Id).SingleOrDefault();

        //            if (itemInDB != null)
        //            {
        //                itemInDB.Read = arg.Result.SchoolNews.Read;
        //                itemInDB.Favorite = arg.Result.SchoolNews.Favorite;
        //                itemInDB.Like = arg.Result.SchoolNews.Like;
        //                //...Do nothing with CanLike or CanFavorite
        //            }
        //            else
        //                return;

        //            db.SubmitChanges();
        //        }

        //        this.Dispatcher.BeginInvoke(() =>
        //        {
        //            var sn = this.DataContext as SchoolNewsExt;

        //            if (sn.Favorite != itemInDB.Favorite)
        //            {
        //                sn.Favorite = itemInDB.Favorite;
        //                sn.SendPropertyChanged("Favorite");
        //            }
        //            if (sn.Like != itemInDB.Like)
        //            {
        //                sn.Like = itemInDB.Like;
        //                sn.SendPropertyChanged("Like");
        //            }
        //        });
        //    };

        //    client.Execute(req);
        //}

        /// <summary>
        /// Send Read request to server.
        /// </summary>
        /// <remarks>
        /// This function should be executed in UI thread.
        /// </remarks>
        private void ReadCurrentPerson()
        {
            var req = new PersonReadRequest<WTResponse>();
            var client = new WTDefaultClient<WTResponse>();

            req.Id = (this.DataContext as PersonExt).Id;

            client.ExecuteCompleted += (o, e) =>
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    using (var db = WTShareDataContext.ShareDB)
                    {
                        var itemInDB = db.People.Where((person) => person.Id == req.Id).SingleOrDefault();
                        ++itemInDB.Read;
                        db.SubmitChanges();
                    }
                });
            };

            if (!String.IsNullOrEmpty(Global.Instance.CurrentUserID))
                client.Execute(req, Global.Instance.Session, Global.Instance.Settings.UID);
            else
                client.Execute(req);
        }

        #endregion

        private void IgnoreSelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            var lb = sender as ListBox;

            if (-1 == lb.SelectedIndex)
                return;
            lb.SelectedIndex = -1;
        }

        private void TapToViewSourceImage(Object sender, Microsoft.Phone.Controls.GestureEventArgs e)
        {
            var img = sender as Image;
            var imgExt = img.DataContext as ImageExt;
            ImageViewer.CoreImageName = imgExt.Id;
            ImageViewer.CoreImageSource = img.Source as System.Windows.Media.Imaging.BitmapSource;
            this.NavigationService.Navigate(new Uri("/Pages/ImageViewer.xaml", UriKind.RelativeOrAbsolute));
        }
    }
}