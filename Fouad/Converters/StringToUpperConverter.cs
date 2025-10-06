using System;
using System.Globalization;
using System.Windows.Data;

namespace Fouad.Converters
{
    public class StringToUpperConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return stringValue.ToUpper();
            }
            return value?.ToString()?.ToUpper() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return stringValue.ToLower();
            }
            return value?.ToString()?.ToLower() ?? string.Empty;
        }
    }
}