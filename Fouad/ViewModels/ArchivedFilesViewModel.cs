using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Fouad.Models;
using Fouad.Commands;
using Fouad.Services;
using System.IO;

namespace Fouad.ViewModels
{
    /// <summary>
    /// ViewModel for the archived files window.
    /// Manages archived files and provides options for working with them.
    /// </summary>
    public class ArchivedFilesViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly IDatabaseService _databaseService;
        private readonly FileDataService _fileDataService;
        private bool _isLoading;
        private string _searchTerm = string.Empty;
        private ArchivedFile? _selectedFile;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private string? _selectedUploadedBy;
        private bool _showLargeFilesOnly;
        private ObservableCollection<string> _uploadedByList;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchivedFilesViewModel"/> class.
        /// </summary>
        /// <param name="databaseService">The database service to use.</param>
        /// <param name="fileDataService">The file data service to use.</param>
        public ArchivedFilesViewModel(IDatabaseService databaseService, FileDataService fileDataService)
        {
            LoggingService.LogInfo("Creating ArchivedFilesViewModel");
            _databaseService = databaseService;
            _fileDataService = fileDataService;
            
            ArchivedFiles = new ObservableCollection<ArchivedFile>();
            SelectedFiles = new ObservableCollection<ArchivedFile>();
            _uploadedByList = new ObservableCollection<string>();
            
            LoadArchivedFilesCommand = new RelayCommand(async o => await LoadArchivedFilesAsync());
            WorkOnSelectedFileCommand = new RelayCommand(o => WorkOnSelectedFile(), o => SelectedFiles.Count == 1);
            WorkOnMultipleFilesCommand = new RelayCommand(o => WorkOnMultipleFiles(), o => SelectedFiles.Count > 1);
            CompareSelectedFilesCommand = new RelayCommand(o => CompareSelectedFiles(), o => SelectedFiles.Count >= 2);
            ViewLatestInfoCommand = new RelayCommand(o => ViewLatestInfo(), o => SelectedFiles.Count == 1);
            SearchCommand = new RelayCommand(async o => await LoadArchivedFilesAsync());
            ApplyFiltersCommand = new RelayCommand(async o => await LoadArchivedFilesAsync());
            ClearFiltersCommand = new RelayCommand(o => ClearFilters());
            
            // Load archived files when initialized
            LoggingService.LogInfo("Loading archived files");
            _ = LoadArchivedFilesAsync();
            LoggingService.LogInfo("ArchivedFilesViewModel created successfully");
        }

        /// <summary>
        /// Gets the collection of archived files.
        /// </summary>
        public ObservableCollection<ArchivedFile> ArchivedFiles { get; }

        /// <summary>
        /// Gets or sets the collection of selected files.
        /// </summary>
        public ObservableCollection<ArchivedFile> SelectedFiles { get; }

        /// <summary>
        /// Gets or sets the selected file.
        /// </summary>
        public ArchivedFile? SelectedFile
        {
            get => _selectedFile;
            set
            {
                _selectedFile = value;
                OnPropertyChanged();
                
                // Update the SelectedFiles collection when a single file is selected
                SelectedFiles.Clear();
                if (_selectedFile != null)
                {
                    SelectedFiles.Add(_selectedFile);
                }
                
                // Raise CanExecuteChanged for commands that depend on selection
                ((RelayCommand)WorkOnSelectedFileCommand).RaiseCanExecuteChanged();
                ((RelayCommand)WorkOnMultipleFilesCommand).RaiseCanExecuteChanged();
                ((RelayCommand)CompareSelectedFilesCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ViewLatestInfoCommand).RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the start date for filtering.
        /// </summary>
        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the end date for filtering.
        /// </summary>
        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the selected uploaded by filter.
        /// </summary>
        public string? SelectedUploadedBy
        {
            get => _selectedUploadedBy;
            set
            {
                _selectedUploadedBy = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show only large files.
        /// </summary>
        public bool ShowLargeFilesOnly
        {
            get => _showLargeFilesOnly;
            set
            {
                _showLargeFilesOnly = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the list of uploaded by values for the filter.
        /// </summary>
        public ObservableCollection<string> UploadedByList
        {
            get => _uploadedByList;
            set
            {
                _uploadedByList = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the view model is currently loading.
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the search term for filtering archived files.
        /// </summary>
        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the load archived files command.
        /// </summary>
        public ICommand LoadArchivedFilesCommand { get; }

        /// <summary>
        /// Gets the work on selected file command.
        /// </summary>
        public ICommand WorkOnSelectedFileCommand { get; }

        /// <summary>
        /// Gets the work on multiple files command.
        /// </summary>
        public ICommand WorkOnMultipleFilesCommand { get; }

        /// <summary>
        /// Gets the compare selected files command.
        /// </summary>
        public ICommand CompareSelectedFilesCommand { get; }

        /// <summary>
        /// Gets the view latest info command.
        /// </summary>
        public ICommand ViewLatestInfoCommand { get; }

        /// <summary>
        /// Gets the search command.
        /// </summary>
        public ICommand SearchCommand { get; }

        /// <summary>
        /// Gets the apply filters command.
        /// </summary>
        public ICommand ApplyFiltersCommand { get; }

        /// <summary>
        /// Gets the clear filters command.
        /// </summary>
        public ICommand ClearFiltersCommand { get; }

        /// <summary>
        /// Clears all filters.
        /// </summary>
        private void ClearFilters()
        {
            SearchTerm = string.Empty;
            StartDate = null;
            EndDate = null;
            SelectedUploadedBy = null;
            ShowLargeFilesOnly = false;
            _ = LoadArchivedFilesAsync();
        }

        /// <summary>
        /// Loads archived files from the database.
        /// </summary>
        /// <returns>Task representing the asynchronous operation.</returns>
        private async Task LoadArchivedFilesAsync()
        {
            try
            {
                LoggingService.LogInfo("Starting LoadArchivedFilesAsync");
                IsLoading = true;
                ArchivedFiles.Clear();

                // Get archived files from the database
                LoggingService.LogInfo("Getting all archive logs from database");
                var archiveLogs = await _databaseService.GetAllArchiveLogsAsync();
                LoggingService.LogInfo($"Retrieved {archiveLogs.Count} archive logs from database");
                
                // Get unique uploaded by values
                var uploadedByValues = archiveLogs.Select(log => log.UploadedBy).Where(name => !string.IsNullOrEmpty(name)).Distinct().ToList();
                UploadedByList.Clear();
                UploadedByList.Add(""); // Empty option for no filter
                foreach (var name in uploadedByValues)
                {
                    if (!string.IsNullOrEmpty(name))
                        UploadedByList.Add(name);
                }

                foreach (var log in archiveLogs)
                {
                    LoggingService.LogInfo($"Processing archive log: {log.ArchiveId}, {log.FileName}");
                    // Create an ArchivedFile from the ArchiveLog
                    var archivedFile = new ArchivedFile
                    {
                        ArchiveId = log.ArchiveId,
                        FileName = log.FileName,
                        UploadDate = log.UploadDate,
                        UploadedBy = log.UploadedBy,
                        FilePath = log.FilePath
                    };
                    
                    // Try to get file information if the file still exists
                    if (!string.IsNullOrEmpty(log.FilePath) && File.Exists(log.FilePath))
                    {
                        try
                        {
                            var fileInfo = new FileInfo(log.FilePath);
                            archivedFile.FileSize = fileInfo.Length;
                            
                            // Try to get row/column count from the file data service
                            try
                            {
                                await _fileDataService.LoadFileAsync(log.FilePath);
                                archivedFile.RowCount = _fileDataService.GetRowCount();
                                archivedFile.ColumnCount = _fileDataService.GetColumnHeaders().Count;
                            }
                            catch (Exception ex)
                            {
                                LoggingService.LogError($"Error getting file data for {log.FilePath}", ex);
                                // Set default values if we can't read the file data
                                archivedFile.RowCount = 0;
                                archivedFile.ColumnCount = 0;
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggingService.LogError($"Error getting file info for {log.FilePath}", ex);
                        }
                    }
                    else
                    {
                        // Set default values if file doesn't exist
                        archivedFile.FileSize = 0;
                        archivedFile.RowCount = 0;
                        archivedFile.ColumnCount = 0;
                    }
                    
                    // Apply all filters
                    if (ShouldIncludeFile(archivedFile))
                    {
                        ArchivedFiles.Add(archivedFile);
                    }
                }
                LoggingService.LogInfo($"Added {ArchivedFiles.Count} files to ArchivedFiles collection");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error loading archived files", ex);
                // Show error message to user
                System.Windows.MessageBox.Show($"Error loading archived files: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
                LoggingService.LogInfo("Finished LoadArchivedFilesAsync");
            }
        }

        /// <summary>
        /// Determines whether a file should be included based on current filters.
        /// </summary>
        /// <param name="file">The file to check.</param>
        /// <returns>True if the file should be included, false otherwise.</returns>
        private bool ShouldIncludeFile(ArchivedFile file)
        {
            // Apply search filter - enhanced with linguistic matching
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                // Check if the search term matches any part of the file information
                bool matches = false;
                
                // Check file name
                if (!string.IsNullOrEmpty(file.FileName) && 
                    file.FileName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    matches = true;
                }
                
                // Check uploaded by
                if (!string.IsNullOrEmpty(file.UploadedBy) && 
                    file.UploadedBy.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    matches = true;
                }
                
                // Check file size (convert to string for matching)
                if (file.FileSize > 0 && 
                    file.FileSize.ToString().Contains(SearchTerm))
                {
                    matches = true;
                }
                
                // If no matches found, exclude the file
                if (!matches)
                {
                    return false;
                }
            }

            // Apply date range filter
            if (StartDate.HasValue && file.UploadDate < StartDate.Value)
            {
                return false;
            }

            if (EndDate.HasValue && file.UploadDate > EndDate.Value)
            {
                return false;
            }

            // Apply uploaded by filter
            if (!string.IsNullOrEmpty(SelectedUploadedBy) && 
                !string.IsNullOrEmpty(file.UploadedBy) && 
                file.UploadedBy != SelectedUploadedBy)
            {
                return false;
            }

            // Apply large files only filter
            if (ShowLargeFilesOnly && file.FileSize < 1024 * 1024) // 1MB
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Works on the selected file.
        /// </summary>
        private void WorkOnSelectedFile()
        {
            if (SelectedFiles.Count == 1)
            {
                var selectedFile = SelectedFiles[0];
                LoggingService.LogInfo($"Working on file: {selectedFile.FileName}");
                
                try
                {
                    // Load the file into the main application
                    if (!string.IsNullOrEmpty(selectedFile.FilePath) && File.Exists(selectedFile.FilePath))
                    {
                        // Close this window and signal the main application to load the file
                        var mainWindow = System.Windows.Application.Current.MainWindow;
                        if (mainWindow != null && mainWindow.DataContext is MainViewModel mainViewModel)
                        {
                            // Load the file in the main application
                            _ = LoadFileInMainApplication(mainViewModel, selectedFile.FilePath);
                            
                            // Close this window
                            var window = System.Windows.Application.Current.Windows
                                .OfType<System.Windows.Window>()
                                .FirstOrDefault(w => w.DataContext == this);
                            window?.Close();
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(
                            $"File not found: {selectedFile.FileName}", 
                            "File Not Found", 
                            System.Windows.MessageBoxButton.OK, 
                            System.Windows.MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    LoggingService.LogError($"Error working on file: {selectedFile.FileName}", ex);
                    System.Windows.MessageBox.Show(
                        $"Error loading file: {ex.Message}", 
                        "Error", 
                        System.Windows.MessageBoxButton.OK, 
                        System.Windows.MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Loads a file in the main application.
        /// </summary>
        /// <param name="mainViewModel">The main view model.</param>
        /// <param name="filePath">The path to the file to load.</param>
        private async Task LoadFileInMainApplication(MainViewModel mainViewModel, string filePath)
        {
            try
            {
                // Get the file data service from the top bar
                var fileDataService = mainViewModel.TopBar?.GetFileDataService();
                if (fileDataService != null)
                {
                    await fileDataService.LoadFileAsync(filePath);
                    
                    // Update the info bar with file information
                    if (mainViewModel.InfoBar != null && fileDataService.IsFileLoaded())
                    {
                        var fileInfo = fileDataService.GetFileInfo();
                        if (fileInfo != null)
                        {
                            mainViewModel.InfoBar.FileName = fileInfo.Name;
                            mainViewModel.InfoBar.FileSize = FormatFileSize(fileInfo.Length);
                            mainViewModel.InfoBar.LastModified = fileInfo.LastWriteTime;
                        }
                        
                        // Get column count from headers
                        var headers = fileDataService.GetColumnHeaders();
                        mainViewModel.InfoBar.ColumnCount = headers.Count;
                        
                        // Get row count
                        mainViewModel.InfoBar.RowCount = fileDataService.GetRowCount();
                        
                        // Update ColumnSelector with column headers
                        mainViewModel.ColumnSelector?.UpdateColumns(headers);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Error loading file in main application: {filePath}", ex);
                System.Windows.MessageBox.Show(
                    $"Error loading file: {ex.Message}", 
                    "Error", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
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
        /// Works on multiple selected files.
        /// </summary>
        private void WorkOnMultipleFiles()
        {
            if (SelectedFiles.Count > 1)
            {
                LoggingService.LogInfo($"Working on {SelectedFiles.Count} files");
                
                try
                {
                    // For multiple files, we'll load the first file and show a message about the others
                    var firstFile = SelectedFiles[0];
                    var otherFiles = SelectedFiles.Skip(1).ToList();
                    
                    if (!string.IsNullOrEmpty(firstFile.FilePath) && File.Exists(firstFile.FilePath))
                    {
                        // Load the first file into the main application
                        var mainWindow = System.Windows.Application.Current.MainWindow;
                        if (mainWindow != null && mainWindow.DataContext is MainViewModel mainViewModel)
                        {
                            // Load the first file
                            _ = LoadFileInMainApplication(mainViewModel, firstFile.FilePath);
                            
                            // Show information about other selected files
                            var otherFileNames = string.Join(", ", otherFiles.Select(f => f.FileName));
                            System.Windows.MessageBox.Show(
                                $"Loaded file: {firstFile.FileName}\n\nOther selected files:\n{otherFileNames}\n\nYou can work with these files separately.", 
                                "Multiple Files Selected", 
                                System.Windows.MessageBoxButton.OK, 
                                System.Windows.MessageBoxImage.Information);
                            
                            // Close this window
                            var window = System.Windows.Application.Current.Windows
                                .OfType<System.Windows.Window>()
                                .FirstOrDefault(w => w.DataContext == this);
                            window?.Close();
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(
                            $"First file not found: {firstFile.FileName}", 
                            "File Not Found", 
                            System.Windows.MessageBoxButton.OK, 
                            System.Windows.MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    LoggingService.LogError($"Error working on multiple files", ex);
                    System.Windows.MessageBox.Show(
                        $"Error loading files: {ex.Message}", 
                        "Error", 
                        System.Windows.MessageBoxButton.OK, 
                        System.Windows.MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Compares selected files.
        /// </summary>
        private void CompareSelectedFiles()
        {
            if (SelectedFiles.Count >= 2)
            {
                // Implementation to compare selected files
                // This would show differences between files
                var fileNames = string.Join(", ", SelectedFiles.Select(f => f.FileName));
                LoggingService.LogInfo($"Comparing {SelectedFiles.Count} files: {fileNames}");
                
                try
                {
                    // For now, we'll show a comparison window or message
                    // In a full implementation, this would open a dedicated comparison view
                    var comparisonInfo = new StringBuilder();
                    comparisonInfo.AppendLine($"Comparing {SelectedFiles.Count} files:");
                    
                    foreach (var file in SelectedFiles)
                    {
                        comparisonInfo.AppendLine($"\nFile: {file.FileName}");
                        comparisonInfo.AppendLine($"  Size: {FormatFileSize(file.FileSize)}");
                        comparisonInfo.AppendLine($"  Rows: {file.RowCount}");
                        comparisonInfo.AppendLine($"  Columns: {file.ColumnCount}");
                        comparisonInfo.AppendLine($"  Uploaded: {file.UploadDate}");
                    }
                    
                    System.Windows.MessageBox.Show(
                        comparisonInfo.ToString(), 
                        "File Comparison", 
                        System.Windows.MessageBoxButton.OK, 
                        System.Windows.MessageBoxImage.Information);
                    
                    // In a full implementation, we would open a comparison window here
                    // ShowComparisonWindow(SelectedFiles);
                }
                catch (Exception ex)
                {
                    LoggingService.LogError($"Error comparing files", ex);
                    System.Windows.MessageBox.Show(
                        $"Error comparing files: {ex.Message}", 
                        "Error", 
                        System.Windows.MessageBoxButton.OK, 
                        System.Windows.MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Views the latest information for the selected file.
        /// </summary>
        private void ViewLatestInfo()
        {
            if (SelectedFiles.Count == 1)
            {
                var selectedFile = SelectedFiles[0];
                LoggingService.LogInfo($"Viewing latest info for file: {selectedFile.FileName}");
                
                try
                {
                    // Show detailed file information
                    var info = new StringBuilder();
                    info.AppendLine($"File Name: {selectedFile.FileName}");
                    info.AppendLine($"Archive ID: {selectedFile.ArchiveId}");
                    info.AppendLine($"Upload Date: {selectedFile.UploadDate:yyyy-MM-dd HH:mm:ss}");
                    info.AppendLine($"Uploaded By: {selectedFile.UploadedBy ?? "Unknown"}");
                    info.AppendLine($"File Size: {FormatFileSize(selectedFile.FileSize)} ({selectedFile.FileSize} bytes)");
                    info.AppendLine($"Rows: {selectedFile.RowCount}");
                    info.AppendLine($"Columns: {selectedFile.ColumnCount}");
                    info.AppendLine($"File Path: {selectedFile.FilePath ?? "Unknown"}");
                    
                    // Check if file exists
                    if (!string.IsNullOrEmpty(selectedFile.FilePath) && File.Exists(selectedFile.FilePath))
                    {
                        info.AppendLine($"File Status: Available");
                    }
                    else
                    {
                        info.AppendLine($"File Status: Not Found (File may have been moved or deleted)");
                    }
                    
                    System.Windows.MessageBox.Show(
                        info.ToString(), 
                        "File Information", 
                        System.Windows.MessageBoxButton.OK, 
                        System.Windows.MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    LoggingService.LogError($"Error viewing file info: {selectedFile.FileName}", ex);
                    System.Windows.MessageBox.Show(
                        $"Error retrieving file information: {ex.Message}", 
                        "Error", 
                        System.Windows.MessageBoxButton.OK, 
                        System.Windows.MessageBoxImage.Error);
                }
            }
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

        /// <summary>
        /// Disposes of the resources used by the ArchivedFilesViewModel.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of the resources used by the ArchivedFilesViewModel.
        /// </summary>
        /// <param name="disposing">True if disposing managed resources, false otherwise.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    _fileDataService?.ClearCache();
                }

                _disposed = true;
            }
        }
    }
}