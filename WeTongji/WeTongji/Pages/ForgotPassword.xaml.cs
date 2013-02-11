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
    public partial class ForgotPassword : PhoneApplicationPage
    {
        public ForgotPassword()
        {
            InitializeComponent();
        }

        private void UpdateSendButton(Object sender, TextChangedEventArgs e)
        {
            if (!String.IsNullOrEmpty(TextBox_Id.Text) && !String.IsNullOrEmpty(TextBox_Name.Text))
            {
                var btn = this.ApplicationBar.Buttons[0] as ApplicationBarIconButton;
                btn.IsEnabled = true;
            }
            else
            {
                var btn = this.ApplicationBar.Buttons[0] as ApplicationBarIconButton;
                btn.IsEnabled = false;
            }
        }

        #region [Overridden]

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ThemeManager.ToDarkTheme();
        }

        #endregion
    }
}