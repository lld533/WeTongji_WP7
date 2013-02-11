using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeTongji.Extensions.GoogleMapsSDK
{
    public class Address
    {
        public AddressComponent[] address_components { get; set; }

        public String formatted_address { get; set; }

        public Geometry geometry { get; set; }

        public AddressType[] types { get; set; }

        public Boolean partial_match { get; set; }
    }
}
