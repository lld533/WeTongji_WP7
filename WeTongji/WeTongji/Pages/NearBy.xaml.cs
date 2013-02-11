using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace WeTongji
{
    public partial class NearBy : PhoneApplicationPage
    {
        public NearBy()
        {
            InitializeComponent();
        }

        #region [Overridden]

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ThemeManager.ToDarkTheme();
        }

        #endregion

        #region [Functions]

        private void ViewMapAddress(object sender, RoutedEventArgs e)
        {
#if VIEW_IN_BROWSER
            //WebBrowserTask wbt = new WebBrowserTask();
            //wbt.Uri = new Uri("http://maps.google.com/?q=" + HttpUtility.UrlEncode((sender as Button).Content.ToString()));
            //wbt.Show();
#endif
            this.NavigationService.Navigate(new Uri("/Pages/MapAddress.xaml?q=" + (sender as Button).Content.ToString(), UriKind.RelativeOrAbsolute));
        }

        private void MakePhoneCall(Object sender, RoutedEventArgs e)
        {
            var pct = new PhoneCallTask();
            pct.DisplayName = TextBlock_Title.Text;
            pct.PhoneNumber = (sender as Button).Content.ToString();
            pct.Show();
        }

        #endregion
    }
}