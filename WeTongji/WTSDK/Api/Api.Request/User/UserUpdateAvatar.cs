using System;
using System.Collections.Generic;
using System.IO;

namespace WeTongji.Api.Request
{
    public class UserUpdateAvatar<T> : WTRequest<T>, IWTUploadRequest<T> where T : WeTongji.Api.WTResponse
    {
        #region [Constructor]

        public UserUpdateAvatar()
        {
            base.dict["Image"] = String.Empty;
        }

        #endregion

        #region [Property]

        public String JpegPhotoName { get; set; }

        public Stream JpegPhotoStream { get; set; }

        #endregion

        #region [Overridden]

        public override IDictionary<String, String> GetParameters()
        {
            Dictionary<String, String> dict = new Dictionary<String, String>(base.dict);

            dict["Image"] = JpegPhotoName;

            return dict;
        }

        public override String GetApiName()
        {
            return "User.Update.Avatar";
        }

        public override void Validate()
        {
            if (String.IsNullOrEmpty(JpegPhotoName))
            {
                throw new ArgumentNullException("JpegPhotoName");
            }

            var photoName = JpegPhotoName.ToLower();
            if (!photoName.EndsWith("jpg") && !photoName.EndsWith("jpeg"))
            {
                throw new ArgumentOutOfRangeException("JpegPhotoName", "Expect a JPEG file.");
            }

            if (JpegPhotoStream == null)
                throw new ArgumentNullException("JpegPhotoStream");
            if (!(JpegPhotoStream.CanRead && JpegPhotoStream.CanSeek))
            {
                throw new ArgumentOutOfRangeException("JpegPhotoStream", "Can not read or seek stream.");
            }
        }

        #endregion

        #region [Implementation]

        public System.IO.Stream GetRequestStream()
        {
            return JpegPhotoStream;
        }

        public String GetContentType()
        {
            return null;
        }

        #endregion
    }
}
