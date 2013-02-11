using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeTongji.Api;
using WeTongji.Api.Request;
using WeTongji.Api.Response;
using System.Diagnostics;
using System.Net;

namespace WTSDKTester
{
    class Program
    {
        public static void MyExecuteCompleted(Object sender, WTExecuteCompletedEventArgs<UserLogOnResponse> e)
        {
            var response = e.Result;
            Debug.WriteLine(response.User.DisplayName);
        }

        public static void MyExecuteFailed(Object sender, WTExecuteFailedEventArgs<UserLogOnResponse> e)
        {
            var err = e.Error;
            Debug.WriteLine(err);
        }

        static void Main(string[] args)
        {
            var req = new WeTongji.Api.Request.UserLogOnRequest();
            req.NO = "092983";
            req.Password = "123456";

            var client = new WTDefaultClient<UserLogOnResponse>();
            client.ExecuteCompleted += MyExecuteCompleted;
            client.ExecuteFailed += MyExecuteFailed;

            client.Execute(req);
        }
    }
}
