using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using WeTongji.Api.Request;
using WeTongji.Api.Response;
using WeTongji.Api;
using WeTongji.Business;

namespace WeTongji
{
    public partial class UpdatePassword : PhoneApplicationPage
    {
        public UpdatePassword()
        {
            InitializeComponent();
        }

        private void UpdateSendButton(Object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(PasswordBox_Old.Password) && !String.IsNullOrEmpty(PasswordBox_New.Password) && !String.IsNullOrEmpty(PasswordBox_Repeat.Password))
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

        private void Button_Send_Click(Object sender, EventArgs e)
        {
            //...Hide Software Input Panel
            this.Focus();

            if (PasswordBox_New.Password != PasswordBox_Repeat.Password)
            {
                MessageBox.Show("您输入的两次密码不相同，请重试", "提示", MessageBoxButton.OK);
                PasswordBox_Repeat.Focus();
                PasswordBox_Repeat.SelectAll();
                return;
            }


            var req = new UserUpdatePasswordRequest<UserUpdatePasswordResponse>();
            var client = new WTDefaultClient<UserUpdatePasswordResponse>();

            req.OldPassword = PasswordBox_Old.Password;
            req.NewPassword = PasswordBox_New.Password;

            client.ExecuteCompleted += (obj, arg) =>
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        Global.Instance.UpdateSettings(Global.Instance.Settings.UID, req.NewPassword, arg.Result.Session);
                        var result = MessageBox.Show("恭喜，已经成功修改密码~", "修改成功", MessageBoxButton.OK);
                        this.NavigationService.GoBack();
                    });
                };

            client.ExecuteFailed += (obj, arg) =>
                {
                    if (arg.Error is System.Net.WebException)
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            WTToast.Instance.Show("网络异常，请稍后再试");
                            this.NavigationService.GoBack();
                        });
                    }
                    else if (arg.Error is WTException)
                    {
                        var ex = arg.Error as WTException;
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            switch (ex.StatusCode.Id)
                            {
                                case Api.Util.Status.InvalidPassword:
                                    {
                                        MessageBox.Show("旧密码输入错误，请检查后重试。", "提示", MessageBoxButton.OK);
                                        PasswordBox_Old.Focus();
                                        PasswordBox_Old.SelectAll();
                                    }
                                    break;
                                case Api.Util.Status.NoAuth:
                                    {
                                        MessageBox.Show("您是不是在其他客户端中登录？请重新登录。", "提示", MessageBoxButton.OK);
                                        this.NavigationService.RemoveBackEntry();
                                        this.NavigationService.GoBack();
                                    }
                                    break;
                                default:
                                    MessageBox.Show("修改密码失败，请重试。", "提示", MessageBoxButton.OK);
                                    break;
                            }
                        });
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            MessageBox.Show("修改密码失败，请重试。", "提示", MessageBoxButton.OK);
                        });
                    }
                };

            client.Execute(req, Global.Instance.Session, Global.Instance.Settings.UID);
        }

        private void EnableScrolling(Object sender, RoutedEventArgs e)
        {
            CoreScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
        }

        private void DisableScrolling(Object sender, RoutedEventArgs e)
        {
            CoreScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
        }
    }
}