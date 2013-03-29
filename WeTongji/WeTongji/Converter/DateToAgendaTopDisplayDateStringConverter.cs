using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace WeTongji.Converter
{
    public class DateToAgendaTopDisplayDateStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var date = (DateTime)value;

            //...Todo @_@ Localizable
            if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "zh")
            {
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
            else
            {
                if (date.Date == DateTime.Now.Date)
                    return "Today, " + date.DayOfWeek.ToString().ToUpper();

                var sb = new StringBuilder(date.DayOfWeek.ToString().ToUpper());
                sb.Append(", ");
                switch (date.Month)
                {
                    case 1:
                        sb.Append("JANUARY");
                        break;
                    case 2:
                        sb.Append("FEBURARY");
                        break;
                    case 3:
                        sb.Append("MARCH");
                        break;
                    case 4:
                        sb.Append("APRIL");
                        break;
                    case 5:
                        sb.Append("MAY");
                        break;
                    case 6:
                        sb.Append("JUNE");
                        break;
                    case 7:
                        sb.Append("JULY");
                        break;
                    case 8:
                        sb.Append("AUGUST");
                        break;
                    case 9:
                        sb.Append("SEPTEMBER");
                        break;
                    case 10:
                        sb.Append("OCTOBER");
                        break;
                    case 11:
                        sb.Append("NOVEMBER");
                        break;
                    case 12:
                        sb.Append("DECEMBER");
                        break;
                    default:
                        break;
                }

                sb.AppendFormat(" {0:dd}, {0:yyyy}", date);
                return sb.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}