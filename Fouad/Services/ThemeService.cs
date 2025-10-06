using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace Fouad.Services
{
    /// <summary>
    /// Service for managing application themes (light/dark mode).
    /// </summary>
    public class ThemeService : INotifyPropertyChanged
    {
        private readonly IConfigurationService _configurationService;
        private ThemeMode _currentTheme = ThemeMode.Light;
        private bool _isTransitioning;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ThemeService(IConfigurationService configurationService)
        {
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _currentTheme = _configurationService.EnableDarkMode ? ThemeMode.Dark : ThemeMode.Light;
        }

        /// <summary>
        /// Gets or sets the current theme mode.
        /// </summary>
        public ThemeMode CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    OnPropertyChanged();
                    ApplyTheme(value);
                }
            }
        }

        /// <summary>
        /// Gets whether a theme transition is currently in progress.
        /// </summary>
        public bool IsTransitioning
        {
            get => _isTransitioning;
            private set
            {
                _isTransitioning = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Toggles between light and dark themes.
        /// </summary>
        public void ToggleTheme()
        {
            CurrentTheme = _currentTheme == ThemeMode.Light ? ThemeMode.Dark : ThemeMode.Light;
        }

        /// <summary>
        /// Applies the specified theme to the application.
        /// </summary>
        /// <param name="theme">The theme to apply.</param>
        public void ApplyTheme(ThemeMode theme)
        {
            if (Application.Current == null) return;

            IsTransitioning = true;

            try
            {
                // Update configuration
                _configurationService.EnableDarkMode = theme == ThemeMode.Dark;

                // Remove existing theme dictionaries
                var dictionaries = Application.Current.Resources.MergedDictionaries;
                var themeDictionaries = new ResourceDictionary[dictionaries.Count];
                dictionaries.CopyTo(themeDictionaries, 0);

                foreach (var dict in themeDictionaries)
                {
                    if (dict.Source?.ToString().Contains("Theme") == true)
                    {
                        dictionaries.Remove(dict);
                    }
                }

                // Add the appropriate theme dictionary
                var themeDictionary = new ResourceDictionary
                {
                    Source = new Uri($"/Fouad;component/Themes/{theme}Theme.xaml", UriKind.Relative)
                };

                dictionaries.Add(themeDictionary);

                // Also ensure core resources are loaded
                var appStylesDictionary = new ResourceDictionary
                {
                    Source = new Uri("/Fouad;component/Themes/AppStyles.xaml", UriKind.Relative)
                };
                dictionaries.Add(appStylesDictionary);
            }
            finally
            {
                IsTransitioning = false;
            }
        }

        /// <summary>
        /// Initializes the theme based on user preferences.
        /// </summary>
        public void InitializeTheme()
        {
            ApplyTheme(_currentTheme);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? string.Empty));
        }
    }

    /// <summary>
    /// Represents the available theme modes.
    /// </summary>
    public enum ThemeMode
    {
        Light,
        Dark
    }
}