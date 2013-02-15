using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace WeTongji.Api.Request
{
    public class UserUpdateRequest<T> : WTRequest<T>, IWTUploadRequest<T> where T : WeTongji.Api.WTResponse
    {
        #region [Field]

        private WeTongji.Api.Domain.User user = null;

        #endregion

        #region [Constructor]

        public UserUpdateRequest() { }

        #endregion

        #region [Property]

        public WeTongji.Api.Domain.User User
        {
            get { return user; }
            set
            {
                user = value.Clone();
            }
        }
        public String DisplayName
        {
            get
            {
                return user == null ? String.Empty : user.DisplayName;
            }
            set
            {
                if (user != null)
                {
                    user.DisplayName = value;
                }
            }
        }
        public String Email
        {
            get
            {
                return user == null ? String.Empty : user.Email;
            }
            set
            {
                if (user != null)
                {
                    user.Email = value;
                }
            }
        }
        public String QQ
        {
            get
            {
                return user == null ? String.Empty : user.QQ;
            }
            set
            {
                if (user != null)
                {
                    user.QQ = value;
                }
            }
        }
        public String Phone
        {
            get
            {
                return user == null ? String.Empty : user.Phone;
            }
            set
            {
                if (user != null)
                {
                    user.Phone = value;
                }
            }
        }
        public String SinaWeibo
        {
            get
            {
                return user == null ? String.Empty : user.SinaWeibo;
            }
            set
            {
                if (user != null)
                {
                    user.SinaWeibo = value;
                }
            }
        }

        #endregion

        #region [Implementation]

        public override IDictionary<String, String> GetParameters()
        {
            return new Dictionary<String,String>(base.dict);
        }

        public override String GetApiName()
        {
            return "User.Update";
        }

        public override void Validate()
        {
            if (user == null)
            {
                throw new ArgumentNullException("User");
            }
        }

        #endregion

        public System.IO.Stream GetRequestStream()
        {
            var str = JsonConvert.SerializeObject(user);
            var stream = new System.IO.MemoryStream();

            System.IO.StreamWriter sw = new System.IO.StreamWriter(stream);
            sw.Write(str);
            sw.Flush();
            return stream;
        }

        public String GetContentType()
        {
            return "application/json";
        }
    }
}
