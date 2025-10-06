using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using Fouad.ViewModels;

namespace Fouad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private MainViewModel _viewModel = new MainViewModel();
        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;
            
            // Subscribe to the language change event
            if (_viewModel.TopBar != null)
            {
                _viewModel.TopBar.PropertyChanged += TopBarPropertyChanged;
            }
        }
        
        private void TopBarPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModels.TopBarViewModel.IsArabic))
            {
                FlowDirection = _viewModel.TopBar?.IsArabic == true ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            }
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged(string? propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Removed TopBarView_Loaded method since we're setting DataContext in XAML
    }
}