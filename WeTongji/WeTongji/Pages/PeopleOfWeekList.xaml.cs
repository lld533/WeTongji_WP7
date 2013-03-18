using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using WeTongji.DataBase;
using WeTongji.Api.Domain;
using System.Collections.ObjectModel;
using WeTongji.Business;
using WeTongji.Api;
using System.Diagnostics;
using WeTongji.Utility;

namespace WeTongji
{
    public partial class PeopleOfWeekList : PhoneApplicationPage
    {
        public PeopleOfWeekList()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Load PeopleOfWeek Table from database when navigate to this page for the first time.
        /// Otherwise, check for not loaded person and reload data if it needs.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            #region [First time]

            if (e.NavigationMode == NavigationMode.New)
            {
                PersonExt[] people = null;

                using (var db = WTShareDataContext.ShareDB)
                {
                    var q = from PersonExt p in db.People
                            orderby p.Id descending
                            select p;
                    people = q.ToArray();
                }

                this.Dispatcher.BeginInvoke(() =>
                {
                    ListBox_Core.ItemsSource = people;
                });
            }

            #endregion
            #region [Otherwise]

            else
            {
                var src = ListBox_Core.ItemsSource as IEnumerable<PersonExt>;

                // Any person in db has not been loaded.
                bool flag = false;

                PersonExt[] people = null;
                using (var db = WTShareDataContext.ShareDB)
                {
                    if (src != null && src.Count() > 0)
                    {
                        people = db.People.ToArray();
                    }
                }

                if (people != null)
                    flag = (from PersonExt p in people
                            where src.Where((person) => person.Id == p.Id).SingleOrDefault() == null
                            select p).Count() > 0;

                //...Reload data if there is at least one person not inserted to the ListBox.
                if (flag)
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        using (var db = WTShareDataContext.ShareDB)
                        {
                            var q = from PersonExt p in db.People
                                    orderby p.Id descending
                                    select p;
                            ListBox_Core.ItemsSource = q.ToArray();
                        }
                    });
                }
            }

            #endregion

            DownloadUnStoredAvatars();
        }

        private void DownloadUnStoredAvatars()
        {
            var people = ListBox_Core.ItemsSource as IEnumerable<PersonExt>;
            if (people != null)
            {
                int count = people.Count();
                for (int i = 0; i < count; ++i)
                {
                    var p = people.ElementAt(i);

                    if (!p.Avatar.EndsWith("missing.png") && !p.AvatarExists())
                    {
                        var client = new WTDownloadImageClient();

                        client.DownloadImageStarted += (obj, arg) =>
                        {
                            Debug.WriteLine("Download person's avatar started: {0}", arg.Url);
                        };

                        client.DownloadImageFailed += (obj, arg) =>
                        {
                            Debug.WriteLine("Download person's avatar FAILED: {0}\nErr: {1}", arg.Url, arg.Error);
                        };

                        client.DownloadImageCompleted += (obj, arg) =>
                        {
                            Debug.WriteLine("Download person's avatar completed: {0}", arg.Url);

                            this.Dispatcher.BeginInvoke(() =>
                            {
                                p.SendPropertyChanged("AvatarImageBrush");
                            });
                        };

                        client.Execute(p.Avatar, p.AvatarGuid + "." + p.Avatar.GetImageFileExtension());
                    }
                }
            }
        }

        private void ListBox_Core_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            var lb = sender as ListBox;

            if (lb.SelectedIndex == -1)
                return;

            var p = lb.SelectedItem as PersonExt;
            lb.SelectedIndex = -1;
            if (p != null)
                this.NavigationService.Navigate(new Uri(String.Format("/Pages/PeopleOfWeek.xaml?q={0}", p.Id), UriKind.RelativeOrAbsolute));
        }
    }
}