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

namespace Fouad.Views
{
    /// <summary>
    /// Interaction logic for HistoryTableView.xaml
    /// </summary>
    public partial class HistoryTableView : UserControl
    {
        public HistoryTableView()
        {
            InitializeComponent();
            this.DataContextChanged += HistoryTableView_DataContextChanged;
        }

        private void HistoryTableView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is HistoryTableViewModel oldViewModel)
            {
                oldViewModel.ColumnsChanged -= ViewModel_ColumnsChanged;
            }
            
            if (e.NewValue is HistoryTableViewModel newViewModel)
            {
                newViewModel.ColumnsChanged += ViewModel_ColumnsChanged;
                // Initial column setup
                UpdateDataGridColumns();
            }
        }

        private void ViewModel_ColumnsChanged(object? sender, EventArgs e)
        {
            // Update columns when selection changes
            UpdateDataGridColumns();
        }

        private void UpdateDataGridColumns()
        {
            HistoryDataGrid.Columns.Clear();

            // Add the Selection Column (Frozen to left)
            var selectColumn = new DataGridCheckBoxColumn
            {
                Header = Application.Current.FindResource("Select")?.ToString() ?? "Select",
                Binding = new Binding("IsSelected"),
                Width = new DataGridLength(50)
            };
            HistoryDataGrid.Columns.Add(selectColumn);

            // Add the ID Column (Frozen to left)
            var idColumn = new DataGridTextColumn
            {
                Header = Application.Current.FindResource("ID")?.ToString() ?? "ID",
                Binding = new Binding("Id"),
                Width = new DataGridLength(60)
            };
            HistoryDataGrid.Columns.Add(idColumn);

            // Get the main window to access the main view model
            var mainWindow = Application.Current.MainWindow as MainWindow;
            
            // Get the main view model from the main window
            var mainViewModel = mainWindow?.DataContext as MainViewModel;
            
            // Get selected columns from the column selector
            var selectedColumns = mainViewModel?.ColumnSelector?.GetSelectedColumns() ?? new List<string>();

            // Add dynamic columns based on selected columns from the column selector
            for (int i = 0; i < selectedColumns.Count; i++)
            {
                var columnName = selectedColumns[i];
                var textColumn = new DataGridTextColumn
                {
                    Header = columnName,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star)
                };
                
                // Use a converter to safely access the array element
                var binding = new Binding("DynamicColumnValues")
                {
                    Converter = new Converters.DynamicColumnValueConverter(),
                    ConverterParameter = i
                };
                textColumn.Binding = binding;
                
                HistoryDataGrid.Columns.Add(textColumn);
            }

            // Add the Action Buttons Column (Fixed width of 70px)
            var actionsColumn = new DataGridTemplateColumn
            {
                Header = Application.Current.FindResource("Actions")?.ToString() ?? "Actions",
                Width = new DataGridLength(70) // Fixed width as requested
            };

            // Create the data template for action buttons
            var template = new DataTemplate();
            var stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
            stackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            // View Button
            var viewButtonFactory = new FrameworkElementFactory(typeof(Button));
            viewButtonFactory.SetValue(Button.ContentProperty, Application.Current.FindResource("View")?.ToString() ?? "View");
            viewButtonFactory.SetValue(Button.StyleProperty, Application.Current.FindResource("TextButtonStyle"));
            viewButtonFactory.SetValue(Button.PaddingProperty, new Thickness(5, 2, 5, 2));
            viewButtonFactory.SetValue(Button.MarginProperty, new Thickness(0, 0, 2, 0));
            viewButtonFactory.SetValue(Button.FontSizeProperty, 10.0);
            viewButtonFactory.SetBinding(Button.CommandProperty, new Binding("DataContext.ViewCommand")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(UserControl), 1)
            });
            viewButtonFactory.SetBinding(Button.CommandParameterProperty, new Binding("."));

            // Edit Button
            var editButtonFactory = new FrameworkElementFactory(typeof(Button));
            editButtonFactory.SetValue(Button.ContentProperty, Application.Current.FindResource("Edit")?.ToString() ?? "Edit");
            editButtonFactory.SetValue(Button.StyleProperty, Application.Current.FindResource("TextButtonStyle"));
            editButtonFactory.SetValue(Button.PaddingProperty, new Thickness(5, 2, 5, 2));
            editButtonFactory.SetValue(Button.MarginProperty, new Thickness(0, 0, 2, 0));
            editButtonFactory.SetValue(Button.FontSizeProperty, 10.0);
            editButtonFactory.SetBinding(Button.CommandProperty, new Binding("DataContext.EditCommand")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(UserControl), 1)
            });
            editButtonFactory.SetBinding(Button.CommandParameterProperty, new Binding("."));

            // Delete Button
            var deleteButtonFactory = new FrameworkElementFactory(typeof(Button));
            deleteButtonFactory.SetValue(Button.ContentProperty, Application.Current.FindResource("Delete")?.ToString() ?? "Delete");
            deleteButtonFactory.SetValue(Button.StyleProperty, Application.Current.FindResource("DeleteConfirmationStyle"));
            deleteButtonFactory.SetValue(Button.PaddingProperty, new Thickness(5, 2, 5, 2));
            deleteButtonFactory.SetValue(Button.FontSizeProperty, 10.0);
            deleteButtonFactory.SetValue(Button.WidthProperty, 50.0);
            deleteButtonFactory.SetBinding(Button.CommandProperty, new Binding("DataContext.DeleteItemCommand")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(UserControl), 1)
            });
            deleteButtonFactory.SetBinding(Button.CommandParameterProperty, new Binding("."));

            stackPanelFactory.AppendChild(viewButtonFactory);
            stackPanelFactory.AppendChild(editButtonFactory);
            stackPanelFactory.AppendChild(deleteButtonFactory);

            template.VisualTree = stackPanelFactory;
            actionsColumn.CellTemplate = template;

            HistoryDataGrid.Columns.Add(actionsColumn);
        }
    }
}