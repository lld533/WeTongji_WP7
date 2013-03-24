using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WeTongji.Converter
{
    public class DateToAgendaTopDisplayDateStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var date = (DateTime)value;
            String dayOfWeek = String.Empty;

            switch (date.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    dayOfWeek = "星期日";
                    break;
                case DayOfWeek.Monday:
                    dayOfWeek = "星期一";
                    break;
                case DayOfWeek.Tuesday:
                    dayOfWeek = "星期二";
                    break;
                case DayOfWeek.Wednesday:
                    dayOfWeek = "星期三";
                    break;
                case DayOfWeek.Thursday:
                    dayOfWeek = "星期四";
                    break;
                case DayOfWeek.Friday:
                    dayOfWeek = "星期五";
                    break;
                case DayOfWeek.Saturday:
                    dayOfWeek = "星期六";
                    break;
            }

            if (date.Date == DateTime.Now.Date)
            {
                return "今日，" + dayOfWeek;
            }

            return dayOfWeek + "，" + date.ToString("yyyy年M月d日");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}