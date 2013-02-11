using System;
using System.Collections.Generic;
using System.IO;

namespace WeTongji.Api.Request
{
    public class UserUpdateAvatar<T> : WTRequest<T>, IWTUploadRequest<T> where T : WeTongji.Api.WTResponse
    {
        #region [Constructor]

        public UserUpdateAvatar() { }

        #endregion

        #region [Property]

        public Stream Avatar { get; set; }

        #endregion

        #region [Overridden]

        public KeyValuePair<String, WeTongji.Api.Util.FileItem> GetFileParameter()
        {
            return new KeyValuePair<String, WeTongji.Api.Util.FileItem>("Image", new Util.FileItem("Avatar.jpg", "Image/jpeg", Avatar));
        }

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
            if (Avatar == null)
            {
                throw new ArgumentNullException("Avatar");
            }
        }

        #endregion
    }
}
