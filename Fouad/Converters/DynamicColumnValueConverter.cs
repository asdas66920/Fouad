using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Fouad.Converters
{
    /// <summary>
    /// Converter to safely access elements in a list by index.
    /// Prevents ArgumentOutOfRangeException when accessing DynamicColumnValues.
    /// </summary>
    public class DynamicColumnValueConverter : IValueConverter
    {
        /// <summary>
        /// Converts a list to a specific element by index.
        /// </summary>
        /// <param name="value">The list of values.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The index to access.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>The value at the specified index, or empty string if index is out of range.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is List<string> values && parameter is int index)
            {
                if (index >= 0 && index < values.Count)
                {
                    return values[index];
                }
                return string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Not implemented for this converter.
        /// </summary>
        /// <param name="value">The value to convert back.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>Not implemented.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}