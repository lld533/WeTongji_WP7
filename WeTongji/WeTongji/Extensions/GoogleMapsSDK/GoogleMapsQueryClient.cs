using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;

namespace WeTongji.Extensions.GoogleMapsSDK
{
    public class GoogleMapsQueryClient
    {
        #region [Event Handlers]

        public EventHandler<GoogleMapsQueryCompletedEventArgs> ExecuteCompleted;

        public EventHandler<GoogleMapsQueryFailedEventArgs> ExecuteFailed;

        private void OnExecuteCompleted(GoogleMapsQueryRequest req, GoogleMapsQueryResponse res)
        {
            var handler = ExecuteCompleted;
            if (handler != null)
            {
                handler(new object(), new GoogleMapsQueryCompletedEventArgs(req, res));
            }
        }

        private void OnExecuteFailed(Exception err)
        {
            var handler = ExecuteFailed;
            if (handler != null)
            {
                handler(new object(), new GoogleMapsQueryFailedEventArgs(err));
            }
        }

        #endregion

        #region [Execute]

        /// <summary>
        /// Execute query to Google Maps
        /// </summary>
        /// <param name="req"></param>
        /// <remarks>
        /// This method uses reflection.
        /// </remarks>
        public void ExecuteAsync(GoogleMapsQueryRequest req)
        {
            try
            {
                if (req == null)
                    throw new ArgumentNullException("req");

                #region [Make Url]

                var url = "http://maps.googleapis.com/maps/api/geocode/json?";

                var properties = typeof(GoogleMapsQueryRequest).GetProperties();
                String[] strs = new String[properties.Count()];

                int i = 0;
                foreach (var pi in properties)
                {
                    strs[i++] = String.Format("{0}={1}", pi.Name, pi.GetGetMethod(false).Invoke(req, null).ToString().ToLower());
                }

                url += strs.Aggregate((a, b) => a + "&" + b);

                #endregion

                var webRequest = WebRequest.CreateHttp(url);

                webRequest.BeginGetResponse((args) =>
                {
                    try
                    {
                        var webResponse = webRequest.EndGetResponse(args);
                        using (var sr = new StreamReader(webResponse.GetResponseStream()))
                        {
                            var str = sr.ReadToEnd();
                            var res = JsonConvert.DeserializeObject<GoogleMapsQueryResponse>(str);
                            if (res.status != Status.OK)
                            {
                                throw new GoogleMapsQueryException(res.status);
                            }
                            OnExecuteCompleted(req, res);
                        }
                        webResponse.Close();
                    }
                    catch (System.Exception ex)
                    {
                        OnExecuteFailed(ex);
                    }
                }, new object());
            }
            catch (System.Exception ex)
            {
                OnExecuteFailed(ex);
            }
        }

        #endregion
    }

    #region [EventArgs]

    public class GoogleMapsQueryCompletedEventArgs : EventArgs
    {
        public GoogleMapsQueryRequest Request { get; private set; }
        public GoogleMapsQueryResponse Response { get; private set; }

        public GoogleMapsQueryCompletedEventArgs(GoogleMapsQueryRequest req, GoogleMapsQueryResponse res)
        {
            Request = req;
            Response = res;
        }
    }

    public class GoogleMapsQueryFailedEventArgs : EventArgs
    {
        public Exception Error { get; private set; }

        public GoogleMapsQueryFailedEventArgs(Exception err)
        {
            Error = err;
        }
    }

    #endregion
}
