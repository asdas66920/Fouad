using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Fouad.Commands;

namespace Fouad.ViewModels
{
    /// <summary>
    /// ViewModel for the column selector view.
    /// Manages column selection and provides functionality for selecting/deselecting columns.
    /// </summary>
    public class ColumnSelectorViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ColumnItem> _columns = new ObservableCollection<ColumnItem>();
        private bool _areAllSelected = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnSelectorViewModel"/> class.
        /// </summary>
        public ColumnSelectorViewModel()
        {
            ToggleAllColumnsCommand = new RelayCommand(ToggleAllColumns);
        }

        /// <summary>
        /// Gets or sets the collection of columns.
        /// </summary>
        public ObservableCollection<ColumnItem> Columns
        {
            get { return _columns; }
            set
            {
                _columns = value ?? new ObservableCollection<ColumnItem>();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether all columns are selected.
        /// </summary>
        public bool AreAllSelected
        {
            get { return _areAllSelected; }
            set
            {
                _areAllSelected = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Event raised when column selection changes.
        /// </summary>
        public event EventHandler? ColumnSelectionChanged;

        /// <summary>
        /// Updates the column list with new column headers.
        /// </summary>
        /// <param name="columnHeaders">List of column headers.</param>
        public void UpdateColumns(IEnumerable<string> columnHeaders)
        {
            _columns.Clear();
            if (columnHeaders != null)
            {
                int index = 0;
                foreach (var header in columnHeaders)
                {
                    var columnItem = new ColumnItem { Name = header, IsSelected = true };
                    columnItem.SetSelectionChangedCallback(OnColumnSelectionChanged);
                    _columns.Add(columnItem);
                    index++;
                }
            }
            _areAllSelected = true;
            OnPropertyChanged(nameof(Columns));
            OnPropertyChanged(nameof(AreAllSelected));
        }

        /// <summary>
        /// Called when any column's selection state changes.
        /// </summary>
        private void OnColumnSelectionChanged()
        {
            // Update the AreAllSelected property based on individual column selections
            _areAllSelected = _columns.All(c => c.IsSelected);
            OnPropertyChanged(nameof(AreAllSelected));
            
            // Notify subscribers that column selection has changed
            ColumnSelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets the list of selected column names.
        /// </summary>
        /// <returns>List of selected column names.</returns>
        public List<string> GetSelectedColumns()
        {
            return _columns.Where(c => c.IsSelected).Select(c => c.Name).ToList();
        }

        /// <summary>
        /// Resets the column selector to its initial state.
        /// </summary>
        public void Reset()
        {
            _columns.Clear();
            AreAllSelected = true;
            OnPropertyChanged(nameof(Columns));
            OnPropertyChanged(nameof(AreAllSelected));
        }

        // Commands
        /// <summary>
        /// Gets the toggle all columns command.
        /// </summary>
        public RelayCommand ToggleAllColumnsCommand { get; private set; }

        // Command implementations
        /// <summary>
        /// Toggles selection of all columns.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void ToggleAllColumns(object? parameter)
        {
            // Check if all columns are currently selected
            bool allSelected = _columns.All(c => c.IsSelected);
            
            // If all are selected, unselect all; otherwise, select all
            foreach (var column in _columns)
            {
                column.IsSelected = !allSelected;
            }
            
            // Update the AreAllSelected property
            _areAllSelected = !allSelected;
            OnPropertyChanged(nameof(AreAllSelected));
        }

        /// <summary>
        /// Event raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Represents a column item in the column selector.
    /// </summary>
    public class ColumnItem : INotifyPropertyChanged
    {
        private string _name = "";
        private bool _isSelected = true;
        private Action? _selectionChangedCallback;

        /// <summary>
        /// Gets or sets the name of the column.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value ?? "";
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the column is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged();
                // Notify the callback that selection has changed
                _selectionChangedCallback?.Invoke();
            }
        }

        /// <summary>
        /// Sets a callback for when selection changes.
        /// </summary>
        /// <param name="callback">The callback to invoke when selection changes.</param>
        public void SetSelectionChangedCallback(Action callback)
        {
            _selectionChangedCallback = callback;
        }

        /// <summary>
        /// Event raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}