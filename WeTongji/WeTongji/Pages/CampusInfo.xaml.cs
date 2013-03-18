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
using WeTongji.Utility;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace WeTongji
{
    public partial class CampusInfo : PhoneApplicationPage
    {
        private ObservableCollection<SchoolNewsExt> SchoolNewsSource
        {
            get
            {
                return ListBox_TongjiNews.ItemsSource == null
                    ? null
                    : ListBox_TongjiNews.ItemsSource as ObservableCollection<SchoolNewsExt>;
            }
            set
            {
                if (value == null || value.Count == 0)
                {
                    ListBox_TongjiNews.ItemsSource = null;
                    TextBlock_NoTongjiNews.Visibility = Visibility.Visible;

                    SchoolNewsSource.CollectionChanged -= SchoolNewsSourceChanged;
                }
                else
                {
                    if (SchoolNewsSource != null)
                        SchoolNewsSource.CollectionChanged -= SchoolNewsSourceChanged;

                    ObservableCollection<SchoolNewsExt> tmp = null;
                    ListBox_TongjiNews.ItemsSource = tmp = new ObservableCollection<SchoolNewsExt>();

                    tmp.CollectionChanged += SchoolNewsSourceChanged;
                    foreach (var item in value)
                        tmp.Add(item);

                    ListBox_TongjiNews.Visibility = Visibility.Visible;
                    TextBlock_NoTongjiNews.Visibility = Visibility.Collapsed;
                }
            }
        }

        private ObservableCollection<AroundExt> AroundNewsSource
        {
            get
            {
                return ListBox_NearBy.ItemsSource == null
                    ? null
                    : ListBox_NearBy.ItemsSource as ObservableCollection<AroundExt>;
            }
            set
            {
                if (value == null || value.Count == 0)
                {
                    ListBox_NearBy.ItemsSource = null;
                    TextBlock_NoAroundNews.Visibility = Visibility.Visible;

                    AroundNewsSource.CollectionChanged -= AroundNewsSourceChanged;
                }
                else
                {
                    if (AroundNewsSource != null)
                        AroundNewsSource.CollectionChanged -= AroundNewsSourceChanged;

                    ObservableCollection<AroundExt> tmp = null;
                    ListBox_NearBy.ItemsSource = tmp = new ObservableCollection<AroundExt>();

                    tmp.CollectionChanged += AroundNewsSourceChanged;
                    foreach (var item in value)
                        tmp.Add(item);

                    ListBox_NearBy.Visibility = Visibility.Visible;
                    TextBlock_NoAroundNews.Visibility = Visibility.Collapsed;
                }
            }
        }

        private ObservableCollection<ForStaffExt> OfficialNotesSource
        {
            get
            {
                return ListBox_OfficialNotes.ItemsSource == null
                    ? null
                    : ListBox_OfficialNotes.ItemsSource as ObservableCollection<ForStaffExt>;
            }
            set
            {
                if (value == null || value.Count == 0)
                {
                    ListBox_OfficialNotes.ItemsSource = null;
                    TextBlock_NoOfficialNote.Visibility = Visibility.Visible;

                    OfficialNotesSource.CollectionChanged -= OfficalNoteSourceChanged;
                }
                else
                {
                    if (OfficialNotesSource != null)
                        OfficialNotesSource.CollectionChanged -= OfficalNoteSourceChanged;

                    ObservableCollection<ForStaffExt> tmp = null;
                    ListBox_OfficialNotes.ItemsSource = tmp = new ObservableCollection<ForStaffExt>();

                    tmp.CollectionChanged += OfficalNoteSourceChanged;
                    foreach (var item in value)
                        tmp.Add(item);

                    ListBox_OfficialNotes.Visibility = Visibility.Visible;
                    TextBlock_NoOfficialNote.Visibility = Visibility.Collapsed;
                }
            }
        }

        private ObservableCollection<ClubNewsExt> ClubNewsSource
        {
            get
            {
                return ListBox_SocietyNews.ItemsSource == null
                    ? null
                    : ListBox_SocietyNews.ItemsSource as ObservableCollection<ClubNewsExt>;
            }
            set
            {
                if (value == null || value.Count == 0)
                {
                    ListBox_SocietyNews.ItemsSource = null;
                    TextBlock_NoClubNews.Visibility = Visibility.Visible;

                    ClubNewsSource.CollectionChanged -= ClubNewsSourceChanged;
                }
                else
                {
                    if (ClubNewsSource != null)
                        ClubNewsSource.CollectionChanged -= ClubNewsSourceChanged;

                    ObservableCollection<ClubNewsExt> tmp = null;
                    ListBox_SocietyNews.ItemsSource = tmp = new ObservableCollection<ClubNewsExt>();

                    tmp.CollectionChanged += ClubNewsSourceChanged;
                    foreach (var item in value)
                        tmp.Add(item);


                    ListBox_SocietyNews.Visibility = Visibility.Visible;
                    TextBlock_NoClubNews.Visibility = Visibility.Collapsed;
                }
            }
        }

        public CampusInfo()
        {
            InitializeComponent();
        }

        private void SchoolNewsSourceChanged(Object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in e.NewItems)
            {
                var sn = item as SchoolNewsExt;

                if (!sn.IsInvalidSchoolNews && !String.IsNullOrEmpty(sn.ImageExtList) && !sn.ImageExists())
                {
                    var client = new WTDownloadImageClient();

                    client.DownloadImageCompleted += (obj, arg) =>
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            sn.SendPropertyChanged("FirstImageBrush");
                        });
                    };

                    client.Execute(sn.GetImagesURL().First(), sn.ImageExtList.GetImageFilesNames().First());
                }
            }
        }
        private void AroundNewsSourceChanged(Object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in e.NewItems)
            {
                var an = item as AroundExt;

                if (!an.IsInvalidAround && !String.IsNullOrEmpty(an.TitleImage) && !an.IsTitleImageExists())
                {
                    var client = new WTDownloadImageClient();

                    client.DownloadImageCompleted += (obj, arg) =>
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            an.SendPropertyChanged("TitleImageBrush");
                        });
                    };

                    client.Execute(an.TitleImage, an.TitleImageGuid + "." + an.TitleImage.GetImageFileExtension());
                }
            }
        }
        private void OfficalNoteSourceChanged(Object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in e.NewItems)
            {
                var fs = item as ForStaffExt;

                if (!fs.IsInvalidForStaff && !String.IsNullOrEmpty(fs.ImageExtList) && !fs.ImageExists())
                {
                    var client = new WTDownloadImageClient();

                    client.DownloadImageCompleted += (obj, arg) =>
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            fs.SendPropertyChanged("FirstImageBrush");
                        });
                    };

                    client.Execute(fs.GetImagesURL().First(), fs.ImageExtList.GetImageFilesNames().First());
                }
            }
        }
        private void ClubNewsSourceChanged(Object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in e.NewItems)
            {
                var cn = item as ClubNewsExt;

                if (!cn.IsInvalidClubNews && !String.IsNullOrEmpty(cn.ImageExtList) && !cn.ImageExists())
                {
                    var client = new WTDownloadImageClient();

                    client.DownloadImageCompleted += (obj, arg) =>
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            cn.SendPropertyChanged("FirstImageBrush");
                        });
                    };

                    client.Execute(cn.GetImagesURL().First(), cn.ImageExtList.GetImageFilesNames().First());
                }
            }
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
                idx = idx > Pivot_Core.Items.Count ? 0 : idx;

                Pivot_Core.SelectedIndex = idx;

                var thread = new System.Threading.Thread(new System.Threading.ThreadStart(LoadData));
                thread.Start();
            }
        }

        private void LoadData()
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                ProgressBarPopup.Instance.Open();
            });

            #region [School News]
            SchoolNewsExt[] snArr = null;
            using (var db = WTShareDataContext.ShareDB)
            {
                snArr = db.SchoolNewsTable.ToArray();
            }
            if (snArr != null && snArr.Count() > 0)
            {
                var src = snArr.OrderByDescending((news) => news.CreatedAt);

                this.Dispatcher.BeginInvoke(() =>
                {
                    if (src.Count() > 5)
                    {
                        var tmp = new ObservableCollection<SchoolNewsExt>(src.Take(5));
                        tmp.Add(SchoolNewsExt.InvalidSchoolNews());
                        SchoolNewsSource = tmp;
                    }
                    else
                    {
                        SchoolNewsSource = new ObservableCollection<SchoolNewsExt>(src);
                    }
                });
            }
            else
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    SchoolNewsSource = null;
                });
            }
            #endregion

            #region [Around News]
            AroundExt[] anArr = null;
            using (var db = WTShareDataContext.ShareDB)
            {
                anArr = db.AroundTable.ToArray();
            }
            if (anArr != null && anArr.Count() > 0)
            {
                var src = anArr.OrderByDescending((news) => news.CreatedAt);

                this.Dispatcher.BeginInvoke(() =>
                {
                    if (anArr.Count() > 5)
                    {
                        var tmp = new ObservableCollection<AroundExt>(src.Take(5));
                        tmp.Add(AroundExt.InvalidAround());
                        AroundNewsSource = tmp;
                    }
                    else
                    {
                        AroundNewsSource = new ObservableCollection<AroundExt>(src);
                    }
                });
            }
            else
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    AroundNewsSource = null;
                });
            }
            #endregion

            #region [Official Notes]
            ForStaffExt[] fsArr = null;
            using (var db = WTShareDataContext.ShareDB)
            {
                fsArr = db.ForStaffTable.ToArray();
            }
            if (fsArr != null && fsArr.Count() > 0)
            {
                var src = fsArr.OrderByDescending((news) => news.CreatedAt);

                this.Dispatcher.BeginInvoke(() =>
                {
                    if (fsArr.Count() > 5)
                    {
                        var tmp = new ObservableCollection<ForStaffExt>(src.Take(5));
                        tmp.Add(ForStaffExt.InvalidForStaff());
                        OfficialNotesSource = tmp;
                    }
                    else
                    {
                        OfficialNotesSource = new ObservableCollection<ForStaffExt>(src);
                    }
                });
            }
            else
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    OfficialNotesSource = null;
                });
            }
            #endregion

            #region [Club News]
            ClubNewsExt[] cnArr = null;
            using (var db = WTShareDataContext.ShareDB)
            {
                cnArr = db.ClubNewsTable.ToArray();
            }
            if (cnArr != null && cnArr.Count() > 0)
            {
                var src = cnArr.OrderByDescending((news) => news.CreatedAt);

                this.Dispatcher.BeginInvoke(() =>
                {
                    if (cnArr.Count() > 5)
                    {
                        var tmp = new ObservableCollection<ClubNewsExt>(src.Take(5));
                        tmp.Add(ClubNewsExt.InvalidClubNews());
                        ClubNewsSource = tmp;
                    }
                    else
                    {
                        ClubNewsSource = new ObservableCollection<ClubNewsExt>(src);
                    }
                });
            }
            else
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    ClubNewsSource = null;
                });
            }
            #endregion

            this.Dispatcher.BeginInvoke(() =>
            {
                ProgressBarPopup.Instance.Close();
            });
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
    }
}