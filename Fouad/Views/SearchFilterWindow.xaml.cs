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
using System.Windows.Shapes;
using Fouad.ViewModels;

namespace Fouad.Views
{
    /// <summary>
    /// Interaction logic for SearchFilterWindow.xaml
    /// </summary>
    public partial class SearchFilterWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchFilterWindow"/> class.
        /// </summary>
        public SearchFilterWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchFilterWindow"/> class with a view model.
        /// </summary>
        /// <param name="viewModel">The search filter view model.</param>
        public SearchFilterWindow(SearchFilterViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }

        /// <summary>
        /// Handles the SelectionChanged event of the ColumnValueFiltersDataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void ColumnValueFiltersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is SearchFilterViewModel viewModel)
            {
                // Update the IsSelected property for all items
                foreach (var item in viewModel.ColumnValueFilters)
                {
                    item.IsSelected = ColumnValueFiltersDataGrid.SelectedItems.Contains(item);
                }
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the ValueRangeFiltersDataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void ValueRangeFiltersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is SearchFilterViewModel viewModel)
            {
                // Update the IsSelected property for all items
                foreach (var item in viewModel.ValueRangeFilters)
                {
                    item.IsSelected = ValueRangeFiltersDataGrid.SelectedItems.Contains(item);
                }
            }
        }
    }
}