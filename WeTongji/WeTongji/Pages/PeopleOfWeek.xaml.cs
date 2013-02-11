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

namespace WeTongji
{
    public partial class PeopleOfWeek : PhoneApplicationPage
    {
        public PeopleOfWeek()
        {
            InitializeComponent();
            this.Loaded += (o, e) => 
            {
                ListBox_Pic.ItemsSource = new int[5];
            };
        }

        #region [Overridden]

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ThemeManager.ToDarkTheme();
        }

        #endregion

        private void NavToHistory(Object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Pages/PeopleOfWeekList.xaml", UriKind.RelativeOrAbsolute));
        }
    }
}