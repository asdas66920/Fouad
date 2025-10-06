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
    /// Interaction logic for ArchivedFilesWindow.xaml
    /// </summary>
    public partial class ArchivedFilesWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArchivedFilesWindow"/> class.
        /// </summary>
        public ArchivedFilesWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event of the CloseButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles the SelectionChanged event of the ArchivedFilesDataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void ArchivedFilesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ArchivedFilesViewModel viewModel)
            {
                // Clear the SelectedFiles collection
                viewModel.SelectedFiles.Clear();
                
                // Add all currently selected items to the SelectedFiles collection
                foreach (var item in ArchivedFilesDataGrid.SelectedItems)
                {
                    if (item is Models.ArchivedFile archivedFile)
                    {
                        viewModel.SelectedFiles.Add(archivedFile);
                    }
                }
                
                // Raise CanExecuteChanged for commands that depend on selection
                ((Commands.RelayCommand)viewModel.WorkOnSelectedFileCommand).RaiseCanExecuteChanged();
                ((Commands.RelayCommand)viewModel.WorkOnMultipleFilesCommand).RaiseCanExecuteChanged();
                ((Commands.RelayCommand)viewModel.CompareSelectedFilesCommand).RaiseCanExecuteChanged();
                ((Commands.RelayCommand)viewModel.ViewLatestInfoCommand).RaiseCanExecuteChanged();
            }
        }
    }
}