using System;
using System.Collections.Generic;

namespace WeTongji.Api.Request
{
    public class UserFindRequest<T> : WTRequest<T> where T : WeTongji.Api.WTResponse
    {
        #region [Constructor]

        public UserFindRequest()
        {
            base.dict["NO"] = NO = String.Empty;
            base.dict["Name"] = Name = String.Empty;
        }

        #endregion

        #region [Properties]

        public String NO { get; set; }

        public String Name { get; set; }

        #endregion

        #region [Overridden]

        public override String GetApiName()
        {
            return "User.Find";
        }

        public override IDictionary<String, String> GetParameters()
        {
            base.dict["NO"] = NO;
            base.dict["Name"] = Name;

            return base.dict;
        }

        public override void Validate()
        {
            #region [NO]

            if (String.IsNullOrEmpty(NO) || String.IsNullOrWhiteSpace(NO))
            {
                throw new ArgumentNullException("NO", "NO can NOT be empty.");
            }

            foreach (var c in NO)
            {
                if (!Char.IsDigit(c))
                {
                    throw new ArgumentOutOfRangeException("NO", "NO can only contain digit.");
                }
            }

            #endregion

            #region [Name]

            if (String.IsNullOrEmpty(Name))
            {
                throw new ArgumentNullException("Name", "Name can NOT be empty.");
            }

            #endregion

        }

        #endregion
    }
}