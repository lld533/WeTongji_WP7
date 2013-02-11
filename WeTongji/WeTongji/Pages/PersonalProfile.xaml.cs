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
    public partial class PersonalProfile : PhoneApplicationPage
    {
        public PersonalProfile()
        {
            InitializeComponent();
        }

        private void EditPersonalProfile(Object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Pages/EditPersonalProfile.xaml", UriKind.RelativeOrAbsolute));
        }
    }
}