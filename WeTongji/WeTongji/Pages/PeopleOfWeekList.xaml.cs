using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace WeTongji
{
    public partial class PeopleOfWeekList : PhoneApplicationPage
    {
        public PeopleOfWeekList()
        {
            InitializeComponent();

            this.Loaded += (o, e) =>
            {
                ListBox_Core.ItemsSource = new object[10];
            };
        }

        private void ListBox_Core_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            var lb = sender as ListBox;

            if(lb.SelectedIndex == -1)
                return;

            lb.SelectedIndex = -1;
            this.NavigationService.Navigate(new Uri("/Pages/PeopleOfWeek.xaml", UriKind.RelativeOrAbsolute));
        }
    }
}