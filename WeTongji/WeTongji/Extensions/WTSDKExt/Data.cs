using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using WeTongji.DataBase;
using WeTongji.Utility;
using System.Windows.Media;
using System.IO.IsolatedStorage;
using System.IO;
using System.ComponentModel;
using System.Net;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;

namespace WeTongji.Api.Domain
{
    public interface IWTObjectExt
    {
        void SetObject(WTObject obj);
        WTObject GetObject();
        Type ExpectedType();
    }

    [Table(Name = "Image")]
    public class ImageExt : IWTObjectExt, INotifyPropertyChanged
    {
        #region [Properties]

        [Column(IsPrimaryKey = true)]
        public String Id { get; set; }

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

        #region [PropertyChanged]

        public event PropertyChangedEventHandler PropertyChanged;

        public void SendPropertyChanged(String propertyName)
        {
            NotifyPropertyChanged(propertyName);
        }

        private void NotifyPropertyChanged(String propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #endregion

        #region [Data Binding]

        public ImageSource ImageBrush
        {
            get
            {
                return String.Format("{0}.{1}", Id, Url.GetImageFileExtension()).GetImageSource();
            }
        }

        #endregion

        #region [Constructor]

        public ImageExt()
        {
        }

        public ImageExt(String url, String des = "")
        {
            Url = url;
            Description = des;
        }

        #endregion
    }

    [Table(Name = "Person")]
    public class PersonExt : IWTObjectExt, INotifyPropertyChanged
    {
        #region [Basic Properties]

        [Column(IsPrimaryKey = true)]
        public int Id { get; set; }

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

        #endregion

        #region [Extended Properties]

        /// <summary>
        /// "[Guid(0)]":"[FileExt(0)]";"[Guid(1)]":"[FileExt(1)]";...."[Guid(n)]":"[FileExt(n)]"
        /// where n = number of images
        /// </summary>
        [Column()]
        public String ImageExtList { get; set; }

        [Column()]
        public String AvatarGuid { get; set; }

        #endregion

        #region [Extended Methods]

        /// <summary>
        /// Key: Url
        /// Value: Description
        /// </summary>
        /// <returns></returns>
        public Dictionary<String, String> GetImages()
        {
            var Images = new Dictionary<String, String>();

            var imgList = ImageExtList.Split(';');

            using (var db = WTShareDataContext.ShareDB)
            {
                foreach (var img in imgList)
                {
                    var guid = img.Split(':').First().Trim('\"');
                    var target = db.Images.Where((dbImg) => dbImg.Id == guid).SingleOrDefault();

                    if (target != null)
                        Images[target.Url] = target.Description;
                }
            }


            return Images;
        }

        public Boolean AvatarExists()
        {
            var store = IsolatedStorageFile.GetUserStoreForApplication();

            return store.FileExists(String.Format("{0}.{1}", this.AvatarGuid, this.Avatar.GetImageFileExtension()));
        }

        public Boolean ImageExists(int index = 0)
        {
            try
            {
                var imgKVs = ImageExtList.Split(';');
                var imgKV = imgKVs[index].Split(':');
                var fileName = String.Format("{0}.{1}", imgKV[0].Trim('\"'), imgKV[1].Trim('\"'));

                var store = IsolatedStorageFile.GetUserStoreForApplication();
                return store.FileExists(fileName);
            }
            catch
            {
                return false;
            }
        }

        public void SaveAvatar(Stream stream)
        {
            try
            {
                stream.Seek(0, SeekOrigin.Begin);
                var store = IsolatedStorageFile.GetUserStoreForApplication();
                using (var fileStream = store.CreateFile(String.Format("{0}.{1}", AvatarGuid, Avatar.GetImageFileExtension())))
                {
                    stream.CopyTo(fileStream);
                    fileStream.Flush();
                    fileStream.Close();
                }
            }
            catch { }
        }

        /// <summary>
        /// Save an image stream.
        /// </summary>
        /// <param name="stream">image stream</param>
        /// <param name="index">
        /// The zero-based index of Person's images. By default, the value is 0.
        /// </param>
        public void SaveImage(Stream stream, int index = 0)
        {
            try
            {
                var imgKVs = ImageExtList.Split(';');
                var imgKV = imgKVs[index].Split(':');
                var fileName = String.Format("{0}.{1}", imgKV[0].Trim('\"'), imgKV[1].Trim('\"'));

                var store = IsolatedStorageFile.GetUserStoreForApplication();
                using (var fileStream = store.CreateFile(fileName))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);
                    fileStream.Flush();
                    fileStream.Close();
                }
            }
            catch { }
        }

        public IEnumerable<ImageExt> GetImageExts()
        {
            var result = new ObservableCollection<ImageExt>();

            var imgList = ImageExtList.Split(';');

            using (var db = WTShareDataContext.ShareDB)
            {
                foreach (var img in imgList)
                {
                    var guid = img.Split(':').First().Trim('\"');
                    var target = db.Images.Where((dbImg) => dbImg.Id == guid).SingleOrDefault();

                    if (target != null)
                        result.Add(target);
                }
            }

            return result;
        }

        #endregion

        #region [Data Binding]

        public ImageSource AvatarImageBrush
        {
            get
            {
                if (AvatarGuid.EndsWith("missing.png"))
                    return new BitmapImage(new Uri("/Images/missing.png", UriKind.RelativeOrAbsolute));

                var fileExt = Avatar.GetImageFileExtension();

                var imgSrc = String.Format("{0}.{1}", AvatarGuid, fileExt).GetImageSource();

                if (imgSrc == null)
                    return new BitmapImage(new Uri("/Images/missing.png", UriKind.RelativeOrAbsolute));
                else
                    return imgSrc;
            }
        }

        public ImageSource FirstImageBrush
        {
            get
            {
                if (String.IsNullOrEmpty(ImageExtList))
                    return null;

                var imgKV = ImageExtList.Split(';').FirstOrDefault();
                if (String.IsNullOrEmpty(imgKV))
                    return null;

                var fileKV = imgKV.Split(':');

                var fileName = String.Format("{0}.{1}", fileKV[0].Trim('\"'), fileKV[1].Trim('\"'));

                return fileName.GetImageSource();
            }
        }

        public IEnumerable<ImageSource> ImageBrushList
        {
            get
            {
                if (String.IsNullOrEmpty(ImageExtList))
                    return null;

                var imgKVs = ImageExtList.Split(';');

                var result = new ObservableCollection<ImageSource>();

                foreach (var imgKV in imgKVs)
                {
                    var fileKV = imgKV.Split(':');

                    var fileName = String.Format("{0}.{1}", fileKV[0].Trim('\"'), fileKV[1].Trim('\"'));

                    var imgSrc = fileName.GetImageSource();

                    if (imgSrc != null)
                        result.Add(imgSrc);
                }

                return result;
            }
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

            if (p.Images.Count() > 0)
            {
                #region [Make ImageExt List]

                {
                    var imgList = new ImageExt[p.Images.Count()];
                    int i = 0;
                    StringBuilder sb = new StringBuilder();

                    foreach (var kvp in p.Images)
                    {
                        imgList[i] = new WeTongji.Api.Domain.ImageExt(kvp.Key, kvp.Value)
                            {
                                Id = Guid.NewGuid().ToString()
                            };
                        sb.AppendFormat("\"{0}\":\"{1}\";", imgList[i].Id, imgList[i].Url.GetImageFileExtension());
                        ++i;
                    }

                    using (var db = WTShareDataContext.ShareDB)
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
                ImageExtList = String.Empty;
            }

            #region [Save Avatar]

            {
                var avatarImg = new ImageExt() { Id = Guid.NewGuid().ToString() };
                using (var db = WTShareDataContext.ShareDB)
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

        #region [PropertyChanged]

        public event PropertyChangedEventHandler PropertyChanged;

        public void SendPropertyChanged(String propertyName)
        {
            NotifyPropertyChanged(propertyName);
        }

        private void NotifyPropertyChanged(String propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #endregion

        #region [Constructor]

        public PersonExt() { }

        public PersonExt(Person p) { SetObject(p); }

        #endregion
    }

    [Table(Name = "User")]
    public class UserExt : IWTObjectExt, INotifyPropertyChanged
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
        public String AvatarGuid { get; set; }

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
            using (var db = new WTUserDataContext(user.UID))
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

        #region [PropertyChanged]

        public event PropertyChangedEventHandler PropertyChanged;

        public void SendPropertyChanged(String propertyName)
        {
            NotifyPropertyChanged(propertyName);
        }

        private void NotifyPropertyChanged(String propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #endregion
    }

    [Table(Name = "Activity")]
    public class ActivityExt : IWTObjectExt, INotifyPropertyChanged
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

        [Column()]
        public String ImageGuid { get; set; }

        [Column()]
        public String OrganizerAvatarGuid { get; set; }

        #endregion

        #region [Data Binding]

        public String DisplayTime
        {
            get
            {
                return String.Format("{0:yyyy}/{0:MM}/{0:dd}({1}) {0:HH}:{0:mm}~{2:HH}:{2:mm}", Begin, Begin.GetChineseDate(), End);
            }
        }

        /// <summary>
        /// Get the organizer avatar for data binding.
        /// [Fall back value] missing.png
        /// </summary>
        public ImageSource OrganizerAvatarImageBrush
        {
            get
            {
                if (OrganizerAvatar.EndsWith("missing.png"))
                    return new BitmapImage(new Uri("/Images/missing.png", UriKind.RelativeOrAbsolute));

                var fileExt = OrganizerAvatar.GetImageFileExtension();

                var imgSrc = String.Format("{0}.{1}", OrganizerAvatarGuid, fileExt).GetImageSource();

                if (imgSrc == null)
                    return new BitmapImage(new Uri("/Images/missing.png", UriKind.RelativeOrAbsolute));
                else
                    return imgSrc;
            }
        }

        /// <summary>
        /// Get the illustration of activity for data binding
        /// [Fall back value] missing.png
        /// </summary>
        public ImageSource ActivityImageBrush
        {
            get
            {
                if (Image.EndsWith("missing.png"))
                    return new BitmapImage(new Uri("/Images/missing.png", UriKind.RelativeOrAbsolute));

                var fileExt = Image.GetImageFileExtension();

                return String.Format("{0}.{1}", ImageGuid, fileExt).GetImageSource();
            }
        }

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
            this.CreatedAt = a.CreatedAt;

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

        #region [PropertyChanged]

        public event PropertyChangedEventHandler PropertyChanged;

        public void SendPropertyChanged(String propertyName)
        {
            NotifyPropertyChanged(propertyName);
        }

        private void NotifyPropertyChanged(String propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #endregion

        #region [Extended Functions]

        public Boolean AvatarExists()
        {
            var store = IsolatedStorageFile.GetUserStoreForApplication();

            return store.FileExists(String.Format("{0}.{1}", this.OrganizerAvatarGuid, this.OrganizerAvatar.GetImageFileExtension()));
        }

        public Boolean ImageExists()
        {
            var store = IsolatedStorageFile.GetUserStoreForApplication();

            return store.FileExists(String.Format("{0}.{1}", this.ImageGuid, this.Image.GetImageFileExtension()));
        }

        /// <summary>
        /// Save image stream of the organizer avatar of the activity.
        /// Called after downloading it.
        /// </summary>
        /// <param name="stream">the organizer avatar file stream</param>
        /// <remarks>
        /// The name of the image file created in the isolated storage file:
        /// Name = [OrganizerAvatarGuid].[OrganizerAvatar.GetImageFileExtension()]
        /// where the OrganizerAvatarGuid is a new Guid string.
        /// </remarks>
        public void SaveAvatar(Stream stream)
        {
            var ava = new ImageExt()
            {
                Id = Guid.NewGuid().ToString(),
                Url = OrganizerAvatar
            };

            OrganizerAvatarGuid = ava.Id;

            using (var db = WTShareDataContext.ShareDB)
            {
                db.Images.InsertOnSubmit(ava);

                var act = db.Activities.Where((a) => a.Id == this.Id).FirstOrDefault();
                if (act != null)
                    act.OrganizerAvatarGuid = ava.Id;

                db.SubmitChanges();
            }

            using (var db = WTShareDataContext.ShareDB)
            {
                var act = db.Activities.Where((a) => a.Id == this.Id).FirstOrDefault();
            }

            var store = IsolatedStorageFile.GetUserStoreForApplication();

            using (var fileStream = store.CreateFile(String.Format("{0}.{1}", ava.Id, OrganizerAvatar.GetImageFileExtension())))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
                fileStream.Flush();
                fileStream.Close();
            }
        }

        /// <summary>
        /// Save image stream of the illustration of the activity.
        /// Called after downloading it.
        /// </summary>
        /// <param name="stream">the image file stream</param>
        /// <remarks>
        /// The name of the image file created in the isolated storage file:
        /// Name = [ImageGuid].[Image.GetImageFileExtension()]
        /// where the ImageGuid is a new Guid string.
        /// </remarks>
        public void SaveImage(Stream stream)
        {
            if (!String.IsNullOrEmpty(Image))
            {
                var img = new ImageExt()
                {
                    Id = Guid.NewGuid().ToString(),
                    Url = Image
                };

                ImageGuid = img.Id.ToString();

                using (var db = WTShareDataContext.ShareDB)
                {
                    db.Images.InsertOnSubmit(img);

                    var act = db.Activities.Where((a) => a.Id == this.Id).FirstOrDefault();
                    if (act != null)
                        act.ImageGuid = this.ImageGuid;

                    db.SubmitChanges();
                }

                var store = IsolatedStorageFile.GetUserStoreForApplication();

                using (var fileStream = store.CreateFile(String.Format("{0}.{1}", img.Id, Image.GetImageFileExtension())))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);
                    fileStream.Flush();
                    fileStream.Close();
                }
            }
        }
        #endregion
    }

    [Table(Name = "Channel")]
    public class ChannelExt : IWTObjectExt, INotifyPropertyChanged
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
        public String ImageGuid { get; set; }

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
            using (var db = WTShareDataContext.ShareDB)
            {
                db.Images.InsertOnSubmit(img);
                db.SubmitChanges();
            }
            ImageGuid = img.Id;

            #endregion
        }

        public Type ExpectedType() { return typeof(WeTongji.Api.Domain.Channel); }

        #region [PropertyChanged]

        public event PropertyChangedEventHandler PropertyChanged;

        public void SendPropertyChanged(String propertyName)
        {
            NotifyPropertyChanged(propertyName);
        }

        private void NotifyPropertyChanged(String propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #endregion
    }

    [Table(Name = "SchoolNews")]
    public class SchoolNewsExt : IWTObjectExt, INotifyPropertyChanged
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

        /// <summary>
        /// "[Guid(0)]":"[FileExt(0)]";"[Guid(1)]":"[FileExt(1)]";...."[Guid(n)]":"[FileExt(n)]"
        /// where n = number of images
        /// </summary>
        [Column()]
        public String ImageExtList { get; set; }

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

            if (news.Images.Count() > 0)
            {
                StringBuilder sb = new StringBuilder();

                using (var db = WTShareDataContext.ShareDB)
                {
                    foreach (var img in news.Images)
                    {
                        var imgExt = new ImageExt(img) { Id = Guid.NewGuid().ToString() };

                        sb.AppendFormat("\"{0}\":\"{1}\";", imgExt.Id, imgExt.Url.GetImageFileExtension());

                        db.Images.InsertOnSubmit(imgExt);
                    }

                    db.SubmitChanges();
                }

                ImageExtList = sb.ToString(0, sb.Length - 1);
            }

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

            if (String.IsNullOrEmpty(ImageExtList))
            {
                news.Images = new String[0];
            }
            else
            {
                var imgs = ImageExtList.Split(';');
                for (int i = 0; i < imgs.Count(); ++i)
                {
                    var tmp = imgs[i].Split(':').First();
                    imgs[i] = tmp.Trim('\"');
                }
                news.Images = imgs;
            }

            return news;
        }

        public virtual Type ExpectedType() { return typeof(WeTongji.Api.Domain.WTNews); }

        #region [PropertyChanged]

        public event PropertyChangedEventHandler PropertyChanged;

        public void SendPropertyChanged(String propertyName)
        {
            NotifyPropertyChanged(propertyName);
        }

        private void NotifyPropertyChanged(String propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #endregion

        #region [Extended Methods]

        public Boolean ImageExists(int index = 0)
        {
            try
            {
                var imgKVs = ImageExtList.Split(';');
                var imgKV = imgKVs[index].Split(':');
                var fileName = String.Format("{0}.{1}", imgKV[0].Trim('\"'), imgKV[1].Trim('\"'));

                var store = IsolatedStorageFile.GetUserStoreForApplication();
                return store.FileExists(fileName);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Save an image stream.
        /// </summary>
        /// <param name="stream">image stream</param>
        /// <param name="index">
        /// The zero-based index of Person's images. By default, the value is 0.
        /// </param>
        public void SaveImage(Stream stream, int index = 0)
        {
            try
            {
                var imgKVs = ImageExtList.Split(';');
                var imgKV = imgKVs[index].Split(':');
                var fileName = String.Format("{0}.{1}", imgKV[0].Trim('\"'), imgKV[1].Trim('\"'));

                var store = IsolatedStorageFile.GetUserStoreForApplication();
                using (var fileStream = store.CreateFile(fileName))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);
                    fileStream.Flush();
                    fileStream.Close();
                }
            }
            catch { }
        }

        public IEnumerable<ImageExt> GetImageExts()
        {
            var result = new ObservableCollection<ImageExt>();

            var imgList = ImageExtList.Split(';');

            using (var db = WTShareDataContext.ShareDB)
            {
                foreach (var img in imgList)
                {
                    var guid = img.Split(':').First().Trim('\"');
                    var target = db.Images.Where((dbImg) => dbImg.Id == guid).SingleOrDefault();

                    if (target != null)
                        result.Add(target);
                }
            }

            return result;
        }

        public IEnumerable<String> GetImagesURL()
        {
            var Images = new List<String>();

            var imgList = ImageExtList.Split(';');

            using (var db = WTShareDataContext.ShareDB)
            {
                foreach (var img in imgList)
                {
                    var guid = img.Split(':').First().Trim('\"');
                    var target = db.Images.Where((dbImg) => dbImg.Id == guid).SingleOrDefault();

                    if (target != null)
                        Images.Add(target.Url);
                }
            }

            return Images;
        }

        #endregion

        #region [Data Binding]

        public ImageSource FirstImageBrush
        {
            get
            {
                if (String.IsNullOrEmpty(ImageExtList))
                    return null;

                var imgKV = ImageExtList.Split(';').FirstOrDefault();
                if (String.IsNullOrEmpty(imgKV))
                    return null;

                var fileKV = imgKV.Split(':');

                var fileName = String.Format("{0}.{1}", fileKV[0].Trim('\"'), fileKV[1].Trim('\"'));

                return fileName.GetImageSource();
            }
        }

        public Boolean IsIllustrated
        {
            get
            {
                return !String.IsNullOrEmpty(ImageExtList);
            }
        }

        /// <summary>
        /// 5分钟前 or 5小时前 or 10:20:05 or 2013/02/05
        /// </summary>
        public String DisplayCreationTime
        {
            get 
            {
                var span = DateTime.Now - CreatedAt;

                if (span < TimeSpan.FromHours(1))
                {
                    return String.Format("{0}分钟前", (int)span.TotalMinutes);
                }
                else if (span < TimeSpan.FromHours(6))
                {
                    return String.Format("{0}小时前", (int)span.TotalHours);
                }
                else if (CreatedAt.Date == DateTime.Now.Date)
                {
                    return CreatedAt.ToString("HH:mm:ss");
                }
                else
                    return CreatedAt.ToString("yyyy/MM/dd");
            }
        }

        /// <summary>
        /// 2013/02/03 20:18
        /// </summary>
        public String FullDisplayCreationTime
        {
            get
            {
                return CreatedAt.ToString("yyyy/MM/dd hh:mm");
            }
        }

        #endregion
    }

    [Table(Name = "Around")]
    public class AroundExt : IWTObjectExt, INotifyPropertyChanged
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

        /// <summary>
        /// The url of the title image
        /// </summary>
        /// <remarks>
        /// Refers to "Image" property of class Around
        /// </remarks>
        [Column()]
        public String TitleImage { get; set; }

        [Column()]
        public String Location { get; set; }

        [Column()]
        public String Contact { get; set; }

        [Column()]
        public String TicketService { get; set; }

        #endregion

        #region [Extended Properties]

        /// <summary>
        /// "[Guid(0)]":"[FileExt(0)]";"[Guid(1)]":"[FileExt(1)]";...."[Guid(n)]":"[FileExt(n)]"
        /// where n = number of images
        /// </summary>
        [Column()]
        public String ImageExtList { get; set; }

        [Column]
        public String TitleImageGuid { get; set; }

        #endregion

        #region [Implementation]

        public virtual void SetObject(WTObject obj)
        {
            #region [Check Arguments]
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (!(obj is WeTongji.Api.Domain.Around))
                throw new ArgumentOutOfRangeException("obj");
            #endregion

            var news = obj as WeTongji.Api.Domain.Around;

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
            this.TitleImage = news.Image;
            this.Location = news.Location;
            this.Contact = news.Contact;
            this.TicketService = news.TicketService;

            #endregion

            #region [Save Extended Properties]

            //...Save Images
            if (news.Images.Count() > 0)
            {
                StringBuilder sb = new StringBuilder();

                using (var db = WTShareDataContext.ShareDB)
                {
                    foreach (var img in news.Images)
                    {
                        var imgExt = new ImageExt(img) { Id = Guid.NewGuid().ToString() };

                        sb.AppendFormat("\"{0}\":\"{1}\";", imgExt.Id, imgExt.Url.GetImageFileExtension());

                        db.Images.InsertOnSubmit(imgExt);
                    }

                    db.SubmitChanges();
                }

                ImageExtList = sb.ToString(0, sb.Length - 1);
            }

            //...Save title image
            {
                var titleImg = new ImageExt() { Id = Guid.NewGuid().ToString() };
                using (var db = WTShareDataContext.ShareDB)
                {
                    db.Images.InsertOnSubmit(titleImg);
                    db.SubmitChanges();
                }
                TitleImageGuid = titleImg.Id;
            }

            #endregion
        }

        public virtual WTObject GetObject()
        {
            var news = new WeTongji.Api.Domain.Around();

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
            news.Image = this.TitleImage;
            news.Location = this.Location;
            news.Contact = this.Contact;
            news.TicketService = this.TicketService;

            if (String.IsNullOrEmpty(ImageExtList))
            {
                news.Images = new String[0];
            }
            else
            {
                var imgs = ImageExtList.Split(';');
                for (int i = 0; i < imgs.Count(); ++i)
                {
                    var tmp = imgs[i].Split(':').First();
                    imgs[i] = tmp.Trim('\"');
                }
                news.Images = imgs;
            }

            return news;
        }

        public virtual Type ExpectedType() { return typeof(WeTongji.Api.Domain.Around); }

        #region [PropertyChanged]

        public event PropertyChangedEventHandler PropertyChanged;

        public void SendPropertyChanged(String propertyName)
        {
            NotifyPropertyChanged(propertyName);
        }

        private void NotifyPropertyChanged(String propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #endregion

        #region [Data Binding]

        public ImageSource FirstImageBrush
        {
            get
            {
                if (String.IsNullOrEmpty(ImageExtList))
                    return null;

                var imgKV = ImageExtList.Split(';').FirstOrDefault();
                if (String.IsNullOrEmpty(imgKV))
                    return null;

                var fileKV = imgKV.Split(':');

                var fileName = String.Format("{0}.{1}", fileKV[0].Trim('\"'), fileKV[1].Trim('\"'));

                return fileName.GetImageSource();
            }
        }

        public ImageSource TitleImageBrush
        {
            get
            {
                var fileName = String.Format("{0}.{1}", TitleImageGuid, TitleImage.GetImageFileExtension());

                return fileName.GetImageSource();
            }
        }

        public Boolean IsIllustrated
        {
            get { return !String.IsNullOrEmpty(this.ImageExtList); }
        }

        public Boolean HasTicket
        {
            get { return !String.IsNullOrEmpty(TicketService); }
        }

        /// <summary>
        /// 5分钟前 or 5小时前 or 10:20:05 or 2013/02/05
        /// </summary>
        public String DisplayCreationTime
        {
            get
            {
                var span = DateTime.Now - CreatedAt;

                if (span < TimeSpan.FromHours(1))
                {
                    return String.Format("{0}分钟前", (int)span.TotalMinutes);
                }
                else if (span < TimeSpan.FromHours(7))
                {
                    return String.Format("{0}小时前", (int)span.TotalHours);
                }
                else if (CreatedAt.Date == DateTime.Now.Date)
                {
                    return CreatedAt.ToString("HH:mm:ss");
                }
                else
                    return CreatedAt.ToString("yyyy/MM/dd");
            }
        }

        /// <summary>
        /// 2013/02/03 20:18
        /// </summary>
        public String FullDisplayCreationTime
        {
            get 
            {
                return CreatedAt.ToString("yyyy/MM/dd hh:mm");
            }
        }

        #endregion

        #region [Extended Methods]

        public IEnumerable<String> GetImagesURL()
        {
            var Images = new List<String>();

            var imgList = ImageExtList.Split(';');

            using (var db = WTShareDataContext.ShareDB)
            {
                foreach (var img in imgList)
                {
                    var guid = img.Split(':').First().Trim('\"');
                    var target = db.Images.Where((dbImg) => dbImg.Id == guid).SingleOrDefault();

                    if (target != null)
                        Images.Add(target.Url);
                }
            }

            return Images;
        }

        public Boolean ImageExists(int index = 0)
        {
            try
            {
                var imgKVs = ImageExtList.Split(';');
                var imgKV = imgKVs[index].Split(':');
                var fileName = String.Format("{0}.{1}", imgKV[0].Trim('\"'), imgKV[1].Trim('\"'));

                var store = IsolatedStorageFile.GetUserStoreForApplication();
                return store.FileExists(fileName);
            }
            catch
            {
                return false;
            }
        }

        public Boolean IsTitleImageExists()
        {
            var store = IsolatedStorageFile.GetUserStoreForApplication();

            return store.FileExists(String.Format("{0}.{1}", TitleImageGuid, TitleImage.GetImageFileExtension()));
        }

        public void SaveTitleImage(Stream stream)
        {
            var store = IsolatedStorageFile.GetUserStoreForApplication();

            var fileName = String.Format("{0}.{1}", TitleImageGuid, TitleImage.GetImageFileExtension());

            using (var fileStream = store.CreateFile(fileName))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
                fileStream.Flush();
                stream.Close();
            }
        }

        /// <summary>
        /// Save an image stream.
        /// </summary>
        /// <param name="stream">image stream</param>
        /// <param name="index">
        /// The zero-based index of Person's images. By default, the value is 0.
        /// </param>
        public void SaveImage(Stream stream, int index = 0)
        {
            try
            {
                var imgKVs = ImageExtList.Split(';');
                var imgKV = imgKVs[index].Split(':');
                var fileName = String.Format("{0}.{1}", imgKV[0].Trim('\"'), imgKV[1].Trim('\"'));

                var store = IsolatedStorageFile.GetUserStoreForApplication();
                using (var fileStream = store.CreateFile(fileName))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);
                    fileStream.Flush();
                    fileStream.Close();
                }
            }
            catch { }
        }

        public IEnumerable<ImageExt> GetImageExts()
        {
            var result = new ObservableCollection<ImageExt>();

            var imgList = ImageExtList.Split(';');

            using (var db = WTShareDataContext.ShareDB)
            {
                foreach (var img in imgList)
                {
                    var guid = img.Split(':').First().Trim('\"');
                    var target = db.Images.Where((dbImg) => dbImg.Id == guid).SingleOrDefault();

                    if (target != null)
                        result.Add(target);
                }
            }

            return result;
        }

        #endregion
    }

    [Table(Name = "ForStaff")]
    public class ForStaffExt : IWTObjectExt, INotifyPropertyChanged
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

        /// <summary>
        /// "[Guid(0)]":"[FileExt(0)]";"[Guid(1)]":"[FileExt(1)]";...."[Guid(n)]":"[FileExt(n)]"
        /// where n = number of images
        /// </summary>
        [Column()]
        public String ImageExtList { get; set; }

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

            if (news.Images.Count() > 0)
            {
                StringBuilder sb = new StringBuilder();

                using (var db = WTShareDataContext.ShareDB)
                {
                    foreach (var img in news.Images)
                    {
                        var imgExt = new ImageExt(img) { Id = Guid.NewGuid().ToString() };

                        sb.AppendFormat("\"{0}\":\"{1}\";", imgExt.Id, imgExt.Url.GetImageFileExtension());

                        db.Images.InsertOnSubmit(imgExt);
                    }

                    db.SubmitChanges();
                }

                ImageExtList = sb.ToString(0, sb.Length - 1);
            }

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

            if (String.IsNullOrEmpty(ImageExtList))
            {
                news.Images = new String[0];
            }
            else
            {
                var imgs = ImageExtList.Split(';');
                for (int i = 0; i < imgs.Count(); ++i)
                {
                    var tmp = imgs[i].Split(':').First();
                    imgs[i] = tmp.Trim('\"');
                }
                news.Images = imgs;
            }

            return news;
        }

        public virtual Type ExpectedType() { return typeof(WeTongji.Api.Domain.WTNews); }

        #region [PropertyChanged]

        public event PropertyChangedEventHandler PropertyChanged;

        public void SendPropertyChanged(String propertyName)
        {
            NotifyPropertyChanged(propertyName);
        }

        private void NotifyPropertyChanged(String propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #endregion

        #region [Data Binding]

        public ImageSource FirstImageBrush
        {
            get
            {
                if (String.IsNullOrEmpty(ImageExtList))
                    return null;

                var imgKV = ImageExtList.Split(';').FirstOrDefault();
                if (String.IsNullOrEmpty(imgKV))
                    return null;

                var fileKV = imgKV.Split(':');

                var fileName = String.Format("{0}.{1}", fileKV[0].Trim('\"'), fileKV[1].Trim('\"'));

                return fileName.GetImageSource();
            }
        }

        public Boolean IsIllustrated
        {
            get { return !String.IsNullOrEmpty(ImageExtList); }
        }

        /// <summary>
        /// 5分钟前 or 5小时前 or 10:20:05 or 2013/02/05
        /// </summary>
        public String DisplayCreationTime
        {
            get
            {
                var span = DateTime.Now - CreatedAt;

                if (span < TimeSpan.FromHours(1))
                {
                    return String.Format("{0}分钟前", (int)span.TotalMinutes);
                }
                else if (span < TimeSpan.FromHours(6))
                {
                    return String.Format("{0}小时前", (int)span.TotalHours);
                }
                else if (CreatedAt.Date == DateTime.Now.Date)
                {
                    return CreatedAt.ToString("HH:mm:ss");
                }
                else
                    return CreatedAt.ToString("yyyy/MM/dd");
            }
        }

        /// <summary>
        /// 2013/02/03 20:18
        /// </summary>
        public String FullDisplayCreationTime
        {
            get
            {
                return CreatedAt.ToString("yyyy/MM/dd hh:mm");
            }
        }

        #endregion

        #region [Extended Methods]

        public IEnumerable<String> GetImagesURL()
        {
            var Images = new List<String>();

            var imgList = ImageExtList.Split(';');

            using (var db = WTShareDataContext.ShareDB)
            {
                foreach (var img in imgList)
                {
                    var guid = img.Split(':').First().Trim('\"');
                    var target = db.Images.Where((dbImg) => dbImg.Id == guid).SingleOrDefault();

                    if (target != null)
                        Images.Add(target.Url);
                }
            }

            return Images;
        }

        public Boolean ImageExists(int index = 0)
        {
            try
            {
                var imgKVs = ImageExtList.Split(';');
                var imgKV = imgKVs[index].Split(':');
                var fileName = String.Format("{0}.{1}", imgKV[0].Trim('\"'), imgKV[1].Trim('\"'));

                var store = IsolatedStorageFile.GetUserStoreForApplication();
                return store.FileExists(fileName);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Save an image stream.
        /// </summary>
        /// <param name="stream">image stream</param>
        /// <param name="index">
        /// The zero-based index of Person's images. By default, the value is 0.
        /// </param>
        public void SaveImage(Stream stream, int index = 0)
        {
            try
            {
                var imgKVs = ImageExtList.Split(';');
                var imgKV = imgKVs[index].Split(':');
                var fileName = String.Format("{0}.{1}", imgKV[0].Trim('\"'), imgKV[1].Trim('\"'));

                var store = IsolatedStorageFile.GetUserStoreForApplication();
                using (var fileStream = store.CreateFile(fileName))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);
                    fileStream.Flush();
                    fileStream.Close();
                }
            }
            catch { }
        }

        public IEnumerable<ImageExt> GetImageExts()
        {
            var result = new ObservableCollection<ImageExt>();

            var imgList = ImageExtList.Split(';');

            using (var db = WTShareDataContext.ShareDB)
            {
                foreach (var img in imgList)
                {
                    var guid = img.Split(':').First().Trim('\"');
                    var target = db.Images.Where((dbImg) => dbImg.Id == guid).SingleOrDefault();

                    if (target != null)
                        result.Add(target);
                }
            }

            return result;
        }

        #endregion
    }

    [Table(Name = "ClubNews")]
    public class ClubNewsExt : INotifyPropertyChanged
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
        public String ImageExtList { get; set; }

        [Column()]
        public String OrganizerAvatarGuid { get; set; }

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

            //...Avatar
            {
                var img = new ImageExt()
                {
                    Id = Guid.NewGuid().ToString(),
                    Url = cn.OrganizerAvatar
                };
                using (var db = WTShareDataContext.ShareDB)
                {
                    db.Images.InsertOnSubmit(img);
                    db.SubmitChanges();
                }
                OrganizerAvatarGuid = img.Id;
            }

            //...Images
            {
                if (cn.Images.Count() > 0)
                {
                    StringBuilder sb = new StringBuilder();

                    using (var db = WTShareDataContext.ShareDB)
                    {
                        foreach (var img in cn.Images)
                        {
                            var imgExt = new ImageExt(img) { Id = Guid.NewGuid().ToString() };

                            sb.AppendFormat("\"{0}\":\"{1}\";", imgExt.Id, imgExt.Url.GetImageFileExtension());

                            db.Images.InsertOnSubmit(imgExt);
                        }

                        db.SubmitChanges();
                    }

                    ImageExtList = sb.ToString(0, sb.Length - 1);
                }
            }

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

            if (String.IsNullOrEmpty(ImageExtList))
            {
                cn.Images = new String[0];
            }
            else
            {
                var imgs = ImageExtList.Split(';');
                for (int i = 0; i < imgs.Count(); ++i)
                {
                    var tmp = imgs[i].Split(':').First();
                    imgs[i] = tmp.Trim('\"');
                }
                cn.Images = imgs;
            }

            return cn;
        }

        #region [PropertyChanged]

        public event PropertyChangedEventHandler PropertyChanged;

        public void SendPropertyChanged(String propertyName)
        {
            NotifyPropertyChanged(propertyName);
        }

        private void NotifyPropertyChanged(String propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #endregion

        #region [Extended Methods]

        public Boolean ImageExists(int index = 0)
        {
            try
            {
                var imgKVs = ImageExtList.Split(';');
                var imgKV = imgKVs[index].Split(':');
                var fileName = String.Format("{0}.{1}", imgKV[0].Trim('\"'), imgKV[1].Trim('\"'));

                var store = IsolatedStorageFile.GetUserStoreForApplication();
                return store.FileExists(fileName);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Save an image stream.
        /// </summary>
        /// <param name="stream">image stream</param>
        /// <param name="index">
        /// The zero-based index of Person's images. By default, the value is 0.
        /// </param>
        public void SaveImage(Stream stream, int index = 0)
        {
            try
            {
                var imgKVs = ImageExtList.Split(';');
                var imgKV = imgKVs[index].Split(':');
                var fileName = String.Format("{0}.{1}", imgKV[0].Trim('\"'), imgKV[1].Trim('\"'));

                var store = IsolatedStorageFile.GetUserStoreForApplication();
                using (var fileStream = store.CreateFile(fileName))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);
                    fileStream.Flush();
                    fileStream.Close();
                }
            }
            catch { }
        }

        public void SaveAvatar(Stream stream)
        {
            try
            {
                stream.Seek(0, SeekOrigin.Begin);
                var store = IsolatedStorageFile.GetUserStoreForApplication();
                using (var fileStream = store.CreateFile(String.Format("{0}.{1}", OrganizerAvatarGuid, OrganizerAvatar.GetImageFileExtension())))
                {
                    stream.CopyTo(fileStream);
                    fileStream.Flush();
                    fileStream.Close();
                }
            }
            catch { }
        }

        public IEnumerable<ImageExt> GetImageExts()
        {
            var result = new ObservableCollection<ImageExt>();

            var imgList = ImageExtList.Split(';');

            using (var db = WTShareDataContext.ShareDB)
            {
                foreach (var img in imgList)
                {
                    var guid = img.Split(':').First().Trim('\"');
                    var target = db.Images.Where((dbImg) => dbImg.Id == guid).SingleOrDefault();

                    if (target != null)
                        result.Add(target);
                }
            }

            return result;
        }

        public IEnumerable<String> GetImagesURL()
        {
            var Images = new List<String>();

            var imgList = ImageExtList.Split(';');

            using (var db = WTShareDataContext.ShareDB)
            {
                foreach (var img in imgList)
                {
                    var guid = img.Split(':').First().Trim('\"');
                    var target = db.Images.Where((dbImg) => dbImg.Id == guid).SingleOrDefault();

                    if (target != null)
                        Images.Add(target.Url);
                }
            }

            return Images;
        }

        public Boolean AvatarExists()
        {
            var store = IsolatedStorageFile.GetUserStoreForApplication();

            return store.FileExists(String.Format("{0}.{1}", this.OrganizerAvatarGuid, this.OrganizerAvatar.GetImageFileExtension()));
        }

        #endregion

        #region [Data Binding]

        public ImageSource AvatarImageBrush
        {
            get
            {
                if (OrganizerAvatar.EndsWith("missing.png"))
                    return new BitmapImage(new Uri("/Images/missing.png", UriKind.RelativeOrAbsolute));

                var fileExt = OrganizerAvatar.GetImageFileExtension();

                var imgSrc = String.Format("{0}.{1}", OrganizerAvatarGuid, fileExt).GetImageSource();

                if (imgSrc == null)
                    return new BitmapImage(new Uri("/Images/missing.png", UriKind.RelativeOrAbsolute));
                else
                    return imgSrc;
            }
        }

        public ImageSource FirstImageBrush
        {
            get
            {
                if (String.IsNullOrEmpty(ImageExtList))
                    return null;

                var imgKV = ImageExtList.Split(';').FirstOrDefault();
                if (String.IsNullOrEmpty(imgKV))
                    return null;

                var fileKV = imgKV.Split(':');

                var fileName = String.Format("{0}.{1}", fileKV[0].Trim('\"'), fileKV[1].Trim('\"'));

                return fileName.GetImageSource();
            }
        }

        public Boolean IsIllustrated
        {
            get { return !String.IsNullOrEmpty(ImageExtList); }
        }

        /// <summary>
        /// 5分钟前 or 5小时前 or 10:20:05 or 2013/02/05
        /// </summary>
        public String DisplayCreationTime
        {
            get
            {
                var span = DateTime.Now - CreatedAt;

                if (span < TimeSpan.FromHours(1))
                {
                    return String.Format("{0}分钟前", (int)span.TotalMinutes);
                }
                else if (span < TimeSpan.FromHours(6))
                {
                    return String.Format("{0}小时前", (int)span.TotalHours);
                }
                else if (CreatedAt.Date == DateTime.Now.Date)
                {
                    return CreatedAt.ToString("HH:mm:ss");
                }
                else
                    return CreatedAt.ToString("yyyy/MM/dd");
            }
        }

        /// <summary>
        /// 2013/02/03 20:18
        /// </summary>
        public String FullDisplayCreationTime
        {
            get
            {
                return CreatedAt.ToString("yyyy/MM/dd hh:mm");
            }
        }

        #endregion
    }
}
