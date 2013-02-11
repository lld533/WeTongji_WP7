using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Windows.Navigation;

namespace WeTongji
{
    public class AppSquareItem : INotifyPropertyChanged
    {
        #region [Field]
        /// <summary>
        /// Image uri string
        /// </summary>
        private string _img;

        /// <summary>
        /// Item name
        /// </summary>
        private string _name;

        private string _navigation;

        private NavigationService _ns;
        #endregion

        #region [Bindings]

        public BitmapImage Image
        {
            get
            {
                BitmapImage img = new BitmapImage();
                if (!string.IsNullOrEmpty(_img))
                    img = new BitmapImage(new Uri(_img, UriKind.RelativeOrAbsolute));
                return img;
            }
            set { }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set { }
        }

        public Func<bool> Do
        {
            get
            {
                return () =>
                {
                    var result = false;
                    if (_ns != null)
                    {
                        result = _ns.Navigate(new Uri("/" + _navigation + ".xaml", UriKind.Relative));
                    }
                    return result;
                };
            }
        }

        public NavigationService SetNavigationService
        {
            set { _ns = value; }
        }

        #endregion

        #region [Constructor]

        public AppSquareItem(string imgUri, string name, string navigation)
        {
            _img = imgUri;
            _name = name;
            _navigation = navigation;
        }

        #endregion

        #region [Property changed event members]
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            var temp = PropertyChanged;
            if (null != temp)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }

    /// <summary>
    /// AppSquare part
    /// </summary>
    public partial class MainPage
    {
        
        
        private void stackPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var sp = sender as StackPanel;
            sp.Background = new SolidColorBrush();
        }

        private void stackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var sp = sender as StackPanel;
            //...Turn blue FF13CECA
            sp.Background = new SolidColorBrush(new Color() { A = Byte.MaxValue, R = 19, G = 206, B = 202 });
        }

        private void stackPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            var sp = sender as StackPanel;
            sp.Background = new SolidColorBrush();
        }
    }
}
