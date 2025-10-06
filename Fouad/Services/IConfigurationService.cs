namespace Fouad.Services
{
    /// <summary>
    /// Interface for configuration service operations.
    /// </summary>
    public interface IConfigurationService
    {
        /// <summary>
        /// Gets or sets the default page size for results.
        /// </summary>
        int DefaultPageSize { get; set; }
        
        /// <summary>
        /// Gets or sets whether to enable audio feedback.
        /// </summary>
        bool EnableAudioFeedback { get; set; }
        
        /// <summary>
        /// Gets or sets the maximum number of search results to display.
        /// </summary>
        int MaxSearchResults { get; set; }
        
        /// <summary>
        /// Gets or sets whether to enable fuzzy search.
        /// </summary>
        bool EnableFuzzySearch { get; set; }
        
        /// <summary>
        /// Gets or sets whether to enable case sensitive search.
        /// </summary>
        bool EnableCaseSensitiveSearch { get; set; }
        
        /// <summary>
        /// Gets or sets whether to enable dark mode.
        /// </summary>
        bool EnableDarkMode { get; set; }
        
        /// <summary>
        /// Gets or sets the selected language.
        /// </summary>
        string SelectedLanguage { get; set; }
        
        /// <summary>
        /// Gets or sets the database path.
        /// </summary>
        string DatabasePath { get; set; }
        
        /// <summary>
        /// Gets or sets the maximum cache size in MB.
        /// </summary>
        long MaxCacheSize { get; set; }
        
        /// <summary>
        /// Gets or sets whether to enable auto cleanup.
        /// </summary>
        bool EnableAutoCleanup { get; set; }
        
        /// <summary>
        /// Gets or sets whether to enable logging.
        /// </summary>
        bool EnableLogging { get; set; }
        
        /// <summary>
        /// Gets or sets the maximum log size in MB.
        /// </summary>
        int MaxLogSize { get; set; }
        
        /// <summary>
        /// Gets or sets whether to enable performance optimization.
        /// </summary>
        bool EnablePerformanceOptimization { get; set; }
        
        /// <summary>
        /// Gets or sets a setting value with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the setting value.</typeparam>
        /// <param name="key">The key of the setting.</param>
        /// <param name="defaultValue">The default value to return if the setting is not found.</param>
        /// <returns>The setting value, or the default value if not found.</returns>
        T GetSetting<T>(string key, T defaultValue);
        
        /// <summary>
        /// Saves a setting value with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the setting value.</typeparam>
        /// <param name="key">The key of the setting.</param>
        /// <param name="value">The value to save.</param>
        void SaveSetting<T>(string key, T value);
    }
}