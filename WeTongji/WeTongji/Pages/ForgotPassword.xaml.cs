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

        private void Button_Send_Click(Object sender, EventArgs e)
        {
            //...Hide Software Input Panel
            this.Focus();

            var req = new UserResetPassword<WTResponse>();
            var client = new WTDefaultClient<WTResponse>();

            req.NO = TextBox_Id.Text;
            req.Name = TextBox_Name.Text;

            client.ExecuteCompleted += (obj, arg) =>
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        var result = MessageBox.Show("前往您的同济邮箱查看邮件，并且点击充值密码链接。", "重置密码成功", MessageBoxButton.OKCancel);
                        if (result == MessageBoxResult.OK)
                        {
                            var task = new Microsoft.Phone.Tasks.WebBrowserTask();
                            task.Uri = new Uri("http://mail.tongji.edu.cn");
                            task.Show();
                        }
                    });
                };

            client.ExecuteFailed += (obj, arg) =>
                {
                    if (arg.Error is System.Net.WebException)
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            MessageBox.Show("重置密码失败，请检查Wifi或网络连接。", "提示", MessageBoxButton.OK);
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
                                case Api.Util.Status.NoAccount:
                                    {
                                        MessageBox.Show("用户不存在，请检查后重试。", "提示", MessageBoxButton.OK);
                                        TextBox_Id.Focus();
                                        TextBox_Id.SelectAll();
                                    }
                                    break;
                                case Api.Util.Status.NotActivatedAccount:
                                    {
                                        MessageBox.Show("该用户未激活，请检查后重试。", "提示", MessageBoxButton.OK);
                                        TextBox_Id.Focus();
                                        TextBox_Id.SelectAll();
                                    }
                                    break;
                                case Api.Util.Status.NotRegistered:
                                    {
                                        MessageBox.Show("该用户未注册，请检查后重试。", "提示", MessageBoxButton.OK);
                                        TextBox_Id.Focus();
                                        TextBox_Id.SelectAll();
                                    }
                                    break;
                                case Api.Util.Status.IdNameDismatch:
                                    {
                                        MessageBox.Show("姓名与学号不符，请检查后重试。", "提示", MessageBoxButton.OK);
                                        TextBox_Name.Focus();
                                        TextBox_Name.SelectAll();
                                    }
                                    break;
                                default:
                                    MessageBox.Show("重置密码失败，请重试。", "提示", MessageBoxButton.OK);
                                    break;
                            }
                        });
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            MessageBox.Show("重置密码失败，请重试。", "提示", MessageBoxButton.OK);
                        });
                    }
                };

            client.Execute(req);
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