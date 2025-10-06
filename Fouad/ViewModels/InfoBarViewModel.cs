using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using Fouad.Commands;
using Fouad.Services;
using Fouad.Views;
using Fouad.ViewModels;

namespace Fouad.ViewModels
{
    /// <summary>
    /// ViewModel for the info bar view.
    /// Displays file information such as name, size, row count, column count, and last modified date.
    /// </summary>
    public class InfoBarViewModel : INotifyPropertyChanged
    {
        private string _fileName = "No file selected";
        private string _fileSize = "0 KB";
        private int _rowCount = 0;
        private int _columnCount = 0;
        private DateTime _lastModified = DateTime.Now;
        private DatabaseService? _databaseService;
        private FileDataService? _fileDataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfoBarViewModel"/> class.
        /// </summary>
        public InfoBarViewModel()
        {
            OpenArchivedFilesCommand = new RelayCommand(OpenArchivedFilesWindow);
            OpenColumnSelectorCommand = new RelayCommand(OpenColumnSelector);
            ReviewDataCommand = new RelayCommand(ReviewData);
        }

        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value ?? "No file selected";
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the file size.
        /// </summary>
        public string FileSize
        {
            get { return _fileSize; }
            set
            {
                _fileSize = value ?? "0 KB";
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the row count.
        /// </summary>
        public int RowCount
        {
            get { return _rowCount; }
            set
            {
                _rowCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FormattedRowCount));
            }
        }

        /// <summary>
        /// Gets or sets the column count.
        /// </summary>
        public int ColumnCount
        {
            get { return _columnCount; }
            set
            {
                _columnCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FormattedColumnCount));
            }
        }

        /// <summary>
        /// Gets or sets the last modified date.
        /// </summary>
        public DateTime LastModified
        {
            get { return _lastModified; }
            set
            {
                _lastModified = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the formatted row count with thousand separators.
        /// </summary>
        public string FormattedRowCount
        {
            get { return _rowCount.ToString("N0", CultureInfo.CurrentCulture); }
        }

        /// <summary>
        /// Gets the formatted column count with thousand separators.
        /// </summary>
        public string FormattedColumnCount
        {
            get { return _columnCount.ToString("N0", CultureInfo.CurrentCulture); }
        }

        /// <summary>
        /// Gets the open archived files command.
        /// </summary>
        public ICommand OpenArchivedFilesCommand { get; }

        /// <summary>
        /// Gets the open column selector command.
        /// </summary>
        public ICommand OpenColumnSelectorCommand { get; }

        /// <summary>
        /// Gets the review data command.
        /// </summary>
        public ICommand ReviewDataCommand { get; }

        /// <summary>
        /// Sets the database service for the view model.
        /// </summary>
        /// <param name="databaseService">The database service to use.</param>
        public void SetDatabaseService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        /// <summary>
        /// Sets the file data service for the view model.
        /// </summary>
        /// <param name="fileDataService">The file data service to use.</param>
        public void SetFileDataService(FileDataService fileDataService)
        {
            _fileDataService = fileDataService;
        }

        /// <summary>
        /// Opens the archived files window.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void OpenArchivedFilesWindow(object? parameter)
        {
            try
            {
                LoggingService.LogInfo("Attempting to open archived files window");
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
        /// Opens the column selector window.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void OpenColumnSelector(object? parameter)
        {
            try
            {
                LoggingService.LogInfo("Attempting to open column selector window");
                
                if (_fileDataService != null)
                {
                    LoggingService.LogInfo("Creating ColumnSelectorViewModel and ColumnSelectorWindow");
                    var columnSelectorViewModel = new ColumnSelectorViewModel();
                    // Update columns with current file data service columns
                    var columnHeaders = _fileDataService.GetColumnHeaders();
                    if (columnHeaders != null)
                    {
                        columnSelectorViewModel.UpdateColumns(columnHeaders);
                    }
                    
                    var columnSelectorWindow = new Window
                    {
                        Title = "Column Selector",
                        Content = new ColumnSelectorView { DataContext = columnSelectorViewModel },
                        Width = 300,
                        Height = 400,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    
                    // Get the main window to set as owner
                    var mainWindow = Application.Current?.MainWindow;
                    if (mainWindow != null)
                    {
                        columnSelectorWindow.Owner = mainWindow;
                        LoggingService.LogInfo("Set main window as owner of ColumnSelectorWindow");
                    }
                    
                    LoggingService.LogInfo("Showing ColumnSelectorWindow");
                    columnSelectorWindow.ShowDialog();
                    LoggingService.LogInfo("ColumnSelectorWindow closed");
                }
                else
                {
                    LoggingService.LogWarning("FileDataService is null, cannot open column selector window");
                    // Show a message to the user
                    MessageBox.Show("File service not initialized. Please load a file first.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error opening column selector window", ex);
                MessageBox.Show($"Error opening column selector window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Opens the review data window.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void ReviewData(object? parameter)
        {
            try
            {
                LoggingService.LogInfo("Attempting to open review data window");
                
                if (_databaseService != null)
                {
                    LoggingService.LogInfo("Creating ReviewViewModel and ReviewWindow");
                    // For now, we'll create a basic review window since we don't have specific data
                    var reviewWindow = new ReviewWindow();
                    // You might want to set up the ReviewViewModel here if needed
                    
                    // Get the main window to set as owner
                    var mainWindow = Application.Current?.MainWindow;
                    if (mainWindow != null)
                    {
                        reviewWindow.Owner = mainWindow;
                        LoggingService.LogInfo("Set main window as owner of ReviewWindow");
                    }
                    
                    LoggingService.LogInfo("Showing ReviewWindow");
                    reviewWindow.ShowDialog();
                    LoggingService.LogInfo("ReviewWindow closed");
                }
                else
                {
                    LoggingService.LogWarning("DatabaseService is null, cannot open review data window");
                    // Show a message to the user
                    MessageBox.Show("Database service not initialized. Please load a file first.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error opening review data window", ex);
                MessageBox.Show($"Error opening review data window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Resets all properties to their default values.
        /// </summary>
        public void Reset()
        {
            FileName = "No file selected";
            FileSize = "0 KB";
            RowCount = 0;
            ColumnCount = 0;
            LastModified = DateTime.Now;
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