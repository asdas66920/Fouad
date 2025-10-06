using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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

namespace Fouad.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : Window
    {
        public SettingsView()
        {
            InitializeComponent();
            
            // Set the flow direction based on the current culture
            UpdateFlowDirection();
        }
        
        /// <summary>
        /// Updates the flow direction based on the current culture.
        /// </summary>
        private void UpdateFlowDirection()
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            bool isArabic = currentCulture.Name.StartsWith("ar", StringComparison.OrdinalIgnoreCase);
            FlowDirection = isArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }
    }
}