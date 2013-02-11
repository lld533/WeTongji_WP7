using System;
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
            var req = new PeopleGetRequest<PeopleGetResponse>();

            var client = new WTDefaultClient<PeopleGetResponse>();

            client.ExecuteCompleted += MyExecuteCompleted;
            client.ExecuteFailed += MyExecuteFailed;

            client.Execute(req);
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