using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace WeTongji.Api.Request
{
    public class PersonGetLatestRequest<T> : WTRequest<T> where T : WeTongji.Api.Response.ForStaffsGetResponse
    {
        #region [Constructor]

        public PersonGetLatestRequest() 
        { 
            Id = -1;
            base.dict["Id"] = "-1";
        }

        #endregion

        #region [Property]

        public int Id { get; set; }

        #endregion

        #region [Overridden]

        public override String GetApiName()
        {
            return "Person.GetLatest";
        }

        public override IDictionary<String, String> GetParameters()
        {
            base.dict["Id"] = JsonConvert.SerializeObject(Id);
            return base.dict;
        }

        public override void Validate()
        {
            if (0 > Id)
                throw new ArgumentOutOfRangeException("Id");
        }

        #endregion
    }
}
