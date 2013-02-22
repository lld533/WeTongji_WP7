using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using WeTongji.Api.Domain;

namespace WeTongji.DataBase
{
    public class WTShareDataContext : DataContext
    {
        private static String ShareDBConnectionString = "Data Source='isostore:/WeTongji.sdf'";

        private WTShareDataContext(String connectionString) : base(connectionString) { }

        public static WTShareDataContext ShareDB
        {
            get 
            {
                return new WTShareDataContext(ShareDBConnectionString);
            }
        }

        #region [Tables]

        public Table<PersonExt> People;

        public Table<ImageExt> Images;

        public Table<ActivityExt> Activities;

        public Table<ChannelExt> Channels;

        public Table<SchoolNewsExt> SchoolNewsTable;

        public Table<AroundExt> AroundTable;

        public Table<ForStaffExt> ForStaffTable;

        public Table<ClubNewsExt> ClubNewsTable;

        public Table<Event> Events;

        #endregion
    }

    public class WTUserDataContext : DataContext
    {
        public WTUserDataContext(String uid) : base(String.Format("Data Source='isostore:/{0}.sdf'", uid)) { }

        #region [Tables]

        public Table<UserExt> UserInfo;
        
        public Table<ImageExt> Images;
        
        public Table<Course> Courses;

        public Table<Exam> Exams;

        #endregion
    }
}
