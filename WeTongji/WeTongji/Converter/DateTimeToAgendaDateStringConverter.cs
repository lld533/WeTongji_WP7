using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WeTongji.Converter
{
    public class DateTimeToAgendaDateStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var date = (DateTime)value;
            if (date == DateTime.Now.Date)
            {
                return "今日";
            }
            else
            {
                return date.ToString("M月d日");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}