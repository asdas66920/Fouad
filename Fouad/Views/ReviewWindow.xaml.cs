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
using System.Windows.Shapes;
using Fouad.ViewModels;

namespace Fouad.Views
{
    /// <summary>
    /// Interaction logic for ReviewWindow.xaml
    /// </summary>
    public partial class ReviewWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReviewWindow"/> class.
        /// </summary>
        public ReviewWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Loaded event of the window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ReviewViewModel viewModel)
            {
                viewModel.CloseRequested += OnCloseRequested;
            }
        }

        /// <summary>
        /// Handles the CloseRequested event of the view model.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="bool"/> instance containing the event data.</param>
        private void OnCloseRequested(object? sender, bool e)
        {
            DialogResult = e;
            Close();
        }
    }
}