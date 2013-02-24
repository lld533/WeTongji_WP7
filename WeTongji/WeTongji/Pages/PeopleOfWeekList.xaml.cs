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

namespace WeTongji
{
    public partial class PeopleOfWeekList : PhoneApplicationPage
    {
        public PeopleOfWeekList()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            #region [Update items source]

            if (e.NavigationMode == NavigationMode.New)
            {
                WTDispatcher.Instance.Do(() =>
                {
                    PersonExt[] people = null;

                    using (var store = WTShareDataContext.ShareDB)
                    {
                        var q = from PersonExt p in store.People
                                orderby p.NO descending
                                select p;
                        people = q.ToArray();
                    }

                    this.Dispatcher.BeginInvoke(() =>
                    {
                        ListBox_Core.ItemsSource = people;
                    });

                    #region [Download unstored Avatar images]
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

                                p.SaveAvatar(arg.ImageStream);

                                this.Dispatcher.BeginInvoke(() =>
                                {
                                    p.SendPropertyChanged("AvatarImageBrush");
                                });
                            };

                            client.Execute(p.Avatar);
                        }
                    }
                    #endregion
                });
            }
            else
            {
                WTDispatcher.Instance.Do(() =>
                {
                    using (var db = WTShareDataContext.ShareDB)
                    {
                        var people = ListBox_Core.ItemsSource as IEnumerable<PersonExt>;
                        var dbPeople = db.People.ToArray();
                        var q = from PersonExt pdb in dbPeople
                                where people.Where( (p)=>p.Id == pdb.Id).SingleOrDefault() == null
                                select pdb;

                        if (q != null && q.Count() > 0)
                        {
                            this.Dispatcher.BeginInvoke(() =>
                            {
                                ListBox_Core.ItemsSource = people;
                            });

                            #region [Download unstored Avatar images]
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

                                        p.SaveAvatar(arg.ImageStream);

                                        this.Dispatcher.BeginInvoke(() =>
                                        {
                                            p.SendPropertyChanged("AvatarImageBrush");
                                        });
                                    };

                                    client.Execute(p.Avatar);
                                }
                            }
                            #endregion
                        }
                    }
                });
            }

            #endregion

            #region [Update Avatar]

            #endregion
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