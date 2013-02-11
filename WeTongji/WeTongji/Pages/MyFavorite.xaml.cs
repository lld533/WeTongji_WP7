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
    public partial class MyFavorite : PhoneApplicationPage
    {
        public MyFavorite()
        {
            InitializeComponent();

            this.Loaded += (o, e) =>
            {
                ListBox_Activity.ItemsSource = new object[10];
                ListBox_PeopleOfWeek.ItemsSource = new object[20];
                ListBox_CampusInfo.ItemsSource = new FakeCampusInfoItem[]
                {
                    new FakeCampusInfoItem(){FakeType = FakeCampusInfoItem.FakeCampusInfoType.NearBy},
                    new FakeCampusInfoItem(){FakeType = FakeCampusInfoItem.FakeCampusInfoType.OfficialNote},
                    new FakeCampusInfoItem(){FakeType = FakeCampusInfoItem.FakeCampusInfoType.SocietyNews},
                    new FakeCampusInfoItem(){FakeType = FakeCampusInfoItem.FakeCampusInfoType.SocietyNews},
                    new FakeCampusInfoItem(){FakeType = FakeCampusInfoItem.FakeCampusInfoType.TongjiNews}
                };
            };
        }
        #region [Nav]

        private void ListBox_Activity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lb = sender as ListBox;

            if (lb.SelectedIndex == -1)
                return;

            lb.SelectedIndex = -1;
            this.NavigationService.Navigate(new Uri("/Pages/Activity.xaml", UriKind.RelativeOrAbsolute));
        }

        private void ListBox_PeopleOfWeek_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            var lb = sender as ListBox;

            if (lb.SelectedIndex == -1)
                return;

            lb.SelectedIndex = -1;
            this.NavigationService.Navigate(new Uri("/Pages/PeopleOfWeek.xaml", UriKind.RelativeOrAbsolute));
        }

        private void Listbox_CampusInfo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lb = sender as ListBox;

            if (lb.SelectedIndex == -1)
                return;

            lb.SelectedIndex = -1;

            var item = lb.SelectedItem as FakeCampusInfoItem;
            this.NavigationService.Navigate(new Uri(String.Format("/Pages/{0}.xaml", item.FakeType.ToString()), UriKind.RelativeOrAbsolute));
        }

        #endregion
    }

    public class FakeCampusInfoItem
    {
        public enum FakeCampusInfoType
        {
            TongjiNews,
            NearBy,
            OfficialNote,
            SocietyNews
        }

        public FakeCampusInfoType FakeType
        {
            get;
            set;
        }

        public Boolean IsSocietyNews
        {
            get { return FakeType == FakeCampusInfoType.SocietyNews; }
        }
    }
}