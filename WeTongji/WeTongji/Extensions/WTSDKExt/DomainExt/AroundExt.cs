﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Linq.Mapping;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WeTongji.DataBase;
using WeTongji.Utility;

namespace WeTongji.Api.Domain
{
    [Table(Name = "Around")]
    public class AroundExt : IWTObjectExt, ICampusInfo, INotifyPropertyChanged
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

        public String DisplaySummary
        {
            get { return Summary; }
        }

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

            #region [Save Extended Properties]

            #region [Save Images]

            if (news.Images.Count() > 0)
            {
                //...Save images if all images are not stored.
                if (String.IsNullOrEmpty(ImageExtList))
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
                else
                {
                    //...do nothing if previous images exist.
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(ImageExtList))
                {
                    var kvpairs = ImageExtList.Split(';');

                    var store = IsolatedStorageFile.GetUserStoreForApplication();

                    foreach (var pair in kvpairs)
                    {
                        var imgkv = pair.Split(':');
                        var imgId = imgkv[0].Trim('\"');
                        var imgExt = imgkv[1].Trim('\"');

                        using (var db = WTShareDataContext.ShareDB)
                        {
                            var imgInDB = db.Images.Where((img) => img.Id == imgId).SingleOrDefault();
                            if (imgInDB != null)
                            {
                                db.Images.DeleteOnSubmit(imgInDB);
                                db.SubmitChanges();
                            }
                        }

                        var fileName = String.Format("{0}.{1}", imgId, imgExt);

                        if (store.FileExists(fileName))
                        {
                            store.DeleteFile(fileName);
                        }
                    }

                    ImageExtList = String.Empty;
                }
            }

            #endregion

            #region [Save title image]

            if (this.TitleImage != news.Image)
            {
                if (String.IsNullOrEmpty(this.TitleImage))
                {
                    var titleImg = new ImageExt()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Url = news.Image
                    };
                    using (var db = WTShareDataContext.ShareDB)
                    {
                        db.Images.InsertOnSubmit(titleImg);
                        db.SubmitChanges();
                    }
                    TitleImageGuid = titleImg.Id;
                }
                else
                {
                    using (var db = WTShareDataContext.ShareDB)
                    {
                        var imgInDB = db.Images.Where((img) => img.Id == this.TitleImageGuid).SingleOrDefault();

                        if (imgInDB != null)
                        {
                            imgInDB.Url = news.Image;
                            db.SubmitChanges();
                        }
                    }

                    var store = IsolatedStorageFile.GetUserStoreForApplication();
                    var fileName = String.Format("{0}.{1}", TitleImageGuid, TitleImage.GetImageFileExtension());

                    if (store.FileExists(fileName))
                        store.DeleteFile(fileName);

                    TitleImageGuid = String.Empty;
                }

            }

            #endregion

            #endregion

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

        public ImageSource CampusInfoImageBrush
        {
            get { return TitleImageBrush; }
        }

        public String CampusInfoImageUrl { get { return TitleImage; } }

        public String CampusInfoImageFileName
        {
            get { return TitleImageGuid + "." + TitleImage.GetImageFileExtension(); }
        }

        public Boolean CampusInfoImageExists { get { return IsTitleImageExists(); } }

        public void SaveCampusInfoImage(Stream stream)
        {
            SaveTitleImage(stream);
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
                //...Todo @_@ Localizable

                if (DateTime.Now < CreatedAt)
                {
                    return String.Format(StringLibrary.Common_JustNow);
                }

                var span = DateTime.Now - CreatedAt;

                if (span < TimeSpan.FromHours(1))
                {
                    return String.Format(StringLibrary.Common_WithinOneHourTemplate, (int)span.TotalMinutes);
                }
                else if (span < TimeSpan.FromHours(2))
                {
                    return String.Format(StringLibrary.Common_WithinTwoHours, (int)span.TotalHours);
                }
                else if (span < TimeSpan.FromHours(6))
                {
                    return String.Format(StringLibrary.Common_WithinSixHoursTemplate, (int)span.TotalHours);
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

        public Boolean ContactIsPhone
        {
            get 
            {
                var regex = new System.Text.RegularExpressions.Regex("^[0-9()（）+-]");
                return regex.IsMatch(Contact);
            }
        }

        public Boolean IsInvalidAround
        {
            get { return Id == int.MinValue; }
        }

        #endregion

        #region [Extended Methods]

        public AroundExt Clone()
        {
            return this.MemberwiseClone() as AroundExt;
        }

        public IEnumerable<String> GetImagesURL()
        {
            var Images = new List<String>();

            if (!String.IsNullOrEmpty(ImageExtList))
            {
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

            if (!String.IsNullOrEmpty(ImageExtList))
            {
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
            }

            return result;
        }

        public static AroundExt InvalidAround()
        {
            return new AroundExt() { Id = int.MinValue };
        }


        #endregion
    }
}
