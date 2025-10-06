using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Fouad.Services
{
    /// <summary>
    /// Service for managing application configuration settings.
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
        private readonly string _configPath;
        private AppSettings _settings;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationService"/> class.
        /// </summary>
        /// <param name="configFilePath">Path to the configuration file</param>
        public ConfigurationService(string configFilePath)
        {
            _configPath = configFilePath;
            _settings = new AppSettings();
            LoadSettings();
        }
        
        /// <summary>
        /// Gets or sets the default page size for results.
        /// </summary>
        public int DefaultPageSize
        {
            get => _settings.DefaultPageSize;
            set
            {
                _settings.DefaultPageSize = value;
                SaveSettings();
            }
        }
        
        /// <summary>
        /// Gets or sets whether to enable audio feedback.
        /// </summary>
        public bool EnableAudioFeedback
        {
            get => _settings.EnableAudioFeedback;
            set
            {
                _settings.EnableAudioFeedback = value;
                SaveSettings();
            }
        }
        
        /// <summary>
        /// Gets or sets the maximum number of search results to display.
        /// </summary>
        public int MaxSearchResults
        {
            get => _settings.MaxSearchResults;
            set
            {
                _settings.MaxSearchResults = value;
                SaveSettings();
            }
        }
        
        /// <summary>
        /// Gets or sets whether to enable fuzzy search.
        /// </summary>
        public bool EnableFuzzySearch
        {
            get => _settings.EnableFuzzySearch;
            set
            {
                _settings.EnableFuzzySearch = value;
                SaveSettings();
            }
        }
        
        /// <summary>
        /// Gets or sets whether to enable case sensitive search.
        /// </summary>
        public bool EnableCaseSensitiveSearch
        {
            get => _settings.EnableCaseSensitiveSearch;
            set
            {
                _settings.EnableCaseSensitiveSearch = value;
                SaveSettings();
            }
        }
        
        /// <summary>
        /// Gets or sets whether to enable dark mode.
        /// </summary>
        public bool EnableDarkMode
        {
            get => _settings.EnableDarkMode;
            set
            {
                _settings.EnableDarkMode = value;
                SaveSettings();
            }
        }
        
        /// <summary>
        /// Gets or sets the selected language.
        /// </summary>
        public string SelectedLanguage
        {
            get => _settings.SelectedLanguage;
            set
            {
                _settings.SelectedLanguage = value;
                SaveSettings();
            }
        }
        
        /// <summary>
        /// Gets or sets the database path.
        /// </summary>
        public string DatabasePath
        {
            get => _settings.DatabasePath;
            set
            {
                _settings.DatabasePath = value;
                SaveSettings();
            }
        }
        
        /// <summary>
        /// Gets or sets the maximum cache size in MB.
        /// </summary>
        public long MaxCacheSize
        {
            get => _settings.MaxCacheSize;
            set
            {
                _settings.MaxCacheSize = value;
                SaveSettings();
            }
        }
        
        /// <summary>
        /// Gets or sets whether to enable auto cleanup.
        /// </summary>
        public bool EnableAutoCleanup
        {
            get => _settings.EnableAutoCleanup;
            set
            {
                _settings.EnableAutoCleanup = value;
                SaveSettings();
            }
        }
        
        /// <summary>
        /// Gets or sets whether to enable logging.
        /// </summary>
        public bool EnableLogging
        {
            get => _settings.EnableLogging;
            set
            {
                _settings.EnableLogging = value;
                SaveSettings();
            }
        }
        
        /// <summary>
        /// Gets or sets the maximum log size in MB.
        /// </summary>
        public int MaxLogSize
        {
            get => _settings.MaxLogSize;
            set
            {
                _settings.MaxLogSize = value;
                SaveSettings();
            }
        }
        
        /// <summary>
        /// Gets or sets whether to enable performance optimization.
        /// </summary>
        public bool EnablePerformanceOptimization
        {
            get => _settings.EnablePerformanceOptimization;
            set
            {
                _settings.EnablePerformanceOptimization = value;
                SaveSettings();
            }
        }
        
        /// <summary>
        /// Gets or sets individual sound settings.
        /// </summary>
        public Dictionary<string, bool> SoundSettings
        {
            get => _settings.SoundSettings;
            set
            {
                _settings.SoundSettings = value;
                SaveSettings();
            }
        }
        
        /// <summary>
        /// Gets a specific sound setting.
        /// </summary>
        /// <param name="soundName">The name of the sound</param>
        /// <returns>Whether the sound is enabled</returns>
        public bool GetSoundSetting(string soundName)
        {
            if (_settings.SoundSettings.ContainsKey(soundName))
            {
                return _settings.SoundSettings[soundName];
            }
            return true; // Default to enabled
        }
        
        /// <summary>
        /// Sets a specific sound setting.
        /// </summary>
        /// <param name="soundName">The name of the sound</param>
        /// <param name="enabled">Whether the sound should be enabled</param>
        public void SetSoundSetting(string soundName, bool enabled)
        {
            _settings.SoundSettings[soundName] = enabled;
            SaveSettings();
        }
        
        /// <summary>
        /// Loads settings from the configuration file.
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                    {
                        _settings = settings;
                    }
                }
                else
                {
                    // Create default settings file
                    SaveSettings();
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error loading configuration settings", ex);
            }
        }
        
        /// <summary>
        /// Saves settings to the configuration file.
        /// </summary>
        private void SaveSettings()
        {
            try
            {
                var json = JsonSerializer.Serialize(_settings, JsonOptions);
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error saving configuration settings", ex);
            }
        }
        
        /// <summary>
        /// Gets a setting value with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the setting value.</typeparam>
        /// <param name="key">The key of the setting.</param>
        /// <param name="defaultValue">The default value to return if the setting is not found.</param>
        /// <returns>The setting value, or the default value if not found.</returns>
        public T GetSetting<T>(string key, T defaultValue)
        {
            // For now, we'll just return the default value since we don't have a generic settings storage
            // In a real implementation, this would retrieve the setting from the configuration file
            return defaultValue;
        }
        
        /// <summary>
        /// Saves a setting value with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the setting value.</typeparam>
        /// <param name="key">The key of the setting.</param>
        /// <param name="value">The value to save.</param>
        public void SaveSetting<T>(string key, T value)
        {
            // For now, we'll just do nothing since we don't have a generic settings storage
            // In a real implementation, this would save the setting to the configuration file
        }
    }
    
    /// <summary>
    /// Application settings model.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Gets or sets the default page size for results.
        /// </summary>
        public int DefaultPageSize { get; set; } = 50;
        
        /// <summary>
        /// Gets or sets whether to enable audio feedback.
        /// </summary>
        public bool EnableAudioFeedback { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the maximum number of search results to display.
        /// </summary>
        public int MaxSearchResults { get; set; } = 1000;
        
        /// <summary>
        /// Gets or sets whether to enable fuzzy search.
        /// </summary>
        public bool EnableFuzzySearch { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to enable case sensitive search.
        /// </summary>
        public bool EnableCaseSensitiveSearch { get; set; } = false;
        
        /// <summary>
        /// Gets or sets whether to enable dark mode.
        /// </summary>
        public bool EnableDarkMode { get; set; } = false;
        
        /// <summary>
        /// Gets or sets the selected language.
        /// </summary>
        public string SelectedLanguage { get; set; } = "English";
        
        /// <summary>
        /// Gets or sets the database path.
        /// </summary>
        public string DatabasePath { get; set; } = "";
        
        /// <summary>
        /// Gets or sets the maximum cache size in MB.
        /// </summary>
        public long MaxCacheSize { get; set; } = 100;
        
        /// <summary>
        /// Gets or sets whether to enable auto cleanup.
        /// </summary>
        public bool EnableAutoCleanup { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to enable logging.
        /// </summary>
        public bool EnableLogging { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the maximum log size in MB.
        /// </summary>
        public int MaxLogSize { get; set; } = 10;
        
        /// <summary>
        /// Gets or sets whether to enable performance optimization.
        /// </summary>
        public bool EnablePerformanceOptimization { get; set; } = true;
        
        /// <summary>
        /// Gets or sets individual sound settings.
        /// </summary>
        public Dictionary<string, bool> SoundSettings { get; set; } = new Dictionary<string, bool>
        {
            { "Added", true },
            { "AlreadyAdded", true },
            { "Error", true },
            { "NoResults", true },
            { "Success", true }
        };
    }
}