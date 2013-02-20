using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace WeTongji
{
    public partial class SocietyNews : PhoneApplicationPage
    {
        public SocietyNews()
        {
            InitializeComponent();
			
			this.Loaded += (o,e) =>
			{
				NoIllustrationGrid.Visibility = Visibility.Visible;
			};
        }

        #region [Overridden]

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ThemeManager.ToDarkTheme();
        }

        #endregion
    }
}