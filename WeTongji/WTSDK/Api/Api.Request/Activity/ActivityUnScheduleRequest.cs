﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace WeTongji.Api.Request
{
    public class ActivityUnScheduleRequest<T> : WTRequest<T> where T : WeTongji.Api.WTResponse
    {
        public ActivityUnScheduleRequest() 
        { 
            Id = -1;
            base.dict["Id"] = "-1";
        }

        public int Id { get; set; }

        public override String GetApiName()
        {
            return "Activity.UnSchedule";
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
    }
}
