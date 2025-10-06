using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Fouad.Views
{
    /// <summary>
    /// Interaction logic for SideBarView.xaml
    /// </summary>
    public partial class SideBarView : UserControl
    {
        private bool _isCollapsed = false;
        private GridLength _originalWidth;

        public SideBarView()
        {
            InitializeComponent();
            _originalWidth = new GridLength(250); // Increased width for better content display
        }

        private void CollapseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var parentGrid = this.Parent as Grid;
                if (parentGrid != null)
                {
                    // Find the column definition that contains this sidebar
                    var sidebarColumn = FindSidebarColumn(parentGrid);
                    if (sidebarColumn != null)
                    {
                        if (!_isCollapsed)
                        {
                            sidebarColumn.Width = new GridLength(30);
                            CollapseButton.Content = ">";
                            // Hide content when collapsed
                            SidebarContentPanel.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            sidebarColumn.Width = _originalWidth;
                            CollapseButton.Content = "<";
                            // Show content when expanded
                            SidebarContentPanel.Visibility = Visibility.Visible;
                        }

                        _isCollapsed = !_isCollapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't crash the application
                System.Diagnostics.Debug.WriteLine($"Error in CollapseButton_Click: {ex.Message}");
            }
        }

        /// <summary>
        /// Finds the column in the parent grid that contains this sidebar
        /// </summary>
        /// <param name="parentGrid">The parent grid</param>
        /// <returns>The column definition or null if not found</returns>
        private ColumnDefinition? FindSidebarColumn(Grid parentGrid)
        {
            // Look for the column that contains this control
            for (int i = 0; i < parentGrid.ColumnDefinitions.Count; i++)
            {
                // This is a simplified approach - in a real implementation you might want to
                // use more sophisticated methods to identify the correct column
                if (parentGrid.ColumnDefinitions[i].Width.Value >= 200) // Assume sidebar column is wider
                {
                    return parentGrid.ColumnDefinitions[i];
                }
            }
            
            // Fallback to first column if not found
            return parentGrid.ColumnDefinitions.Count > 0 ? parentGrid.ColumnDefinitions[0] : null;
        }
    }
}