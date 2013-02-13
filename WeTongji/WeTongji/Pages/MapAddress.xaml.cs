using System;
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


namespace WeTongji
{
    public partial class MapAddress : PhoneApplicationPage
    {
        #region [Fields]

        static GeoCoordinate CurrentLocation = new GeoCoordinate();
        static GeoCoordinateWatcher GCW = new GeoCoordinateWatcher();

        private String queryString = String.Empty;
        private DispatcherTimer reverseQueryDispatcherTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(5) };

        #endregion

        #region [Nokia Maps AppId & Token]

        const String AppId = "Or94PrudKATbv6vmtnzb";
        const String Token = "FbGjsdJkxVvTF2OcdOuBGA ";

        #endregion

        /// <remarks>
        /// [Query] /Pages/MapAddress.xaml?q=[%s]
        /// </remarks>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            GCW.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(CurrentPositionChanged);
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

        public MapAddress()
        {
            InitializeComponent();

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

                GoogleMapsQueryRequest gRequest = new GoogleMapsQueryRequest(queryString, CurrentLocation, true);
                GoogleMapsQueryClient gClient = new GoogleMapsQueryClient();

                gClient.ExecuteCompleted += (obj, args) =>
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        DirectionButton.IsEnabled = true;

                        var result = args.Response.results.FirstOrDefault();
                        if (result != null)
                        {
                            var coordinate = new GeoCoordinate()
                            {
                                Latitude = result.geometry.location.lat,
                                Longitude = result.geometry.location.lng
                            };

                            //...Set view
                            MyMap.SetView(coordinate, 18);

                            //...Set Pushpin
                            TargetPushpin.Location = coordinate;
                            TargetBillboardPushpin.Location = coordinate;
                            var bbi = TargetBillboardPushpin.DataContext as BillBoardItem;
                            bbi.Address = result.formatted_address;

                            //TextBlock_Address.Text = result.formatted_address;
                            //String[] strTypes = new String[result.types.Count()];
                            //int i = 0;
                            //foreach (var t in result.types)
                            //{
                            //    strTypes[i++] = t.ToString();
                            //}
                            //TextBlock_AddressType.Text = strTypes.Aggregate((a, b) => a + " " + b);
                            //TextBlock_LatLng.Text = String.Format("{0},{1}", result.geometry.location.lat, result.geometry.location.lng);
                        }
                    });
                };

                                
                gClient.ExecuteFailed += (obj, args) =>    
                {
                    this.Dispatcher.BeginInvoke(() =>
                        {
                            DirectionButton.IsEnabled = false;
                            MessageBox.Show(args.Error.GetType() + " " + args.Error.Message);
                        });                        
                };

                gClient.ExecuteAsync(gRequest);

                QueryRequest request = new QueryRequest()
                    {
                        AppId = AppId,
                        Token = Token,
                        Query = queryString,
                        CurrentPosition = new GeoPoint() { Latitude = CurrentLocation.Latitude, Longitude = CurrentLocation.Longitude }
                    };

                NokiaMapQueryClient client = new NokiaMapQueryClient();
                client.ExecuteCompleted += (obj, args) =>
                {
                    //Debug.WriteLine("Success!");
                    var first_result = args.Response.results.items.FirstOrDefault();
                    if (first_result != null)
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            var coordinate = new GeoCoordinate()
                            {
                                Latitude = first_result.GeoPosition.Latitude,
                                Longitude = first_result.GeoPosition.Longitude
                            };

                            //...Set view
                            MyMap.SetView(coordinate, 18);

                            //...Set pushpin
                            TargetPushpin.Location = coordinate;
                            TargetBillboardPushpin.Location = coordinate;
                            var bbi = TargetBillboardPushpin.DataContext as BillBoardItem;
                            bbi.Distance = first_result.DisplayDistance;



                            int tickCount = 0;
                            DispatcherTimer dt = new DispatcherTimer();
                            dt.Interval = TimeSpan.FromSeconds(1);
                            dt.Tick += (TickObj, TickArg) =>
                            {
#if DEBUG
                                if ((++tickCount) == 25)
#else
                                if ((++tickCount) == 8)
#endif
                                {
                                    dt.Stop();

                                    //...Open TargetPushpin
                                    var p = VisualTreeHelper.GetChild(TargetPushpin, 0) as Popup;
                                    (p.Resources["Open"] as Storyboard).Begin();
                                }
                            };
                            dt.Start();
                        });
                    }
                };

                client.ExecuteFailed += (obj,args)=>
                {
                    var bbi = TargetBillboardPushpin.DataContext as BillBoardItem;
                    bbi.Distance = "? KM";
                };

                client.ExecuteAsync(request, new object());
            };
        }
        
        private void ReverseQueryTick(Object sender, EventArgs e)
        {
            if (CurrentBillboardPushpin.Visibility == Visibility.Visible)
            {
                #region [Update Current Address]
                {
                    GoogleMapsReverseQueryRequest req = new GoogleMapsReverseQueryRequest(CurrentLocation.Latitude, CurrentLocation.Longitude);
                    GoogleMapsQueryClient client = new GoogleMapsQueryClient();
                    client.ExecuteCompleted += (o, args) =>
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            DirectionButton.IsEnabled = true;

                            var bbi = CurrentBillboardPushpin.DataContext as BillBoardItem;
                            bbi.Address = args.Response.results.First().formatted_address;
                        });
                    };
                    client.ExecuteFailed += (o, args) =>
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            DirectionButton.IsEnabled = false;
                            MessageBox.Show(args.Error.GetType()+ " " + args.Error.Message);
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
                    client.ExecuteCompleted += (obj, args) =>
                    {
                        var first_result = args.Response.results.items.FirstOrDefault();
                        if (first_result != null)
                        {
                            this.Dispatcher.BeginInvoke(() =>
                            {
                                var bbi = TargetBillboardPushpin.DataContext as BillBoardItem;
                                bbi.Distance = first_result.DisplayDistance;
                            });
                        }
                    };

                    client.ExecuteFailed += (obj, args) =>
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            var bbi = TargetBillboardPushpin.DataContext as BillBoardItem;
                                bbi.Distance = "? KM";
                        });
                    };

                    client.ExecuteAsync(request, new object());
                }
                #endregion
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
    }

    public sealed class BillBoardItem : INotifyPropertyChanged
    {
        private String address = String.Empty;
        private String distance = String.Empty;

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

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyChanged(String param)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(new object(), new PropertyChangedEventArgs(param));
        }
    }
}