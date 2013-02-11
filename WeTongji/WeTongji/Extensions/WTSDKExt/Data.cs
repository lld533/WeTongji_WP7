using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using WeTongji.DataBase;

namespace WeTongji.Api.Domain
{
    public interface IWTObjectExt
    {
        void SetObject(WTObject obj);
        WTObject GetObject();
        Type ExpectedType();
    }

    [Table(Name = "Image")]
    public class ImageExt : IWTObjectExt
    {
        #region [Properties]

        [Column(IsPrimaryKey=true)]
        public Guid Id { get; set; }

        [Column]
        public String Description { get; set; }

        [Column]
        public String Url { get; set; }

        #endregion

        #region [Implementation]

        /// <summary>
        /// Throw NotImplementedException
        /// </summary>
        /// <param name="obj"></param>
        public void SetObject(WTObject obj) { throw new NotImplementedException(); }

        /// <summary>
        /// Throw NotImplementedException
        /// </summary>
        /// <returns></returns>
        public WTObject GetObject() { throw new NotImplementedException(); }

        /// <summary>
        /// Throw NotImplementedException
        /// </summary>
        /// <returns></returns>
        public Type ExpectedType() { throw new NotImplementedException(); }

        #endregion

        #region [Constructor]

        public ImageExt() 
        {
            Id = Guid.NewGuid();
        }

        public ImageExt(String url, String des = "")
        {
            Id = Guid.NewGuid();
            Url = url;
            Description = des;
        }

        #endregion
    }

    [Table(Name = "Person")]
    public class PersonExt : IWTObjectExt
    {
        #region [Basic Properties]

        [Column(IsPrimaryKey = true)]
        public String Id { get; set; }

        [Column()]
        public String Name { get; set; }

        [Column()]
        public String JobTitle { get; set; }

        [Column()]
        public String Words { get; set; }

        [Column()]
        public String NO { get; set; }

        [Column()]
        public String Avatar { get; set; }

        [Column()]
        public String StudentNO { get; set; }

        [Column()]
        public String Description { get; set; }

        [Column()]
        public String Read { get; set; }

        [Column()]
        public int Like { get; set; }

        [Column()]
        public bool CanLike { get; set; }

        [Column()]
        public int Favorite { get; set; }

        [Column()]
        public bool CanFavorite { get; set; }

        [Column()]
        public String SerializedImages { get; set; }

        #endregion

        #region [Extended Properties]

        [Column()]
        public String ImageExtList { get; set; }

        [Column()]
        public Guid AvatarGuid { get; set; }

        #endregion

        #region [Extended Methods]

        public Dictionary<String, String> GetImages()
        {
            var Images = new Dictionary<string, string>();

            if (!String.IsNullOrEmpty(SerializedImages))
            {
                var kvPairs = SerializedImages.Split(',');
                foreach (var pair in kvPairs)
                {
                    var strs = pair.Split(':');
                    Images.Add(strs[0].Trim('\"'), strs[1].Trim('\"'));
                }
            }

            return Images;
        }

        #endregion

        #region [Implementation]

        public WTObject GetObject()
        {
            var p = new WeTongji.Api.Domain.Person();

            p.Id = this.Id;
            p.Name = this.Name;
            p.JobTitle = this.JobTitle;
            p.Words = this.Words;
            p.NO = this.NO;
            p.Avatar = this.Avatar;
            p.StudentNO = this.StudentNO;
            p.Images = GetImages();
            p.Description = this.Description;
            p.Read = this.Read;
            p.Like = this.Like;
            p.CanLike = this.CanLike;
            p.Favorite = this.Favorite;
            p.CanFavorite = this.CanFavorite;

            return p;
        }

        public void SetObject(WTObject obj)
        {
            #region [Check argument]

            if (obj == null)
                throw new ArgumentNullException("obj");

            if (!(obj is WeTongji.Api.Domain.Person))
            {
                throw new ArgumentOutOfRangeException("obj");
            }

            #endregion

            var p = obj as WeTongji.Api.Domain.Person;

            #region [Save Images]

            if (p.Images.Count > 0)
            {
                #region [Make SerializedImages]

                {
                    String[] strs = new String[p.Images.Count];
                    StringBuilder sb = new StringBuilder();

                    foreach (var kvp in p.Images)
                    {
                        sb.Append(String.Format("\"{0}\":\"{1}\",", kvp.Key, kvp.Value));
                    }

                    SerializedImages = sb.ToString().TrimEnd(',');
                }

                #endregion

                #region [Make ImageExt List]

                {
                    var imgList = new ImageExt[p.Images.Count];
                    int i = 0;
                    StringBuilder sb = new StringBuilder();

                    foreach (var kvp in p.Images)
                    {
                        imgList[i] = new WeTongji.Api.Domain.ImageExt(kvp.Key, kvp.Value);
                        sb.AppendFormat("{0};", imgList[i++].Id.ToString());
                    }

                    using (var db = new WTDataContext(WTDataContext.DBConntectionString))
                    {
                        db.Images.InsertAllOnSubmit(imgList);
                        db.SubmitChanges();
                    }

                    ImageExtList = sb.ToString().TrimEnd(';');
                }

                #endregion
            }
            else
            {
                SerializedImages = String.Empty;
            }

            #region [Save Avatar]

            {
                var avatarImg = new ImageExt();
                using (var db = new WTDataContext(WTDataContext.DBConntectionString))
                {
                    db.Images.InsertOnSubmit(avatarImg);
                    db.SubmitChanges();
                }
                AvatarGuid = avatarImg.Id;
            }

            #endregion

            #endregion

            #region [Save Basic Properties]

            this.Id = p.Id;
            this.Name = p.Name;
            this.JobTitle = p.JobTitle;
            this.Words = p.Words;
            this.NO = p.NO;
            this.Avatar = p.Avatar;
            this.StudentNO = p.StudentNO;
            this.Description = p.Description;
            this.Read = p.Read;
            this.Like = p.Like;
            this.CanLike = p.CanLike;
            this.Favorite = p.Favorite;
            this.CanFavorite = p.CanFavorite;

            #endregion
        }

        public Type ExpectedType() { return typeof(WeTongji.Api.Domain.Person); }

        #endregion

        #region [Constructor]

        public PersonExt() { }

        public PersonExt(Person p) { SetObject(p); }

        #endregion
    }

    [Table(Name = "User")]
    public class UserExt : IWTObjectExt
    {
        #region [Basic Properties]

        [Column(IsPrimaryKey = true)]
        public String NO { get; set; }

        [Column()]
        public String Name { get; set; }

        [Column()]
        public String Avatar { get; set; }

        [Column()]
        public String UID { get; set; }

        [Column()]
        public String Phone { get; set; }

        [Column()]
        public String DisplayName { get; set; }

        [Column()]
        public String Major { get; set; }

        [Column()]
        public String NativePlace { get; set; }

        [Column()]
        public String Degree { get; set; }

        [Column()]
        public String Gender { get; set; }

        [Column()]
        public String Year { get; set; }

        [Column()]
        public DateTime Birthday { get; set; }

        [Column()]
        public String Plan { get; set; }

        [Column()]
        public String SinaWeibo { get; set; }

        [Column()]
        public String QQ { get; set; }

        [Column()]
        public String Department { get; set; }

        [Column()]
        public String Email { get; set; }

        #endregion

        #region [Extended Properties]

        [Column()]
        public Guid AvatarGuid { get; set; }

        #endregion

        #region [Implementation]

        public void SetObject(WTObject obj)
        {
            #region [Check argument]

            if (obj == null)
                throw new ArgumentNullException("obj");

            if (!(obj is WeTongji.Api.Domain.User))
                throw new ArgumentOutOfRangeException("obj");

            #endregion

            var user = obj as WeTongji.Api.Domain.User;

            #region [Save Basic Properties]

            this.NO = user.NO;
            this.Name = user.Name;
            this.Avatar = user.Avatar;
            this.UID = user.UID;
            this.Phone = user.Phone;
            this.DisplayName = user.DisplayName;
            this.Major = user.Major;
            this.NativePlace = user.NativePlace;
            this.Degree = user.Degree;
            this.Gender = user.Gender;
            this.Year = user.Year;
            this.Birthday = user.Birthday;
            this.Plan = user.Plan;
            this.SinaWeibo = user.SinaWeibo;
            this.QQ = user.QQ;
            this.Department = user.Department;
            this.Email = user.Email;

            #endregion

            #region [Save Extended Properties]

            var img = new ImageExt() { Url = Avatar };
            using (var db = new WTDataContext(WTDataContext.DBConntectionString))
            {
                db.Images.InsertOnSubmit(img);
                db.SubmitChanges();
            }
            AvatarGuid = img.Id;

            #endregion
        }

        public WTObject GetObject()
        {
            var user = new WeTongji.Api.Domain.User();

            user.NO = this.NO;
            user.Name = this.Name;
            user.Avatar = this.Avatar;
            user.UID = this.UID;
            user.Phone = this.Phone;
            user.DisplayName = this.DisplayName;
            user.Major = this.Major;
            user.NativePlace = this.NativePlace;
            user.Degree = this.Degree;
            user.Gender = this.Gender;
            user.Year = this.Year;
            user.Birthday = this.Birthday;
            user.Plan = this.Plan;
            user.SinaWeibo = this.SinaWeibo;
            user.QQ = this.QQ;
            user.Department = this.Department;
            user.Email = this.Email;

            return user;
        }

        public Type ExpectedType()
        {
            return typeof(WeTongji.Api.Domain.User);
        }

        #endregion
    }

    [Table(Name = "Activity")]
    public class ActivityExt : IWTObjectExt
    {
        #region [Basic Properties]

        [Column(IsPrimaryKey = true)]
        public int Id { get; set; }

        [Column()]
        public DateTime Begin { get; set; }

        [Column()]
        public DateTime End { get; set; }

        [Column()]
        public String Title { get; set; }

        [Column()]
        public String Location { get; set; }

        [Column()]
        public String Description { get; set; }

        [Column()]
        public int Like { get; set; }

        [Column()]
        public bool CanLike { get; set; }

        [Column()]
        public int Favorite { get; set; }

        [Column()]
        public bool CanFavorite { get; set; }

        [Column()]
        public int Schedule { get; set; }

        [Column()]
        public bool CanSchedule { get; set; }

        [Column()]
        public int Channel_id { get; set; }

        [Column()]
        public String Organizer { get; set; }

        [Column()]
        public String OrganizerAvatar { get; set; }

        [Column()]
        public String Status { get; set; }

        [Column()]
        public String Image { get; set; }

        [Column()]
        public DateTime CreatedAt { get; set; }

        #endregion

        #region [Extended Properties]

        public Guid ImageGuid { get; set; }

        public Guid OrganizerAvatarGuid { get; set; }

        #endregion

        #region [Implementation]

        public void SetObject(WTObject obj)
        {
            #region [Check Argument]

            if (obj == null)
                throw new ArgumentNullException("obj");
            if (!(obj is WeTongji.Api.Domain.Activity))
                throw new ArgumentOutOfRangeException("obj");

            #endregion

            var a = obj as WeTongji.Api.Domain.Activity;

            #region [Save Basic Properties]

            this.Id = a.Id;
            this.Begin = a.Begin;
            this.End = a.End;
            this.Title = a.Title;
            this.Location = a.Location;
            this.Description = a.Description;
            this.Like = a.Like;
            this.CanLike = a.CanLike;
            this.Favorite = a.Favorite;
            this.CanFavorite = a.CanFavorite;
            this.Schedule = a.Schedule;
            this.CanSchedule = a.CanSchedule;
            this.Organizer = a.Organizer;
            this.OrganizerAvatar = a.OrganizerAvatar;
            this.Status = a.Status;
            this.Image = a.Image;
            this.CreatedAt = this.CreatedAt;

            #endregion

            #region [Save Extended Properties]

            {
                if (!String.IsNullOrEmpty(Image))
                {
                    var img = new ImageExt() { Url = Image };
                    using (var db = new WTDataContext(WTDataContext.DBConntectionString))
                    {
                        db.Images.InsertOnSubmit(img);
                        db.SubmitChanges();
                    }
                    ImageGuid = img.Id;
                }
            }

            {
                if (!String.IsNullOrEmpty(OrganizerAvatar))
                {
                    var img = new ImageExt() { Url = OrganizerAvatar };
                    using (var db = new WTDataContext(WTDataContext.DBConntectionString))
                    {
                        db.Images.InsertOnSubmit(img);
                        db.SubmitChanges();
                    }
                    OrganizerAvatarGuid = img.Id;
                }
            }

            #endregion
        }

        public WTObject GetObject()
        {
            var a = new WeTongji.Api.Domain.Activity();
            a.Id = this.Id;
            a.Begin = this.Begin;
            a.End = this.End;
            a.Title = this.Title;
            a.Location = this.Location;
            a.Description = this.Description;
            a.Like = this.Like;
            a.CanLike = this.CanLike;
            a.Favorite = this.Favorite;
            a.CanFavorite = this.CanFavorite;
            a.Schedule = this.Schedule;
            a.CanSchedule = this.CanSchedule;
            a.Channel_id = this.Channel_id;
            a.Organizer = this.Organizer;
            a.OrganizerAvatar = this.OrganizerAvatar;
            a.Status = this.Status;
            a.Image = this.Image;
            a.CreatedAt = this.CreatedAt;

            return a;
        }

        public Type ExpectedType() { return typeof(WeTongji.Api.Domain.Activity); }

        #endregion
    }

    [Table(Name = "Channel")]
    public class ChannelExt : IWTObjectExt
    {
        #region [Basic Properties]

        [Column(IsPrimaryKey = true)]
        public int Id { get; set; }

        [Column()]
        public String Title { get; set; }

        [Column()]
        public String Image { get; set; }

        [Column()]
        public int Follow { get; set; }

        [Column()]
        public String Description { get; set; }

        #endregion

        #region [Extended Property]

        [Column()]
        public Guid ImageGuid { get; set; }

        #endregion

        #region [Implementation]

        public WTObject GetObject()
        {
            var c = new WeTongji.Api.Domain.Channel();

            c.Id = this.Id;
            c.Title = this.Title;
            c.Image = this.Image;
            c.Follow = this.Follow;
            c.Description = this.Description;

            return c;
        }

        public void SetObject(WTObject obj)
        {
            #region [Check Argument]

            if (obj == null)
                throw new ArgumentNullException("obj");

            if (!(obj is WeTongji.Api.Domain.Channel))
                throw new ArgumentOutOfRangeException("obj");

            #endregion

            var c = obj as WeTongji.Api.Domain.Channel;

            #region [Save Basic Properties]

            this.Id = c.Id;
            this.Title = c.Title;
            this.Image = c.Image;
            this.Follow = c.Follow;
            this.Description = c.Description;

            #endregion

            #region [Save Extended Property]

            var img = new ImageExt() { Url = Image };
            using (var db = new WTDataContext(WTDataContext.DBConntectionString))
            {
                db.Images.InsertOnSubmit(img);
                db.SubmitChanges();
            }
            ImageGuid = img.Id;

            #endregion
        }

        public Type ExpectedType() { return typeof(WeTongji.Api.Domain.Channel); }

        #endregion
    }

    [Table(Name="SchoolNews")]
    public class SchoolNewsExt : IWTObjectExt
    {
        #region [Basic Properties]

        [Column(IsPrimaryKey = true)]
        public int Id { get; set; }

        [Column()]
        public String Title { get; set; }

        [Column()]
        public String Source { get; set; }

        [Column()]
        public String Summary { get; set; }

        [Column()]
        public String Context { get; set; }

        [Column()]
        public DateTime CreatedAt { get; set; }

        [Column()]
        public int Read { get; set; }

        [Column()]
        public int Like { get; set; }

        [Column()]
        public bool CanLike { get; set; }

        [Column()]
        public int Favorite { get; set; }

        [Column()]
        public bool CanFavorite { get; set; }

        #endregion

        #region [Extended Properties]

        [Column()]
        public String SerializedImages { get; set; }

        #endregion

        #region [Implementation]

        public virtual void SetObject(WTObject obj)
        {
            #region [Check Arguments]
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (!(obj is WeTongji.Api.Domain.WTNews))
                throw new ArgumentOutOfRangeException("obj");
            #endregion

            var news = obj as WeTongji.Api.Domain.WTNews;

            #region [Save Basic Properties]

            this.Id = news.Id;
            this.Title = news.Title;
            this.Source = news.Source;
            this.Summary = news.Summary;
            this.Context = news.Context;
            this.CreatedAt = news.CreatedAt;
            this.Read = news.Read;
            this.Like = news.Like;
            this.CanLike = news.CanLike;
            this.Favorite = news.Favorite;
            this.CanFavorite = news.CanFavorite;

            #endregion

            #region [Save Extended Properties]

            SerializedImages = news.Images.Aggregate((a, b) => String.Format("\"{0}\",\"{1}\"", a, b));

            #endregion

        }

        public virtual WTObject GetObject()
        {
            var news = new WeTongji.Api.Domain.WTNews();

            news.Id = this.Id;
            news.Title = this.Title;
            news.Source = this.Source;
            news.Summary = this.Summary;
            news.Context = this.Context;
            news.CreatedAt = this.CreatedAt;
            news.Read = this.Read;
            news.Like = this.Like;
            news.CanLike = this.CanLike;
            news.Favorite = this.Favorite;
            news.CanFavorite = this.CanFavorite;

            var imgs = SerializedImages.Split(',');
            for (int i = 0; i < imgs.Count(); ++i)
            {
                imgs[i] = imgs[i].Trim('\"');
            }
            news.Images = imgs;

            return news;
        }

        public virtual Type ExpectedType() { return typeof(WeTongji.Api.Domain.WTNews); }

        #endregion
    }

    [Table(Name="Around")]
    public class AroundExt : IWTObjectExt
    {
        #region [Basic Properties]

        [Column(IsPrimaryKey = true)]
        public int Id { get; set; }

        [Column()]
        public String Title { get; set; }

        [Column()]
        public String Source { get; set; }

        [Column()]
        public String Summary { get; set; }

        [Column()]
        public String Context { get; set; }

        [Column()]
        public DateTime CreatedAt { get; set; }

        [Column()]
        public int Read { get; set; }

        [Column()]
        public int Like { get; set; }

        [Column()]
        public bool CanLike { get; set; }

        [Column()]
        public int Favorite { get; set; }

        [Column()]
        public bool CanFavorite { get; set; }

        #endregion

        #region [Extended Properties]

        [Column()]
        public String SerializedImages { get; set; }

        #endregion

        #region [Implementation]

        public virtual void SetObject(WTObject obj)
        {
            #region [Check Arguments]
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (!(obj is WeTongji.Api.Domain.WTNews))
                throw new ArgumentOutOfRangeException("obj");
            #endregion

            var news = obj as WeTongji.Api.Domain.WTNews;

            #region [Save Basic Properties]

            this.Id = news.Id;
            this.Title = news.Title;
            this.Source = news.Source;
            this.Summary = news.Summary;
            this.Context = news.Context;
            this.CreatedAt = news.CreatedAt;
            this.Read = news.Read;
            this.Like = news.Like;
            this.CanLike = news.CanLike;
            this.Favorite = news.Favorite;
            this.CanFavorite = news.CanFavorite;

            #endregion

            #region [Save Extended Properties]

            SerializedImages = news.Images.Aggregate((a, b) => String.Format("\"{0}\",\"{1}\"", a, b));

            #endregion

        }

        public virtual WTObject GetObject()
        {
            var news = new WeTongji.Api.Domain.WTNews();

            news.Id = this.Id;
            news.Title = this.Title;
            news.Source = this.Source;
            news.Summary = this.Summary;
            news.Context = this.Context;
            news.CreatedAt = this.CreatedAt;
            news.Read = this.Read;
            news.Like = this.Like;
            news.CanLike = this.CanLike;
            news.Favorite = this.Favorite;
            news.CanFavorite = this.CanFavorite;

            var imgs = SerializedImages.Split(',');
            for (int i = 0; i < imgs.Count(); ++i)
            {
                imgs[i] = imgs[i].Trim('\"');
            }
            news.Images = imgs;

            return news;
        }

        public virtual Type ExpectedType() { return typeof(WeTongji.Api.Domain.WTNews); }

        #endregion
    }

    [Table(Name="ForStaff")]
    public class ForStaffExt : IWTObjectExt
    {
        #region [Basic Properties]

        [Column(IsPrimaryKey = true)]
        public int Id { get; set; }

        [Column()]
        public String Title { get; set; }

        [Column()]
        public String Source { get; set; }

        [Column()]
        public String Summary { get; set; }

        [Column()]
        public String Context { get; set; }

        [Column()]
        public DateTime CreatedAt { get; set; }

        [Column()]
        public int Read { get; set; }

        [Column()]
        public int Like { get; set; }

        [Column()]
        public bool CanLike { get; set; }

        [Column()]
        public int Favorite { get; set; }

        [Column()]
        public bool CanFavorite { get; set; }

        #endregion

        #region [Extended Properties]

        [Column()]
        public String SerializedImages { get; set; }

        #endregion

        #region [Implementation]

        public virtual void SetObject(WTObject obj)
        {
            #region [Check Arguments]
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (!(obj is WeTongji.Api.Domain.WTNews))
                throw new ArgumentOutOfRangeException("obj");
            #endregion

            var news = obj as WeTongji.Api.Domain.WTNews;

            #region [Save Basic Properties]

            this.Id = news.Id;
            this.Title = news.Title;
            this.Source = news.Source;
            this.Summary = news.Summary;
            this.Context = news.Context;
            this.CreatedAt = news.CreatedAt;
            this.Read = news.Read;
            this.Like = news.Like;
            this.CanLike = news.CanLike;
            this.Favorite = news.Favorite;
            this.CanFavorite = news.CanFavorite;

            #endregion

            #region [Save Extended Properties]

            SerializedImages = news.Images.Aggregate((a, b) => String.Format("\"{0}\",\"{1}\"", a, b));

            #endregion

        }

        public virtual WTObject GetObject()
        {
            var news = new WeTongji.Api.Domain.WTNews();

            news.Id = this.Id;
            news.Title = this.Title;
            news.Source = this.Source;
            news.Summary = this.Summary;
            news.Context = this.Context;
            news.CreatedAt = this.CreatedAt;
            news.Read = this.Read;
            news.Like = this.Like;
            news.CanLike = this.CanLike;
            news.Favorite = this.Favorite;
            news.CanFavorite = this.CanFavorite;

            var imgs = SerializedImages.Split(',');
            for (int i = 0; i < imgs.Count(); ++i)
            {
                imgs[i] = imgs[i].Trim('\"');
            }
            news.Images = imgs;

            return news;
        }

        public virtual Type ExpectedType() { return typeof(WeTongji.Api.Domain.WTNews); }

        #endregion
    }

    [Table(Name = "ClubNews")]
    public class ClubNewsExt
    {
        #region [Basic Properties]

        [Column()]
        public String Organizer { get; set; }

        [Column()]
        public String OrganizerAvatar { get; set; }

        [Column(IsPrimaryKey = true)]
        public int Id { get; set; }

        [Column()]
        public String Title { get; set; }

        [Column()]
        public String Source { get; set; }

        [Column()]
        public String Summary { get; set; }

        [Column()]
        public String Context { get; set; }

        [Column()]
        public DateTime CreatedAt { get; set; }

        [Column()]
        public int Read { get; set; }

        [Column()]
        public int Like { get; set; }

        [Column()]
        public bool CanLike { get; set; }

        [Column()]
        public int Favorite { get; set; }

        [Column()]
        public bool CanFavorite { get; set; }


        #endregion

        #region [Extended Properties]

        [Column()]
        public String SerializedImages { get; set; }

        [Column()]
        public Guid OrganizerAvatarGuid { get; set; }

        #endregion

        #region [Implementation]

        public Type ExpectedType()
        {
            return typeof(WeTongji.Api.Domain.ClubNews);
        }

        public void SetObject(WTObject obj)
        {
            #region [Check Argument]
            
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (!(obj is WeTongji.Api.Domain.ClubNews))
                throw new ArgumentOutOfRangeException("obj");

            #endregion

            var cn = obj as WeTongji.Api.Domain.ClubNews;

            #region [Save Basic Properties]

            this.Id = cn.Id;
            this.Title = cn.Title;
            this.Source = cn.Source;
            this.Summary = cn.Summary;
            this.Context = cn.Context;
            this.CreatedAt = cn.CreatedAt;
            this.Read = cn.Read;
            this.Like = cn.Like;
            this.CanLike = cn.CanLike;
            this.Favorite = cn.Favorite;
            this.CanFavorite = cn.CanFavorite;
            this.Organizer = cn.Organizer;
            this.OrganizerAvatar = cn.OrganizerAvatar;

            #endregion

            #region [Save Extended Property]

            var img = new ImageExt() { Url = cn.OrganizerAvatar };
            using (var db = new WTDataContext(WTDataContext.DBConntectionString))
            {
                db.Images.InsertOnSubmit(img);
                db.SubmitChanges();
            }
            OrganizerAvatarGuid = img.Id;

            #endregion
        }

        public WTObject GetObject()
        {
            var cn = new WeTongji.Api.Domain.ClubNews();

            cn.Id = this.Id;
            cn.Title = this.Title;
            cn.Source = this.Source;
            cn.Summary = this.Summary;
            cn.Context = this.Context;
            cn.CreatedAt = this.CreatedAt;
            cn.Read = this.Read;
            cn.Like = this.Like;
            cn.CanLike = this.CanLike;
            cn.Favorite = this.Favorite;
            cn.CanFavorite = this.CanFavorite;
            cn.Organizer = this.Organizer;
            cn.OrganizerAvatar = this.OrganizerAvatar;

            var imgs = this.SerializedImages.Split(',');
            for (int i = 0; i < imgs.Count(); ++i)
            {
                imgs[i] = imgs[i].Trim('\"');
            }
            cn.Images = imgs;

            return cn;
        }
        
        #endregion
    }
}
