using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using WeTongji.Business;
using WeTongji.Api.Domain;
using WeTongji.DataBase;
using WeTongji.Utility;
using WeTongji.Api;
using System.Diagnostics;
using WeTongji.Pages;

namespace WeTongji
{
    public partial class NearBy : PhoneApplicationPage
    {
        public NearBy()
        {
            InitializeComponent();
        }

        #region [Overridden]

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var uri = e.Uri.ToString().TrimStart("/Pages/NearBy.xaml".ToCharArray());

            WTDispatcher.Instance.Do(() =>
            {
                AroundExt an = null;

                if (String.IsNullOrEmpty(uri))
                {
                    using (var db = WTShareDataContext.ShareDB)
                    {
                        an = db.AroundTable.LastOrDefault();
                    }
                }
                else
                {
                    if (uri.StartsWith("?q="))
                    {
                        uri = uri.TrimStart("?q=".ToCharArray());

                        int id;
                        if (Int32.TryParse(uri, out id))
                        {
                            using (var db = WTShareDataContext.ShareDB)
                            {
                                an = db.AroundTable.Where((news) => news.Id == id).SingleOrDefault();
                            }
                        }
                    }
                }

                if (an == null)
                    return;

                #region [Handling Binding]

                this.Dispatcher.BeginInvoke(() =>
                {
                    this.DataContext = an;

                    #region [Set description]

                    var tbs = an.Context.GetInlineCollection();

                    StackPanel_Description.Children.Clear();

                    var number = tbs.Count();
                    for (int i = 0; i < number; ++i)
                    {
                        var tb = tbs.ElementAt(i);
                        tb.Style = this.Resources["DescriptionTextBlockStyle"] as Style;
                        StackPanel_Description.Children.Add(tb);
                    }

                    #endregion
                });

                #endregion

                #region [Handle Images]

                #region [Title image]

                {
                    int syncCount = 0;

                    if (!an.IsTitleImageExists())
                    {
                        var client = new WTDownloadImageClient();

                        client.DownloadImageStarted += (obj, arg) =>
                        {
                            Debug.WriteLine("Download title image of NearBy news[Id={0}] started: {1}",
                                an.Id, arg.Url);

                            this.Dispatcher.BeginInvoke(() =>
                            {
                                ProgressBarPopup.Instance.Open();
                            });

                            ++syncCount;
                        };
                        client.DownloadImageFailed += (obj, arg) =>
                        {
                            Debug.WriteLine("Download title image of NearBy news[Id={0}] FAILED: {1}.\nError:{2}",
                                an.Id, arg.Url, arg.Error);

                            if (0 == (--syncCount))
                            {
                                this.Dispatcher.BeginInvoke(() =>
                                {
                                    ProgressBarPopup.Instance.Close();
                                });
                            }
                        };
                        client.DownloadImageCompleted += (obj, arg) =>
                        {
                            Debug.WriteLine("Download title image of NearBy news[Id={0}] completed: {1}",
                                an.Id, arg.Url);

                            an.SaveTitleImage(arg.ImageStream);

                            this.Dispatcher.BeginInvoke(() =>
                            {
                                an.SendPropertyChanged("TitleImageBrush");

                                if (0 == (--syncCount))
                                {
                                    ProgressBarPopup.Instance.Close();
                                }
                            });
                        };
                    }
                }

                #endregion

                #region [Related images]

                #region [illustrated]
                if (an.IsIllustrated)
                {
                    var images = an.GetImageExts();

                    this.Dispatcher.BeginInvoke(() =>
                    {
                        ListBox_Pic.ItemsSource = images;
                    });

                    int count = images.Count();
                    int syncCount = 0;

                    for (int i = 0; i < count; ++i)
                    {
                        int j = i;
                        var img = images.ElementAt(i);
                        if (!img.Url.EndsWith("missing.png") && !an.ImageExists(i))
                        {
                            var client = new WTDownloadImageClient();

                            client.DownloadImageStarted += (obj, arg) =>
                            {
                                Debug.WriteLine("Download NO.{0} image of NearBy news[Id={1}] started: {2}",
                                    j, an.Id, arg.Url);

                                this.Dispatcher.BeginInvoke(() =>
                                {
                                    ProgressBarPopup.Instance.Open();
                                });

                                ++syncCount;
                            };
                            client.DownloadImageFailed += (obj, arg) =>
                            {
                                Debug.WriteLine("Download NO.{0} image of NearBy news[Id={1}] FAILED: {2}.\nError:{3}",
                                    j, an.Id, arg.Url, arg.Error);

                                if (0 == (--syncCount))
                                {
                                    this.Dispatcher.BeginInvoke(() =>
                                    {
                                        ProgressBarPopup.Instance.Close();
                                    });
                                }
                            };
                            client.DownloadImageCompleted += (obj, arg) =>
                            {
                                Debug.WriteLine("Download NO.{0} image of NearBy news[Id={1}] completed: {2}",
                                    j, an.Id, arg.Url);

                                an.SaveImage(arg.ImageStream, j);

                                this.Dispatcher.BeginInvoke(() =>
                                {
                                    //if (j == 0)
                                    //    an.SendPropertyChanged("FirstImageBrush");

                                    img.SendPropertyChanged("ImageBrush");

                                    if (0 == (--syncCount))
                                    {
                                        ProgressBarPopup.Instance.Close();
                                    }
                                });
                            };

                            client.Execute(img.Url);
                        }
                    }
                }
                #endregion
                #region [not illustrated]

                else
                {
                    //...Do nothing
                }

                #endregion
                
                #endregion

                #endregion
            });

        }

        #endregion

        #region [Functions]

        private void ViewMapAddress(object sender, RoutedEventArgs e)
        {
#if VIEW_IN_BROWSER
            //WebBrowserTask wbt = new WebBrowserTask();
            //wbt.Uri = new Uri("http://maps.google.com/?q=" + HttpUtility.UrlEncode((sender as Button).Content.ToString()));
            //wbt.Show();
#endif
            var news = this.DataContext as AroundExt;

            if(news !=null && !String.IsNullOrEmpty(news.Location))
                this.NavigationService.Navigate(new Uri("/Pages/MapAddress.xaml?q=" + news.Location, UriKind.RelativeOrAbsolute));
        }

        private void MakePhoneCall(Object sender, RoutedEventArgs e)
        {
            var pct = new PhoneCallTask();
            pct.DisplayName = TextBlock_Title.Text;
            pct.PhoneNumber = (sender as Button).Content.ToString();
            pct.Show();
        }

        #endregion
    }
}