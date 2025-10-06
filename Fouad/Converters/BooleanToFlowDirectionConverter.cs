using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Fouad.Converters
{
    /// <summary>
    /// Converts a boolean value to a FlowDirection value.
    /// True = RightToLeft, False = LeftToRight
    /// </summary>
    public class BooleanToFlowDirectionConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to a FlowDirection value.
        /// </summary>
        /// <param name="value">The boolean value to convert.</param>
        /// <param name="targetType">The target type (FlowDirection).</param>
        /// <param name="parameter">Optional parameter.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>FlowDirection.RightToLeft if value is true, otherwise FlowDirection.LeftToRight.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isArabic)
            {
                return isArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            }
            return FlowDirection.LeftToRight;
        }

        /// <summary>
        /// Converts a FlowDirection value back to a boolean value.
        /// </summary>
        /// <param name="value">The FlowDirection value to convert.</param>
        /// <param name="targetType">The target type (boolean).</param>
        /// <param name="parameter">Optional parameter.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>True if value is FlowDirection.RightToLeft, otherwise false.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FlowDirection flowDirection)
            {
                return flowDirection == FlowDirection.RightToLeft;
            }
            return false;
        }
    }
}