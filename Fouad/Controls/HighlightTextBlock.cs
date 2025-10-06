using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Fouad.Controls
{
    /// <summary>
    /// A TextBlock that can highlight search terms with different colors.
    /// </summary>
    public class HighlightTextBlock : TextBlock
    {
        /// <summary>
        /// Identifies the Text dependency property.
        /// </summary>
        public static new readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(HighlightTextBlock), 
                new PropertyMetadata(string.Empty, OnTextChanged));

        /// <summary>
        /// Identifies the SearchTerm dependency property.
        /// </summary>
        public static readonly DependencyProperty SearchTermProperty =
            DependencyProperty.Register("SearchTerm", typeof(string), typeof(HighlightTextBlock), 
                new PropertyMetadata(string.Empty, OnSearchTermChanged));

        /// <summary>
        /// Identifies the IsHighlightingEnabled dependency property.
        /// </summary>
        public static readonly DependencyProperty IsHighlightingEnabledProperty =
            DependencyProperty.Register("IsHighlightingEnabled", typeof(bool), typeof(HighlightTextBlock), 
                new PropertyMetadata(false, OnHighlightingEnabledChanged));

        /// <summary>
        /// Gets or sets the text to display.
        /// </summary>
        public new string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Gets or sets the search term to highlight.
        /// </summary>
        public string SearchTerm
        {
            get { return (string)GetValue(SearchTermProperty); }
            set { SetValue(SearchTermProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether highlighting is enabled.
        /// </summary>
        public bool IsHighlightingEnabled
        {
            get { return (bool)GetValue(IsHighlightingEnabledProperty); }
            set { SetValue(IsHighlightingEnabledProperty, value); }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (HighlightTextBlock)d;
            control.UpdateHighlighting();
        }

        private static void OnSearchTermChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (HighlightTextBlock)d;
            control.UpdateHighlighting();
        }

        private static void OnHighlightingEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (HighlightTextBlock)d;
            control.UpdateHighlighting();
        }

        private void UpdateHighlighting()
        {
            // Clear existing inlines
            Inlines.Clear();

            // If no text, nothing to display
            if (string.IsNullOrEmpty(Text))
                return;

            // If highlighting is not enabled or no search term, display text normally
            if (!IsHighlightingEnabled || string.IsNullOrEmpty(SearchTerm))
            {
                Inlines.Add(new Run(Text));
                return;
            }

            // Check if this cell contains the search term
            bool containsSearchTerm = Text.IndexOf(SearchTerm, StringComparison.OrdinalIgnoreCase) >= 0;
            
            // Only highlight if this cell contains the search term
            if (containsSearchTerm)
            {
                // Convert search term to lowercase for case-insensitive comparison
                string lowerText = Text.ToLower();
                string lowerSearchTerm = SearchTerm.ToLower();

                int lastIdx = 0;
                int idx;

                // Find all occurrences of the search term
                while ((idx = lowerText.IndexOf(lowerSearchTerm, lastIdx)) != -1)
                {
                    // Add text before the match (if any)
                    if (idx > lastIdx)
                    {
                        var beforeText = Text.Substring(lastIdx, idx - lastIdx);
                        var beforeRun = new Run(beforeText);
                        beforeRun.SetResourceReference(FrameworkContentElement.StyleProperty, "BoldGreenText");
                        Inlines.Add(beforeRun);
                    }

                    // Add the matched text
                    var matchedText = Text.Substring(idx, SearchTerm.Length);
                    var matchedRun = new Run(matchedText);
                    matchedRun.SetResourceReference(FrameworkContentElement.StyleProperty, "BoldBlueText");
                    Inlines.Add(matchedRun);

                    lastIdx = idx + SearchTerm.Length;
                }

                // Add remaining text after the last match (if any)
                if (lastIdx < Text.Length)
                {
                    var afterText = Text.Substring(lastIdx);
                    var afterRun = new Run(afterText);
                    afterRun.SetResourceReference(FrameworkContentElement.StyleProperty, "BoldGreenText");
                    Inlines.Add(afterRun);
                }
            }
            else
            {
                // No match in this cell, display normally
                Inlines.Add(new Run(Text));
            }
        }
    }
}