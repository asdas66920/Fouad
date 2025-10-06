using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Fouad.Converters
{
    /// <summary>
    /// Converts a boolean value to a Visibility value.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to a Visibility value.
        /// </summary>
        /// <param name="value">The boolean value to convert.</param>
        /// <param name="targetType">The target type (should be Visibility).</param>
        /// <param name="parameter">Optional parameter to invert the conversion.</param>
        /// <param name="culture">The culture to use in the conversion.</param>
        /// <returns>Visibility.Visible if true, Visibility.Collapsed if false.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = value is bool && (bool)value;
            
            // Check if we should invert the conversion
            if (parameter is string paramString && paramString.Equals("Invert", StringComparison.OrdinalIgnoreCase))
            {
                boolValue = !boolValue;
            }
            
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Converts a Visibility value back to a boolean value.
        /// </summary>
        /// <param name="value">The Visibility value to convert.</param>
        /// <param name="targetType">The target type (should be bool).</param>
        /// <param name="parameter">Optional parameter to invert the conversion.</param>
        /// <param name="culture">The culture to use in the conversion.</param>
        /// <returns>True if Visibility.Visible, false otherwise.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = value is Visibility visibility && visibility == Visibility.Visible;
            
            // Check if we should invert the conversion
            if (parameter is string paramString && paramString.Equals("Invert", StringComparison.OrdinalIgnoreCase))
            {
                result = !result;
            }
            
            return result;
        }
    }
}