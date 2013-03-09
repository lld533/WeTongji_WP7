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
using WeTongji.Api.Request;
using WeTongji.Api.Response;
using WeTongji.Api;

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
            var req = new UserActiveRequest<WTResponse>();
            var client = new WTDefaultClient<WTResponse>();

            req.NO = TextBox_Id.Text;
            req.Name = TextBox_Name.Text;
            req.Password = PasswordBox_Password.Password;

            client.ExecuteCompleted += (obj, args) =>
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("请登录", "注册成功", MessageBoxButton.OK);
                    this.NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
                });
            };

            client.ExecuteFailed += (obj, args) =>
                {
                    if (args.Error is WTException)
                    {
                        var err = args.Error as WTException;
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            switch (err.StatusCode.Id)
                            {
                                case Api.Util.Status.AlreadyRegistered:
                                    MessageBox.Show("账号已经注册,请登录");
                                    this.NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
                                    return;
                                case Api.Util.Status.NoAccount:
                                    MessageBox.Show("您输入的账号不存在，请检查后重试");
                                    return;
                                case Api.Util.Status.IdNameDismatch:
                                    MessageBox.Show("学号与姓名不符，请检查后重试");
                                    TextBox_Id.Focus();
                                    TextBox_Id.SelectAll();
                                    return;
                                case Api.Util.Status.InvalidParameter:
                                    MessageBox.Show("注册信息有误，请检查后重试");
                                    TextBox_Id.Focus();
                                    TextBox_Id.SelectAll();
                                    return;
                                default:
                                    MessageBox.Show("注册失败，请重试");
                                    return;
                            }
                        });

                    }
                    else if (args.Error is System.Net.WebException)
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            MessageBox.Show("注册失败，请检查Wifi或网络连接后重试");
                        });
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            MessageBox.Show("注册失败，请重试");
                        });
                    }
                };


            client.Execute(req);
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