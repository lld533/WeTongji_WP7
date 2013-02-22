using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WeTongji.Api;
using WeTongji.Api.Domain;
using WeTongji.Api.Response;
using System.Net;

namespace WeTongji.Api
{
    public class WTDownloadImageClient
    {
        public event EventHandler<WTDownloadImageStartedEventArgs> DownloadImageStarted;
        public event EventHandler<WTDownloadImageCompletedEventArgs> DownloadImageCompleted;
        public event EventHandler<WTDownloadImageFailedEventArgs> DownloadImageFailed;

        private void OnDownloadImageStarted(String url)
        {
            var handler = DownloadImageStarted;
            if (handler != null)
                handler(this, new WTDownloadImageStartedEventArgs(url));
        }

        private void OnDownloadImageCompleted(String url, Stream stream)
        {
            var handler = DownloadImageCompleted;
            if (handler != null)
                handler(this, new WTDownloadImageCompletedEventArgs(url, stream));
        }

        private void OnDownloadImageFailed(String url, Exception err)
        {
            var handler = DownloadImageFailed;
            if (handler != null)
                handler(this, new WTDownloadImageFailedEventArgs(url, err));
        }

        public void Execute(String url)
        {
            try
            {
                var req = HttpWebRequest.CreateHttp(url);

                OnDownloadImageStarted(url);
                req.BeginGetResponse((args) =>
                {
                    try
                    {
                        var res = req.EndGetResponse(args);

                        var stream = new MemoryStream();

                        using (var rs = res.GetResponseStream())
                        {
                            rs.CopyTo(stream);
                        }

                        res.Close();

                        OnDownloadImageCompleted(url, stream);
                    }
                    catch (System.Exception ex)
                    {
                        OnDownloadImageFailed(url, ex);
                    }

                }, new object());

            }
            catch (Exception ex)
            {
                OnDownloadImageFailed(url, ex);
            }
        }
    }

    public class WTDownloadImageStartedEventArgs : EventArgs
    {
        public String Url { get; private set; }

        public WTDownloadImageStartedEventArgs(String url)
        {
            Url = url;
        }
    }

    public class WTDownloadImageCompletedEventArgs : EventArgs
    {
        public String Url { get; private set; }
        public Stream ImageStream { get; private set; }

        public WTDownloadImageCompletedEventArgs(String url, Stream img)
        {
            Url = url;
            ImageStream = img;
        }
    }

    public class WTDownloadImageFailedEventArgs : EventArgs
    {
        public String Url { get; private set; }
        public Exception Error { get; private set; }

        public WTDownloadImageFailedEventArgs(String url, Exception err)
        {
            Url = url;
            Error = err;
        }
    }
}
