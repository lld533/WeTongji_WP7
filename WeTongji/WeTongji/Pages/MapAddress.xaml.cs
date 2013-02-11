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
using WeTongji.Bing.GeoCode;
using System.IO;
using System.Device.Location;
using Newtonsoft;
using Newtonsoft.Json;
using WeTongji.Extensions;
using WeTongji.Extensions.NokiaMapsSDK;
using System.Diagnostics;
using System.Windows.Media;
using WeTongji.Extensions.GoogleMapsSDK;


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

        public MapAddress()
        {
            InitializeComponent();
            /*this.Loaded += (o, e) =>
            {
                try
                {

#if BING_MAP
                    //GeocodeRequest request = new GeocodeRequest();
                    //request.Credentials = new Microsoft.Phone.Controls.Maps.Credentials();
                    //request.Credentials.ApplicationId = "AtGEKtXv_hB0Brppk_gQUIUt1JHUC-ldvJe1DxEYG-xOQSniPnyWR6nmF3Hk6m0P";
                    //request.Culture = "zh-cn";
                    //request.ExecutionOptions = new WeTongji.Bing.GeoCode.ExecutionOptions();

                    //request.Query = "上海市赤峰路";
                    //GeocodeServiceClient client = new GeocodeServiceClient("BasicHttpBinding_IGeocodeService");

                    //client.GeocodeCompleted += (a, b) =>
                    //{
                    //    var response = b.Result;
                    //    var result = response.Results.FirstOrDefault();
                    //    if (result != null)
                    //    {
                    //        var l = result.Locations.FirstOrDefault();
                    //        if (l != null)
                    //        {
                    //            System.Diagnostics.Debug.WriteLine("altitude:{0}, longtitude:{1}, latitude:{2}", l.Altitude, l.Longitude, l.Latitude);
                    //        }
                    //    }
                    //};
#endif
#if NOKIA_MAP
                    //client.GeocodeAsync(request);
                    var query = "上海市赤峰路35号";
                    var request = HttpWebRequest.CreateHttp(
                        String.Format("http://places.nlp.nokia.com.cn/places/v1/discover/search?at={0}%2C{1}&q={2}&tf=plain&pretty=y&size=10&app_id=Or94PrudKATbv6vmtnzb&app_code=FbGjsdJkxVvTF2OcdOuBGA",
                        CurrentLocation.Latitude, CurrentLocation.Longitude, HttpUtility.UrlEncode(query)
                    ));

                    var request = HttpWebRequest.CreateHttp("http://places.nlp.nokia.com.cn/places/v1/places/loc-dmVyc2lvbj0xO3RpdGxlPUppemhlbitSZCsxMjA7bGF0PTMxLjIzOTU0OTA2MjcxNzU7bG9uPTEyMS4zNzA3NjU4OTA2MjU7c3RyZWV0PUppemhlbitSZDtob3VzZT0xMjA7Y2l0eT1TaGFuZ2hhaTtwb3N0YWxDb2RlPTtjb3VudHJ5PUNITjtkaXN0cmljdD1QdXR1bytEaXN0cmljdDtzdGF0ZUNvZGU9U2hhbmdoYWk7Y2F0ZWdvcnlJZD1idWlsZGluZw;context=Zmxvdy1pZD1hYjQzOThjYS03ODQ1LTQ4YzItYmI4MC1jZjRkMmExN2Y0ZmRfMTM1OTkwMDMwMjE1M18wXzkzODM?app_id=Or94PrudKATbv6vmtnzb&app_code=FbGjsdJkxVvTF2OcdOuBGA");
                    request.BeginGetResponse((arg) =>
                    {
                        var response = request.EndGetResponse(arg);

                        var stream = response.GetResponseStream();

                        StreamReader sr = new StreamReader(stream);

                        var str = sr.ReadToEnd();
                        response.Close();
                    }, new object());
#endif
*/
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
                            MyGoogleMap.SetView(
                                new LocationRect(result.geometry.viewport.northeast.lat, 
                                    result.geometry.viewport.southwest.lng, 
                                    result.geometry.viewport.southwest.lat, 
                                    result.geometry.viewport.northeast.lng
                                ));

                            var pp = new Pushpin();
                            pp.Background = new SolidColorBrush(Colors.Green);
                            pp.Location = new GeoCoordinate(result.geometry.location.lat, result.geometry.location.lng);
                            GoogleMap_PushPinLayer.Children.Add(pp);

                            TextBlock_Address.Text = result.formatted_address;
                            String[] strTypes = new String[result.types.Count()];
                            int i = 0;
                            foreach (var t in result.types)
                            {
                                strTypes[i++] = t.ToString();
                            }
                            TextBlock_AddressType.Text = strTypes.Aggregate((a, b) => a + " " + b);
                            TextBlock_LatLng.Text = String.Format("{0},{1}", result.geometry.location.lat, result.geometry.location.lng);
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
                            MyNokiaMap.SetView(coordinate, 18);

                            //...Add pushpin
                            NokiaMap_PushPinLayer.Children.Clear();
                            Pushpin pushpin1 = new Pushpin();
                            pushpin1.Background = new SolidColorBrush(Colors.Red);
                            pushpin1.Location = coordinate;

                            NokiaMap_PushPinLayer.Children.Add(pushpin1);
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

        private void StaticMap_ManipulationCompleted_1(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            var map = sender as JeffWilcox.Controls.StaticMap;
            var pnt = e.TotalManipulation.Translation;

            map.SetValue(JeffWilcox.Controls.StaticMap.MapCenterProperty,
                new GeoCoordinate(map.MapCenter.Latitude - Math.Sign(pnt.Y) * 0.01, map.MapCenter.Longitude - Math.Sign(pnt.X) * 0.005));
        }
    }
}