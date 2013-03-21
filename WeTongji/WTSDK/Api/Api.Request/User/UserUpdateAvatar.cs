using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace WeTongji.Api.Request
{
    public class UserUpdateAvatarRequest<T> : WTRequest<T>, IWTUploadRequest<T> where T : WeTongji.Api.WTResponse
    {
        #region [Constructor]

        public UserUpdateAvatarRequest()
        {
        }

        #endregion

        #region [Property]

        public Stream JpegPhotoStream { get; set; }

        #endregion

        #region [Overridden]

        public override IDictionary<String, String> GetParameters()
        {
            return base.dict;
        }

        public override String GetApiName()
        {
            return "User.Update.Avatar";
        }

        public override void Validate()
        {
            if (JpegPhotoStream == null)
            {
                throw new ArgumentNullException("JpegPhotoStream");
            }
            else if (!JpegPhotoStream.CanRead)
            {
                throw new NotSupportedException("JpegPhotoStream should be able to read.");
            }
            else if (!JpegPhotoStream.CanSeek)
            {
                throw new NotSupportedException("JpegPhotoStream should be able to seek.");
            }
        }

        #endregion

        #region [Implementation]

        public System.IO.Stream GetRequestStream()
        {
            JpegPhotoStream.Seek(0, SeekOrigin.Begin);
            var stream = new System.IO.MemoryStream();

            System.IO.StreamWriter sw = new System.IO.StreamWriter(stream);
            sw.Write("Image={");
            sw.Flush();
            JpegPhotoStream.CopyTo(stream);
            sw.Write("}");
            sw.Flush();
            return stream;
        }

        public String GetContentType()
        {
            return null;
        }

        #endregion
    }
}
