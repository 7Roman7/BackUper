using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BackUper.Utilities
{
    public class Converters
    {
        public class DateTimeToDateConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return ((DateTime)value).ToString("dd.MM.yyyy mm:ss");
            }
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return DateTime.Now.ToString("dd.MM.yyyy");
            }
        }


    }
}
