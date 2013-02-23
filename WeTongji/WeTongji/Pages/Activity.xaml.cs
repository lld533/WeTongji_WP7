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
using WeTongji.Api;
using System.IO.IsolatedStorage;
using WeTongji.Pages;
using WeTongji.Business;

namespace WeTongji
{
    public partial class Activity : PhoneApplicationPage
    {
        public Activity()
        {
            InitializeComponent();
        }

        #region [Overridden]

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ThemeManager.ToDarkTheme();

            var path = e.Uri.ToString();
            path = path.TrimStart("/Pages/Activity.xaml".ToCharArray());

            ActivityExt a = null;

            if (String.IsNullOrEmpty(path))
            {
                using (var db = WTShareDataContext.ShareDB)
                {
                    a = db.Activities.FirstOrDefault();

                    if (a == null)
                        //...Todo @_@ Friendly MsgBox
                        throw new ArgumentNullException("No activity found in database.");
                }
            }
            else
            {
                path = path.Trim("?q=".ToCharArray());

                int id;

                if (!int.TryParse(path, out id))
                    //...Todo @_@ Friendly MsgBox
                    throw new ArgumentOutOfRangeException("Invalid query string");

                using (var db = WTShareDataContext.ShareDB)
                {
                    a = db.Activities.Where((act) => act.Id == id).SingleOrDefault();

                    if (a == null)
                        //...Todo @_@ Friendly MsgBox
                        throw new ArgumentOutOfRangeException(String.Format("Cannot find activity[Id={0}] in database.", id));
                }
            }

            this.DataContext = a;

            WTDispatcher.Instance.Do(() =>
            {
                int imagesDownloading = 0;

                if (!a.OrganizerAvatar.EndsWith("missing.png") && String.IsNullOrEmpty(a.OrganizerAvatarGuid) && !a.AvatarExists())
                {
                    WTDownloadImageClient client = new WTDownloadImageClient();
                    client.DownloadImageStarted += (obj, arg) =>
                        {
                            this.Dispatcher.BeginInvoke(() =>
                            {
                                ++imagesDownloading;
                                ProgressBarPopup.Instance.Open();
                            });
                            System.Diagnostics.Debug.WriteLine("download avatar started: {0}", arg.Url);
                        };
                    client.DownloadImageFailed += (obj, arg) =>
                        {
                            this.Dispatcher.BeginInvoke(() =>
                            {
                                --imagesDownloading;
                                if (0 == imagesDownloading)
                                    ProgressBarPopup.Instance.Close();
                            });

                            System.Diagnostics.Debug.WriteLine("download avatar failed: {0}\nError: {1}", arg.Url, arg.Error);
                        };
                    client.DownloadImageCompleted += (obj, arg) =>
                        {
                            System.Diagnostics.Debug.WriteLine("download completed: {0}", arg.Url);

                            this.Dispatcher.BeginInvoke(() =>
                            {
                                if (String.IsNullOrEmpty(a.OrganizerAvatarGuid))
                                {
                                    a.SaveAvatar(arg.ImageStream);
                                    (this.DataContext as ActivityExt).SendPropertyChanged("OrganizerAvatarImageBrush");
                                }

                                --imagesDownloading;
                                if (0 == imagesDownloading)
                                    ProgressBarPopup.Instance.Close();
                            });
                        };
                    client.Execute(a.OrganizerAvatar);
                }

                //...Current activity is illustrated.
                if (!a.Image.EndsWith("missing.png"))
                {
                    #region [Illustration is in isolated storage folder]

                    if (a.ImageExists())
                    {
                        NoIllustrationHint.Visibility = Visibility.Collapsed;
                    }

                    #endregion
                    #region [Illustration needs downloading from the server]

                    else
                    {
                        WTDownloadImageClient client = new WTDownloadImageClient();
                        client.DownloadImageStarted += (obj, arg) =>
                        {
                            System.Diagnostics.Debug.WriteLine("download image started: {0}", arg.Url);

                            this.Dispatcher.BeginInvoke(() =>
                            {
                                ++imagesDownloading;
                                ProgressBarPopup.Instance.Open();
                            });
                        };
                        client.DownloadImageFailed += (obj, arg) =>
                        {
                            System.Diagnostics.Debug.WriteLine("download image failed: {0}\nError: {1}", arg.Url, arg.Error);

                            this.Dispatcher.BeginInvoke(() =>
                            {
                                --imagesDownloading;
                                if (0 == imagesDownloading)
                                    ProgressBarPopup.Instance.Close();
                            });
                        };
                        client.DownloadImageCompleted += (obj, arg) =>
                        {
                            System.Diagnostics.Debug.WriteLine("download image completed: {0}", arg.Url);

                            if (String.IsNullOrEmpty(a.ImageGuid))
                            {
                                a.SaveImage(arg.ImageStream);
                            }

                            this.Dispatcher.BeginInvoke(() =>
                            {
                                (this.DataContext as ActivityExt).SendPropertyChanged("ActivityImageBrush");
                                Illustration.Visibility = Visibility.Visible;

                                --imagesDownloading;
                                if (0 == imagesDownloading)
                                    ProgressBarPopup.Instance.Close();
                            });
                        };
                        client.Execute(a.Image);
                    }

                    #endregion
                }
                else
                {
                    //...Do nothing.
                }
            });
        }

        #endregion
    }
}