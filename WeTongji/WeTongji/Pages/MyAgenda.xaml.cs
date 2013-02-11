using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;

namespace WeTongji
{
    public partial class MyAgenda : PhoneApplicationPage
    {
        public MyAgenda()
        {
            InitializeComponent();

            this.Loaded += (o, e) =>
            {
                ListBox_Core.ItemsSource = new FakeAgendaItem[] 
                {
                    new FakeAgendaItem()
                    {
                        FakeTitle="\"灯光.人.社会.社会责任\" 主题座谈会",
                        FakePlace = "同济大学中芬中心",
                        FakeType = FakeAgendaItem.FakeAgendaType.Activity,
                        FakeTimeSource = DateTime.Now
                    },

                    new FakeAgendaItem()
                    {
                        FakeTitle = "软件工程",
                        FakePlace = "专用教室",
                        FakeType = FakeAgendaItem.FakeAgendaType.RequiredCourse,
                        FakeTimeSource = new DateTime(2013, 2, 3, 18, 30, 0)
                    },

                    new FakeAgendaItem()
                    {
                        FakeTitle = "羽毛球",
                        FakePlace = "马桶楼",
                        FakeType = FakeAgendaItem.FakeAgendaType.OptionalCourse,
                        FakeTimeSource = new DateTime(2013, 1, 1, 8, 30, 0)
                    },

                    new FakeAgendaItem()
                    {
                        FakeTitle = "高等数学上考试",
                        FakePlace = "南楼101",
                        FakeType = FakeAgendaItem.FakeAgendaType.ExamInfo,
                        FakeTimeSource = new DateTime(2012, 7, 1, 10, 30, 0)
                    }
                };
            };
        }

        private void ListBox_Core_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lb = sender as ListBox;

            if (-1 == lb.SelectedIndex)
                return;

            var item = lb.SelectedItem as FakeAgendaItem;
            lb.SelectedIndex = -1;

            switch (item.FakeType)
            {
                case FakeAgendaItem.FakeAgendaType.Activity:
                    this.NavigationService.Navigate(new Uri("/Pages/Activity.xaml", UriKind.RelativeOrAbsolute));
                    break;
                case FakeAgendaItem.FakeAgendaType.ExamInfo:
                    this.NavigationService.Navigate(new Uri("/Pages/CourseDetail.xaml?v=1", UriKind.RelativeOrAbsolute));
                    break;
                case FakeAgendaItem.FakeAgendaType.RequiredCourse:
                case FakeAgendaItem.FakeAgendaType.OptionalCourse:
                    this.NavigationService.Navigate(new Uri("/Pages/CourseDetail.xaml", UriKind.RelativeOrAbsolute));
                    break;
            }
        }
    }

    public class FakeAgendaItem
    {
        public String FakeTitle { get; set; }

        public String FakePlace { get; set; }

        public String FakeDate
        {
            get
            {
                if (DateTime.Now.Date == FakeTimeSource.Date)
                {
                    return "今日";
                }
                else
                {
                    return FakeTimeSource.ToString("M月d日");
                }
            }
        }

        public String FakeTime
        {
            get { return FakeTimeSource.ToString("HH:mm"); }
        }

        public Brush FakeTitleBrush
        {
            get 
            {
                switch (FakeType)
                {
                    case FakeAgendaType.Activity:
                        return App.Current.Resources["ActivityAgendaTitleBrush"] as SolidColorBrush;
                    case FakeAgendaType.ExamInfo:
                        return App.Current.Resources["ExamInfoAgendaTitleBrush"] as SolidColorBrush;
                    case FakeAgendaType.RequiredCourse:
                        return App.Current.Resources["RequiredCourseAgendaTitleBrush"] as SolidColorBrush;
                    case FakeAgendaType.OptionalCourse:
                        return App.Current.Resources["OptionalCourseAgendaTitleBrush"] as SolidColorBrush;
                    default:
                        break;
                }
                return null;
            }
        }

        public FakeAgendaType FakeType { get; set; }

        public DateTime FakeTimeSource { set; private get; }

        public enum FakeAgendaType
        {
            Activity,
            RequiredCourse,
            OptionalCourse,
            ExamInfo
        }
    }
}