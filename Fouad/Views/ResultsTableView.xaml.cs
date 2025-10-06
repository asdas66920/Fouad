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
using Fouad.ViewModels;
using Fouad.Models;
using Fouad.Converters;
using Fouad.Controls;

namespace Fouad.Views
{
    /// <summary>
    /// Interaction logic for ResultsTableView.xaml
    /// </summary>
    public partial class ResultsTableView : UserControl
    {
        public ResultsTableView()
        {
            InitializeComponent();
            this.DataContextChanged += ResultsTableView_DataContextChanged;
            this.Loaded += ResultsTableView_Loaded;
        }

        private void ResultsTableView_Loaded(object sender, RoutedEventArgs e)
        {
            // Focus the search box when the view loads
            SearchTextBox.Focus();
        }

        private void ResultsTableView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ResultsTableViewModel oldViewModel)
            {
                oldViewModel.ColumnsChanged -= ViewModel_ColumnsChanged;
                oldViewModel.DuplicateResultAdded -= ViewModel_DuplicateResultAdded;
                oldViewModel.ResultAddedSuccessfully -= ViewModel_ResultAddedSuccessfully;
            }
            
            if (e.NewValue is ResultsTableViewModel newViewModel)
            {
                newViewModel.ColumnsChanged += ViewModel_ColumnsChanged;
                newViewModel.DuplicateResultAdded += ViewModel_DuplicateResultAdded;
                newViewModel.ResultAddedSuccessfully += ViewModel_ResultAddedSuccessfully;
                // Initial column setup
                UpdateDataGridColumns();
            }
        }

        private void ViewModel_ColumnsChanged(object? sender, EventArgs e)
        {
            // Update columns when selection changes
            UpdateDataGridColumns();
        }

        // Handle duplicate result added event
        private void ViewModel_DuplicateResultAdded(object? sender, Result e)
        {
            // Flash the row red for duplicate addition
            FlashRowRed(e);
        }

        // Handle successful result addition event
        private void ViewModel_ResultAddedSuccessfully(object? sender, Result e)
        {
            // Clear search and return focus after successful addition
            Dispatcher.BeginInvoke(new Action(() => {
                SearchTextBox.Text = "";
                SearchTextBox.Focus();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void UpdateDataGridColumns()
        {
            ResultsDataGrid.Columns.Clear();

            // Add the ID column first
            var idColumn = new DataGridTextColumn
            {
                Header = "ID",
                Binding = new Binding("Id"),
                Width = new DataGridLength(50)
            };
            ResultsDataGrid.Columns.Add(idColumn);

            // Add the "Add" column immediately after the ID column
            var addButtonColumn = new DataGridTemplateColumn
            {
                Header = "Add",
                Width = new DataGridLength(80)
            };
            
            // Create the data template for the button
            var template = new DataTemplate();
            var factory = new FrameworkElementFactory(typeof(Button));
            factory.SetValue(Button.ContentProperty, "Add");
            factory.SetValue(Button.CommandProperty, new Binding("DataContext.AddToHistoryCommand") 
            { 
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGrid), 1) 
            });
            factory.SetBinding(Button.CommandParameterProperty, new Binding("."));
            
            // Set button style based on whether the item has been added
            var style = new Style(typeof(Button));
            var trigger = new DataTrigger();
            trigger.Binding = new Binding("IsAddedToHistory");
            trigger.Value = true;
            trigger.Setters.Add(new Setter(Button.BackgroundProperty, Brushes.Red));
            trigger.Setters.Add(new Setter(Button.IsEnabledProperty, false));
            style.Triggers.Add(trigger);
            
            factory.SetValue(Button.StyleProperty, style);
            template.VisualTree = factory;
            addButtonColumn.CellTemplate = template;
            
            ResultsDataGrid.Columns.Add(addButtonColumn);

            // Get the column selector from the main view model
            var mainViewModel = Application.Current.MainWindow?.DataContext as ViewModels.MainViewModel;
            if (mainViewModel?.ColumnSelector != null)
            {
                // Add dynamic columns based on selection
                var selectedColumns = mainViewModel.ColumnSelector.GetSelectedColumns();
                for (int i = 0; i < selectedColumns.Count; i++)
                {
                    var columnName = selectedColumns[i];
                    
                    // Create a template column for highlighted text
                    var templateColumn = new DataGridTemplateColumn
                    {
                        Header = columnName,
                        Width = new DataGridLength(1, DataGridLengthUnitType.Star)
                    };
                    
                    // Create data template for the cell content with highlighting
                    var cellTemplate = new DataTemplate();
                    
                    // Create a HighlightTextBlock to display the highlighted text
                    var textBlockFactory = new FrameworkElementFactory(typeof(Controls.HighlightTextBlock));
                    
                    // Bind to the cell value with safety check
                    // Use a converter to safely access the array element
                    var binding = new Binding("DynamicColumnValues")
                    {
                        Converter = new DynamicColumnValueConverter(),
                        ConverterParameter = i
                    };
                    textBlockFactory.SetBinding(Controls.HighlightTextBlock.TextProperty, binding);
                    
                    // Bind to the search term from the ViewModel
                    textBlockFactory.SetBinding(Controls.HighlightTextBlock.SearchTermProperty, new Binding("DataContext.SearchText") 
                    { 
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGrid), 1) 
                    });
                    
                    // Bind to the highlighting enabled flag from the ViewModel
                    textBlockFactory.SetBinding(Controls.HighlightTextBlock.IsHighlightingEnabledProperty, new Binding("DataContext.IsHighlightingEnabled") 
                    { 
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGrid), 1) 
                    });
                    
                    cellTemplate.VisualTree = textBlockFactory;
                    templateColumn.CellTemplate = cellTemplate;
                    
                    ResultsDataGrid.Columns.Add(templateColumn);
                }
            }
        }
        
        // Handle Enter key press in the search box
        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DataContext is ResultsTableViewModel viewModel)
                {
                    viewModel.HandleEnterKeyPress();
                    
                    // Automatically focus the first row's Add button after search completes
                    // Give the DataGrid time to update
                    Dispatcher.BeginInvoke(new Action(() => {
                        if (viewModel.Results.Count > 0)
                        {
                            ResultsDataGrid.SelectedIndex = 0;
                            ResultsDataGrid.Focus();
                            
                            // Try to focus the Add button in the first row
                            var firstRow = ResultsDataGrid.ItemContainerGenerator.ContainerFromIndex(0) as DataGridRow;
                            if (firstRow != null)
                            {
                                var addButton = FindVisualChild<Button>(firstRow);
                                if (addButton != null)
                                {
                                    addButton.Focus();
                                    
                                    // Check if this row was already added and flash it red if so
                                    if (viewModel.Results.Count > 0 && viewModel.Results[0].IsAddedToHistory)
                                    {
                                        FlashRowRed(viewModel.Results[0]);
                                        // Play duplicate sound
                                        viewModel.AudioService.PlaySound("AlreadyAdded");
                                        
                                        // Return focus to search box
                                        Dispatcher.BeginInvoke(new Action(() => {
                                            SearchTextBox.Focus();
                                        }), System.Windows.Threading.DispatcherPriority.Background);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // No results found, clear the field and return focus
                            SearchTextBox.Text = "";
                            SearchTextBox.Focus();
                        }
                    }), System.Windows.Threading.DispatcherPriority.Background);
                }
                e.Handled = true;
            }
        }
        
        // Handle Enter key press in the DataGrid
        private void ResultsTableView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && ResultsDataGrid.SelectedItem is Result selectedResult)
            {
                // Add the selected result to history
                if (DataContext is ResultsTableViewModel viewModel)
                {
                    viewModel.HandleAddEnterKeyPress(selectedResult);
                }
                e.Handled = true;
            }
        }
        
        // Handle GotFocus event for search box to clear text
        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
        }
        
        // Helper method to find visual child elements
        private static T? FindVisualChild<T>(DependencyObject? parent) where T : DependencyObject
        {
            if (parent == null) return null;
            
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T)
                    return (T)child;
                    
                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }
        
        // Method to flash a row red when a duplicate is added
        private void FlashRowRed(Result result)
        {
            // Find the DataGridRow for the result
            var rowIndex = ResultsDataGrid.Items.IndexOf(result);
            if (rowIndex >= 0)
            {
                var row = ResultsDataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
                if (row != null)
                {
                    // Create a red brush for flashing
                    var redBrush = new SolidColorBrush(Colors.Red);
                    var originalBrush = row.Background;
                    
                    // Flash the row red
                    row.Background = redBrush;
                    
                    // Reset the color after a delay
                    var timer = new System.Windows.Threading.DispatcherTimer();
                    timer.Interval = TimeSpan.FromMilliseconds(500);
                    timer.Tick += (s, e) => {
                        row.Background = originalBrush ?? SystemColors.WindowBrush;
                        timer.Stop();
                    };
                    timer.Start();
                }
            }
        }
    }
}