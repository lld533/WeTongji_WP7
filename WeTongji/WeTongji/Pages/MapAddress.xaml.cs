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


namespace WeTongji
{
    public partial class MapAddress : PhoneApplicationPage
    {
        #region [Fields]

        static GeoCoordinate CurrentLocation = new GeoCoordinate();
        static GeoCoordinateWatcher GCW = new GeoCoordinateWatcher();

        private String queryString = String.Empty;

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
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            GCW.PositionChanged -= CurrentPositionChanged;
            GCW.Stop();
        }

        private static void CurrentPositionChanged(Object sender, GeoPositionChangedEventArgs<GeoCoordinate> arg)
        {
            CurrentLocation = arg.Position.Location;
        }

        private void BillboardPopup_Opened(object sender, EventArgs e)
        {
            var p = sender as Popup;
            var tr = p.RenderTransform as CompositeTransform;
            p.Child.UpdateLayout();
            tr.TranslateY = -(p.Child as UIElement).RenderSize.Height + 4;
        }

        public MapAddress()
        {
            InitializeComponent();

            this.Loaded += (o, e) =>
            {
                GoogleMapsQueryRequest gRequest = new GoogleMapsQueryRequest(queryString, CurrentLocation, true);
                GoogleMapsQueryClient gClient = new GoogleMapsQueryClient();

                gClient.ExecuteCompleted += (obj, args) => 
                {
                    this.Dispatcher.BeginInvoke(() => 
                    {
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
                            MyPushpin.Location = coordinate;
                            MyBillboardPushpin.Location = coordinate;

                            var g = VisualTreeHelper.GetChild(MyBillboardPushpin, 0) as Popup;
                            (g.Resources["Open"] as Storyboard).Begin();


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
                            MyPushpin.Location = coordinate;
                            MyBillboardPushpin.Location = coordinate;
                            MyMap.ViewChangeEnd += MyMapViewChangedEnd;
                        });
                    }
                };

                client.ExecuteFailed += (obj, args) =>
                    {
                        this.Dispatcher.BeginInvoke(() => 
                        {
                            MessageBox.Show(String.Format("查询 {0} 失败", queryString));
                        });
                    };

                client.ExecuteAsync(request, new object());
            };
        }

        private void MyMapViewChangedEnd(Object sender, MapEventArgs e)
        {
            MyPushpin.Visibility = Visibility.Visible;
            MyMap.ViewChangeEnd -= MyMapViewChangedEnd;
        }

        private void StaticMap_ManipulationCompleted_1(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            var map = sender as JeffWilcox.Controls.StaticMap;
            var pnt = e.TotalManipulation.Translation;

            map.SetValue(JeffWilcox.Controls.StaticMap.MapCenterProperty,
                new GeoCoordinate(map.MapCenter.Latitude - Math.Sign(pnt.Y) * 0.01, map.MapCenter.Longitude - Math.Sign(pnt.X) * 0.005));
        }
    }
}