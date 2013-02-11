using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeTongji.Extensions.GoogleMapsSDK
{
    public class AddressComponent
    {
        public String long_name { get; set; }

        public String short_name { get; set; }

        public AddressType[] types { get; set; }
    }
}
