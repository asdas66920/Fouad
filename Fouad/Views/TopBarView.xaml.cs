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
using Fouad.ViewModels;

namespace Fouad.Views
{
    /// <summary>
    /// Interaction logic for TopBarView.xaml
    /// </summary>
    public partial class TopBarView : UserControl
    {
        public TopBarView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LanguageButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the DataContext which should be TopBarViewModel
            if (this.DataContext is TopBarViewModel viewModel)
            {
                // Execute the toggle language command
                if (viewModel.ToggleLanguageCommand.CanExecute(null))
                {
                    viewModel.ToggleLanguageCommand.Execute(null);
                }
            }
        }
    }
}