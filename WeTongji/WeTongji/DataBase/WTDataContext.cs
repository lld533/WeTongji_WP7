using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using WeTongji.Api.Domain;

namespace WeTongji.DataBase
{
    public class WTDataContext : DataContext
    {
        public static String DBConntectionString = "Data Source='isostore:/WeTongji.sdf'";

        public WTDataContext(String connectionString) : base(connectionString) { }

        #region [Tables]

        public Table<PersonExt> People;

        public Table<ImageExt> Images;

        public Table<UserExt> Users;

        public Table<ActivityExt> Activities;

        public Table<ChannelExt> Channels;

        public Table<SchoolNewsExt> SchoolNewsTable;

        public Table<AroundExt> AroundTable;

        public Table<ForStaffExt> ForStaffTable;

        public Table<ClubNewsExt> ClubNewsTable;

        public Table<Course> Courses;

        public Table<Event> Events;

        public Table<Exam> Exams;

        #endregion
    }
}
