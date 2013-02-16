﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NokiaMapSDK;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls.Maps;
using System.IO;
using System.Device.Location;
using Newtonsoft;
using Newtonsoft.Json;
using WeTongji.Extensions;
using WeTongji.Extensions.NokiaMapsSDK;
using System.Diagnostics;
using System.Windows.Media;
using WeTongji.Extensions.GoogleMapsSDK;
using System.Windows.Media.Animation;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Input;


namespace WeTongji
{
    public partial class MapAddress : PhoneApplicationPage
    {
        #region [Fields]

        static GeoCoordinate TargetLocation = new GeoCoordinate();
        static GeoCoordinate CurrentLocation = new GeoCoordinate();
        static GeoCoordinateWatcher GCW = new GeoCoordinateWatcher();

        private Boolean isCoreQueryExecuted = false;
        private Boolean isCurrentLocationQueryExecuted = false;
        private String queryString = String.Empty;
        private DispatcherTimer reverseQueryDispatcherTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(20) };

        #endregion

        #region [Nokia Maps AppId & Token]

        const String AppId = "Or94PrudKATbv6vmtnzb";
        const String Token = "FbGjsdJkxVvTF2OcdOuBGA ";

        #endregion

        #region [Protected Functions]
        /// <remarks>
        /// [Query] /Pages/MapAddress.xaml?q=[%s]
        /// </remarks>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            GCW.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(CurrentPositionChanged);
            GCW.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(GeoPositionStatusChanged);
            GCW.Start();

            var uri = e.Uri.ToString();
            queryString = uri.TrimStart("/Pages/MapAddress.xaml?q=".ToCharArray());

            reverseQueryDispatcherTimer.Tick += ReverseQueryTick;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            GCW.PositionChanged -= CurrentPositionChanged;
            GCW.Stop();

            reverseQueryDispatcherTimer.Tick -= ReverseQueryTick;
            reverseQueryDispatcherTimer.Stop();
        }
        #endregion

        #region [Construction]

        public MapAddress()
        {
            InitializeComponent();

            #region [Loaded]
            this.Loaded += (o, e) =>
                        {
                            {
                                var popup = VisualTreeHelper.GetChild(TargetBillboardPushpin, 0) as Popup;
                                popup.Opened += (obj, args) =>
                                {
                                    (popup.Child as UIElement).MouseLeftButtonUp += TargetBillboard_MouseLeftButtonUp;
                                };
                            }

                            {
                                var popup = VisualTreeHelper.GetChild(CurrentBillboardPushpin, 0) as Popup;
                                popup.Opened += (obj, args) =>
                                {
                                    (popup.Child as UIElement).MouseLeftButtonUp += CurrentBillboard_MouseLeftButtonUp;
                                };
                            }

                            {
                                MyMap.ViewChangeEnd += MyMap_ViewChangeEnd;
                            }
                        };
            #endregion
        }
        #endregion

        #region [Event handlers]
        /// <summary>
        /// Update current icon when current position is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg"></param>
        private void CurrentPositionChanged(Object sender, GeoPositionChangedEventArgs<GeoCoordinate> arg)
        {
            #region [Update pushpin in the viewport]

            CurrentLocation = arg.Position.Location;
            CurrentPositionPushpin.Location = CurrentLocation;
            CurrentBillboardPushpin.Location = CurrentLocation;

            #endregion

            if (!reverseQueryDispatcherTimer.IsEnabled)
                reverseQueryDispatcherTimer.Start();
        }

        private void GeoPositionStatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            if (e.Status == GeoPositionStatus.Ready)
            {
                DirectionButton.IsEnabled = true;
                ExecuteCoreQuery();
            }
            else
            {
                DirectionButton.IsEnabled = false;
            }
        }

        private void BillboardPopup_Opened(object sender, EventArgs e)
        {
            var p = sender as Popup;
            var tr = p.RenderTransform as CompositeTransform;
            p.Child.UpdateLayout();
            tr.TranslateY = -(p.Child as UIElement).RenderSize.Height + 4;
        }

        private void CurrentPositionPushpin_MouseLeftButtonUp(Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var popup = VisualTreeHelper.GetChild(CurrentBillboardPushpin, 0) as Popup;

            if (popup.IsOpen)
            {
                (popup.Resources["Close"] as Storyboard).Begin();
            }
            else
            {
                (popup.Resources["Open"] as Storyboard).Begin();
            }
        }

        private void TargetBillboard_MouseLeftButtonUp(Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var popup = VisualTreeHelper.GetChild(TargetBillboardPushpin, 0) as Popup;
            if (popup.IsOpen)
            {
                var sb = popup.Resources["Close"] as Storyboard;
                sb.Completed += ReOpenIconPushpin;
                sb.Begin();
            }
        }

        private void CurrentBillboard_MouseLeftButtonUp(Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var popup = VisualTreeHelper.GetChild(CurrentBillboardPushpin, 0) as Popup;
            if (popup.IsOpen)
            {
                (popup.Resources["Close"] as Storyboard).Begin();
            }
        }

        private void ReOpenIconPushpin(Object sender, EventArgs e)
        {
            var popup = VisualTreeHelper.GetChild(TargetBillboardPushpin, 0) as Popup;
            (popup.Resources["Close"] as Storyboard).Completed -= ReOpenIconPushpin;

            popup = VisualTreeHelper.GetChild(TargetPushpin, 0) as Popup;
            (popup.Resources["Open"] as Storyboard).Begin();
        }

        private void ReverseQueryTick(Object sender, EventArgs e)
        {
            var popup = VisualTreeHelper.GetChild(CurrentBillboardPushpin, 0) as Popup;

            Debug.WriteLine("Reverse query tick!" + DateTime.Now);

            Action coreAction = () =>
            {
                #region [Update Current Address]
                {
                    GoogleMapsReverseQueryRequest req = new GoogleMapsReverseQueryRequest(CurrentLocation.Latitude, CurrentLocation.Longitude);
                    GoogleMapsQueryClient client = new GoogleMapsQueryClient();

                    client.ExecuteStarted += (o, args) =>
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            var bbi = CurrentBillboardPushpin.DataContext as BillBoardItem;
                            bbi.IsSyncing = true;
                        });
                    };
                    client.ExecuteCompleted += (o, args) =>
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            DirectionButton.IsEnabled = true;

                            var bbi = CurrentBillboardPushpin.DataContext as BillBoardItem;
                            bbi.Address = args.Response.results.First().formatted_address;
                            bbi.IsSyncing = false;
                        });
                    };
                    client.ExecuteFailed += (o, args) =>
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            DirectionButton.IsEnabled = false;
                            var bbi = CurrentBillboardPushpin.DataContext as BillBoardItem;
                            bbi.IsSyncing = false;
                        });
                    };

                    client.ExecuteAsync(req);
                }
                #endregion

                #region [Update Distance]
                {
                    QueryRequest request = new QueryRequest()
                    {
                        AppId = AppId,
                        Token = Token,
                        Query = queryString,
                        CurrentPosition = new GeoPoint() { Latitude = CurrentLocation.Latitude, Longitude = CurrentLocation.Longitude }
                    };

                    NokiaMapQueryClient client = new NokiaMapQueryClient();

                    client.ExecuteStarted += (obj, args) =>
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            var bbi = TargetBillboardPushpin.DataContext as BillBoardItem;
                            bbi.IsSyncing = true;
                        });
                    };

                    client.ExecuteCompleted += (obj, args) =>
                    {
                        var first_result = args.Response.results.items.FirstOrDefault();
                        if (first_result != null)
                        {
                            this.Dispatcher.BeginInvoke(() =>
                            {
                                var bbi = TargetBillboardPushpin.DataContext as BillBoardItem;
                                bbi.Distance = first_result.DisplayDistance;
                                bbi.IsSyncing = false;
                            });
                        }
                    };

                    client.ExecuteFailed += (obj, args) =>
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            var bbi = TargetBillboardPushpin.DataContext as BillBoardItem;
                            bbi.Distance = "? KM";
                            bbi.IsSyncing = false;
                        });
                    };

                    client.ExecuteAsync(request, new object());
                }
                #endregion
            };

            if (popup.IsOpen)
            {
                coreAction.Invoke();
            }
            else
            {
                if (String.IsNullOrEmpty((CurrentBillboardPushpin.DataContext as BillBoardItem).Address))
                {
                    coreAction.Invoke();
                }
            }
        }

        private void ViewCurrentLocation(Object sender, RoutedEventArgs e)
        {
            MyMap.SetView(CurrentLocation, 18);
        }

        private void MyIconPushpinImage_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var popup = VisualTreeHelper.GetChild(TargetPushpin, 0) as Popup;
            if (popup.IsOpen)
            {
                var sb = popup.Resources["Close"] as Storyboard;
                sb.Completed += CloseMyPushpinCompleted;
                sb.Begin();
            }
        }

        private void CloseMyPushpinCompleted(Object sender, EventArgs e)
        {
            var popup_img = VisualTreeHelper.GetChild(TargetPushpin, 0) as Popup;
            (popup_img.Resources["Close"] as Storyboard).Completed -= CloseMyPushpinCompleted;

            var popup_bb = VisualTreeHelper.GetChild(TargetBillboardPushpin, 0) as Popup;
            (popup_bb.Resources["Open"] as Storyboard).Begin();
        }

        /// <summary>
        /// Set current address in advance and update layout to ensure
        /// that current billboard locates in the center the green icon
        /// if its ancestor popup is open.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// This function relies on this.resources["BillboardPushpin"], which is defined in MapAdrress.xaml.cs
        /// </remarks>
        private void CurrentPositionBillBoardItem_PropertyChanged(Object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Address")
            {
                var popup = VisualTreeHelper.GetChild(CurrentBillboardPushpin, 0) as Popup;
                var obj = popup.Child as DependencyObject;
                for (int i = 0; i < 4; ++i)
                    obj = VisualTreeHelper.GetChild(obj, 0);
                var txtblk = obj as TextBlock;
                txtblk.Text = (CurrentBillboardPushpin.DataContext as BillBoardItem).Address;

                if (popup.IsOpen)
                {
                    var tr = popup.RenderTransform as CompositeTransform;
                    txtblk.UpdateLayout();

                    tr.TranslateY = -(popup.Child as UIElement).RenderSize.Height + 4;
                }
            }
        }

        private void MyMap_ViewChangeEnd(Object sender, MapEventArgs e)
        {
            if (!e.Handled && DirectionButton.IsEnabled)
            {
                //var maplayer = VisualTreeHelper.GetParent(DirectionButton) as MapLayer;
                var pnt = CurrentPositionPushpin.TransformToVisual(null).Transform(new Point());
                if (pnt.X > -CurrentPositionPushpin.ActualWidth && pnt.X < this.ActualWidth
                    && pnt.Y > -CurrentPositionPushpin.ActualHeight && pnt.Y < this.ActualHeight)
                    return;

                //...Get storyboard
                var obj = VisualTreeHelper.GetChild(DirectionButton, 0);

                var CoreStoryboard = (obj as FrameworkElement).Resources["PointDirection"] as Storyboard;
                var RevealStoryboard = (obj as FrameworkElement).Resources["RevealPointer"] as Storyboard;


                //...Get Map Pointer
                obj = VisualTreeHelper.GetChild(obj, 1);
                var pointer = VisualTreeHelper.GetChild(obj, 3) as UIElement;
                var center = MyMap.TargetCenter;

                //...Normalize rotation
                if ((pointer.RenderTransform as CompositeTransform).Rotation > 180)
                {
                    (pointer.RenderTransform as CompositeTransform).Rotation -= 360;
                }
                else if ((pointer.RenderTransform as CompositeTransform).Rotation < -180)
                {
                    (pointer.RenderTransform as CompositeTransform).Rotation += 360;
                }

                const double tol = 0.001F;
                double theta = 0.0F;

                CoreStoryboard.Completed -= HideMapPointer;
                CoreStoryboard.Pause();

                //...Raw target rotation angle
                if (Math.Abs(CurrentLocation.Latitude - center.Latitude) < tol)
                    if (center.Longitude < CurrentLocation.Longitude)
                        theta = 0;
                    else
                    {
                        theta = (pointer.RenderTransform as CompositeTransform).Rotation < 0 ? -180 : 180;
                    }
                else
                {
                    double delta_lat = CurrentLocation.Latitude - center.Latitude;
                    double delta_lng = CurrentLocation.Longitude - center.Longitude;


                    if (delta_lng < 0)
                    {
                        if (delta_lat > 0)
                        {
                            theta = Math.Atan(delta_lng / delta_lat) / Math.PI * 180;
                        }
                        else
                        {
                            theta = Math.Atan(delta_lng / delta_lat) / Math.PI * 180 - 180;
                        }
                    }
                    else
                    {
                        if (delta_lat > 0)
                        {
                            theta = Math.Atan(delta_lng / delta_lat) / Math.PI * 180;
                        }
                        else
                        {
                            theta = -Math.Atan(delta_lat / delta_lng) / Math.PI * 180 + 90;
                        }
                    }
                }

                //...Make shortest rotation way, clockwise or anti-clockwise.
                {
                    var distance = theta - (pointer.RenderTransform as CompositeTransform).Rotation;

                    if (distance > 180)
                        theta -= 360;
                    else if (distance < -180)
                        theta += 360;
                }


                var animation = CoreStoryboard.Children.Single() as DoubleAnimationUsingKeyFrames;
                animation.KeyFrames[0].Value = (pointer.RenderTransform as CompositeTransform).Rotation;
                animation.KeyFrames[1].Value = theta;

                CoreStoryboard.Completed += HideMapPointer;
                RevealStoryboard.Begin();
                CoreStoryboard.Begin();
            }
        }

        private void HideMapPointer(Object sender, EventArgs e)
        {
            //...Get storyboard and Map Pointer
            var obj = VisualTreeHelper.GetChild(DirectionButton, 0);

            var CoreStoryboard = (obj as FrameworkElement).Resources["PointDirection"] as Storyboard;
            var HideStoryboard = (obj as FrameworkElement).Resources["HidePointer"] as Storyboard;

            //...Get Map Pointer
            obj = VisualTreeHelper.GetChild(obj, 1);
            var pointer = VisualTreeHelper.GetChild(obj, 3) as UIElement;

            var tr = pointer.RenderTransform as CompositeTransform;
            if (tr.Rotation > 180)
            {
                tr.Rotation -= 360;
            }
            else
            {
                if (tr.Rotation < -180)
                    tr.Rotation += 360;
            }

            CoreStoryboard.Completed -= HideMapPointer;
            HideStoryboard.Begin();
        }

        private void DirectionButtonHold(Object sender, GestureEventArgs e)
        {
            if (!e.Handled)
            {
                SetBestView();
            }
        }

        private void DirectionButtonIsEnabledChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                if ((Boolean)e.NewValue)
                {
                    SetBestView();
                }
                else
                {
                    MessageBox.Show("请关闭飞行模式，并检查网络连接后重试。", "暂时无法查询地图", MessageBoxButton.OK);
                }
            });
        }

        #endregion

        #region [Private Functions]

        private void SetBestView()
        {
            if (!TargetLocation.IsUnknown && !CurrentLocation.IsUnknown)
                MyMap.SetView(new LocationRect(
                    Math.Max(TargetLocation.Latitude, CurrentLocation.Latitude),
                    Math.Min(TargetLocation.Longitude, CurrentLocation.Longitude),
                    Math.Min(TargetLocation.Latitude, CurrentLocation.Latitude),
                    Math.Max(TargetLocation.Longitude, CurrentLocation.Longitude)
                    ));
        }

        private void ExecuteCoreQuery()
        {
            #region [Do Core Query]
            if (!isCoreQueryExecuted)
            {
                #region [GeoCode query location by using GoogleMapsSDK]

                GoogleMapsQueryRequest gRequest = new GoogleMapsQueryRequest(queryString, CurrentLocation, true);
                GoogleMapsQueryClient gClient = new GoogleMapsQueryClient();

                gClient.ExecuteStarted += (obj, args) =>
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        var bbi = TargetBillboardPushpin.DataContext as BillBoardItem;
                        bbi.IsSyncing = true;
                    });
                };
                gClient.ExecuteCompleted += (obj, args) =>
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        DirectionButton.IsEnabled = true;
                        isCoreQueryExecuted = true;

                        var result = args.Response.results.FirstOrDefault();
                        if (result != null)
                        {
                            TargetLocation = new GeoCoordinate()
                            {
                                Latitude = result.geometry.location.lat,
                                Longitude = result.geometry.location.lng
                            };

                            //...Try to set view
                            SetBestView();

                            //...Set Pushpin
                            TargetPushpin.Location = TargetLocation;
                            TargetBillboardPushpin.Location = TargetLocation;
                            var bbi = TargetBillboardPushpin.DataContext as BillBoardItem;
                            bbi.Address = result.formatted_address;
                            bbi.IsSyncing = false;

                            //...Delay a while to open target pushpin
                            int tickCount = 0;
                            DispatcherTimer dt = new DispatcherTimer();
                            dt.Interval = TimeSpan.FromSeconds(1);
                            dt.Tick += (TickObj, TickArg) =>
                            {
                                if ((++tickCount) == 6)
                                {
                                    dt.Stop();

                                    //...Open TargetPushpin
                                    var p = VisualTreeHelper.GetChild(TargetPushpin, 0) as Popup;
                                    (p.Resources["Open"] as Storyboard).Begin();
                                }
                            };
                            dt.Start();
                        }
                    });
                };


                gClient.ExecuteFailed += (obj, args) =>
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        DirectionButton.IsEnabled = false;
                        var bbi = TargetBillboardPushpin.DataContext as BillBoardItem;
                        bbi.IsSyncing = false;
                    });
                };

                gClient.ExecuteAsync(gRequest);

                #endregion

                #region [Query distance and Image Pushpin icon by using NokiaMapsSDK]

                if (!isCurrentLocationQueryExecuted)
                {
                    QueryRequest request = new QueryRequest()
                                    {
                                        AppId = AppId,
                                        Token = Token,
                                        Query = queryString,
                                        CurrentPosition = new GeoPoint() { Latitude = CurrentLocation.Latitude, Longitude = CurrentLocation.Longitude }
                                    };

                    NokiaMapQueryClient client = new NokiaMapQueryClient();

                    client.ExecuteStarted += (obj, args) =>
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            var bbi = TargetBillboardPushpin.DataContext as BillBoardItem;
                            bbi.IsSyncing = true;
                        });
                    };

                    client.ExecuteCompleted += (obj, args) =>
                    {
                        this.Dispatcher.BeginInvoke(() =>
                            {
                                isCurrentLocationQueryExecuted = true;
                                var bbi = TargetBillboardPushpin.DataContext as BillBoardItem;
                                bbi.IsSyncing = false;

                                //...Set Distance
                                var first_result = args.Response.results.items.FirstOrDefault();
                                if (first_result != null)
                                {
                                    bbi.Distance = first_result.DisplayDistance;
                                }
                                else
                                {
                                    bbi.Distance = "? km";
                                }

                                //...Set Icon
                                if (first_result != null && !String.IsNullOrEmpty(first_result.icon))
                                    TargetPushpin.DataContext = new Uri(first_result.icon);
                                else
                                    TargetPushpin.DataContext = new Uri("/icons/DefaultBuildingIcon.png", UriKind.RelativeOrAbsolute);
                            });

                    };

                    client.ExecuteFailed += (obj, args) =>
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            //...Set Distance
                            var bbi = TargetBillboardPushpin.DataContext as BillBoardItem;
                            bbi.Distance = "? km";
                            bbi.IsSyncing = false;

                            //...Set Icon
                            TargetPushpin.DataContext = new Uri("/icons/DefaultBuildingIcon.png", UriKind.RelativeOrAbsolute);
                        });
                    };

                    client.ExecuteAsync(request, new object());
                }

                #endregion
            }
            #endregion

            #region [Query current location]
            {
                #region [Update Current Address]
                {
                    GoogleMapsReverseQueryRequest req = new GoogleMapsReverseQueryRequest(CurrentLocation.Latitude, CurrentLocation.Longitude);
                    GoogleMapsQueryClient client = new GoogleMapsQueryClient();

                    client.ExecuteStarted += (obj, args) =>
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            var bbi = CurrentBillboardPushpin.DataContext as BillBoardItem;
                            bbi.IsSyncing = true;
                        });
                    };
                    client.ExecuteCompleted += (obj, args) =>
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            DirectionButton.IsEnabled = true;

                            var bbi = CurrentBillboardPushpin.DataContext as BillBoardItem;
                            bbi.Address = args.Response.results.First().formatted_address;
                            bbi.IsSyncing = false;
                        });
                    };
                    client.ExecuteFailed += (obj, args) =>
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            DirectionButton.IsEnabled = false;
                            var bbi = CurrentBillboardPushpin.DataContext as BillBoardItem;
                            bbi.IsSyncing = false;
                        });
                    };

                    client.ExecuteAsync(req);
                }
                #endregion

                #region [Update Distance]
                {
                    QueryRequest request = new QueryRequest()
                    {
                        AppId = AppId,
                        Token = Token,
                        Query = queryString,
                        CurrentPosition = new GeoPoint() { Latitude = CurrentLocation.Latitude, Longitude = CurrentLocation.Longitude }
                    };

                    NokiaMapQueryClient client = new NokiaMapQueryClient();

                    client.ExecuteStarted += (obj, args) =>
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            var bbi = TargetBillboardPushpin.DataContext as BillBoardItem;
                            bbi.IsSyncing = true;
                        });
                    };

                    client.ExecuteCompleted += (obj, args) =>
                    {
                        var first_result = args.Response.results.items.FirstOrDefault();
                        if (first_result != null)
                        {
                            this.Dispatcher.BeginInvoke(() =>
                            {
                                var bbi = TargetBillboardPushpin.DataContext as BillBoardItem;
                                bbi.Distance = first_result.DisplayDistance;
                                bbi.IsSyncing = false;
                            });
                        }
                    };

                    client.ExecuteFailed += (obj, args) =>
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            var bbi = TargetBillboardPushpin.DataContext as BillBoardItem;
                            bbi.Distance = "? KM";
                            bbi.IsSyncing = false;
                        });
                    };

                    client.ExecuteAsync(request, new object());
                }
                #endregion
            }
            #endregion
        }

        #endregion
    }

    public sealed class BillBoardItem : INotifyPropertyChanged
    {
        private String address = String.Empty;
        private String distance = String.Empty;
        private Boolean isSyncing = false;

        public String Address
        {
            get
            {
                return address;
            }
            set
            {
                if (address != value)
                {
                    address = value;
                    NotifyChanged("Address");
                }
            }
        }
        public String Distance
        {
            get { return distance; }
            set
            {
                if (distance != value)
                {
                    distance = value;
                    NotifyChanged("Distance");
                }
            }
        }
        public Boolean IsSyncing
        {
            get { return isSyncing; }
            set
            {
                if (value != isSyncing)
                {
                    isSyncing = value;
                    NotifyChanged("IsSyncing");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyChanged(String param)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(new object(), new PropertyChangedEventArgs(param));
        }
    }
}