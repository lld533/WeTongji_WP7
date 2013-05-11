using System;

namespace WeTongji.Utility
{
    public class PlatformVersionHelper
    {
        private static Version TargetVersion = new Version(7, 10, 8858);

        public static Boolean IsTargetVersion
        {
            get
            {
                return System.Environment.OSVersion.Version >= TargetVersion;
            }
        }
    }
}
