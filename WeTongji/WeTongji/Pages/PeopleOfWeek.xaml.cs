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
            ThemeManager.ToDarkTheme();

            var uri = e.Uri.ToString().TrimStart("/Pages/PeopleOfWeek.xaml".ToCharArray());

            PersonExt p = null;

            if (String.IsNullOrEmpty(uri))
            {
                using (var db = WTShareDataContext.ShareDB)
                {
                    var q = from PersonExt person in db.People
                            orderby p.NO descending
                            select person;

                    if (q != null)
                        p = q.FirstOrDefault();
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
                            p = db.People.Where((person) => person.Id == id).SingleOrDefault();
                        }
                    }
                }
            }

            if (p != null)
            {
                var images = p.GetImageExts();

                this.Dispatcher.BeginInvoke(() =>
                {
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

                    ListBox_Pic.ItemsSource = images;

                    Pivot_Core.Visibility = Visibility.Visible;
                });

                int count = images.Count();
                int syncCount = 0;

                for (int i = 0; i < count; ++i)
                {
                    int j = i;
                    var img = images.ElementAt(i);
                    if (!img.Url.EndsWith("missing.png") && !p.ImageExists(i))
                    {
                        var client = new WTDownloadImageClient();

                        client.DownloadImageStarted += (obj, arg) =>
                        {
                            Debug.WriteLine("Download NO.{0} image of person[Id={1}] started: {2}",
                                j, p.Id, arg.Url);

                            this.Dispatcher.BeginInvoke(() =>
                            {
                                ProgressBarPopup.Instance.Open();
                            });

                            ++syncCount;
                        };
                        client.DownloadImageFailed += (obj, arg) =>
                        {
                            Debug.WriteLine("Download NO.{0} image of person[Id={1}] FAILED: {2}.\nError:{3}",
                                j, p.Id, arg.Url, arg.Error);

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
                            this.Dispatcher.BeginInvoke(() =>
                            {
                                if (j == 0)
                                    p.SendPropertyChanged("FirstImageBrush");

                                img.SendPropertyChanged("ImageBrush");

                                if (0 == (--syncCount))
                                {
                                    ProgressBarPopup.Instance.Close();
                                }
                            });
                        };

                        client.Execute(img.Url, img.Id + "." + img.Url.GetImageFileExtension());
                    }
                }
            }
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