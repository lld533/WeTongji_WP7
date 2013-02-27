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
using System.Text.RegularExpressions;
using System.Text;
using WeTongji.DataBase;
using WeTongji.Pages;
using WeTongji.Api.Domain;
using WeTongji.Business;
using WeTongji.Api;
using System.Diagnostics;

namespace WeTongji
{
    public partial class CampusInfo : PhoneApplicationPage
    {
        private int loadDataCounter = 0;

        private int LoadDataCounter
        {
            get { return loadDataCounter; }
            set
            {
                if (value == loadDataCounter)
                    return;

                loadDataCounter = value;

                if (loadDataCounter > 0)
                    ProgressBarPopup.Instance.Open();
                else
                {
                    OnLoadDataCompleted();
                    ProgressBarPopup.Instance.Close();
                }
            }
        }

        private event EventHandler LoadDataCompleted;

        public CampusInfo()
        {
            LoadDataCompleted += LoadUnstoredImages;
            InitializeComponent();
        }

        private void OnLoadDataCompleted()
        {
            var handler = LoadDataCompleted;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void LoadUnstoredImages(object sender, EventArgs e)
        {
            #region [Tongji News]

            if (ListBox_TongjiNews.ItemsSource != null)
            {
                var src = ListBox_TongjiNews.ItemsSource as IEnumerable<SchoolNewsExt>;

                for (int i = 0; i < src.Count(); ++i)
                {
                    var news = src.ElementAt(i);

                    if (!String.IsNullOrEmpty(news.ImageExtList) && !news.ImageExists())
                    {
                        WTDispatcher.Instance.Do(() =>
                        {
                            var client = new WTDownloadImageClient();

                            client.DownloadImageStarted += (obj, arg) =>
                                {
                                    Debug.WriteLine("Download 1st image of school news[{0}] started: {1}", news.Id, arg.Url);
                                };

                            client.DownloadImageFailed += (obj, arg) =>
                            {
                                Debug.WriteLine("Download 1st image of school news[{0}] FAILED: {1}\nError:{2}", news.Id, arg.Url, arg.Error);
                            };

                            client.DownloadImageCompleted += (obj, arg) =>
                            {
                                Debug.WriteLine("Download 1st image of school news[{0}] completed: {1}", news.Id, arg.Url);

                                news.SaveImage(arg.ImageStream);

                                this.Dispatcher.BeginInvoke(() =>
                                {
                                    news.SendPropertyChanged("FirstImageBrush");
                                });
                            };

                            client.Execute(news.GetImagesURL().First());
                        });
                    }
                }
            }

            #endregion

            #region [Society News]

            if (ListBox_SocietyNews.ItemsSource != null)
            {
                var src = ListBox_SocietyNews.ItemsSource as IEnumerable<ClubNewsExt>;

                for (int i = 0; i < src.Count(); ++i)
                {
                    var news = src.ElementAt(i);

                    if (!String.IsNullOrEmpty(news.ImageExtList) && !news.ImageExists())
                    {
                        WTDispatcher.Instance.Do(() =>
                        {
                            var client = new WTDownloadImageClient();

                            client.DownloadImageStarted += (obj, arg) =>
                            {
                                Debug.WriteLine("Download 1st image of club news[{0}] started: {1}", news.Id, arg.Url);
                            };

                            client.DownloadImageFailed += (obj, arg) =>
                            {
                                Debug.WriteLine("Download 1st image of club news[{0}] FAILED: {1}\nError:{2}", news.Id, arg.Url, arg.Error);
                            };

                            client.DownloadImageCompleted += (obj, arg) =>
                            {
                                Debug.WriteLine("Download 1st image of club news[{0}] completed: {1}", news.Id, arg.Url);

                                news.SaveImage(arg.ImageStream);

                                this.Dispatcher.BeginInvoke(() =>
                                {
                                    news.SendPropertyChanged("FirstImageBrush");
                                });
                            };

                            client.Execute(news.GetImagesURL().First());
                        });
                    }
                }
            }

            #endregion

            #region [Official Notes]

            if (ListBox_OfficialNotes.ItemsSource != null)
            {
                var src = ListBox_OfficialNotes.ItemsSource as IEnumerable<ForStaffExt>;

                for (int i = 0; i < src.Count(); ++i)
                {
                    var news = src.ElementAt(i);

                    if (!String.IsNullOrEmpty(news.ImageExtList) && !news.ImageExists())
                    {
                        WTDispatcher.Instance.Do(() =>
                        {
                            var client = new WTDownloadImageClient();

                            client.DownloadImageStarted += (obj, arg) =>
                            {
                                Debug.WriteLine("Download 1st image of official note[{0}] started: {1}", news.Id, arg.Url);
                            };

                            client.DownloadImageFailed += (obj, arg) =>
                            {
                                Debug.WriteLine("Download 1st image of official note[{0}] FAILED: {1}\nError:{2}", news.Id, arg.Url, arg.Error);
                            };

                            client.DownloadImageCompleted += (obj, arg) =>
                            {
                                Debug.WriteLine("Download 1st image of official note[{0}] completed: {1}", news.Id, arg.Url);

                                news.SaveImage(arg.ImageStream);

                                this.Dispatcher.BeginInvoke(() =>
                                {
                                    news.SendPropertyChanged("FirstImageBrush");
                                });
                            };

                            client.Execute(news.GetImagesURL().First());
                        });
                    }
                }
            }

            #endregion

            #region [Nearby News]

            if (ListBox_NearBy.ItemsSource != null)
            {
                var src = ListBox_NearBy.ItemsSource as IEnumerable<AroundExt>;

                for (int i = 0; i < src.Count(); ++i)
                {
                    var news = src.ElementAt(i);

                    if (!news.IsTitleImageExists())
                    {
                        WTDispatcher.Instance.Do(() =>
                        {
                            var client = new WTDownloadImageClient();

                            client.DownloadImageStarted += (obj, arg) =>
                            {
                                Debug.WriteLine("Download title image of nearby news[{0}] started: {1}", news.Id, arg.Url);
                            };

                            client.DownloadImageFailed += (obj, arg) =>
                            {
                                Debug.WriteLine("Download title image of nearby news[{0}] FAILED: {1}\nError:{2}", news.Id, arg.Url, arg.Error);
                            };

                            client.DownloadImageCompleted += (obj, arg) =>
                            {
                                Debug.WriteLine("Download title image of nearby news[{0}] completed: {1}", news.Id, arg.Url);

                                news.SaveTitleImage(arg.ImageStream);

                                this.Dispatcher.BeginInvoke(() =>
                                {
                                    news.SendPropertyChanged("TitleImageBrush");
                                });
                            };

                            client.Execute(news.TitleImage);
                        });
                    }
                }
            }

            #endregion
        }

        #region [Overridden]

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// [Query] like /Pages/CampusInfo.xaml?q={Int32}
        /// where the query string is the selected index of the Pivot
        /// </remarks>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New)
            {
                var str = e.Uri.ToString();
                var q = str.Substring(str.IndexOf('?'));
                q = q.TrimStart("?q=".ToCharArray());

                Int32 idx = 0;
                Int32.TryParse(q, out idx);
                idx = Math.Max(idx, 0);
                idx = idx > (Int32)CampusInfoType.SocietyNews ? 0 : idx;

                Pivot_Core.SelectedIndex = idx;

                if (ListBox_TongjiNews.ItemsSource != null)
                    ListBox_TongjiNews.Visibility = Visibility.Visible;

                if (ListBox_NearBy.ItemsSource != null)
                    ListBox_NearBy.Visibility = Visibility.Visible;

                if (ListBox_OfficialNotes.ItemsSource != null)
                    ListBox_OfficialNotes.Visibility = Visibility.Visible;

                if (ListBox_SocietyNews.ItemsSource != null)
                    ListBox_SocietyNews.Visibility = Visibility.Visible;

                #region [Load actions]

                LoadDataCounter = 4;

                var actions = new Action[] 
                    { 
                        ()=>
                        {
                            WTDispatcher.Instance.Do(() => 
                            {
                                SchoolNewsExt[] schoolnews = null;
                                using (var db = WTShareDataContext.ShareDB)
                                {
                                    schoolnews = db.SchoolNewsTable.ToArray();
                                }
                                var src = schoolnews.Reverse();
                                int count = src.Count();

                                this.Dispatcher.BeginInvoke(() => 
                                {
                                    --LoadDataCounter;

                                    if (ListBox_TongjiNews.ItemsSource == null)
                                    {
                                        ListBox_TongjiNews.ItemsSource = src;
                                    }
                                    else if ((ListBox_TongjiNews.ItemsSource as IEnumerable<SchoolNewsExt>).Count() != count)
                                    {
                                        ListBox_TongjiNews.ItemsSource = src;
                                    }
                                    else
                                        return;

                                    if (count > 0)
                                        ListBox_TongjiNews.Visibility = Visibility.Visible;
                                });
                            });
                        },
  
                        ()=>
                        {
                            WTDispatcher.Instance.Do(() =>
                            {
                                AroundExt[] an = null;
                                using (var db = WTShareDataContext.ShareDB)
                                {
                                    an = db.AroundTable.ToArray();
                                }
                                var src = an.Reverse();
                                int count = src.Count();

                                this.Dispatcher.BeginInvoke(() =>
                                {
                                    --LoadDataCounter;

                                    if (ListBox_NearBy.ItemsSource == null)
                                    {
                                        ListBox_NearBy.ItemsSource = src;
                                    }
                                    else if ((ListBox_NearBy.ItemsSource as IEnumerable<AroundExt>).Count() != count)
                                    {
                                        ListBox_NearBy.ItemsSource = src;
                                    }
                                    else
                                        return;

                                    if (count > 0)
                                        ListBox_NearBy.Visibility = Visibility.Visible;
                                });
                            });
                        },

                        ()=>
                        {
                            WTDispatcher.Instance.Do(() =>
                            {
                                ForStaffExt[] fs = null;
                                using (var db = WTShareDataContext.ShareDB)
                                {
                                    fs = db.ForStaffTable.ToArray();
                                }
                                var src = fs.Reverse();
                                int count = src.Count();

                                this.Dispatcher.BeginInvoke(() =>
                                {
                                    --LoadDataCounter;

                                    if (ListBox_OfficialNotes.ItemsSource == null)
                                    {
                                        ListBox_OfficialNotes.ItemsSource = src;
                                    }
                                    else if ((ListBox_OfficialNotes.ItemsSource as IEnumerable<ForStaffExt>).Count() != count)
                                    {
                                        ListBox_OfficialNotes.ItemsSource = src;
                                    }
                                    else
                                        return;

                                    if (count > 0)
                                        ListBox_OfficialNotes.Visibility = Visibility.Visible;
                                });
                            });
                        },

                        ()=>
                        {
                            WTDispatcher.Instance.Do(() =>
                            {
                                ClubNewsExt[] sn = null;
                                using (var db = WTShareDataContext.ShareDB)
                                {
                                    sn = db.ClubNewsTable.ToArray();
                                }
                                var src = sn.Reverse();
                                int count = src.Count();

                                this.Dispatcher.BeginInvoke(() =>
                                {
                                    --LoadDataCounter;

                                    if (ListBox_SocietyNews.ItemsSource == null)
                                    {
                                        ListBox_SocietyNews.ItemsSource = src;
                                    }
                                    else if ((ListBox_SocietyNews.ItemsSource as IEnumerable<ClubNewsExt>).Count() != count)
                                    {
                                        ListBox_SocietyNews.ItemsSource = src;
                                    }
                                    else
                                        return;

                                    if (count > 0)
                                        ListBox_SocietyNews.Visibility = Visibility.Visible;
                                });
                            });
                        }
                    };

                #endregion

                int startIndex = Math.Max(0, Pivot_Core.SelectedIndex);

                for (int i = startIndex; i < 4; ++i)
                {
                    actions[i]();
                }

                for (int i = 0; i < startIndex; ++i)
                {
                    actions[i]();
                }
            }
        }

        #endregion

        #region [Nav]

        private void Listbox_TongjiNews_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lb = sender as ListBox;

            if (lb.SelectedIndex == -1)
                return;

            var item = lb.SelectedItem as SchoolNewsExt;
            lb.SelectedIndex = -1;
            this.NavigationService.Navigate(new Uri("/Pages/TongjiNews.xaml?q=" + item.Id, UriKind.RelativeOrAbsolute));
        }

        private void ListBox_NearBy_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            var lb = sender as ListBox;

            if (lb.SelectedIndex == -1)
                return;

            var item = lb.SelectedItem as AroundExt;
            lb.SelectedIndex = -1;
            this.NavigationService.Navigate(new Uri("/Pages/NearBy.xaml?q=" + item.Id, UriKind.RelativeOrAbsolute));
        }

        private void Listbox_OfficialNotes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lb = sender as ListBox;

            if (lb.SelectedIndex == -1)
                return;

            var item = lb.SelectedItem as ForStaffExt;
            lb.SelectedIndex = -1;
            this.NavigationService.Navigate(new Uri("/Pages/OfficialNote.xaml?q=" + item.Id, UriKind.RelativeOrAbsolute));
        }

        private void ListBox_SocietyNews_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lb = sender as ListBox;

            if (lb.SelectedIndex == -1)
                return;

            var item = lb.SelectedItem as ClubNewsExt;
            lb.SelectedIndex = -1;
            this.NavigationService.Navigate(new Uri("/Pages/SocietyNews.xaml?q=" + item.Id, UriKind.RelativeOrAbsolute));
        }

        #endregion

        #region [Class]

        public class TestSource
        {
            public Boolean IsRecommended { get; set; }
            public Boolean HasPic { get; set; }
        }

        #endregion

        #region [Enum]

        private enum CampusInfoType : int
        {
            TongjiNews,
            NearBy,
            OfficialNote,
            SocietyNews
        }

        #endregion
    }
}