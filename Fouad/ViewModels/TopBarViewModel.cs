using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using Microsoft.Win32; // Changed from System.Windows.Forms to Microsoft.Win32 for OpenFileDialog
using Fouad.Commands;
using Fouad.Services;
using System.Windows.Threading;
using System.IO;

namespace Fouad.ViewModels
{
    /// <summary>
    /// ViewModel for the top bar view.
    /// Manages file operations, application settings, and UI state for the top bar.
    /// </summary>
    public class TopBarViewModel : INotifyPropertyChanged
    {
        private string _title = "Easier Searching";
        private string _languageButtonText = "العربية";
        private bool _isArabic = false;
        private FileDataService _fileDataService;
        private DatabaseService? _databaseService;
        private InfoBarViewModel? _infoBarViewModel;
        private ColumnSelectorViewModel? _columnSelectorViewModel;
        private AudioService _audioService;
        private ThemeService? _themeService; // Add ThemeService reference
        private DispatcherTimer? _titleAnimationTimer;
        private int _titleAnimationIndex = 0;
        private string[] _titleVariations;

        /// <summary>
        /// Initializes a new instance of the <see cref="TopBarViewModel"/> class.
        /// </summary>
        /// <param name="infoBarViewModel">The info bar view model.</param>
        /// <param name="columnSelectorViewModel">The column selector view model.</param>
        public TopBarViewModel(InfoBarViewModel? infoBarViewModel, ColumnSelectorViewModel? columnSelectorViewModel)
        {
            _infoBarViewModel = infoBarViewModel;
            _columnSelectorViewModel = columnSelectorViewModel;
            _fileDataService = new FileDataService();
            _audioService = new AudioService();
            
            // Initialize ThemeService
            _themeService = App.ServiceContainer.Resolve<ThemeService>();
            
            // Initialize database service
            var databasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.db");
            _databaseService = new DatabaseService(databasePath);
            _fileDataService.SetDatabaseService(_databaseService);
            
            // Set database service in info bar view model
            _infoBarViewModel?.SetDatabaseService(_databaseService);
            _infoBarViewModel?.SetFileDataService(_fileDataService);
            
            // Initialize title variations for animation
            _titleVariations = new string[] { "Easier Searching", "بحث أسهل", "Easy Search", "بحث سهل" };
            
            InsertFileCommand = new RelayCommand(InsertFile);
            DeleteFileCommand = new RelayCommand(DeleteFile);
            SettingsCommand = new RelayCommand(OpenSettings);
            StoreCommand = new RelayCommand(StoreData);
            ToggleLanguageCommand = new RelayCommand(ToggleLanguage);
            PlayTestSoundCommand = new RelayCommand(PlayTestSound);
            ReviewDataCommand = new RelayCommand(OpenReviewWindow);
            
            // Start title animation
            StartTitleAnimation();
        }

        /// <summary>
        /// Gets or sets the application title.
        /// </summary>
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether dark mode is enabled.
        /// </summary>
        public bool IsDarkMode
        {
            get { return _themeService?.CurrentTheme == Fouad.Services.ThemeMode.Dark; }
            set
            {
                if (_themeService != null)
                {
                    _themeService.CurrentTheme = value ? Fouad.Services.ThemeMode.Dark : Fouad.Services.ThemeMode.Light;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the text for the language toggle button.
        /// </summary>
        public string LanguageButtonText
        {
            get { return _languageButtonText; }
            set
            {
                _languageButtonText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application is in Arabic mode.
        /// </summary>
        public bool IsArabic
        {
            get { return _isArabic; }
            set
            {
                _isArabic = value;
                OnPropertyChanged();
            }
        }

        // Commands
        /// <summary>
        /// Gets the insert file command.
        /// </summary>
        public RelayCommand InsertFileCommand { get; private set; }
        
        /// <summary>
        /// Gets the delete file command.
        /// </summary>
        public RelayCommand DeleteFileCommand { get; private set; }
        
        /// <summary>
        /// Gets the settings command.
        /// </summary>
        public RelayCommand SettingsCommand { get; private set; }
        
        /// <summary>
        /// Gets the store command.
        /// </summary>
        public RelayCommand StoreCommand { get; private set; }
        
        /// <summary>
        /// Gets the toggle language command.
        /// </summary>
        public RelayCommand ToggleLanguageCommand { get; private set; }
        
        /// <summary>
        /// Gets the play test sound command.
        /// </summary>
        public RelayCommand PlayTestSoundCommand { get; private set; }
        
        /// <summary>
        /// Gets the review data command.
        /// </summary>
        public RelayCommand ReviewDataCommand { get; private set; }

        /// <summary>
        /// Exposes FileDataService for other ViewModels.
        /// </summary>
        /// <returns>The FileDataService instance.</returns>
        public FileDataService GetFileDataService()
        {
            return _fileDataService;
        }

        // Command implementations
        /// <summary>
        /// Inserts a file into the application.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private async void InsertFile(object? parameter)
        {
            try
            {
                LoggingService.LogInfo("InsertFile command executed");
                // Create OpenFileDialog
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Excel Files|*.xlsx;*.xls|CSV Files|*.csv|All Files|*.*",
                    Title = Properties.Resources.SelectFileToLoad
                };

                // Show OpenFileDialog by calling ShowDialog method
                var result = openFileDialog.ShowDialog();
                
                // Get the selected file name and display in a TextBox
                if (result == true) // Changed from DialogResult.OK to true
                {
                    try
                    {
                        LoggingService.LogInfo($"File selected: {openFileDialog.FileName}");
                        // Load the file asynchronously
                        await _fileDataService.LoadFileAsync(openFileDialog.FileName);
                        
                        // Update InfoBar with file information
                        if (_infoBarViewModel != null && _fileDataService.IsFileLoaded())
                        {
                            var fileInfo = _fileDataService.GetFileInfo();
                            if (fileInfo != null)
                            {
                                _infoBarViewModel.FileName = fileInfo.Name;
                                _infoBarViewModel.FileSize = FormatFileSize(fileInfo.Length);
                                _infoBarViewModel.LastModified = fileInfo.LastWriteTime;
                                LoggingService.LogInfo($"File info updated: {fileInfo.Name}");
                            }
                            
                            // Get column count from headers
                            var headers = _fileDataService.GetColumnHeaders();
                            LoggingService.LogInfo($"Retrieved headers from FileDataService:");
                            for (int i = 0; i < headers.Count; i++)
                            {
                                LoggingService.LogInfo($"Header {i}: '{headers[i]}'");
                            }
                            _infoBarViewModel.ColumnCount = headers.Count;
                            LoggingService.LogInfo($"Column count: {headers.Count}");
                            
                            // Get row count
                            _infoBarViewModel.RowCount = _fileDataService.GetRowCount();
                            LoggingService.LogInfo($"Row count: {_infoBarViewModel.RowCount}");
                            
                            // Update ColumnSelector with column headers
                            _columnSelectorViewModel?.UpdateColumns(headers);
                            LoggingService.LogInfo("Column selector updated");
                            
                            // Update title to show file name
                            Title = $"Searching in: {fileInfo?.Name ?? "Unknown File"}";
                            
                            // Import file with database integration
                            if (_databaseService != null)
                            {
                                var archiveId = await _fileDataService.ImportFileWithDatabaseAsync(openFileDialog.FileName);
                                LoggingService.LogInfo($"File imported with ArchiveId: {archiveId}");
                            }
                            
                            // Play success sound
                            _audioService.PlaySound("Success");
                            
                            // Show success message
                            LoggingService.LogInfo("Showing success message");
                            MessageBox.Show(Properties.Resources.FileInsertedSuccessfully, Properties.Resources.Success, MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggingService.LogError("Error during file loading process", ex);
                        // Play error sound
                        _audioService.PlaySound("Error");
                        
                        // Handle exception - in a real application, you might want to show a message box
                        MessageBox.Show(string.Format(Properties.Resources.ErrorLoadingFile, ex.Message), Properties.Resources.FileLoadError, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    LoggingService.LogInfo("File selection cancelled");
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error during file selection process", ex);
                // Handle any exceptions that occur during the file selection process
                // Play error sound
                _audioService.PlaySound("Error");
                
                // Show error message
                MessageBox.Show(string.Format(Properties.Resources.ErrorLoadingFile, ex.Message), Properties.Resources.FileLoadError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Formats a file size in bytes to a human-readable string.
        /// </summary>
        /// <param name="bytes">The file size in bytes.</param>
        /// <returns>A formatted string representing the file size.</returns>
        public string FormatFileSize(long bytes)
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
        /// Deletes the currently loaded file.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void DeleteFile(object? parameter)
        {
            // Implementation for deleting a file
            // Reset the application state
            _infoBarViewModel?.Reset();
            _columnSelectorViewModel?.Reset();
            _fileDataService = new FileDataService(); // Create a new instance
            Title = IsArabic ? "بحث أسهل" : "Easier Searching";
            
            // Play delete sound
            _audioService.PlaySound("Success");
            
            MessageBox.Show(Properties.Resources.FileDeletedSuccessfully, Properties.Resources.FileDeleted, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Opens the application settings.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void OpenSettings(object? parameter)
        {
            try
            {
                // Play settings sound
                _audioService.PlaySound("Added");
                
                // Create and show the settings window
                var settingsWindow = new Views.SettingsView();
                var settingsViewModel = new ViewModels.SettingsViewModel();
                settingsWindow.DataContext = settingsViewModel;
                settingsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error opening settings window", ex);
                MessageBox.Show($"Error opening settings: {ex.Message}", "Settings Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Stores application data.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void StoreData(object? parameter)
        {
            // Play store sound
            _audioService.PlaySound("Added");
            
            // Implementation for storing data
            MessageBox.Show(Properties.Resources.DataStoredSuccessfully, Properties.Resources.DataStored, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Toggles the application language between English and Arabic.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void ToggleLanguage(object? parameter)
        {
            IsArabic = !IsArabic;
            if (IsArabic)
            {
                LanguageButtonText = "English";
                // Change the language to Arabic
                ChangeLanguage("ar-SA");
                SwitchToArabicResources();
            }
            else
            {
                LanguageButtonText = "العربية";
                // Change the language to English
                ChangeLanguage("en-US");
                SwitchToEnglishResources();
            }
            
            // Update title based on language
            Title = IsArabic ? "بحث أسهل" : "Easier Searching";
            
            // Play language change sound
            _audioService.PlaySound("Added");
            
            // Notify property changed for the button text
            OnPropertyChanged(nameof(LanguageButtonText));
        }

        /// <summary>
        /// Plays a test sound to verify audio is working.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void PlayTestSound(object? parameter)
        {
            // Play a test sound to verify audio is working
            _audioService.PlaySound("Success");
        }
        
        /// <summary>
        /// Opens the review window for manual data review.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void OpenReviewWindow(object? parameter)
        {
            try
            {
                if (_fileDataService.IsFileLoaded() && _databaseService != null)
                {
                    var archiveId = _fileDataService.GetArchiveId();
                    if (archiveId > 0)
                    {
                        var fileInfo = _fileDataService.GetFileInfo();
                        var fileName = fileInfo?.Name ?? "Unknown File";
                        
                        var reviewService = new ReviewService(_databaseService, "Data Source=" + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.db"));
                        var reviewViewModel = new ReviewViewModel(reviewService, archiveId, fileName);
                        
                        var reviewWindow = new Views.ReviewWindow
                        {
                            DataContext = reviewViewModel
                        };
                        
                        // Initialize the review view model
                        _ = reviewViewModel.InitializeAsync();
                        
                        reviewWindow.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error opening review window", ex);
                MessageBox.Show($"Error opening review window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Changes the application language.
        /// </summary>
        /// <param name="languageCode">The language code to change to.</param>
        private void ChangeLanguage(string languageCode)
        {
            try
            {
                var culture = new CultureInfo(languageCode);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;

                // Update the FlowDirection of the main window
                if (Application.Current?.MainWindow != null)
                {
                    Application.Current.MainWindow.FlowDirection = IsArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                }
                
                // Update the title to use the correct language resource
                Title = IsArabic ? "بحث أسهل" : "Easier Searching";
            }
            catch (Exception ex)
            {
                // Handle exception if needed
                // Using the variable to avoid the unused variable warning
                var _ = ex;
            }
        }

        /// <summary>
        /// Switches to Arabic language resources.
        /// </summary>
        private void SwitchToArabicResources()
        {
            if (Application.Current?.MainWindow != null)
            {
                // Remove English resources
                var englishDict = Application.Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Source?.ToString().Contains("EnglishStrings") == true);
                if (englishDict != null)
                    Application.Current.Resources.MergedDictionaries.Remove(englishDict);
                
                // Add Arabic resources if not already present
                var arabicDict = Application.Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Source?.ToString().Contains("ArabicStrings") == true);
                if (arabicDict == null)
                {
                    var newDict = new ResourceDictionary { Source = new Uri("pack://application:,,,/Fouad;component/Resources/ArabicStrings.xaml") };
                    Application.Current.Resources.MergedDictionaries.Add(newDict);
                }
            }
        }

        /// <summary>
        /// Switches to English language resources.
        /// </summary>
        private void SwitchToEnglishResources()
        {
            if (Application.Current?.MainWindow != null)
            {
                // Remove Arabic resources
                var arabicDict = Application.Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Source?.ToString().Contains("ArabicStrings") == true);
                if (arabicDict != null)
                    Application.Current.Resources.MergedDictionaries.Remove(arabicDict);
                
                // Add English resources if not already present
                var englishDict = Application.Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Source?.ToString().Contains("EnglishStrings") == true);
                if (englishDict == null)
                {
                    var newDict = new ResourceDictionary { Source = new Uri("pack://application:,,,/Fouad;component/Resources/EnglishStrings.xaml") };
                    Application.Current.Resources.MergedDictionaries.Add(newDict);
                }
            }
        }

        /// <summary>
        /// Starts the title animation.
        /// </summary>
        private void StartTitleAnimation()
        {
            _titleAnimationTimer = new DispatcherTimer();
            _titleAnimationTimer.Interval = TimeSpan.FromSeconds(3);
            _titleAnimationTimer.Tick += (sender, e) =>
            {
                _titleAnimationIndex = (_titleAnimationIndex + 1) % _titleVariations.Length;
                Title = _titleVariations[_titleAnimationIndex];
            };
            _titleAnimationTimer.Start();
        }

        /// <summary>
        /// Stops the title animation.
        /// </summary>
        public void StopTitleAnimation()
        {
            _titleAnimationTimer?.Stop();
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