using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using WeTongji.Utility;

namespace WeTongji
{
    public partial class About : PhoneApplicationPage
    {
        public About()
        {
            InitializeComponent();

            this.Loaded += (o, e) =>
                {
                    Run_Version.Text = AppVersion.Current;
                };
        }

        private void Button_ViewAgreement_Click(Object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Pages/Agreement.xaml", UriKind.RelativeOrAbsolute));
        }

        private void Button_Rate_Click(Object sender, RoutedEventArgs e)
        {
            var task = new Microsoft.Phone.Tasks.MarketplaceReviewTask();
            task.Show();
        }

        private void Button_FeedBack_Click(Object sender, RoutedEventArgs e)
        {
            var task = new Microsoft.Phone.Tasks.EmailComposeTask();
            var version = AppVersion.Current;

            task.Body = String.Format("我正在使用微同济Windows Phone v{0}，", version);
            task.Subject = String.Format("[用户反馈]微同济Windows Phone v{0}", version);
            task.To = "we@tongji.edu.cn";
            task.Show();
        }
    }
}