using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Fouad.Commands;
using Fouad.Services;
using System.IO;

namespace Fouad.ViewModels
{
    /// <summary>
    /// ViewModel for the application settings window.
    /// Manages all application settings including audio, storage, and advanced options.
    /// </summary>
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private ConfigurationService _configurationService;
        private AudioService _audioService;
        
        // Audio Settings
        private bool _enableAudioFeedback;
        private string _selectedSoundPack = string.Empty;
        private bool _addedSoundEnabled;
        private bool _alreadyAddedSoundEnabled;
        private bool _errorSoundEnabled;
        private bool _noResultsSoundEnabled;
        private bool _successSoundEnabled;
        
        // Display Settings
        private int _defaultPageSize;
        private bool _enableDarkMode;
        private string _selectedLanguage = string.Empty;
        
        // Search Settings
        private int _maxSearchResults;
        private bool _enableFuzzySearch;
        private bool _enableCaseSensitiveSearch;
        
        // Storage Settings
        private string _databasePath = string.Empty;
        private long _maxCacheSize;
        private bool _enableAutoCleanup;
        
        // Advanced Settings
        private bool _enableLogging;
        private int _maxLogSize;
        private bool _enablePerformanceOptimization;
        
        // Language support
        private bool _isArabic;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
        /// </summary>
        public SettingsViewModel()
        {
            _configurationService = App.ServiceContainer.Resolve<ConfigurationService>();
            _audioService = new AudioService();
            
            // Initialize commands
            SaveCommand = new RelayCommand(SaveSettings);
            CancelCommand = new RelayCommand(CancelSettings);
            TestAudioCommand = new RelayCommand(TestAudio);
            ResetToDefaultsCommand = new RelayCommand(ResetToDefaults);
            
            // Detect current language
            DetectLanguage();
            
            // Load current settings
            LoadSettings();
        }
        
        #region Properties
        
        // Language support
        /// <summary>
        /// Gets or sets whether the application is in Arabic mode.
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
        
        // Audio Settings
        /// <summary>
        /// Gets or sets whether audio feedback is enabled.
        /// </summary>
        public bool EnableAudioFeedback
        {
            get { return _enableAudioFeedback; }
            set
            {
                _enableAudioFeedback = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets the selected sound pack.
        /// </summary>
        public string SelectedSoundPack
        {
            get { return _selectedSoundPack; }
            set
            {
                _selectedSoundPack = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets the available sound packs.
        /// </summary>
        public List<string> SoundPacks { get; } = new List<string> { "Default", "Professional", "Minimal", "Disabled" };
        
        /// <summary>
        /// Gets or sets whether the "Added" sound is enabled.
        /// </summary>
        public bool AddedSoundEnabled
        {
            get { return _addedSoundEnabled; }
            set
            {
                _addedSoundEnabled = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets whether the "AlreadyAdded" sound is enabled.
        /// </summary>
        public bool AlreadyAddedSoundEnabled
        {
            get { return _alreadyAddedSoundEnabled; }
            set
            {
                _alreadyAddedSoundEnabled = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets whether the "Error" sound is enabled.
        /// </summary>
        public bool ErrorSoundEnabled
        {
            get { return _errorSoundEnabled; }
            set
            {
                _errorSoundEnabled = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets whether the "NoResults" sound is enabled.
        /// </summary>
        public bool NoResultsSoundEnabled
        {
            get { return _noResultsSoundEnabled; }
            set
            {
                _noResultsSoundEnabled = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets whether the "Success" sound is enabled.
        /// </summary>
        public bool SuccessSoundEnabled
        {
            get { return _successSoundEnabled; }
            set
            {
                _successSoundEnabled = value;
                OnPropertyChanged();
            }
        }
        
        // Display Settings
        /// <summary>
        /// Gets or sets the default page size for results.
        /// </summary>
        public int DefaultPageSize
        {
            get { return _defaultPageSize; }
            set
            {
                _defaultPageSize = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets whether dark mode is enabled.
        /// </summary>
        public bool EnableDarkMode
        {
            get { return _enableDarkMode; }
            set
            {
                _enableDarkMode = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets the selected language.
        /// </summary>
        public string SelectedLanguage
        {
            get { return _selectedLanguage; }
            set
            {
                _selectedLanguage = value;
                OnPropertyChanged();
                // Update the IsArabic property when language changes
                IsArabic = value.Equals("Arabic", StringComparison.OrdinalIgnoreCase) || 
                          value.Equals("العربية", StringComparison.OrdinalIgnoreCase);
            }
        }
        
        /// <summary>
        /// Gets the available languages.
        /// </summary>
        public List<string> Languages { get; } = new List<string> { "English", "Arabic" };
        
        // Search Settings
        /// <summary>
        /// Gets or sets the maximum number of search results.
        /// </summary>
        public int MaxSearchResults
        {
            get { return _maxSearchResults; }
            set
            {
                _maxSearchResults = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets whether fuzzy search is enabled.
        /// </summary>
        public bool EnableFuzzySearch
        {
            get { return _enableFuzzySearch; }
            set
            {
                _enableFuzzySearch = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets whether case sensitive search is enabled.
        /// </summary>
        public bool EnableCaseSensitiveSearch
        {
            get { return _enableCaseSensitiveSearch; }
            set
            {
                _enableCaseSensitiveSearch = value;
                OnPropertyChanged();
            }
        }
        
        // Storage Settings
        /// <summary>
        /// Gets or sets the database path.
        /// </summary>
        public string DatabasePath
        {
            get { return _databasePath; }
            set
            {
                _databasePath = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets the maximum cache size in MB.
        /// </summary>
        public long MaxCacheSize
        {
            get { return _maxCacheSize; }
            set
            {
                _maxCacheSize = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets whether auto cleanup is enabled.
        /// </summary>
        public bool EnableAutoCleanup
        {
            get { return _enableAutoCleanup; }
            set
            {
                _enableAutoCleanup = value;
                OnPropertyChanged();
            }
        }
        
        // Advanced Settings
        /// <summary>
        /// Gets or sets whether logging is enabled.
        /// </summary>
        public bool EnableLogging
        {
            get { return _enableLogging; }
            set
            {
                _enableLogging = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets the maximum log size in MB.
        /// </summary>
        public int MaxLogSize
        {
            get { return _maxLogSize; }
            set
            {
                _maxLogSize = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets whether performance optimization is enabled.
        /// </summary>
        public bool EnablePerformanceOptimization
        {
            get { return _enablePerformanceOptimization; }
            set
            {
                _enablePerformanceOptimization = value;
                OnPropertyChanged();
            }
        }
        
        #endregion
        
        #region Commands
        
        /// <summary>
        /// Gets the save settings command.
        /// </summary>
        public RelayCommand SaveCommand { get; private set; }
        
        /// <summary>
        /// Gets the cancel settings command.
        /// </summary>
        public RelayCommand CancelCommand { get; private set; }
        
        /// <summary>
        /// Gets the test audio command.
        /// </summary>
        public RelayCommand TestAudioCommand { get; private set; }
        
        /// <summary>
        /// Gets the reset to defaults command.
        /// </summary>
        public RelayCommand ResetToDefaultsCommand { get; private set; }
        
        #endregion
        
        #region Methods
        
        /// <summary>
        /// Detects the current language based on the UI culture.
        /// </summary>
        private void DetectLanguage()
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            IsArabic = currentCulture.Name.StartsWith("ar", StringComparison.OrdinalIgnoreCase);
            SelectedLanguage = IsArabic ? "Arabic" : "English";
        }
        
        /// <summary>
        /// Loads the current settings from the configuration service.
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                // Load audio settings
                EnableAudioFeedback = _configurationService.EnableAudioFeedback;
                SelectedSoundPack = "Default";
                AddedSoundEnabled = _configurationService.GetSoundSetting("Added");
                AlreadyAddedSoundEnabled = _configurationService.GetSoundSetting("AlreadyAdded");
                ErrorSoundEnabled = _configurationService.GetSoundSetting("Error");
                NoResultsSoundEnabled = _configurationService.GetSoundSetting("NoResults");
                SuccessSoundEnabled = _configurationService.GetSoundSetting("Success");
                
                // Load display settings
                DefaultPageSize = _configurationService.DefaultPageSize;
                EnableDarkMode = _configurationService.EnableDarkMode;
                SelectedLanguage = _configurationService.SelectedLanguage;
                
                // Load search settings
                MaxSearchResults = _configurationService.MaxSearchResults;
                EnableFuzzySearch = _configurationService.EnableFuzzySearch;
                EnableCaseSensitiveSearch = _configurationService.EnableCaseSensitiveSearch;
                
                // Load storage settings
                DatabasePath = string.IsNullOrEmpty(_configurationService.DatabasePath) ? 
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.db") : 
                    _configurationService.DatabasePath;
                MaxCacheSize = _configurationService.MaxCacheSize;
                EnableAutoCleanup = _configurationService.EnableAutoCleanup;
                
                // Load advanced settings
                EnableLogging = _configurationService.EnableLogging;
                MaxLogSize = _configurationService.MaxLogSize;
                EnablePerformanceOptimization = _configurationService.EnablePerformanceOptimization;
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error loading settings", ex);
                MessageBox.Show("Error loading settings. Using default values.", "Settings Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        /// <summary>
        /// Saves the settings to the configuration service.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void SaveSettings(object? parameter)
        {
            try
            {
                // Save audio settings
                _configurationService.EnableAudioFeedback = EnableAudioFeedback;
                _configurationService.SetSoundSetting("Added", AddedSoundEnabled);
                _configurationService.SetSoundSetting("AlreadyAdded", AlreadyAddedSoundEnabled);
                _configurationService.SetSoundSetting("Error", ErrorSoundEnabled);
                _configurationService.SetSoundSetting("NoResults", NoResultsSoundEnabled);
                _configurationService.SetSoundSetting("Success", SuccessSoundEnabled);
                
                // Save display settings
                _configurationService.DefaultPageSize = DefaultPageSize;
                _configurationService.EnableDarkMode = EnableDarkMode;
                _configurationService.SelectedLanguage = SelectedLanguage;
                
                // Save search settings
                _configurationService.MaxSearchResults = MaxSearchResults;
                _configurationService.EnableFuzzySearch = EnableFuzzySearch;
                _configurationService.EnableCaseSensitiveSearch = EnableCaseSensitiveSearch;
                
                // Save storage settings
                _configurationService.DatabasePath = DatabasePath;
                _configurationService.MaxCacheSize = MaxCacheSize;
                _configurationService.EnableAutoCleanup = EnableAutoCleanup;
                
                // Save advanced settings
                _configurationService.EnableLogging = EnableLogging;
                _configurationService.MaxLogSize = MaxLogSize;
                _configurationService.EnablePerformanceOptimization = EnablePerformanceOptimization;
                
                // Notify that settings were saved successfully
                var successMessage = IsArabic ? 
                    "تم حفظ الإعدادات بنجاح!" : 
                    "Settings saved successfully!";
                var successTitle = IsArabic ? 
                    "تم حفظ الإعدادات" : 
                    "Settings Saved";
                MessageBox.Show(successMessage, successTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Close the settings window if called from a window
                if (parameter is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error saving settings", ex);
                var errorMessage = IsArabic ? 
                    $"خطأ في حفظ الإعدادات: {ex.Message}" : 
                    $"Error saving settings: {ex.Message}";
                var errorTitle = IsArabic ? 
                    "خطأ في الإعدادات" : 
                    "Settings Error";
                MessageBox.Show(errorMessage, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// Cancels the settings changes and closes the window.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void CancelSettings(object? parameter)
        {
            if (parameter is Window window)
            {
                window.DialogResult = false;
                window.Close();
            }
        }
        
        /// <summary>
        /// Tests the audio settings by playing a sample sound.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void TestAudio(object? parameter)
        {
            if (EnableAudioFeedback)
            {
                _audioService.PlaySound("Success");
            }
            else
            {
                var message = IsArabic ? 
                    "ردود الفعل الصوتية معطلة حالياً." : 
                    "Audio feedback is currently disabled.";
                var title = IsArabic ? 
                    "اختبار الصوت" : 
                    "Audio Test";
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        
        /// <summary>
        /// Resets all settings to their default values.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void ResetToDefaults(object? parameter)
        {
            var confirmationMessage = IsArabic ? 
                "هل أنت متأكد من أنك تريد إعادة تعيين جميع الإعدادات إلى قيمها الافتراضية؟" : 
                "Are you sure you want to reset all settings to their default values?";
            var confirmationTitle = IsArabic ? 
                "إعادة تعيين الإعدادات" : 
                "Reset Settings";
                
            var result = MessageBox.Show(confirmationMessage, confirmationTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                // Reset audio settings
                EnableAudioFeedback = true;
                SelectedSoundPack = "Default";
                AddedSoundEnabled = true;
                AlreadyAddedSoundEnabled = true;
                ErrorSoundEnabled = true;
                NoResultsSoundEnabled = true;
                SuccessSoundEnabled = true;
                
                // Reset display settings
                DefaultPageSize = 50;
                EnableDarkMode = false;
                SelectedLanguage = IsArabic ? "Arabic" : "English";
                
                // Reset search settings
                MaxSearchResults = 1000;
                EnableFuzzySearch = true;
                EnableCaseSensitiveSearch = false;
                
                // Reset storage settings
                DatabasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.db");
                MaxCacheSize = 100;
                EnableAutoCleanup = true;
                
                // Reset advanced settings
                EnableLogging = true;
                MaxLogSize = 10;
                EnablePerformanceOptimization = true;
                
                var successMessage = IsArabic ? 
                    "تمت إعادة تعيين جميع الإعدادات إلى قيمها الافتراضية." : 
                    "All settings have been reset to their default values.";
                var successTitle = IsArabic ? 
                    "تمت إعادة تعيين الإعدادات" : 
                    "Settings Reset";
                MessageBox.Show(successMessage, successTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        
        #endregion
        
        #region INotifyPropertyChanged Implementation
        
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
        
        #endregion
    }
}