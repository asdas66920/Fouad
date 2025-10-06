using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Fouad.Services;
using Microsoft.Data.Sqlite;
using OfficeOpenXml;

namespace Fouad
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private DatabaseService? _databaseService;
        public static ServiceContainer ServiceContainer { get; private set; } = new ServiceContainer();
        
        protected override void OnStartup(StartupEventArgs e)
        {
            LoggingService.LogInfo("Application starting");
            
            // Set EPPlus license for non-commercial use
            // ExcelPackage.License is read-only, so we just need to ensure we're using the correct context
            // The default context should work for non-commercial use
            
            // Initialize database service
            var databasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.db");
            
            // Register default services
            ServiceContainer.RegisterDefaultServices(databasePath);
            
            // Resolve database service
            _databaseService = (DatabaseService)ServiceContainer.Resolve<IDatabaseService>();
            
            // Initialize theme service
            var themeService = ServiceContainer.Resolve<ThemeService>();
            themeService?.InitializeTheme();
            
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            base.OnStartup(e);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            if (exception != null)
            {
                LoggingService.LogError("Unhandled exception in application", exception);
            }
            else
            {
                LoggingService.LogError("Unhandled exception in application");
            }
        }
    }
}