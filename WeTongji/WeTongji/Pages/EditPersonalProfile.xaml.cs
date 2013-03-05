using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using WeTongji.Api.Domain;
using WeTongji.DataBase;
using WeTongji.Business;
using WeTongji.Pages;
using System.Threading;
using System.Reflection;
using WeTongji.Api.Request;
using WeTongji.Api;
using System.Diagnostics;

namespace WeTongji
{
    public partial class EditPersonalProfile : PhoneApplicationPage
    {
        private UserExt copy = null;

        public EditPersonalProfile()
        {
            InitializeComponent();

            this.Loaded += (o, e) =>
                {
                    var thread = new Thread(new ThreadStart(LoadData))
                    {
                        IsBackground = true,
                        Name = "LoadData"
                    };

                    ProgressBarPopup.Instance.Open();
                    thread.Start();
                };
        }
        
        /// <summary>
        /// Select all text if the text of is not empty
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxGotFocus(Object sender, RoutedEventArgs e)
        {
            #region [Check argument]

            if (sender == null)
                throw new ArgumentNullException("sender");

            var txtbx = sender as TextBox;

            if (txtbx == null)
                throw new ArgumentOutOfRangeException("sender");

            #endregion

            if (!String.IsNullOrEmpty(txtbx.Text))
            {
                txtbx.SelectAll();
            }
        }

        private void TextBoxTextChanged(Object sender, RoutedEventArgs e)
        {
            var txtbx = sender as TextBox;

            if (txtbx == null)
                return;

            var user = this.DataContext as UserExt;
            var bindingExpression = txtbx.GetBindingExpression(TextBox.TextProperty);
            var propertyInfo = typeof(UserExt).GetProperty(bindingExpression.ParentBinding.Path.Path);

            propertyInfo.SetValue(user, txtbx.Text, null);
        }

        private void SaveButtonClicked(object sender, EventArgs e)
        {
            this.Focus();

            var newValue = this.DataContext as UserExt;

            if (newValue.Phone == copy.Phone && newValue.Email == copy.Email
                && newValue.QQ == copy.QQ && newValue.SinaWeibo == copy.SinaWeibo)
            {
                this.NavigationService.Navigate(new Uri("/Pages/PersonalProfile.xaml", UriKind.RelativeOrAbsolute));
            }
            else
            {
                var btn = sender as ApplicationBarIconButton;
                btn.IsEnabled = false;

                var thread = new Thread(new ParameterizedThreadStart(SaveData))
                {
                    IsBackground = true,
                    Name = "SaveData"
                };

                ProgressBarPopup.Instance.Open();
                thread.Start(newValue);
            }
        }

        private void LoadData()
        {
            UserExt user = null;

            using (var db = new WTUserDataContext(Global.Instance.Settings.UID))
            {
                user = db.UserInfo.SingleOrDefault();
            }

            this.Dispatcher.BeginInvoke(() =>
            {
                this.copy = user.Clone();
                this.DataContext = user;

                ProgressBarPopup.Instance.Close();
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param">new user's value, typeof(UserExt)</param>
        private void SaveData(Object param)
        {
            var req = new UserUpdateRequest<WTResponse>();
            var client = new WTDefaultClient<WTResponse>();

            Debug.WriteLine((param as UserExt).Avatar);
            req.User = (param as UserExt).GetObject() as User;

            client.ExecuteFailed += (o, e) =>
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        ProgressBarPopup.Instance.Close();
                        (this.ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = true;

                        MessageBox.Show("保存资料失败，请重试");
                    });
                };

            client.ExecuteCompleted += (o, e) =>
                {
                    using (var db = new WTUserDataContext(Global.Instance.Settings.UID))
                    {
                        var userExt = db.UserInfo.SingleOrDefault();
                        var user = req.User;

                        userExt.Email = user.Email;
                        userExt.Phone = user.Phone;
                        userExt.QQ = user.QQ;
                        userExt.SinaWeibo = user.SinaWeibo;

                        db.SubmitChanges();
                    }

                    this.Dispatcher.BeginInvoke(() =>
                    {
                        ProgressBarPopup.Instance.Close();
                        (this.ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = true;

                        this.NavigationService.Navigate(new Uri("/Pages/PersonalProfile.xaml", UriKind.RelativeOrAbsolute));
                    });
                };

            client.Post(req, Global.Instance.Session, Global.Instance.Settings.UID);
        }
    }
}