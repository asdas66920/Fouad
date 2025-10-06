using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Fouad.Commands;
using Fouad.Services;

namespace Fouad.ViewModels
{
    /// <summary>
    /// ViewModel for the theme switcher control.
    /// </summary>
    public class ThemeSwitcherViewModel : INotifyPropertyChanged
    {
        private readonly ThemeService _themeService;
        private bool _isDarkMode;
        private string _themeText = string.Empty;
        private ICommand? _toggleThemeCommand;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ThemeSwitcherViewModel(ThemeService themeService)
        {
            _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
            
            // Initialize properties
            _isDarkMode = _themeService.CurrentTheme == Services.ThemeMode.Dark;
            _themeText = _isDarkMode ? (Properties.Resources.DarkModeEnabled ?? "Dark Mode") : (Properties.Resources.LightModeEnabled ?? "Light Mode");
            
            // Subscribe to theme changes
            _themeService.PropertyChanged += ThemeService_PropertyChanged;
        }

        /// <summary>
        /// Gets or sets whether dark mode is enabled.
        /// </summary>
        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ThemeText));
                    
                    // Apply the theme change
                    _themeService.CurrentTheme = value ? Services.ThemeMode.Dark : Services.ThemeMode.Light;
                }
            }
        }

        /// <summary>
        /// Gets the text to display for the current theme.
        /// </summary>
        public string ThemeText => IsDarkMode ? (Properties.Resources.DarkModeEnabled ?? "Dark Mode") : (Properties.Resources.LightModeEnabled ?? "Light Mode");

        /// <summary>
        /// Gets the command to toggle the theme.
        /// </summary>
        public ICommand ToggleThemeCommand => _toggleThemeCommand ??= new RelayCommand(ToggleTheme);

        private void ToggleTheme(object? parameter)
        {
            IsDarkMode = !IsDarkMode;
        }

        private void ThemeService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ThemeService.CurrentTheme))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _isDarkMode = _themeService.CurrentTheme == Services.ThemeMode.Dark;
                    OnPropertyChanged(nameof(IsDarkMode));
                    OnPropertyChanged(nameof(ThemeText));
                });
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? string.Empty));
        }
    }
}