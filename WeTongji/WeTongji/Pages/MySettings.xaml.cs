﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone;
using System.IO.IsolatedStorage;
using System.IO;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Input;
using WeTongji.Api.Request;
using WeTongji.Api.Domain;
using WeTongji.Api.Response;
using WeTongji.Api;
using System.Threading;
using WeTongji.Pages;
using WeTongji.Utility;

namespace WeTongji
{
    public partial class MySettings : PhoneApplicationPage
    {
        public MySettings()
        {
            InitializeComponent();
        }

        private void SettingsPageLoaded(Object sender, RoutedEventArgs e)
        {
            Run_Version.Text = AppVersion.Current;

            var thread = new Thread(new ThreadStart(ComputeImageCacheSize))
            {
                IsBackground = true
            };

            thread.Start();
        }

        private String WrapCacheSize(long sz, int digits)
        {
            float f = (float)(sz) / (float)(1 << digits);
            return f == 0.0F ? "0" : f.ToString("0.00");
        }

        private void SettingsButtonMouseEnter(Object sender, MouseEventArgs e)
        {
            var btn = sender as Button;
            var obj = VisualTreeHelper.GetChild(btn, 0);
            obj = VisualTreeHelper.GetChild(obj, 0);
            var cc = VisualTreeHelper.GetChild(obj, 0) as ContentControl;

            (cc.Projection as PlaneProjection).RotationY = 10;
        }

        private void SettingsButtonMouseLeave(Object sender, MouseEventArgs e)
        {
            var btn = sender as Button;
            var obj = VisualTreeHelper.GetChild(btn, 0);
            obj = VisualTreeHelper.GetChild(obj, 0);
            var cc = VisualTreeHelper.GetChild(obj, 0) as ContentControl;

            cc.Projection = new PlaneProjection();
        }

        private void CheckVersion(Object sender, RoutedEventArgs e)
        {
            var req = new SystemVersionRequest<SystemVersionResponse>();
            var client = new WTDefaultClient<SystemVersionResponse>();

            client.ExecuteFailed += (obj, args) =>
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("检测新版本失败", "检测新版本", MessageBoxButton.OK);
                });
            };

            client.ExecuteCompleted += (obj, args) =>
                {
                    if (args.Result.Version == null)
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            MessageBox.Show("当前版本已经是最新的，感谢您对微同济团队的支持！", "检测新版本", MessageBoxButton.OK);
                        });
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            var result = MessageBox.Show(String.Format("最新版本为{0}，是否前往商城下载？", args.Result.Version.Latest), "当前版本不是最新", MessageBoxButton.OKCancel);
                            if (result == MessageBoxResult.OK)
                            {
                                Microsoft.Phone.Tasks.MarketplaceDetailTask task = new Microsoft.Phone.Tasks.MarketplaceDetailTask();
                                task.ContentIdentifier = args.Result.Version.Url;
                                task.ContentType = Microsoft.Phone.Tasks.MarketplaceContentType.Applications;
                                task.Show();
                            }
                        });
                    }
                };

            client.Execute(req);
        }

        private void ClearImageCache(Object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("确认清空图片缓存？", "提示", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                var thread = new Thread(new ThreadStart(ClearImageCacheCore))
                {
                    IsBackground = true,
                    Name = "ClearImageCacheCore"
                };

                thread.Start();
            }
        }

        private void ComputeImageCacheSize()
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                ProgressBarPopup.Instance.Open();
                TextBlock_ImageCache.Text = "正在计算...";
            });

            var store = IsolatedStorageFile.GetUserStoreForApplication();
            var files = store.GetFileNames().Where(
                (name) => name.ToLower().EndsWith(".jpg")
                    || name.ToLower().EndsWith(".png")
                    || name.ToLower().EndsWith(".bmp")
                    || name.ToLower().EndsWith(".jpeg")
                    || name.ToLower().EndsWith("gif"));

            long szStore = 0;

            foreach (var f in files)
            {
                try
                {
                    using (var fs = store.OpenFile(f, FileMode.Open))
                        szStore += fs.Length;
                }
                catch { }
            }

            this.Dispatcher.BeginInvoke(() =>
            {
                if (szStore >= (1 << 30))
                {
                    TextBlock_ImageCache.Text = String.Format("共  " + WrapCacheSize(szStore, 30) + " GB");
                }
                else if (szStore >= (1 << 20))
                {
                    TextBlock_ImageCache.Text = String.Format("共  " + WrapCacheSize(szStore, 20) + " MB");
                }
                else
                {
                    TextBlock_ImageCache.Text = String.Format("共  " + WrapCacheSize(szStore, 10) + " KB");
                }

                ProgressBarPopup.Instance.Close();
            });
        }

        private void ClearImageCacheCore()
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                ProgressBarPopup.Instance.Open();
            });

            var store = IsolatedStorageFile.GetUserStoreForApplication();

            var files = store.GetFileNames().Where(
                            (name) => name.ToLower().EndsWith(".jpg")
                                || name.ToLower().EndsWith(".png")
                                || name.ToLower().EndsWith(".bmp")
                                || name.ToLower().EndsWith(".jpeg")
                                || name.ToLower().EndsWith("gif"));

            foreach (var f in files)
            {
                try
                {
                    store.DeleteFile(f);
                }
                catch { }

            }

            this.Dispatcher.BeginInvoke(() =>
            {
                ProgressBarPopup.Instance.Close();
            });

            var thread = new Thread(new ThreadStart(ComputeImageCacheSize))
            {
                IsBackground = true
            };

            thread.Start();
        }
    }
}