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
using System.Windows.Media.Imaging;
using System.IO;

namespace WeTongji
{
    public partial class EditPersonalProfile : PhoneApplicationPage
    {
        private UserExt copy = null;
        private Boolean isAvatarChanged = false;
        private Boolean isProfileChanged = false;

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

            isProfileChanged = true;
        }

        private void SaveButtonClicked(object sender, EventArgs e)
        {
            this.Focus();

            UserExt current = this.DataContext as UserExt;

            if (!isAvatarChanged && current.Phone == copy.Phone && current.Email == copy.Email
                && current.QQ == copy.QQ && current.SinaWeibo == copy.SinaWeibo)
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

                var newValue = new DataWrapper();

                if (isProfileChanged)
                    newValue.User = current;

                var req = new UserUpdateAvatarRequest<WTResponse>();
                var client = new WTDefaultClient<WTResponse>();

                var wb = new WriteableBitmap(this.Image_Avatar.Source as BitmapSource);

                MemoryStream stream = new MemoryStream();
                wb.SaveJpeg(stream, wb.PixelWidth, wb.PixelHeight, 0, 100);
                req.JpegPhotoStream = stream;

                client.ExecuteFailed += (obj, arg) =>
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        ProgressBarPopup.Instance.Close();
                        (this.ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = true;

                        if (arg.Error is System.Net.WebException)
                            WTToast.Instance.Show("网络异常，请稍后再试");
                        else
                            MessageBox.Show("更新头像失败，请重试", "提示", MessageBoxButton.OK);
                    });
                };

                client.ExecuteCompleted += (obj, arg) =>
                {
                    //...Get current user
                };

                client.Post(req, Global.Instance.Session, Global.Instance.Settings.UID);

                ProgressBarPopup.Instance.Open();
                thread.Start(newValue);
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (isAvatarChanged || isProfileChanged)
            {
                var result = MessageBox.Show("资料已修改，是否放弃修改并返回？", "提示", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.Cancel)
                    e.Cancel = true;
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
            var data = param as UserExt;

            #region [Basic info]

            if (data != null)
            {
                var req = new UserUpdateRequest<WTResponse>();
                var client = new WTDefaultClient<WTResponse>();

                req.User = data.GetObject() as User;

                client.ExecuteFailed += (o, e) =>
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            ProgressBarPopup.Instance.Close();
                            (this.ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = true;

                            if (e.Error is System.Net.WebException)
                                WTToast.Instance.Show("网络异常，请稍后再试");
                            else
                                MessageBox.Show("保存资料失败，请重试", "提示", MessageBoxButton.OK);
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
            #endregion
        }

        private void UpdateAvatar(Object sender, RoutedEventArgs e)
        {
            var task = new Microsoft.Phone.Tasks.PhotoChooserTask();
            task.PixelHeight = 256;
            task.PixelWidth = 256;
            task.ShowCamera = true;

            task.Completed += (obj, arg) =>
            {
                switch (arg.TaskResult)
                {
                    case Microsoft.Phone.Tasks.TaskResult.OK:
                        {
                            var img = new BitmapImage();
                            img.SetSource(arg.ChosenPhoto);
                            Image_Avatar.Source = img;
                            isAvatarChanged = true;
                        }
                        break;
                    case Microsoft.Phone.Tasks.TaskResult.None:
                    case Microsoft.Phone.Tasks.TaskResult.Cancel:
                        break;
                }
            };

            task.Show();
        }

        public class DataWrapper
        {
            public UserExt User { get; set; }
            public BitmapSource Avatar { get; set; }
        }
    }
}