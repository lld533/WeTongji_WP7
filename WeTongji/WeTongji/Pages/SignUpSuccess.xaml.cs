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
    public partial class SignUpSuccess : PhoneApplicationPage
    {
        public SignUpSuccess()
        {
            InitializeComponent();

            var btn = new ApplicationBarIconButton(new Uri("/icons/appbar.check.rest.png", UriKind.RelativeOrAbsolute))
                {
                    Text = StringLibrary.SignUpSuccess_AppBarFinishSigningUpText
                };
            btn.Click += NavBackToMainPage;
            this.ApplicationBar.Buttons.Add(btn);
            
        }

        private void NavBackToMainPage(Object sender, EventArgs e)
        {
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.RemoveBackEntry();
            this.NavigationService.GoBack();
        }

        private void BrowseTongjiMail(Object sender, RoutedEventArgs e)
        {
            var task = new Microsoft.Phone.Tasks.WebBrowserTask();
            task.Uri = new Uri("http://mail.tongji.edu.cn");
            task.Show();
        }
    }
}