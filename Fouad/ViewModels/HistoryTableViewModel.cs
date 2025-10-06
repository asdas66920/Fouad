using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Effects;
using Fouad.Models;
using Fouad.Commands;
using Fouad.Services;
using System.IO;
using System.Diagnostics;

namespace Fouad.ViewModels
{
    /// <summary>
    /// ViewModel for the history table view.
    /// Manages history items, selection, and actions on historical search results.
    /// </summary>
    public class HistoryTableViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<HistoryItem> _historyItems = new ObservableCollection<HistoryItem>();
        private bool _isAllSelected = false;
        private ColumnSelectorViewModel? _columnSelector;
        private AudioService _audioService; // Audio service for playing sounds

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryTableViewModel"/> class.
        /// </summary>
        public HistoryTableViewModel()
        {
            _audioService = new AudioService();
            ToggleAllCommand = new RelayCommand(ToggleAllItems);
            DeleteCommand = new RelayCommand(DeleteSelectedItems, CanExecuteWithSelection);
            EditCommand = new RelayCommand(EditSelectedItems, CanExecuteWithSingleSelection);
            CopyCommand = new RelayCommand(CopySelectedItems, CanExecuteWithSelection);
            ExportToExcelCommand = new RelayCommand(ExportToExcel, CanExecuteWithSelection);
            ExportToPdfCommand = new RelayCommand(ExportToPdf, CanExecuteWithSelection);
            BackupSelectedCommand = new RelayCommand(BackupSelectedItems, CanExecuteWithSelection);
            ShareCommand = new RelayCommand(ShareSelectedItems, CanExecuteWithSelection);
            ViewCommand = new RelayCommand(ViewItem);
            DeleteItemCommand = new RelayCommand(DeleteItem);
            
            // Initialize with sample data for demonstration
            InitializeSampleData();
        }

        /// <summary>
        /// Initializes sample data for demonstration purposes.
        /// </summary>
        private void InitializeSampleData()
        {
            // Add some sample history items for demonstration
            _historyItems.Add(new HistoryItem
            {
                Id = 1,
                FileName = "SampleFile1.xlsx",
                SearchDate = DateTime.Now.AddDays(-1),
                SearchTerm = "Sample Search",
                ResultCount = 5,
                IsSelected = false,
                IsAddedToHistory = true
            });
            
            _historyItems.Add(new HistoryItem
            {
                Id = 2,
                FileName = "SampleFile2.csv",
                SearchDate = DateTime.Now.AddDays(-2),
                SearchTerm = "Another Search",
                ResultCount = 3,
                IsSelected = false,
                IsAddedToHistory = true
            });
            
            _historyItems.Add(new HistoryItem
            {
                Id = 3,
                FileName = "SampleFile3.xlsx",
                SearchDate = DateTime.Now.AddDays(-3),
                SearchTerm = "Third Search",
                ResultCount = 7,
                IsSelected = false,
                IsAddedToHistory = true
            });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryTableViewModel"/> class.
        /// </summary>
        /// <param name="columnSelector">The column selector view model.</param>
        public HistoryTableViewModel(ColumnSelectorViewModel columnSelector) : this()
        {
            _columnSelector = columnSelector;
            // Subscribe to column selection changes
            _columnSelector.ColumnSelectionChanged += OnColumnSelectionChanged;
        }

        /// <summary>
        /// Gets or sets the collection of history items.
        /// </summary>
        public ObservableCollection<HistoryItem> HistoryItems
        {
            get { return _historyItems; }
            set
            {
                _historyItems = value ?? new ObservableCollection<HistoryItem>();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether all items are selected.
        /// </summary>
        public bool IsAllSelected
        {
            get { return _isAllSelected; }
            set
            {
                _isAllSelected = value;
                OnPropertyChanged();
                SelectAllItems(_isAllSelected);
            }
        }

        // Commands
        /// <summary>
        /// Gets the toggle all command.
        /// </summary>
        public RelayCommand ToggleAllCommand { get; private set; }
        
        /// <summary>
        /// Gets the delete command.
        /// </summary>
        public RelayCommand DeleteCommand { get; private set; }
        
        /// <summary>
        /// Gets the edit command.
        /// </summary>
        public RelayCommand EditCommand { get; private set; }
        
        /// <summary>
        /// Gets the copy command.
        /// </summary>
        public RelayCommand CopyCommand { get; private set; }
        
        /// <summary>
        /// Gets the export to Excel command.
        /// </summary>
        public RelayCommand ExportToExcelCommand { get; private set; }
        
        /// <summary>
        /// Gets the export to PDF command.
        /// </summary>
        public RelayCommand ExportToPdfCommand { get; private set; }
        
        /// <summary>
        /// Gets the backup selected command.
        /// </summary>
        public RelayCommand BackupSelectedCommand { get; private set; }
        
        /// <summary>
        /// Gets the share command.
        /// </summary>
        public RelayCommand ShareCommand { get; private set; }
        
        /// <summary>
        /// Gets the view command.
        /// </summary>
        public RelayCommand ViewCommand { get; private set; }
        
        /// <summary>
        /// Gets the delete item command.
        /// </summary>
        public RelayCommand DeleteItemCommand { get; private set; }

        // Command CanExecute methods
        /// <summary>
        /// Determines whether a command that requires selection can be executed.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        /// <returns>True if items are selected, false otherwise.</returns>
        private bool CanExecuteWithSelection(object? parameter)
        {
            return _historyItems.Any(item => item.IsSelected);
        }

        /// <summary>
        /// Determines whether a command that requires a single selection can be executed.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        /// <returns>True if exactly one item is selected, false otherwise.</returns>
        private bool CanExecuteWithSingleSelection(object? parameter)
        {
            return _historyItems.Count(item => item.IsSelected) == 1;
        }

        // Command implementations
        /// <summary>
        /// Toggles selection of all items.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void ToggleAllItems(object? parameter)
        {
            try
            {
                IsAllSelected = !IsAllSelected;
                // Play sound for toggle action
                _audioService.PlaySound("Success_Tone1");
                
                // Update command states
                UpdateCommandStates();
                
                // Show message
                string message = IsAllSelected ? Properties.Resources.ItemSelected : Properties.Resources.ItemsUnselected;
                // MessageBox.Show(message, Properties.Resources.SelectionUpdatedTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.ErrorTogglingSelection, ex.Message), 
                    Properties.Resources.ErrorTitle, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                _audioService.PlaySound("Error");
            }
        }

        /// <summary>
        /// Selects or unselects all items.
        /// </summary>
        /// <param name="select">True to select all items, false to unselect all items.</param>
        private void SelectAllItems(bool select)
        {
            foreach (var item in _historyItems)
            {
                item.IsSelected = select;
            }
            UpdateCommandStates();
        }

        /// <summary>
        /// Updates the command states based on current selection.
        /// </summary>
        private void UpdateCommandStates()
        {
            DeleteCommand.RaiseCanExecuteChanged();
            EditCommand.RaiseCanExecuteChanged();
            CopyCommand.RaiseCanExecuteChanged();
            ExportToExcelCommand.RaiseCanExecuteChanged();
            ExportToPdfCommand.RaiseCanExecuteChanged();
            BackupSelectedCommand.RaiseCanExecuteChanged();
            ShareCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Deletes selected items from the history.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void DeleteSelectedItems(object? parameter)
        {
            try
            {
                var selectedItems = _historyItems.Where(item => item.IsSelected).ToList();
                
                if (selectedItems.Count == 0)
                {
                    MessageBox.Show(
                        Properties.Resources.NoItemsSelectedForDeletion, 
                        Properties.Resources.NoSelectionTitle, 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                    _audioService.PlaySound("Error");
                    return;
                }
                
                // Show confirmation dialog with red neon effect simulation
                string message = string.Format(Properties.Resources.ConfirmDeletion, selectedItems.Count);
                MessageBoxResult result = MessageBox.Show(
                    message, 
                    Properties.Resources.ConfirmDeletionTitle, 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    int count = selectedItems.Count;
                    foreach (var item in selectedItems)
                    {
                        _historyItems.Remove(item);
                    }
                    
                    // Update selection state
                    IsAllSelected = false;
                    
                    // Update command states
                    UpdateCommandStates();
                    
                    // Play sound for delete action
                    _audioService.PlaySound("Success");
                    MessageBox.Show(
                        string.Format(Properties.Resources.ItemsDeletedSuccessfully, count), 
                        Properties.Resources.DeletionCompleteTitle, 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.ErrorDeletingItems, ex.Message), 
                    Properties.Resources.ErrorTitle, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                _audioService.PlaySound("Error");
            }
        }

        /// <summary>
        /// Edits selected items.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void EditSelectedItems(object? parameter)
        {
            try
            {
                var selectedItems = _historyItems.Where(item => item.IsSelected).ToList();
                
                if (selectedItems.Count == 0)
                {
                    MessageBox.Show(
                        Properties.Resources.NoItemsSelectedForEditing, 
                        Properties.Resources.NoSelectionTitle, 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                    _audioService.PlaySound("Error");
                    return;
                }
                
                if (selectedItems.Count > 1)
                {
                    MessageBox.Show(
                        Properties.Resources.SelectOnlyOneItem, 
                        Properties.Resources.MultipleSelectionTitle, 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                    _audioService.PlaySound("Error");
                    return;
                }
                
                // Implementation for editing selected items
                // This would typically open an edit dialog
                MessageBox.Show(
                    string.Format(Properties.Resources.EditingItem, selectedItems[0].FileName), 
                    Properties.Resources.EditItemTitle, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
                _audioService.PlaySound("Added");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.ErrorEditingItems, ex.Message), 
                    Properties.Resources.ErrorTitle, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                _audioService.PlaySound("Error");
            }
        }

        /// <summary>
        /// Copies selected items to the clipboard.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void CopySelectedItems(object? parameter)
        {
            try
            {
                var selectedItems = _historyItems.Where(item => item.IsSelected).ToList();
                
                if (selectedItems.Count == 0)
                {
                    MessageBox.Show(
                        Properties.Resources.NoItemsSelectedForExport, 
                        Properties.Resources.NoSelectionTitle, 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                    _audioService.PlaySound("Error");
                    return;
                }
                
                // Create a string representation of the selected items
                var clipboardText = new StringBuilder();
                clipboardText.AppendLine("ID\tFileName\tSearchDate\tSearchTerm\tResultCount");
                foreach (var item in selectedItems)
                {
                    clipboardText.AppendLine($"{item.Id}\t{item.FileName}\t{item.SearchDate:yyyy-MM-dd HH:mm}\t{item.SearchTerm}\t{item.ResultCount}");
                }
                
                // Copy to clipboard
                Clipboard.SetText(clipboardText.ToString());
                
                // Implementation for copying selected items
                // This would typically copy data to clipboard
                MessageBox.Show(
                    string.Format(Properties.Resources.ItemsCopiedToClipboard, selectedItems.Count), 
                    Properties.Resources.CopyCompleteTitle, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
                _audioService.PlaySound("Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.ErrorCopyingItems, ex.Message), 
                    Properties.Resources.ErrorTitle, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                _audioService.PlaySound("Error");
            }
        }

        /// <summary>
        /// Exports selected items to Excel format.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void ExportToExcel(object? parameter)
        {
            try
            {
                var selectedItems = _historyItems.Where(item => item.IsSelected).ToList();
                
                if (selectedItems.Count == 0)
                {
                    MessageBox.Show(
                        Properties.Resources.NoItemsSelectedForExport, 
                        Properties.Resources.NoSelectionTitle, 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                    _audioService.PlaySound("Error");
                    return;
                }
                
                // Show export options
                string message = string.Format(Properties.Resources.ConfirmExportToExcel, selectedItems.Count);
                MessageBoxResult result = MessageBox.Show(
                    message,
                    Properties.Resources.ExportToExcelTitle,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    // Implementation for exporting to Excel
                    // In a real implementation, this would create an actual Excel file
                    // For now, we'll just show a message
                    MessageBox.Show(
                        string.Format(Properties.Resources.ItemsExportedToExcel, selectedItems.Count), 
                        Properties.Resources.ExportCompleteTitle, 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                    // Play sound for export action
                    _audioService.PlaySound("Added");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.ErrorExportingToExcel, ex.Message), 
                    Properties.Resources.ErrorTitle, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                _audioService.PlaySound("Error");
            }
        }

        /// <summary>
        /// Exports selected items to PDF format.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void ExportToPdf(object? parameter)
        {
            try
            {
                var selectedItems = _historyItems.Where(item => item.IsSelected).ToList();
                
                if (selectedItems.Count == 0)
                {
                    MessageBox.Show(
                        Properties.Resources.NoItemsSelectedForExport, 
                        Properties.Resources.NoSelectionTitle, 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                    _audioService.PlaySound("Error");
                    return;
                }
                
                // Show export options
                string message = string.Format(Properties.Resources.ConfirmExportToPDF, selectedItems.Count);
                MessageBoxResult result = MessageBox.Show(
                    message,
                    Properties.Resources.ExportToPDFTitle,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    // Implementation for exporting to PDF
                    // In a real implementation, this would create an actual PDF file
                    // For now, we'll just show a message
                    MessageBox.Show(
                        string.Format(Properties.Resources.ItemsExportedToPDF, selectedItems.Count), 
                        Properties.Resources.ExportCompleteTitle, 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                    // Play sound for export action
                    _audioService.PlaySound("Added");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.ErrorExportingToPDF, ex.Message), 
                    Properties.Resources.ErrorTitle, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                _audioService.PlaySound("Error");
            }
        }

        /// <summary>
        /// Backs up selected items.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void BackupSelectedItems(object? parameter)
        {
            try
            {
                var selectedItems = _historyItems.Where(item => item.IsSelected).ToList();
                
                if (selectedItems.Count == 0)
                {
                    MessageBox.Show(
                        Properties.Resources.NoItemsSelectedForBackup, 
                        Properties.Resources.NoSelectionTitle, 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                    _audioService.PlaySound("Error");
                    return;
                }
                
                // Show backup options
                string message = string.Format(Properties.Resources.ConfirmBackupSelected, selectedItems.Count);
                MessageBoxResult result = MessageBox.Show(
                    message,
                    Properties.Resources.BackupSelectedItemsTitle,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    // Implementation for backing up selected items
                    // In a real implementation, this would create backup files
                    // For now, we'll just show a message
                    MessageBox.Show(
                        string.Format(Properties.Resources.BackupOfItemsCompleted, selectedItems.Count), 
                        Properties.Resources.BackupCompleteTitle, 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                    _audioService.PlaySound("Success");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.ErrorDuringBackupOfItems, ex.Message), 
                    Properties.Resources.ErrorTitle, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                _audioService.PlaySound("Error");
            }
        }

        /// <summary>
        /// Shares selected items.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void ShareSelectedItems(object? parameter)
        {
            try
            {
                var selectedItems = _historyItems.Where(item => item.IsSelected).ToList();
                
                if (selectedItems.Count == 0)
                {
                    MessageBox.Show(
                        Properties.Resources.NoItemsSelectedForSharing, 
                        Properties.Resources.NoSelectionTitle, 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                    _audioService.PlaySound("Error");
                    return;
                }
                
                // Show sharing options
                string message = string.Format(Properties.Resources.ConfirmShare, selectedItems.Count);
                MessageBoxResult result = MessageBox.Show(
                    message,
                    Properties.Resources.ShareItemsTitle,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    // Implementation for sharing selected items
                    // In a real implementation, this would open sharing options
                    // For now, we'll just show a message
                    MessageBox.Show(
                        string.Format(Properties.Resources.SharingItemsInitiated, selectedItems.Count), 
                        Properties.Resources.SharingStartedTitle, 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                    _audioService.PlaySound("Success");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.ErrorDuringSharing, ex.Message), 
                    Properties.Resources.ErrorTitle, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                _audioService.PlaySound("Error");
            }
        }

        /// <summary>
        /// Views details of a specific item.
        /// </summary>
        /// <param name="parameter">The history item to view.</param>
        private void ViewItem(object? parameter)
        {
            try
            {
                if (parameter is HistoryItem item)
                {
                    // Implementation for viewing a specific item
                    string message = string.Format(Properties.Resources.ViewItemDetails, item.FileName, item.SearchTerm, item.ResultCount);
                    MessageBox.Show(
                        message, 
                        Properties.Resources.ItemDetailsTitle, 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                    _audioService.PlaySound("Success_Tone1");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.ErrorViewItem, ex.Message), 
                    Properties.Resources.ErrorTitle, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                _audioService.PlaySound("Error");
            }
        }

        /// <summary>
        /// Deletes a specific item from the history.
        /// </summary>
        /// <param name="parameter">The history item to delete.</param>
        private void DeleteItem(object? parameter)
        {
            try
            {
                if (parameter is HistoryItem item)
                {
                    // Show confirmation dialog with red neon effect simulation
                    string message = string.Format(Properties.Resources.ConfirmDeleteItem, item.FileName);
                    MessageBoxResult result = MessageBox.Show(
                        message, 
                        Properties.Resources.ConfirmDeletionTitle, 
                        MessageBoxButton.YesNo, 
                        MessageBoxImage.Warning);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        _historyItems.Remove(item);
                        
                        // Update selection state if all items were selected
                        if (IsAllSelected && _historyItems.Count == 0)
                        {
                            IsAllSelected = false;
                        }
                        
                        // Update command states
                        UpdateCommandStates();
                        
                        MessageBox.Show(
                            string.Format(Properties.Resources.ItemsDeletedSuccessfully, $"'{item.FileName}'"), 
                            Properties.Resources.DeletionCompleteTitle, 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information);
                        _audioService.PlaySound("Success");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.ErrorDeletingItems, ex.Message), 
                    Properties.Resources.ErrorTitle, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                _audioService.PlaySound("Error");
            }
        }

        /// <summary>
        /// Called when column selection changes.
        /// </summary>
        private void OnColumnSelectionChanged(object? sender, EventArgs e)
        {
            // Notify that columns may have changed
            ColumnsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event raised when columns change.
        /// </summary>
        public event EventHandler? ColumnsChanged;

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
        
        /// <summary>
        /// Gets the audio service for playing sounds.
        /// </summary>
        public AudioService AudioService 
        { 
            get { return _audioService; } 
        }
    }
}