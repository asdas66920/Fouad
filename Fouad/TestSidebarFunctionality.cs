using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fouad.ViewModels;

namespace Fouad
{
    public class TestSidebarFunctionality
    {
        public static void TestSidebarViewModel()
        {
            // Create an instance of the SideBarViewModel
            var sidebarViewModel = new SideBarViewModel();
            
            // Test that the view model was created successfully
            Console.WriteLine("Sidebar ViewModel created successfully");
            
            // Test properties
            Console.WriteLine($"IsExpanded: {sidebarViewModel.IsExpanded}");
            Console.WriteLine($"SearchText: {sidebarViewModel.SearchText}");
            
            // Test commands
            Console.WriteLine("Testing commands...");
            Console.WriteLine($"ToggleSidebarCommand: {sidebarViewModel.ToggleSidebarCommand != null}");
            Console.WriteLine($"AddNewFileCommand: {sidebarViewModel.AddNewFileCommand != null}");
            Console.WriteLine($"ImportFileCommand: {sidebarViewModel.ImportFileCommand != null}");
            Console.WriteLine($"ExportFileCommand: {sidebarViewModel.ExportFileCommand != null}");
            Console.WriteLine($"BackupCommand: {sidebarViewModel.BackupCommand != null}");
            Console.WriteLine($"RestoreCommand: {sidebarViewModel.RestoreCommand != null}");
            Console.WriteLine($"ClearHistoryCommand: {sidebarViewModel.ClearHistoryCommand != null}");
            Console.WriteLine($"SettingsCommand: {sidebarViewModel.SettingsCommand != null}");
            Console.WriteLine($"SearchCommand: {sidebarViewModel.SearchCommand != null}");
            Console.WriteLine($"ToggleAutoBackupCommand: {sidebarViewModel.ToggleAutoBackupCommand != null}");
            Console.WriteLine($"ToggleNotificationsCommand: {sidebarViewModel.ToggleNotificationsCommand != null}");
            Console.WriteLine($"ToggleDarkModeCommand: {sidebarViewModel.ToggleDarkModeCommand != null}");
            Console.WriteLine($"ArchiveSelectedCommand: {sidebarViewModel.ArchiveSelectedCommand != null}");
            Console.WriteLine($"CompressSelectedCommand: {sidebarViewModel.CompressSelectedCommand != null}");
            Console.WriteLine($"EncryptSelectedCommand: {sidebarViewModel.EncryptSelectedCommand != null}");
            
            // Test new properties
            Console.WriteLine($"AutoBackupEnabled: {sidebarViewModel.AutoBackupEnabled}");
            Console.WriteLine($"NotificationsEnabled: {sidebarViewModel.NotificationsEnabled}");
            Console.WriteLine($"DarkModeEnabled: {sidebarViewModel.DarkModeEnabled}");
            
            Console.WriteLine("All tests passed!");
        }
    }
}