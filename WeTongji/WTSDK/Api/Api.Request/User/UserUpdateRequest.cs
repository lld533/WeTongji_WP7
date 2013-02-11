using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WeTongji.Api.Request
{
    public class UserUpdateRequest<T> : WTRequest<T>, IWTUploadRequest<T> where T : WeTongji.Api.WTResponse
    {
        #region [Constructor]

        public UserUpdateRequest()
        {
            User = null;
            DisplayName = null;
            Email = null;
            QQ = null;
            Phone = null;
            SinaWeibo = null;

            base.dict["User"] = String.Empty;
        }

        #endregion

        #region [Property]

        public WeTongji.Api.Domain.User User { get; set; }
        public String DisplayName { get; set; }
        public String Email { get; set; }
        public String QQ { get; set; }
        public String Phone { get; set; }
        public String SinaWeibo { get; set; }

        #endregion

        #region [Implementation]

        public KeyValuePair<String, WeTongji.Api.Util.FileItem> GetFileParameter()
        {
            return new KeyValuePair<String, WeTongji.Api.Util.FileItem>();
        }

        public override IDictionary<String, String> GetParameters()
        {
            if (String.IsNullOrEmpty(DisplayName))
                User.DisplayName = DisplayName;
            if (String.IsNullOrEmpty(Email))
                User.Email = Email;
            if (String.IsNullOrEmpty(QQ))
                User.QQ = QQ;
            if (String.IsNullOrEmpty(Phone))
                User.Phone = Phone;
            if (String.IsNullOrEmpty(SinaWeibo))
                User.SinaWeibo = SinaWeibo;

            base.dict["User"] = JsonConvert.SerializeObject(User);
            return base.dict;
        }

        public String GetApiName()
        {
            return "User.Update";
        }

        public void Validate()
        {
            if (User == null)
            {
                throw new ArgumentNullException("User");
            }

            if (String.IsNullOrEmpty(DisplayName) 
                && String.IsNullOrEmpty(Email) 
                && String.IsNullOrEmpty(QQ) 
                && String.IsNullOrEmpty(Phone) 
                && String.IsNullOrEmpty(SinaWeibo))
            {
                throw new ArgumentNullException("DisplayName,Email,QQ,Phone,SinaWeibo", "At least one parameter should be modified.");
            }
        }

        #endregion
    }
}
