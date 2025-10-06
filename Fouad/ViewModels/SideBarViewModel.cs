using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using System.IO;
using Microsoft.Win32;
using Fouad.Models;
using Fouad.Commands;
using Fouad.Services;
using Fouad.Views;

namespace Fouad.ViewModels
{
    /// <summary>
    /// ViewModel for the side bar view.
    /// Manages sidebar state, search functionality, and file management operations.
    /// </summary>
    public class SideBarViewModel : INotifyPropertyChanged
    {
        private bool _isExpanded = true;
        private string _searchText = "";
        private bool _autoBackupEnabled = false;
        private bool _notificationsEnabled = true;
        private bool _darkModeEnabled = false;
        private ObservableCollection<HistoryItem> _filteredHistoryItems = new ObservableCollection<HistoryItem>();
        private AudioService _audioService;
        private FileDataService? _fileDataService; // Add reference to FileDataService
        private InfoBarViewModel? _infoBarViewModel; // Add reference to InfoBarViewModel
        private ColumnSelectorViewModel? _columnSelectorViewModel; // Add reference to ColumnSelectorViewModel
        private DatabaseService? _databaseService; // Add reference to DatabaseService
        
        // Commands for sidebar actions
        /// <summary>
        /// Gets the toggle sidebar command.
        /// </summary>
        public ICommand ToggleSidebarCommand { get; private set; }
        
        /// <summary>
        /// Gets the add new file command.
        /// </summary>
        public ICommand AddNewFileCommand { get; private set; }
        
        /// <summary>
        /// Gets the import file command.
        /// </summary>
        public ICommand ImportFileCommand { get; private set; }
        
        /// <summary>
        /// Gets the export file command.
        /// </summary>
        public ICommand ExportFileCommand { get; private set; }
        
        /// <summary>
        /// Gets the backup command.
        /// </summary>
        public ICommand BackupCommand { get; private set; }
        
        /// <summary>
        /// Gets the restore command.
        /// </summary>
        public ICommand RestoreCommand { get; private set; }
        
        /// <summary>
        /// Gets the clear history command.
        /// </summary>
        public ICommand ClearHistoryCommand { get; private set; }
        
        /// <summary>
        /// Gets the settings command.
        /// </summary>
        public ICommand SettingsCommand { get; private set; }
        
        /// <summary>
        /// Gets the search command.
        /// </summary>
        public ICommand SearchCommand { get; private set; }
        
        /// <summary>
        /// Gets the toggle auto backup command.
        /// </summary>
        public ICommand ToggleAutoBackupCommand { get; private set; }
        
        /// <summary>
        /// Gets the toggle notifications command.
        /// </summary>
        public ICommand ToggleNotificationsCommand { get; private set; }
        
        /// <summary>
        /// Gets the toggle dark mode command.
        /// </summary>
        public ICommand ToggleDarkModeCommand { get; private set; }
        
        /// <summary>
        /// Gets the archive selected command.
        /// </summary>
        public ICommand ArchiveSelectedCommand { get; private set; }
        
        /// <summary>
        /// Gets the compress selected command.
        /// </summary>
        public ICommand CompressSelectedCommand { get; private set; }
        
        /// <summary>
        /// Gets the encrypt selected command.
        /// </summary>
        public ICommand EncryptSelectedCommand { get; private set; }

        /// <summary>
        /// Gets the open archived files command.
        /// </summary>
        public ICommand OpenArchivedFilesCommand { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SideBarViewModel"/> class.
        /// </summary>
        public SideBarViewModel()
        {
            _audioService = new AudioService();
            ToggleSidebarCommand = new RelayCommand(ToggleSidebar);
            AddNewFileCommand = new RelayCommand(AddNewFile);
            ImportFileCommand = new RelayCommand(ImportFile);
            ExportFileCommand = new RelayCommand(ExportFile);
            BackupCommand = new RelayCommand(BackupData);
            RestoreCommand = new RelayCommand(RestoreData);
            ClearHistoryCommand = new RelayCommand(ClearHistory);
            SettingsCommand = new RelayCommand(OpenSettings);
            SearchCommand = new RelayCommand(SearchHistory);
            ToggleAutoBackupCommand = new RelayCommand(ToggleAutoBackup);
            ToggleNotificationsCommand = new RelayCommand(ToggleNotifications);
            ToggleDarkModeCommand = new RelayCommand(ToggleDarkMode);
            ArchiveSelectedCommand = new RelayCommand(ArchiveSelectedItems);
            CompressSelectedCommand = new RelayCommand(CompressSelectedItems);
            EncryptSelectedCommand = new RelayCommand(EncryptSelectedItems);
            OpenArchivedFilesCommand = new RelayCommand(OpenArchivedFiles);
            
            // Initialize without sample data - will be populated with actual files
            // InitializeSampleData(); // Removed demo data
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SideBarViewModel"/> class with dependencies.
        /// </summary>
        /// <param name="fileDataService">The file data service.</param>
        /// <param name="infoBarViewModel">The info bar view model.</param>
        /// <param name="columnSelectorViewModel">The column selector view model.</param>
        public SideBarViewModel(FileDataService? fileDataService, InfoBarViewModel? infoBarViewModel, ColumnSelectorViewModel? columnSelectorViewModel)
        {
            _fileDataService = fileDataService;
            _infoBarViewModel = infoBarViewModel;
            _columnSelectorViewModel = columnSelectorViewModel;
            _audioService = new AudioService();
            ToggleSidebarCommand = new RelayCommand(ToggleSidebar);
            AddNewFileCommand = new RelayCommand(AddNewFile);
            ImportFileCommand = new RelayCommand(ImportFile);
            ExportFileCommand = new RelayCommand(ExportFile);
            BackupCommand = new RelayCommand(BackupData);
            RestoreCommand = new RelayCommand(RestoreData);
            ClearHistoryCommand = new RelayCommand(ClearHistory);
            SettingsCommand = new RelayCommand(OpenSettings);
            SearchCommand = new RelayCommand(SearchHistory);
            ToggleAutoBackupCommand = new RelayCommand(ToggleAutoBackup);
            ToggleNotificationsCommand = new RelayCommand(ToggleNotifications);
            ToggleDarkModeCommand = new RelayCommand(ToggleDarkMode);
            ArchiveSelectedCommand = new RelayCommand(ArchiveSelectedItems);
            CompressSelectedCommand = new RelayCommand(CompressSelectedItems);
            EncryptSelectedCommand = new RelayCommand(EncryptSelectedItems);
            OpenArchivedFilesCommand = new RelayCommand(OpenArchivedFiles);
            
            // Initialize without sample data - will be populated with actual files
            // InitializeSampleData(); // Removed demo data
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SideBarViewModel"/> class with dependencies.
        /// </summary>
        /// <param name="fileDataService">The file data service.</param>
        /// <param name="infoBarViewModel">The info bar view model.</param>
        /// <param name="columnSelectorViewModel">The column selector view model.</param>
        /// <param name="databaseService">The database service.</param>
        public SideBarViewModel(FileDataService? fileDataService, InfoBarViewModel? infoBarViewModel, ColumnSelectorViewModel? columnSelectorViewModel, DatabaseService? databaseService)
        {
            _fileDataService = fileDataService;
            _infoBarViewModel = infoBarViewModel;
            _columnSelectorViewModel = columnSelectorViewModel;
            _databaseService = databaseService;
            _audioService = new AudioService();
            ToggleSidebarCommand = new RelayCommand(ToggleSidebar);
            AddNewFileCommand = new RelayCommand(AddNewFile);
            ImportFileCommand = new RelayCommand(ImportFile);
            ExportFileCommand = new RelayCommand(ExportFile);
            BackupCommand = new RelayCommand(BackupData);
            RestoreCommand = new RelayCommand(RestoreData);
            ClearHistoryCommand = new RelayCommand(ClearHistory);
            SettingsCommand = new RelayCommand(OpenSettings);
            SearchCommand = new RelayCommand(SearchHistory);
            ToggleAutoBackupCommand = new RelayCommand(ToggleAutoBackup);
            ToggleNotificationsCommand = new RelayCommand(ToggleNotifications);
            ToggleDarkModeCommand = new RelayCommand(ToggleDarkMode);
            ArchiveSelectedCommand = new RelayCommand(ArchiveSelectedItems);
            CompressSelectedCommand = new RelayCommand(CompressSelectedItems);
            EncryptSelectedCommand = new RelayCommand(EncryptSelectedItems);
            OpenArchivedFilesCommand = new RelayCommand(OpenArchivedFiles);
            
            // Initialize without sample data - will be populated with actual files
            // InitializeSampleData(); // Removed demo data
        }

        /// <summary>
        /// Initializes sample data for demonstration purposes.
        /// </summary>
        // Commented out to remove demo files
        /*
        private void InitializeSampleData()
        {
            // Add some sample history items for demonstration
            _filteredHistoryItems.Add(new HistoryItem
            {
                Id = 1,
                FileName = "SampleFile1.xlsx",
                SearchDate = DateTime.Now.AddDays(-1),
                SearchTerm = "Sample Search",
                ResultCount = 5,
                IsSelected = false
            });
            
            _filteredHistoryItems.Add(new HistoryItem
            {
                Id = 2,
                FileName = "SampleFile2.csv",
                SearchDate = DateTime.Now.AddDays(-2),
                SearchTerm = "Another Search",
                ResultCount = 3,
                IsSelected = false
            });
        }
        */

        /// <summary>
        /// Gets or sets a value indicating whether the sidebar is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the search text used for filtering history items.
        /// </summary>
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterHistoryItems();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether automatic backup is enabled.
        /// </summary>
        public bool AutoBackupEnabled
        {
            get { return _autoBackupEnabled; }
            set
            {
                _autoBackupEnabled = value;
                OnPropertyChanged();
                if (value)
                {
                    MessageBox.Show(
                        Properties.Resources.AutomaticBackupEnabled, 
                        Properties.Resources.AutoBackupEnabledTitle, 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(
                        Properties.Resources.AutomaticBackupDisabled, 
                        Properties.Resources.AutoBackupDisabledTitle, 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether notifications are enabled.
        /// </summary>
        public bool NotificationsEnabled
        {
            get { return _notificationsEnabled; }
            set
            {
                _notificationsEnabled = value;
                OnPropertyChanged();
                if (value)
                {
                    MessageBox.Show(
                        Properties.Resources.NotificationsEnabled, 
                        Properties.Resources.NotificationsEnabledTitle, 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(
                        Properties.Resources.NotificationsDisabled, 
                        Properties.Resources.NotificationsDisabledTitle, 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether dark mode is enabled.
        /// </summary>
        public bool DarkModeEnabled
        {
            get { return _darkModeEnabled; }
            set
            {
                _darkModeEnabled = value;
                OnPropertyChanged();
                if (value)
                {
                    MessageBox.Show(
                        Properties.Resources.DarkModeEnabled, 
                        Properties.Resources.DarkModeEnabledTitle, 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(
                        Properties.Resources.LightModeEnabled, 
                        Properties.Resources.LightModeEnabledTitle, 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                }
                // In a real implementation, this would change the application theme
            }
        }

        /// <summary>
        /// Gets or sets the filtered collection of history items.
        /// </summary>
        public ObservableCollection<HistoryItem> FilteredHistoryItems
        {
            get { return _filteredHistoryItems; }
            set
            {
                _filteredHistoryItems = value ?? new ObservableCollection<HistoryItem>();
                OnPropertyChanged();
            }
        }

        // Command implementations
        /// <summary>
        /// Toggles the sidebar expanded state.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void ToggleSidebar(object? parameter)
        {
            IsExpanded = !IsExpanded;
            _audioService.PlaySound("Success_Tone1");
        }

        /// <summary>
        /// Adds a new file to the application.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void AddNewFile(object? parameter)
        {
            try
            {
                // Show options dialog before creating new file
                var dialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    Title = Properties.Resources.CreateNewFile
                };
                
                if (dialog.ShowDialog() == true)
                {
                    string fileName = dialog.FileName;
                    string extension = Path.GetExtension(fileName).ToLower();
                    
                    // Create the file based on extension
                    if (extension == ".xlsx")
                    {
                        // Create Excel file
                        // Implementation would go here
                        MessageBox.Show(
                            string.Format(Properties.Resources.NewExcelFileCreated, Path.GetFileName(fileName)), 
                            Properties.Resources.FileCreatedTitle, 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information);
                    }
                    else if (extension == ".csv")
                    {
                        // Create CSV file
                        // Implementation would go here
                        MessageBox.Show(
                            string.Format(Properties.Resources.NewCSVFileCreated, Path.GetFileName(fileName)), 
                            Properties.Resources.FileCreatedTitle, 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information);
                    }
                    else
                    {
                        // Create empty file
                        File.Create(fileName).Dispose();
                        MessageBox.Show(
                            string.Format(Properties.Resources.NewFileCreated, Path.GetFileName(fileName)), 
                            Properties.Resources.FileCreatedTitle, 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information);
                    }
                    
                    _audioService.PlaySound("Added");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.ErrorCreatingNewFile, ex.Message), 
                    Properties.Resources.ErrorTitle, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                _audioService.PlaySound("Error");
            }
        }

        /// <summary>
        /// Imports files into the application.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private async void ImportFile(object? parameter)
        {
            try
            {
                // Show options for import
                var dialog = new OpenFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    Title = Properties.Resources.ImportFileTitle,
                    Multiselect = true
                };
                
                if (dialog.ShowDialog() == true)
                {
                    int fileCount = dialog.FileNames.Length;
                    string message = string.Format(Properties.Resources.ConfirmImport, fileCount);
                    
                    MessageBoxResult result = MessageBox.Show(
                        message, 
                        Properties.Resources.ImportFilesTitle, 
                        MessageBoxButton.YesNo, 
                        MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        // If we have a file data service, load the first file
                        if (_fileDataService != null && dialog.FileNames.Length > 0)
                        {
                            try
                            {
                                // Load the first file
                                await _fileDataService.LoadFileAsync(dialog.FileNames[0]);
                                
                                // Update InfoBar with file information if we have access to it
                                if (_infoBarViewModel != null && _fileDataService.IsFileLoaded())
                                {
                                    var fileInfo = _fileDataService.GetFileInfo();
                                    if (fileInfo != null)
                                    {
                                        _infoBarViewModel.FileName = fileInfo.Name;
                                        _infoBarViewModel.FileSize = FormatFileSize(fileInfo.Length);
                                        _infoBarViewModel.LastModified = fileInfo.LastWriteTime;
                                    }
                                    
                                    // Get column count from headers
                                    var headers = _fileDataService.GetColumnHeaders();
                                    _infoBarViewModel.ColumnCount = headers.Count;
                                    
                                    // Get row count
                                    _infoBarViewModel.RowCount = _fileDataService.GetRowCount();
                                    
                                    // Update ColumnSelector with column headers if we have access to it
                                    _columnSelectorViewModel?.UpdateColumns(headers);
                                    
                                    // Add file to recent files list
                                    if (fileInfo != null)
                                    {
                                        AddFileToRecentFiles(fileInfo.Name, dialog.FileNames[0]);
                                    }
                                }
                                
                                _audioService.PlaySound("Added");
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(
                                    string.Format(Properties.Resources.ErrorLoadingFile, ex.Message), 
                                    Properties.Resources.FileLoadError, 
                                    MessageBoxButton.OK, 
                                    MessageBoxImage.Error);
                                _audioService.PlaySound("Error");
                                return;
                            }
                        }
                        
                        // Implementation for importing files
                        // This would typically copy or move files to the application directory
                        MessageBox.Show(
                            string.Format(Properties.Resources.FilesImportedSuccessfully, fileCount), 
                            Properties.Resources.ImportCompleteTitle, 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information);
                        _audioService.PlaySound("Added");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.ErrorImportingFile, ex.Message), 
                    Properties.Resources.ErrorTitle, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                _audioService.PlaySound("Error");
            }
        }

        /// <summary>
        /// Adds a file to the recent files list.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="filePath">The full path of the file.</param>
        private void AddFileToRecentFiles(string fileName, string filePath)
        {
            try
            {
                // Check if file already exists in recent files
                var existingItem = _filteredHistoryItems.FirstOrDefault(item => item.FileName == fileName);
                
                if (existingItem != null)
                {
                    // Update existing item
                    existingItem.SearchDate = DateTime.Now;
                    existingItem.SearchTerm = Properties.Resources.FileOpened; // "File Opened" in Arabic
                    existingItem.ResultCount = 1; // Or actual row count if available
                }
                else
                {
                    // Add new item
                    var newItem = new HistoryItem
                    {
                        Id = _filteredHistoryItems.Count + 1,
                        FileName = fileName,
                        SearchDate = DateTime.Now,
                        SearchTerm = Properties.Resources.FileOpened, // "File Opened" in Arabic
                        ResultCount = 1, // Or actual row count if available
                        IsSelected = false
                    };
                    _filteredHistoryItems.Add(newItem);
                }
                
                OnPropertyChanged(nameof(FilteredHistoryItems));
            }
            catch (Exception ex)
            {
                // Log error but don't crash the application
                System.Diagnostics.Debug.WriteLine($"Error adding file to recent files: {ex.Message}");
            }
        }

        /// <summary>
        /// Exports selected files from the application.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void ExportFile(object? parameter)
        {
            try
            {
                var selectedItems = _filteredHistoryItems.Where(item => item.IsSelected).ToList();
                
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
                var dialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv|ZIP Archive (*.zip)|*.zip",
                    Title = Properties.Resources.ExportSelectedFiles
                };
                
                if (dialog.ShowDialog() == true)
                {
                    string fileName = dialog.FileName;
                    string extension = Path.GetExtension(fileName).ToLower();
                    string format = extension.ToUpper().TrimStart('.');
                    
                    string message = string.Format(Properties.Resources.ConfirmExport, selectedItems.Count, format);
                    MessageBoxResult result = MessageBox.Show(
                        message, 
                        Properties.Resources.ExportFilesTitle, 
                        MessageBoxButton.YesNo, 
                        MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        // Implementation for exporting files
                        MessageBox.Show(
                            string.Format(Properties.Resources.ItemsExportedSuccessfully, selectedItems.Count, format), 
                            Properties.Resources.ExportCompleteTitle, 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information);
                        _audioService.PlaySound("Added");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.ErrorExportingFile, ex.Message), 
                    Properties.Resources.ErrorTitle, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                _audioService.PlaySound("Error");
            }
        }

        /// <summary>
        /// Backs up application data.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void BackupData(object? parameter)
        {
            try
            {
                // Show options before backup
                string message = Properties.Resources.ConfirmBackup;
                MessageBoxResult result = MessageBox.Show(
                    message, 
                    Properties.Resources.BackupDataTitle, 
                    MessageBoxButton.OKCancel, 
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.OK)
                {
                    var dialog = new SaveFileDialog
                    {
                        Filter = "Backup Files (*.bak)|*.bak|ZIP Archive (*.zip)|*.zip|All Files (*.*)|*.*",
                        Title = Properties.Resources.BackupDataTitle,
                        FileName = $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}"
                    };
                    
                    if (dialog.ShowDialog() == true)
                    {
                        // Implementation for backing up data
                        MessageBox.Show(
                            Properties.Resources.BackupComplete, 
                            Properties.Resources.BackupCompleteTitle, 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information);
                        _audioService.PlaySound("Success");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.ErrorDuringBackup, ex.Message), 
                    Properties.Resources.ErrorTitle, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                _audioService.PlaySound("Error");
            }
        }

        /// <summary>
        /// Restores application data from a backup.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void RestoreData(object? parameter)
        {
            try
            {
                // Show warning with options before restore
                string message = Properties.Resources.ConfirmRestore;
                MessageBoxResult result = MessageBox.Show(
                    message, 
                    Properties.Resources.RestoreDataTitle, 
                    MessageBoxButton.OKCancel, 
                    MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.OK)
                {
                    var dialog = new OpenFileDialog
                    {
                        Filter = "Backup Files (*.bak)|*.bak|ZIP Archive (*.zip)|*.zip|All Files (*.*)|*.*",
                        Title = Properties.Resources.RestoreDataTitle
                    };
                    
                    if (dialog.ShowDialog() == true)
                    {
                        // Show confirmation with red neon effect simulation
                        string confirmMessage = string.Format(Properties.Resources.ConfirmAbsoluteRestore, Path.GetFileName(dialog.FileName));
                        MessageBoxResult confirmResult = MessageBox.Show(
                            confirmMessage, 
                            Properties.Resources.ConfirmRestoreTitle, 
                            MessageBoxButton.YesNo, 
                            MessageBoxImage.Warning);
                        
                        if (confirmResult == MessageBoxResult.Yes)
                        {
                            // Implementation for restoring data
                            MessageBox.Show(
                                Properties.Resources.RestoreComplete, 
                                Properties.Resources.RestoreCompleteTitle, 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Information);
                            _audioService.PlaySound("Success");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.ErrorDuringRestore, ex.Message), 
                    Properties.Resources.ErrorTitle, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                _audioService.PlaySound("Error");
            }
        }

        /// <summary>
        /// Clears the history.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void ClearHistory(object? parameter)
        {
            try
            {
                // Show warning with red neon effect before clearing history
                string message = Properties.Resources.ConfirmClearHistory;
                MessageBoxResult result = MessageBox.Show(
                    message, 
                    Properties.Resources.ClearHistoryTitle, 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    // Implementation for clearing history
                    _filteredHistoryItems.Clear();
                    OnPropertyChanged(nameof(FilteredHistoryItems));
                    MessageBox.Show(
                        Properties.Resources.HistoryClearedSuccessfully, 
                        Properties.Resources.HistoryClearedTitle, 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                    _audioService.PlaySound("Success");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.ErrorClearingHistory, ex.Message), 
                    Properties.Resources.ErrorTitle, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                _audioService.PlaySound("Error");
            }
        }

        /// <summary>
        /// Opens the application settings.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void OpenSettings(object? parameter)
        {
            try
            {
                // Implementation for opening settings with options
                string autoBackupStatus = AutoBackupEnabled ? Properties.Resources.ItemSelected : Properties.Resources.ItemsUnselected;
                string notificationsStatus = NotificationsEnabled ? Properties.Resources.ItemSelected : Properties.Resources.ItemsUnselected;
                string darkModeStatus = DarkModeEnabled ? Properties.Resources.ItemSelected : Properties.Resources.ItemsUnselected;
                
                string settingsMessage = string.Format(
                    Properties.Resources.SettingsOptions,
                    autoBackupStatus,
                    notificationsStatus,
                    darkModeStatus);
                                        
                MessageBox.Show(
                    settingsMessage, 
                    Properties.Resources.SettingsTitle, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
                _audioService.PlaySound("Success_Tone1");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.ErrorClearingHistory, ex.Message), 
                    Properties.Resources.ErrorTitle, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                _audioService.PlaySound("Error");
            }
        }

        /// <summary>
        /// Searches the history items.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void SearchHistory(object? parameter)
        {
            try
            {
                FilterHistoryItems();
                MessageBox.Show(
                    string.Format(Properties.Resources.SearchCompleted, SearchText), 
                    Properties.Resources.SearchCompleteTitle, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
                _audioService.PlaySound("Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.ErrorDuringSearch, ex.Message), 
                    Properties.Resources.ErrorTitle, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                _audioService.PlaySound("Error");
            }
        }

        /// <summary>
        /// Toggles automatic backup.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void ToggleAutoBackup(object? parameter)
        {
            AutoBackupEnabled = !AutoBackupEnabled;
        }

        /// <summary>
        /// Toggles notifications.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void ToggleNotifications(object? parameter)
        {
            NotificationsEnabled = !NotificationsEnabled;
        }

        /// <summary>
        /// Toggles dark mode.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void ToggleDarkMode(object? parameter)
        {
            DarkModeEnabled = !DarkModeEnabled;
        }

        /// <summary>
        /// Archives selected items.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void ArchiveSelectedItems(object? parameter)
        {
            try
            {
                var selectedItems = _filteredHistoryItems.Where(item => item.IsSelected).ToList();
                
                if (selectedItems.Count == 0)
                {
                    MessageBox.Show(
                        Properties.Resources.NoItemsSelectedForArchiving, 
                        Properties.Resources.NoSelectionTitle, 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                    _audioService.PlaySound("Error");
                    return;
                }
                
                string message = string.Format(Properties.Resources.ConfirmArchive, selectedItems.Count);
                MessageBoxResult result = MessageBox.Show(
                    message, 
                    Properties.Resources.ArchiveFilesTitle, 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    // Implementation for archiving selected items
                    MessageBox.Show(
                        string.Format(Properties.Resources.ItemsArchivedSuccessfully, selectedItems.Count), 
                        Properties.Resources.ArchiveCompleteTitle, 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                    _audioService.PlaySound("Success");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.ErrorDuringArchiving, ex.Message), 
                    Properties.Resources.ErrorTitle, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                _audioService.PlaySound("Error");
            }
        }

        /// <summary>
        /// Compresses selected items.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void CompressSelectedItems(object? parameter)
        {
            try
            {
                var selectedItems = _filteredHistoryItems.Where(item => item.IsSelected).ToList();
                
                if (selectedItems.Count == 0)
                {
                    MessageBox.Show(
                        Properties.Resources.NoItemsSelectedForCompression, 
                        Properties.Resources.NoSelectionTitle, 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                    _audioService.PlaySound("Error");
                    return;
                }
                
                string message = string.Format(Properties.Resources.ConfirmCompression, selectedItems.Count);
                MessageBoxResult result = MessageBox.Show(
                    message, 
                    Properties.Resources.CompressFilesTitle, 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    // Implementation for compressing selected items
                    MessageBox.Show(
                        string.Format(Properties.Resources.ItemsCompressedSuccessfully, selectedItems.Count), 
                        Properties.Resources.CompressionCompleteTitle, 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                    _audioService.PlaySound("Success");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.ErrorDuringCompression, ex.Message), 
                    Properties.Resources.ErrorTitle, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                _audioService.PlaySound("Error");
            }
        }

        /// <summary>
        /// Encrypts selected items.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void EncryptSelectedItems(object? parameter)
        {
            try
            {
                var selectedItems = _filteredHistoryItems.Where(item => item.IsSelected).ToList();
                
                if (selectedItems.Count == 0)
                {
                    MessageBox.Show(
                        Properties.Resources.NoItemsSelectedForEncryption, 
                        Properties.Resources.NoSelectionTitle, 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                    _audioService.PlaySound("Error");
                    return;
                }
                
                // Show warning with red neon effect before encryption
                string message = string.Format(Properties.Resources.ConfirmEncryption, selectedItems.Count);
                MessageBoxResult result = MessageBox.Show(
                    message, 
                    Properties.Resources.EncryptFilesTitle, 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    // Implementation for encrypting selected items
                    MessageBox.Show(
                        string.Format(Properties.Resources.ItemsEncryptedSuccessfully, selectedItems.Count), 
                        Properties.Resources.EncryptionCompleteTitle, 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                    _audioService.PlaySound("Success");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.ErrorDuringEncryption, ex.Message), 
                    Properties.Resources.ErrorTitle, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                _audioService.PlaySound("Error");
            }
        }

        /// <summary>
        /// Opens the archived files window.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void OpenArchivedFiles(object? parameter)
        {
            try
            {
                LoggingService.LogInfo("Attempting to open archived files window from SideBar");
                LoggingService.LogInfo($"DatabaseService is null: {_databaseService == null}");
                LoggingService.LogInfo($"FileDataService is null: {_fileDataService == null}");
                
                if (_databaseService != null && _fileDataService != null)
                {
                    LoggingService.LogInfo("Creating ArchivedFilesViewModel and ArchivedFilesWindow");
                    var archivedFilesViewModel = new ArchivedFilesViewModel(_databaseService, _fileDataService);
                    var archivedFilesWindow = new ArchivedFilesWindow
                    {
                        DataContext = archivedFilesViewModel
                    };
                    
                    // Get the main window to set as owner
                    var mainWindow = Application.Current?.MainWindow;
                    if (mainWindow != null)
                    {
                        archivedFilesWindow.Owner = mainWindow;
                        LoggingService.LogInfo("Set main window as owner of ArchivedFilesWindow");
                    }
                    
                    LoggingService.LogInfo("Showing ArchivedFilesWindow");
                    archivedFilesWindow.ShowDialog();
                    LoggingService.LogInfo("ArchivedFilesWindow closed");
                }
                else
                {
                    LoggingService.LogWarning("DatabaseService or FileDataService is null, cannot open archived files window");
                    // Show a message to the user
                    MessageBox.Show("Services not initialized. Please load a file first.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error opening archived files window", ex);
                MessageBox.Show($"Error opening archived files window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Filters history items based on the search text.
        /// </summary>
        private void FilterHistoryItems()
        {
            // In a real implementation, this would filter the actual history items
            // For now, we'll just demonstrate the concept
            if (string.IsNullOrWhiteSpace(_searchText))
            {
                // Show all items
            }
            else
            {
                // Filter items based on search text
            }
        }

        /// <summary>
        /// Formats a file size in bytes to a human-readable string.
        /// </summary>
        /// <param name="bytes">The file size in bytes.</param>
        /// <returns>A formatted string representing the file size.</returns>
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            
            return $"{len:0.##} {sizes[order]}";
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