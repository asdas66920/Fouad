using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace Fouad.Converters
{
    /// <summary>
    /// Converter to highlight search terms in text with different colors and styles.
    /// </summary>
    public class SearchHighlightConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts a value and search term to a highlighted text.
        /// </summary>
        /// <param name="values">The value array. First element is the text, second is the search term.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>Highlighted text as an Inlines collection.</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2 || values[0] == null)
                return new List<Inline>();

            // Extract text and search term from values
            string text = values[0]?.ToString() ?? "";
            string searchTerm = values[1]?.ToString() ?? "";

            var inlines = new List<Inline>();

            // If no search term or text is empty, return the text as is
            if (string.IsNullOrEmpty(searchTerm) || string.IsNullOrEmpty(text))
            {
                inlines.Add(new Run(text));
                return inlines;
            }

            // Convert search term to lowercase for case-insensitive comparison
            string lowerText = text.ToLower();
            string lowerSearchTerm = searchTerm.ToLower();

            int lastIdx = 0;
            int idx;

            // Find all occurrences of the search term
            while ((idx = lowerText.IndexOf(lowerSearchTerm, lastIdx)) != -1)
            {
                // Add text before the match (if any)
                if (idx > lastIdx)
                {
                    var beforeText = text.Substring(lastIdx, idx - lastIdx);
                    var beforeRun = new Run(beforeText);
                    beforeRun.SetResourceReference(FrameworkContentElement.StyleProperty, "BoldGreenText");
                    inlines.Add(beforeRun);
                }

                // Add the matched text
                var matchedText = text.Substring(idx, searchTerm.Length);
                var matchedRun = new Run(matchedText);
                matchedRun.SetResourceReference(FrameworkContentElement.StyleProperty, "BoldRedText");
                inlines.Add(matchedRun);

                lastIdx = idx + searchTerm.Length;
            }

            // Add remaining text after the last match (if any)
            if (lastIdx < text.Length)
            {
                var afterText = text.Substring(lastIdx);
                var afterRun = new Run(afterText);
                afterRun.SetResourceReference(FrameworkContentElement.StyleProperty, "BoldGreenText");
                inlines.Add(afterRun);
            }

            // If no matches were found, just return the whole text in green
            if (inlines.Count == 0)
            {
                var run = new Run(text);
                run.SetResourceReference(FrameworkContentElement.StyleProperty, "BoldGreenText");
                inlines.Add(run);
            }

            return inlines;
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}