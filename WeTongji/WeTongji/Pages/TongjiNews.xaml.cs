using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using WeTongji.Business;
using WeTongji.Api.Domain;
using WeTongji.DataBase;
using WeTongji.Utility;
using WeTongji.Api;
using System.Diagnostics;
using WeTongji.Pages;

namespace WeTongji
{
    public partial class TongjiNews : PhoneApplicationPage
    {
        public TongjiNews()
        {
            InitializeComponent();
        }

        #region [Overridden]

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var uri = e.Uri.ToString().TrimStart("/Pages/TongjiNews.xaml".ToCharArray());

            WTDispatcher.Instance.Do(() =>
            {
                SchoolNewsExt sn = null;

                if (String.IsNullOrEmpty(uri))
                {
                    using (var db = WTShareDataContext.ShareDB)
                    {
                        sn = db.SchoolNewsTable.LastOrDefault();
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
                                sn = db.SchoolNewsTable.Where((news) => news.Id == id).SingleOrDefault();
                            }
                        }
                    }
                }

                if (sn == null)
                    return;

                #region [Handling Binding]

                this.Dispatcher.BeginInvoke(() =>
                {
                    this.DataContext = sn;

                    #region [Set description]

                    var tbs = sn.Context.GetInlineCollection();

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

                #region [illustrated]
                if (sn.IsIllustrated)
                {
                    var images = sn.GetImageExts();
                    var otherImages = images.Except(new ImageExt[] { images.First() });

                    this.Dispatcher.BeginInvoke(() =>
                    {
                        NoIllustrationGrid.Visibility = Visibility.Collapsed;
                        IllustrationGrid.Visibility = Visibility.Visible;

                        ListBox_Pic.ItemsSource = otherImages;
                    });

                    int count = images.Count();
                    int syncCount = 0;

                    for (int i = 0; i < count; ++i)
                    {
                        int j = i;
                        var img = images.ElementAt(i);
                        if (!img.Url.EndsWith("missing.png") && !sn.ImageExists(i))
                        {
                            var client = new WTDownloadImageClient();

                            client.DownloadImageStarted += (obj, arg) =>
                            {
                                Debug.WriteLine("Download NO.{0} image of Tongji News[Id={1}] started: {2}",
                                    j, sn.Id, arg.Url);

                                this.Dispatcher.BeginInvoke(() =>
                                {
                                    ProgressBarPopup.Instance.Open();
                                });

                                ++syncCount;
                            };
                            client.DownloadImageFailed += (obj, arg) =>
                            {
                                Debug.WriteLine("Download NO.{0} image of Tongji News[Id={1}] FAILED: {2}.\nError:{3}",
                                    j, sn.Id, arg.Url, arg.Error);

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
                                Debug.WriteLine("Download NO.{0} image of Tongji News[Id={1}] completed: {2}",
                                    j, sn.Id, arg.Url);

                                sn.SaveImage(arg.ImageStream, j);

                                this.Dispatcher.BeginInvoke(() =>
                                {
                                    if (j == 0)
                                        sn.SendPropertyChanged("FirstImageBrush");

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
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        IllustrationGrid.Visibility = Visibility.Collapsed;
                        NoIllustrationGrid.Visibility = Visibility.Visible;
                    });
                }

                #endregion

                #endregion
            });
        }

        #endregion
    }
}