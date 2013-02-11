using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeTongji
{
    class Global
    {
        #region [STATIC PROPERTY]

        public static String Session { get; set; }
        public static WeTongji.Api.Domain.User User{ get; set; }

        #endregion
    }
}
