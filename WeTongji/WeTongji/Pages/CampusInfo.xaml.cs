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

namespace WeTongji
{
    public partial class CampusInfo : PhoneApplicationPage
    {
        public CampusInfo()
        {
            InitializeComponent();

            this.Loaded += (o, e) =>
            {
                ListBox_TongjiNews.ItemsSource = new Boolean[] 
                { 
                    true,false,true,true, false, false
                };

                ListBox_NearBy.ItemsSource = new bool[] { false, true, false, false, true, true };

                ListBox_OfficialNotes.ItemsSource = new Boolean[] 
                { 
                    true,false,true,true, false, false
                };

                ListBox_SocietyNews.ItemsSource = new bool[] { true, false, true, true, false, false };
            };
        }

        #region [Overridden]

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// [Query] like /Pages/CampusInfo.xaml?q={Int32}
        /// </remarks>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode != NavigationMode.Back)
            {
                var str = e.Uri.ToString();
                var q = str.Substring(str.IndexOf('?'));
                q = q.TrimStart("?q=".ToCharArray());

                Int32 idx = 0;
                Int32.TryParse(q, out idx);
                idx = Math.Max(idx,0);
                idx = idx > (Int32)CampusInfoType.SocietyNews? 0 : idx;
                

                Pivot_Core.SelectedIndex = idx;
            }
        }

        #endregion

        #region [Nav]

        private void Listbox_TongjiNews_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lb = sender as ListBox;

            if (lb.SelectedIndex == -1)
                return;

            lb.SelectedIndex = -1;
            this.NavigationService.Navigate(new Uri("/Pages/TongjiNews.xaml", UriKind.RelativeOrAbsolute));
        }

        private void ListBox_NearBy_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            var lb = sender as ListBox;

            if (lb.SelectedIndex == -1)
                return;

            lb.SelectedIndex = -1;
            this.NavigationService.Navigate(new Uri("/Pages/NearBy.xaml", UriKind.RelativeOrAbsolute));
        }

        private void Listbox_OfficialNotes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lb = sender as ListBox;

            if (lb.SelectedIndex == -1)
                return;

            lb.SelectedIndex = -1;
            this.NavigationService.Navigate(new Uri("/Pages/OfficialNote.xaml", UriKind.RelativeOrAbsolute));
        }

        private void ListBox_SocietyNews_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lb = sender as ListBox;

            if (lb.SelectedIndex == -1)
                return;

            lb.SelectedIndex = -1;
            this.NavigationService.Navigate(new Uri("/Pages/SocietyNews.xaml", UriKind.RelativeOrAbsolute));
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