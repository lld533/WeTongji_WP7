using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading;
using WeTongji.DataBase;
using WeTongji.Business;
using WeTongji.Api.Domain;
using System.Diagnostics;
using WeTongji.Api;
using WeTongji.Pages;
using System.Windows.Media;
using System.Windows.Data;
using System.Reflection;
using WeTongji.Api.Request;
using WeTongji.Api.Response;
using WeTongji.Utility;

namespace WeTongji
{
    public partial class PersonalProfile : PhoneApplicationPage
    {
        #region [Private parameter]

        /// <summary>
        /// This class is used to construct a background thread, which is used
        /// to update new data to the server and local database.
        /// </summary>
        private class UpdateDataParameter
        {
            public UserExt OldValue { get; private set; }
            public UserExt NewValue { get; private set; }
            public PropertyInfo Info { get; private set; }
            public TextBox Item { get; private set; }

            public UpdateDataParameter(UserExt old_value, UserExt new_value, PropertyInfo info, TextBox item)
            {
                OldValue = old_value;
                NewValue = new_value;
                Info = info;
                Item = item;
            }
        }

        #endregion

        #region [Field]

        private TextBox lastVisibleTextBox = null;

        #endregion

        #region [Constructor]

        public PersonalProfile()
        {
            InitializeComponent();

            this.Loaded += (o, e) =>
                {
                    Thread thread = new Thread(new ThreadStart(LoadPersonalProfile))
                    {
                        IsBackground = true,
                        Name = "LoadPersonalProfile"
                    };

                    ProgressBarPopup.Instance.Open();
                    Debug.WriteLine("Thread [LoadPersonalProfile] started.");
                    thread.Start();
                };
        }

        #endregion

        #region [Functions]

        #region [Load]

        private void LoadPersonalProfile()
        {
            if (String.IsNullOrEmpty(Global.Instance.Settings.UID) || !WTUserDataContext.UserDataContextExists(Global.Instance.Settings.UID))
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    TextBlock_LoadFailed.Visibility = Visibility.Visible;
                    ProgressBarPopup.Instance.Close();
                });
            }
            else
            {
                UserExt user = null;

                using (var db = new WTUserDataContext(Global.Instance.Settings.UID))
                {
                    user = db.UserInfo.SingleOrDefault();
                }

                if (user == null)
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        TextBlock_LoadFailed.Visibility = Visibility.Visible;
                        ProgressBarPopup.Instance.Close();
                    });
                }
                else
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        //...Update UI
                        TextBlock_LoadFailed.Visibility = Visibility.Collapsed;
                        this.DataContext = user;
                        ScrollViewer_Core.Visibility = Visibility.Visible;

                        //...Download image if it needs.
                        Debug.WriteLine(user.Avatar);
                        Debug.WriteLine(user.AvatarGuid);
                        if (!user.Avatar.EndsWith("missing.png") && !user.AvatarImageExists())
                        {
                            var thread = new Thread(new ParameterizedThreadStart(DownloadAvatarImage));

                            thread.Start(user);
                        }
                        else
                        {
                            ProgressBarPopup.Instance.Close();
                        }
                    });                    
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param">UserExt</param>
        private void DownloadAvatarImage(object param)
        {
            #region [Check argument]

            if (param == null)
                throw new ArgumentNullException();

            var user = param as UserExt;
            if (user == null)
                throw new NotSupportedException("UserExt is expected");

            #endregion

            var client = new WTDownloadImageClient();

            #region [Register download event handlers]

            client.DownloadImageCompleted += (o, e) =>
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    user.SendPropertyChanged("AvatarImageBrush");
                });
            };

            #endregion

            client.Execute(user.Avatar, user.AvatarGuid + "." + user.Avatar.GetImageFileExtension());
        }

        #endregion

        #region [Nav]

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.ScrollViewer_Core.ScrollToVerticalOffset(0);
        }

        private void EditPersonalProfile(Object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Pages/EditPersonalProfile.xaml", UriKind.RelativeOrAbsolute));
        }

        private void NavToUpdatePassword(Object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Pages/UpdatePassword.xaml", UriKind.RelativeOrAbsolute));
        }

        #endregion

        #region [Visual]

        /// <summary>
        /// Click to turn over the corresponding editing text box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// This function relies on VisualTree
        /// </remarks>
        private void ClickToTurnOverTextBoxVisibility(Object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var parent = VisualTreeHelper.GetParent(btn) as Panel;
            var txtbx = VisualTreeHelper.GetChild(parent, 1) as TextBox;
            //txtbx.Visibility = Visibility.Visible;
            if (txtbx.Visibility == Visibility.Visible)
            {
                txtbx.Visibility = Visibility.Collapsed;
                lastVisibleTextBox = null;
            }
            else
            {
                if (lastVisibleTextBox != null && lastVisibleTextBox != txtbx && lastVisibleTextBox.Visibility == Visibility.Visible)
                {
                    lastVisibleTextBox.Visibility = Visibility.Collapsed;
                }

                txtbx.Visibility = Visibility.Visible;
                txtbx.Focus();
                lastVisibleTextBox = txtbx;
            }
        }

        private void EditingTextBox_LostFocus(Object sender, RoutedEventArgs e)
        {
            var txtbx = sender as TextBox;
            var user = this.DataContext as UserExt;

            if (txtbx == null || user == null)
                return;

            //this.Top_PlaceHolder.Visibility = Visibility.Collapsed;
            //this.Bottom_PlaceHolder.Visibility = Visibility.Collapsed;

            //...Get property info by data binding by VisualTree
            var parent = VisualTreeHelper.GetParent(txtbx) as StackPanel;
            var bindingExpr = parent.GetBindingExpression(StackPanel.VisibilityProperty);
            var propertyInfo = typeof(UserExt).GetProperty(bindingExpr.ParentBinding.Path.Path);

            string previousValue = (String)propertyInfo.GetValue(user, null);

            if (previousValue != txtbx.Text)
            {
                ProgressBarPopup.Instance.Open();

                var thread = new Thread(new ParameterizedThreadStart(UpdateData))
                {
                    IsBackground = true,
                    Name = "UpdateData"
                };

                var oldvalue = (this.DataContext as UserExt).Clone();
                var newvalue = (this.DataContext as UserExt).Clone();
                propertyInfo.SetValue(newvalue, txtbx.Text, null);

                var param = new UpdateDataParameter(oldvalue, newvalue, propertyInfo, txtbx);
                thread.Start(param);
            }
        }

        #endregion

        #region [Update data]

        private void UpdateData(Object obj)
        {
            if (obj == null)
                throw new ArgumentNullException();

            var param = obj as UpdateDataParameter;
            if (param == null)
                throw new NotSupportedException("UpdateDataParameter is expected");

            var req = new UserUpdateRequest<WTResponse>();
            var client = new WTDefaultClient<WTResponse>();

            req.User = param.NewValue.GetObject() as User;

            client.ExecuteFailed += (o, e) =>
                {
                    Debug.WriteLine("Update [{0}] failed. Error:\n{1}", param.Info.Name, e.Error);

                    //...update ui
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        ProgressBarPopup.Instance.Close();

                        //...Todo @_@ Analyze type of the exception. By default just MsgBox try again.
                        MessageBox.Show("更新失败，请重试");
                    });
                };

            client.ExecuteCompleted += (o, e) =>
                {
                    Debug.WriteLine("Update [{0}] completed.", param.Info.Name);

                    using (var db = new WTUserDataContext(Global.Instance.Settings.UID))
                    {
                        var userInDB = db.UserInfo.SingleOrDefault();

                        userInDB.Phone = req.User.Phone;
                        userInDB.QQ = req.User.QQ;
                        userInDB.Email = req.User.Email;
                        userInDB.SinaWeibo = req.User.SinaWeibo;

                        db.SubmitChanges();
                    }

                    //...Update UI
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        var user = this.DataContext as UserExt;

                        object v = param.Info.GetValue(param.NewValue, null);
                        param.Info.SetValue(user, v, null);
                        user.SendPropertyChanged(param.Info.Name);
                        param.Item.Text = String.Empty;
                        ProgressBarPopup.Instance.Close();
                    });
                };


            client.Post(req, Global.Instance.Session, Global.Instance.Settings.UID);
        }

        #endregion


        #endregion
    }
}