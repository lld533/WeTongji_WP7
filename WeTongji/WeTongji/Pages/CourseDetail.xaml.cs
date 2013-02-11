using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Navigation;
using System.Text.RegularExpressions;
using System.Text;

namespace WeTongji
{
    public partial class CourseDetail : PhoneApplicationPage
    {
        public CourseDetail()
        {
            InitializeComponent();
        }

        /// <remarks>
        /// [View] Optional, e.g. /Pages/CourseDetail.xaml?v=%d
        /// 0 := Course Info
        /// 1 := Exam Info
        /// </remarks>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var uri = e.Uri.ToString();
            var strTrimmed = uri.TrimStart("/Pages/CourseDetail.xaml".ToCharArray());
            if (!String.IsNullOrEmpty(strTrimmed))
            {
                strTrimmed = strTrimmed.TrimStart("?v=".ToCharArray());
                int idx = 0;
                if (int.TryParse(strTrimmed, out idx) && idx > Pivot_Core.Items.Count)
                {
                    idx = 0;
                    idx = Math.Max(0, idx);
                }

                Pivot_Core.SelectedIndex = idx;
            }
        }
    }
}