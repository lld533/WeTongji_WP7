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
using Microsoft.Phone.Shell;
using System.Windows.Navigation;

namespace WeTongji
{
    public partial class SignUp : PhoneApplicationPage
    {

        #region [Constructor]

        public SignUp()
        {
            InitializeComponent();
        }

        #endregion

        #region [Overridden]

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ThemeManager.ToDarkTheme();
        }

        #endregion

        #region [Properties]

        private ApplicationBarIconButton Button_Send
        {
            get { return this.ApplicationBar.Buttons[0] as ApplicationBarIconButton; }
        }

        #endregion

        #region [Functions]

        #region [Api]

        private void Button_Done_Click(Object sender, EventArgs e)
        {
        }

        #endregion

        #region [Nav]

        private void Button_ViewAgreement_Click(Object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Pages/Agreement.xaml", UriKind.RelativeOrAbsolute));
        }

        #endregion

        #region [Visual]

        private void UpdateSendButton(Object sender, RoutedEventArgs e)
        {
            UpdateSendButtonCore(null, null);
        }

        private void IsCheckedChanged(object sender, RoutedEventArgs e)
        {
            UpdateSendButtonCore(null, null);
        }

        private void UpdateSendButtonCore(Object sender, TextChangedEventArgs e)
        {
            if (!String.IsNullOrEmpty(TextBox_Id.Text) &&
                !String.IsNullOrEmpty(TextBox_Name.Text) &&
                !String.IsNullOrEmpty(PasswordBox_Password.Password) &&
                !String.IsNullOrEmpty(PasswordBox_Confirm.Password) &&
                (Boolean)CheckBox_Agreement.IsChecked)
            {
                Button_Send.IsEnabled = true;
            }
            else
            {
                Button_Send.IsEnabled = false;
            }
        }

        #endregion

        #endregion
    }
}