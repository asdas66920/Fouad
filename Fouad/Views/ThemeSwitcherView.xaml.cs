using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Fouad.Services;

namespace Fouad.Views
{
    /// <summary>
    /// Interaction logic for ThemeSwitcherView.xaml
    /// </summary>
    public partial class ThemeSwitcherView : UserControl
    {
        private ThemeService? _themeService;

        public ThemeSwitcherView()
        {
            InitializeComponent();
            Loaded += ThemeSwitcherView_Loaded;
        }

        private void ThemeSwitcherView_Loaded(object sender, RoutedEventArgs e)
        {
            // Get the theme service from the application's service container
            if (App.ServiceContainer != null)
            {
                _themeService = App.ServiceContainer.Resolve<ThemeService>();
                if (_themeService != null)
                {
                    // Update the UI to reflect the current theme
                    UpdateThemeUI(_themeService.CurrentTheme);
                    
                    // Subscribe to theme changes
                    _themeService.PropertyChanged += ThemeService_PropertyChanged;
                }
            }
        }

        private void ThemeService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ThemeService.CurrentTheme) && _themeService != null)
            {
                Dispatcher.Invoke(() =>
                {
                    UpdateThemeUI(_themeService.CurrentTheme);
                });
            }
        }

        private void UpdateThemeUI(Fouad.Services.ThemeMode theme)
        {
            if (ThemeText != null)
            {
                ThemeText.Text = theme == Fouad.Services.ThemeMode.Dark ? 
                    (Properties.Resources.DarkModeEnabled ?? "Dark Mode") : 
                    (Properties.Resources.LightModeEnabled ?? "Light Mode");
            }

            // Animate the theme transition
            if (ThemeToggleButton != null)
            {
                ThemeToggleButton.IsChecked = theme == Fouad.Services.ThemeMode.Dark;
                
                // Create a smooth transition animation
                var storyboard = new Storyboard();
                
                var animation = new DoubleAnimation
                {
                    From = 0.8,
                    To = 1.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                
                Storyboard.SetTarget(animation, ThemeToggleButton);
                Storyboard.SetTargetProperty(animation, new PropertyPath("RenderTransform.ScaleX"));
                storyboard.Children.Add(animation);
                
                var animation2 = new DoubleAnimation
                {
                    From = 0.8,
                    To = 1.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                
                Storyboard.SetTarget(animation2, ThemeToggleButton);
                Storyboard.SetTargetProperty(animation2, new PropertyPath("RenderTransform.ScaleY"));
                storyboard.Children.Add(animation2);
                
                storyboard.Begin();
            }
        }

        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (_themeService != null)
            {
                // Toggle the theme with animation
                _themeService.ToggleTheme();
            }
        }
    }
}