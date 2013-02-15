﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Diagnostics;
using WeTongji.Api;
using WeTongji.Api.Request;
using WeTongji.Api.Response;
using WeTongji.DataBase;
using WeTongji.Api.Domain;

namespace WeTongji
{
    public partial class TestSDKPage : PhoneApplicationPage
    {
        public TestSDKPage()
        {
            InitializeComponent();
        }

        private void Test(Object sender, RoutedEventArgs e)
        {
#if false
            var req = new PeopleGetRequest<PeopleGetResponse>();

            var client = new WTDefaultClient<PeopleGetResponse>();

            client.ExecuteCompleted += MyExecuteCompleted;
            client.ExecuteFailed += MyExecuteFailed;

            client.Execute(req);
#endif

            var req = new UserLogOnRequest<UserLogOnResponse>();
            req.NO = "092983";
            req.Password = "123456";
            
            var client = new WTDefaultClient<UserLogOnResponse>();

            client.ExecuteCompleted += LogOnExecuteCompleted;
            client.ExecuteFailed += LogOnExecuteFailed;

            client.Execute(req);
        }

        private String session = String.Empty;
        private User user = null;

        private void LogOnExecuteCompleted(Object sender, WTExecuteCompletedEventArgs<UserLogOnResponse> e)
        {
            session = e.Result.Session;
            user = e.Result.User;

            UserUpdateRequest<WTResponse> req = new UserUpdateRequest<WTResponse>();
            req.User = e.Result.User;
            req.QQ = "1234567890";

            var client = new WTDefaultClient<WTResponse>();
            
            client.ExecuteCompleted += UpdateUserCompleted;
            client.ExecuteFailed += UpdateUserFailed;

            client.Post(req, session, user.UID);
        }

        private void LogOnExecuteFailed(Object sender, WTExecuteFailedEventArgs<UserLogOnResponse> e)
        {
            var err = e.Error;
            Debug.WriteLine(err);
        }

        private void UpdateUserCompleted(Object sender, WTExecuteCompletedEventArgs<WTResponse> e)
        {
            Debug.WriteLine("update user succeeded!");
        }

        private void UpdateUserFailed(Object sender, WTExecuteFailedEventArgs<WTResponse> e)
        {
            var err = e.Error;
            Debug.WriteLine(err);
        }

        private void ViewDB(Object sender, RoutedEventArgs e)
        {
            using (var db = new WTDataContext(WTDataContext.DBConntectionString))
            {
                var people = db.People;
                foreach (var person in people)
                {
                    Debug.WriteLine(person.Name);
                }
            }
        }

        private void MyExecuteCompleted(Object sender, WTExecuteCompletedEventArgs<PeopleGetResponse> e)
        {
            var response = e.Result;

            try
            {
                using (var db = new WTDataContext(WTDataContext.DBConntectionString))
                {
                    db.DeleteDatabase();
                    db.CreateDatabase();

                    foreach (var p in e.Result.People)
                    {
                        var personExt = new WeTongji.Api.Domain.PersonExt(p);
                        db.People.InsertOnSubmit(personExt);
                    }
                    db.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }


        private void MyExecuteFailed(Object sender, WTExecuteFailedEventArgs<PeopleGetResponse> e)
        {
            var err = e.Error;
            Debug.WriteLine(err);
        }
    }
}