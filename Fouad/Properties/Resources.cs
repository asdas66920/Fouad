using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Fouad.Properties
{
    public static class Resources
    {
        public static void SetCulture(System.Globalization.CultureInfo culture)
        {
            // This method would be used to switch cultures if needed
        }
        
        private static string? GetResourceString(string key, string defaultValue)
        {
            try
            {
                if (Application.Current != null)
                {
                    var resource = Application.Current.FindResource(key);
                    if (resource != null)
                    {
                        var result = resource?.ToString();
                        return result ?? defaultValue;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception but don't crash the application
                System.Diagnostics.Debug.WriteLine($"Error retrieving resource '{key}': {ex.Message}");
            }
            
            // Return default value if resource not found or error occurred
            return defaultValue;
        }
        
        public static string InsertFile => GetResourceString("InsertFile", "Insert File") ?? "Insert File";
        public static string DeleteFile => GetResourceString("DeleteFile", "Delete File") ?? "Delete File";
        public static string Settings => GetResourceString("Settings", "Settings") ?? "Settings";
        public static string Store => GetResourceString("Store", "Store") ?? "Store";
        public static string ManageAudio => GetResourceString("ManageAudio", "Manage Audio") ?? "Manage Audio";
        public static string Results => GetResourceString("Results", "Results") ?? "Results";
        public static string History => GetResourceString("History", "History") ?? "History";
        public static string AdvancedSearch => GetResourceString("AdvancedSearch", "Advanced Search") ?? "Advanced Search";
        public static string SelectAll => GetResourceString("SelectAll", "Select All") ?? "Select All";
        public static string UnselectAll => GetResourceString("UnselectAll", "Unselect All") ?? "Unselect All";
        public static string Delete => GetResourceString("Delete", "Delete") ?? "Delete";
        public static string Edit => GetResourceString("Edit", "Edit") ?? "Edit";
        public static string Copy => GetResourceString("Copy", "Copy") ?? "Copy";
        public static string ExportToExcel => GetResourceString("ExportToExcel", "Export to Excel") ?? "Export to Excel";
        public static string ExportToPDF => GetResourceString("ExportToPDF", "Export to PDF") ?? "Export to PDF";
        public static string FileName => GetResourceString("FileName", "File Name") ?? "File Name";
        public static string Size => GetResourceString("Size", "Size") ?? "Size";
        public static string Rows => GetResourceString("Rows", "Rows") ?? "Rows";
        public static string Columns => GetResourceString("Columns", "Columns") ?? "Columns";
        public static string Modified => GetResourceString("Modified", "Modified") ?? "Modified";
        public static string SearchTerm => GetResourceString("SearchTerm", "Search Term") ?? "Search Term";
        public static string SearchDate => GetResourceString("SearchDate", "Search Date") ?? "Search Date";
        public static string Matches => GetResourceString("Matches", "Matches") ?? "Matches";
        public static string ResultsCount => GetResourceString("ResultsCount", "Results Count") ?? "Results Count";
        public static string SidebarControls => GetResourceString("SidebarControls", "Sidebar Controls") ?? "Sidebar Controls";
        public static string Control1 => GetResourceString("Control1", "Control 1") ?? "Control 1";
        public static string Control2 => GetResourceString("Control2", "Control 2") ?? "Control 2";
        public static string Control3 => GetResourceString("Control3", "Control 3") ?? "Control 3";
        public static string EasierSearching => GetResourceString("EasierSearching", "Easier Searching") ?? "Easier Searching";
        public static string ControlPanel => GetResourceString("ControlPanel", "Control Panel") ?? "Control Panel";
        public static string FileManagement => GetResourceString("FileManagement", "File Management") ?? "File Management";
        public static string AddNewFile => GetResourceString("AddNewFile", "Add New File") ?? "Add New File";
        public static string ImportFile => GetResourceString("ImportFile", "Import File") ?? "Import File";
        public static string ExportFile => GetResourceString("ExportFile", "Export File") ?? "Export File";
        public static string DataManagement => GetResourceString("DataManagement", "Data Management") ?? "Data Management";
        public static string Backup => GetResourceString("Backup", "Backup") ?? "Backup";
        public static string Restore => GetResourceString("Restore", "Restore") ?? "Restore";
        public static string ClearHistory => GetResourceString("ClearHistory", "Clear History") ?? "Clear History";
        public static string Search => GetResourceString("Search", "Search") ?? "Search";
        public static string RecentFiles => GetResourceString("RecentFiles", "Recent Files") ?? "Recent Files";
        public static string FileOpened => GetResourceString("FileOpened", "File Opened") ?? "File Opened";
        public static string Select => GetResourceString("Select", "Select") ?? "Select";
        public static string ID => GetResourceString("ID", "ID") ?? "ID";
        public static string ResultCount => GetResourceString("ResultCount", "Result Count") ?? "Result Count";
        public static string Actions => GetResourceString("Actions", "Actions") ?? "Actions";
        public static string View => GetResourceString("View", "View") ?? "View";
        public static string Share => GetResourceString("Share", "Share") ?? "Share";
        
        // Sidebar Tooltips
        public static string CollapseSidebarTooltip => GetResourceString("CollapseSidebarTooltip", "Collapse/Expand Sidebar") ?? "Collapse/Expand Sidebar";
        public static string AddNewFileTooltip => GetResourceString("AddNewFileTooltip", "Create a new file") ?? "Create a new file";
        public static string ImportFileTooltip => GetResourceString("ImportFileTooltip", "Import an existing file") ?? "Import an existing file";
        public static string ExportFileTooltip => GetResourceString("ExportFileTooltip", "Export selected files") ?? "Export selected files";
        public static string BackupTooltip => GetResourceString("BackupTooltip", "Backup data") ?? "Backup data";
        public static string RestoreTooltip => GetResourceString("RestoreTooltip", "Restore data from backup") ?? "Restore data from backup";
        public static string ClearHistoryTooltip => GetResourceString("ClearHistoryTooltip", "Clear search history") ?? "Clear search history";
        public static string ArchiveSelectedTooltip => GetResourceString("ArchiveSelectedTooltip", "Archive selected items") ?? "Archive selected items";
        public static string CompressSelectedTooltip => GetResourceString("CompressSelectedTooltip", "Compress selected items") ?? "Compress selected items";
        public static string EncryptSelectedTooltip => GetResourceString("EncryptSelectedTooltip", "Encrypt selected items") ?? "Encrypt selected items";
        public static string SearchTooltip => GetResourceString("SearchTooltip", "Enter search term") ?? "Enter search term";
        public static string SearchButtonTooltip => GetResourceString("SearchButtonTooltip", "Execute search") ?? "Execute search";
        public static string EnableAutoBackupTooltip => GetResourceString("EnableAutoBackupTooltip", "Enable automatic backup every 30 minutes") ?? "Enable automatic backup every 30 minutes";
        public static string EnableNotificationsTooltip => GetResourceString("EnableNotificationsTooltip", "Enable application notifications") ?? "Enable application notifications";
        public static string EnableDarkModeTooltip => GetResourceString("EnableDarkModeTooltip", "Switch application theme to dark mode") ?? "Switch application theme to dark mode";
        public static string SettingsButtonTooltip => GetResourceString("SettingsButtonTooltip", "Open application settings") ?? "Open application settings";
        
        public static string AutomaticBackupEnabled => GetResourceString("AutomaticBackupEnabled", "Automatic backup has been enabled. Files will be backed up every 30 minutes.") ?? "Automatic backup has been enabled. Files will be backed up every 30 minutes.";
        public static string AutoBackupEnabledTitle => GetResourceString("AutoBackupEnabledTitle", "Auto Backup Enabled") ?? "Auto Backup Enabled";
        public static string AutomaticBackupDisabled => GetResourceString("AutomaticBackupDisabled", "Automatic backup has been disabled.") ?? "Automatic backup has been disabled.";
        public static string AutoBackupDisabledTitle => GetResourceString("AutoBackupDisabledTitle", "Auto Backup Disabled") ?? "Auto Backup Disabled";
        public static string NotificationsEnabled => GetResourceString("NotificationsEnabled", "Notifications have been enabled.") ?? "Notifications have been enabled.";
        public static string NotificationsEnabledTitle => GetResourceString("NotificationsEnabledTitle", "Notifications Enabled") ?? "Notifications Enabled";
        public static string NotificationsDisabled => GetResourceString("NotificationsDisabled", "Notifications have been disabled.") ?? "Notifications have been disabled.";
        public static string NotificationsDisabledTitle => GetResourceString("NotificationsDisabledTitle", "Notifications Disabled") ?? "Notifications Disabled";
        public static string DarkModeEnabled => GetResourceString("DarkModeEnabled", "Dark mode has been enabled.") ?? "Dark mode has been enabled.";
        public static string DarkModeEnabledTitle => GetResourceString("DarkModeEnabledTitle", "Dark Mode Enabled") ?? "Dark Mode Enabled";
        public static string LightModeEnabled => GetResourceString("LightModeEnabled", "Light mode has been enabled.") ?? "Light mode has been enabled.";
        public static string LightModeEnabledTitle => GetResourceString("LightModeEnabledTitle", "Light Mode Enabled") ?? "Light Mode Enabled";
        public static string CreateNewFile => GetResourceString("CreateNewFile", "Create New File") ?? "Create New File";
        public static string NewExcelFileCreated => GetResourceString("NewExcelFileCreated", "New Excel file created: {0}") ?? "New Excel file created: {0}";
        public static string NewCSVFileCreated => GetResourceString("NewCSVFileCreated", "New CSV file created: {0}") ?? "New CSV file created: {0}";
        public static string NewFileCreated => GetResourceString("NewFileCreated", "New file created: {0}") ?? "New file created: {0";
        public static string FileCreatedTitle => GetResourceString("FileCreatedTitle", "File Created") ?? "File Created";
        public static string ErrorCreatingNewFile => GetResourceString("ErrorCreatingNewFile", "Error creating new file: {0}") ?? "Error creating new file: {0}";
        public static string ErrorTitle => GetResourceString("ErrorTitle", "Error") ?? "Error";
        public static string ImportFileTitle => GetResourceString("ImportFileTitle", "Import File") ?? "Import File";
        public static string ConfirmImport => GetResourceString("ConfirmImport", "Are you sure you want to import {0} file(s)?") ?? "Are you sure you want to import {0} file(s)?";
        public static string ImportFilesTitle => GetResourceString("ImportFilesTitle", "Import Files") ?? "Import Files";
        public static string FilesImportedSuccessfully => GetResourceString("FilesImportedSuccessfully", "{0} file(s) imported successfully!") ?? "{0} file(s) imported successfully!";
        public static string ImportCompleteTitle => GetResourceString("ImportCompleteTitle", "Import Complete") ?? "Import Complete";
        public static string ErrorImportingFile => GetResourceString("ErrorImportingFile", "Error importing file: {0}") ?? "Error importing file: {0}";
        public static string NoItemsSelectedForExport => GetResourceString("NoItemsSelectedForExport", "No items selected for export. Please select items to export.") ?? "No items selected for export. Please select items to export.";
        public static string NoSelectionTitle => GetResourceString("NoSelectionTitle", "No Selection") ?? "No Selection";
        public static string ExportSelectedFiles => GetResourceString("ExportSelectedFiles", "Export Selected Files") ?? "Export Selected Files";
        public static string ConfirmExport => GetResourceString("ConfirmExport", "Export {0} item(s) to {1} format?") ?? "Export {0} item(s) to {1} format?";
        public static string ExportFilesTitle => GetResourceString("ExportFilesTitle", "Export Files") ?? "Export Files";
        public static string ItemsExportedSuccessfully => GetResourceString("ItemsExportedSuccessfully", "Exported {0} item(s) to {1} successfully!") ?? "Exported {0} item(s) to {1} successfully!";
        public static string ExportCompleteTitle => GetResourceString("ExportCompleteTitle", "Export Complete") ?? "Export Complete";
        public static string ErrorExportingFile => GetResourceString("ErrorExportingFile", "Error exporting file: {0}") ?? "Error exporting file: {0}";
        public static string ConfirmBackup => GetResourceString("ConfirmBackup", "Do you want to backup all data?\n\nSelect backup location:") ?? "Do you want to backup all data?\n\nSelect backup location:";
        public static string BackupDataTitle => GetResourceString("BackupDataTitle", "Backup Data") ?? "Backup Data";
        public static string BackupComplete => GetResourceString("BackupComplete", "Data backup completed successfully!") ?? "Data backup completed successfully!";
        public static string BackupCompleteTitle => GetResourceString("BackupCompleteTitle", "Backup Complete") ?? "Backup Complete";
        public static string ErrorDuringBackup => GetResourceString("ErrorDuringBackup", "Error during backup: {0}") ?? "Error during backup: {0}";
        public static string ConfirmRestore => GetResourceString("ConfirmRestore", "Do you want to restore data from backup? This will overwrite current data.\n\nSelect backup file to restore:") ?? "Do you want to restore data from backup? This will overwrite current data.\n\nSelect backup file to restore:";
        public static string RestoreDataTitle => GetResourceString("RestoreDataTitle", "Restore Data") ?? "Restore Data";
        public static string ConfirmAbsoluteRestore => GetResourceString("ConfirmAbsoluteRestore", "Are you absolutely sure you want to restore from '{0}'?\n\nTHIS WILL OVERWRITE ALL CURRENT DATA!") ?? "Are you absolutely sure you want to restore from '{0}'?\n\nTHIS WILL OVERWRITE ALL CURRENT DATA!";
        public static string ConfirmRestoreTitle => GetResourceString("ConfirmRestoreTitle", "CONFIRM RESTORE") ?? "CONFIRM RESTORE";
        public static string RestoreComplete => GetResourceString("RestoreComplete", "Data restore completed successfully!") ?? "Data restore completed successfully!";
        public static string RestoreCompleteTitle => GetResourceString("RestoreCompleteTitle", "Restore Complete") ?? "Restore Complete";
        public static string ErrorDuringRestore => GetResourceString("ErrorDuringRestore", "Error during restore: {0}") ?? "Error during restore: {0}";
        public static string ConfirmClearHistory => GetResourceString("ConfirmClearHistory", "Are you sure you want to clear all history? This action cannot be undone.") ?? "Are you sure you want to clear all history? This action cannot be undone.";
        public static string ClearHistoryTitle => GetResourceString("ClearHistoryTitle", "CLEAR HISTORY") ?? "CLEAR HISTORY";
        public static string HistoryClearedSuccessfully => GetResourceString("HistoryClearedSuccessfully", "History cleared successfully!") ?? "History cleared successfully!";
        public static string HistoryClearedTitle => GetResourceString("HistoryClearedTitle", "History Cleared") ?? "History Cleared";
        public static string ErrorClearingHistory => GetResourceString("ErrorClearingHistory", "Error clearing history: {0}") ?? "Error clearing history: {0}";
        public static string SettingsOptions => GetResourceString("SettingsOptions", "Settings Options:\n• Auto Backup: {0}\n• Notifications: {1}\n• Dark Mode: {2}") ?? "Settings Options:\n• Auto Backup: {0}\n• Notifications: {1}\n• Dark Mode: {2}";
        public static string SettingsTitle => GetResourceString("SettingsTitle", "Settings") ?? "Settings";
        public static string SearchCompleted => GetResourceString("SearchCompleted", "Search completed for: {0}") ?? "Search completed for: {0}";
        public static string SearchCompleteTitle => GetResourceString("SearchCompleteTitle", "Search Complete") ?? "Search Complete";
        public static string ErrorDuringSearch => GetResourceString("ErrorDuringSearch", "Error during search: {0}") ?? "Error during search: {0}";
        public static string NoItemsSelectedForArchiving => GetResourceString("NoItemsSelectedForArchiving", "No items selected for archiving.") ?? "No items selected for archiving.";
        public static string ConfirmArchive => GetResourceString("ConfirmArchive", "Archive {0} selected item(s)?\n\nArchived files will be moved to the archive folder.") ?? "Archive {0} selected item(s)?\n\nArchived files will be moved to the archive folder.";
        public static string ArchiveFilesTitle => GetResourceString("ArchiveFilesTitle", "Archive Files") ?? "Archive Files";
        public static string ItemsArchivedSuccessfully => GetResourceString("ItemsArchivedSuccessfully", "{0} item(s) archived successfully!") ?? "{0} item(s) archived successfully!";
        public static string ArchiveCompleteTitle => GetResourceString("ArchiveCompleteTitle", "Archive Complete") ?? "Archive Complete";
        public static string ErrorDuringArchiving => GetResourceString("ErrorDuringArchiving", "Error during archiving: {0}") ?? "Error during archiving: {0}";
        public static string NoItemsSelectedForCompression => GetResourceString("NoItemsSelectedForCompression", "No items selected for compression.") ?? "No items selected for compression.";
        public static string ConfirmCompression => GetResourceString("ConfirmCompression", "Compress {0} selected item(s)?\n\nCompressed files will be saved as ZIP archives.") ?? "Compress {0} selected item(s)?\n\nCompressed files will be saved as ZIP archives.";
        public static string CompressFilesTitle => GetResourceString("CompressFilesTitle", "Compress Files") ?? "Compress Files";
        public static string ItemsCompressedSuccessfully => GetResourceString("ItemsCompressedSuccessfully", "{0} item(s) compressed successfully!") ?? "{0} item(s) compressed successfully!";
        public static string CompressionCompleteTitle => GetResourceString("CompressionCompleteTitle", "Compression Complete") ?? "Compression Complete";
        public static string ErrorDuringCompression => GetResourceString("ErrorDuringCompression", "Error during compression: {0}") ?? "Error during compression: {0}";
        public static string NoItemsSelectedForEncryption => GetResourceString("NoItemsSelectedForEncryption", "No items selected for encryption.") ?? "No items selected for encryption.";
        public static string ConfirmEncryption => GetResourceString("ConfirmEncryption", "Encrypt {0} selected item(s)?\n\nWARNING: Encrypted files will require a password to open.\n\nTHIS ACTION CANNOT BE UNDONE WITHOUT THE PASSWORD!") ?? "Encrypt {0} selected item(s)?\n\nWARNING: Encrypted files will require a password to open.\n\nTHIS ACTION CANNOT BE UNDONE WITHOUT THE PASSWORD!";
        public static string EncryptFilesTitle => GetResourceString("EncryptFilesTitle", "ENCRYPT FILES") ?? "ENCRYPT FILES";
        public static string ItemsEncryptedSuccessfully => GetResourceString("ItemsEncryptedSuccessfully", "{0} item(s) encrypted successfully!") ?? "{0} item(s) encrypted successfully!";
        public static string EncryptionCompleteTitle => GetResourceString("EncryptionCompleteTitle", "Encryption Complete") ?? "Encryption Complete";
        public static string ErrorDuringEncryption => GetResourceString("ErrorDuringEncryption", "Error during encryption: {0}") ?? "Error during encryption: {0}";
        public static string NoItemsSelectedForDeletion => GetResourceString("NoItemsSelectedForDeletion", "No items selected for deletion.") ?? "No items selected for deletion.";
        public static string ConfirmDeletion => GetResourceString("ConfirmDeletion", "Are you sure you want to delete {0} selected item(s)?\n\nTHIS ACTION CANNOT BE UNDONE!") ?? "Are you sure you want to delete {0} selected item(s)?\n\nTHIS ACTION CANNOT BE UNDONE!";
        public static string ConfirmDeletionTitle => GetResourceString("ConfirmDeletionTitle", "CONFIRM DELETION") ?? "CONFIRM DELETION";
        public static string ItemsDeletedSuccessfully => GetResourceString("ItemsDeletedSuccessfully", "{0} item(s) deleted successfully!") ?? "{0} item(s) deleted successfully!";
        public static string DeletionCompleteTitle => GetResourceString("DeletionCompleteTitle", "Deletion Complete") ?? "Deletion Complete";
        public static string ErrorDeletingItems => GetResourceString("ErrorDeletingItems", "Error deleting items: {0}") ?? "Error deleting items: {0";
        public static string NoItemsSelectedForEditing => GetResourceString("NoItemsSelectedForEditing", "No items selected for editing.") ?? "No items selected for editing.";
        public static string SelectOnlyOneItem => GetResourceString("SelectOnlyOneItem", "Please select only one item to edit.") ?? "Please select only one item to edit.";
        public static string MultipleSelectionTitle => GetResourceString("MultipleSelectionTitle", "Multiple Selection") ?? "Multiple Selection";
        public static string EditingItem => GetResourceString("EditingItem", "Editing item: {0}") ?? "Editing item: {0}";
        public static string EditItemTitle => GetResourceString("EditItemTitle", "Edit Item") ?? "Edit Item";
        public static string ErrorEditingItems => GetResourceString("ErrorEditingItems", "Error editing items: {0}") ?? "Error editing items: {0}";
        public static string ItemsCopiedToClipboard => GetResourceString("ItemsCopiedToClipboard", "{0} item(s) copied to clipboard!") ?? "{0} item(s) copied to clipboard!";
        public static string CopyCompleteTitle => GetResourceString("CopyCompleteTitle", "Copy Complete") ?? "Copy Complete";
        public static string ErrorCopyingItems => GetResourceString("ErrorCopyingItems", "Error copying items: {0}") ?? "Error copying items: {0}";
        public static string ConfirmExportToExcel => GetResourceString("ConfirmExportToExcel", "Export {0} item(s) to Excel format?") ?? "Export {0} item(s) to Excel format?";
        public static string ExportToExcelTitle => GetResourceString("ExportToExcelTitle", "Export to Excel") ?? "Export to Excel";
        public static string ItemsExportedToExcel => GetResourceString("ItemsExportedToExcel", "Exported {0} item(s) to Excel successfully!") ?? "Exported {0} item(s) to Excel successfully!";
        public static string ErrorExportingToExcel => GetResourceString("ErrorExportingToExcel", "Error exporting to Excel: {0}") ?? "Error exporting to Excel: {0}";
        public static string ConfirmExportToPDF => GetResourceString("ConfirmExportToPDF", "Export {0} item(s) to PDF format?") ?? "Export {0} item(s) to PDF format?";
        public static string ExportToPDFTitle => GetResourceString("ExportToPDFTitle", "Export to PDF") ?? "Export to PDF";
        public static string ItemsExportedToPDF => GetResourceString("ItemsExportedToPDF", "Exported {0} item(s) to PDF successfully!") ?? "Exported {0} item(s) to PDF successfully!";
        public static string ErrorExportingToPDF => GetResourceString("ErrorExportingToPDF", "Error exporting to PDF: {0}") ?? "Error exporting to PDF: {0}";
        public static string NoItemsSelectedForBackup => GetResourceString("NoItemsSelectedForBackup", "No items selected for backup.") ?? "No items selected for backup.";
        public static string ConfirmBackupSelected => GetResourceString("ConfirmBackupSelected", "Backup {0} selected item(s)?") ?? "Backup {0} selected item(s)?";
        public static string BackupSelectedItemsTitle => GetResourceString("BackupSelectedItemsTitle", "Backup Selected Items") ?? "Backup Selected Items";
        public static string BackupOfItemsCompleted => GetResourceString("BackupOfItemsCompleted", "Backup of {0} item(s) completed successfully!") ?? "Backup of {0} item(s) completed successfully!";
        public static string ErrorDuringBackupOfItems => GetResourceString("ErrorDuringBackupOfItems", "Error during backup: {0}") ?? "Error during backup: {0}";
        public static string NoItemsSelectedForSharing => GetResourceString("NoItemsSelectedForSharing", "No items selected for sharing.") ?? "No items selected for sharing.";
        public static string ConfirmShare => GetResourceString("ConfirmShare", "Share {0} selected item(s)?") ?? "Share {0} selected item(s)?";
        public static string ShareItemsTitle => GetResourceString("ShareItemsTitle", "Share Items") ?? "Share Items";
        public static string SharingItemsInitiated => GetResourceString("SharingItemsInitiated", "Sharing {0} item(s) initiated!") ?? "Sharing {0} item(s) initiated!";
        public static string SharingStartedTitle => GetResourceString("SharingStartedTitle", "Sharing Started") ?? "Sharing Started";
        public static string ErrorDuringSharing => GetResourceString("ErrorDuringSharing", "Error during sharing: {0}") ?? "Error during sharing: {0}";
        public static string ViewItemDetails => GetResourceString("ViewItemDetails", "Viewing item: {0}\nSearch Term: {1}\nResults: {2}") ?? "Viewing item: {0}\nSearch Term: {1}\nResults: {2}";
        public static string ItemDetailsTitle => GetResourceString("ItemDetailsTitle", "Item Details") ?? "Item Details";
        public static string ErrorViewItem => GetResourceString("ErrorViewItem", "Error viewing item: {0}") ?? "Error viewing item: {0}";
        public static string ConfirmDeleteItem => GetResourceString("ConfirmDeleteItem", "Are you sure you want to delete '{0}'?\n\nTHIS ACTION CANNOT BE UNDONE!") ?? "Are you sure you want to delete '{0}'?\n\nTHIS ACTION CANNOT BE UNDONE!";
        public static string ItemSelected => GetResourceString("ItemSelected", "All items selected") ?? "All items selected";
        public static string ItemsUnselected => GetResourceString("ItemsUnselected", "All items unselected") ?? "All items unselected";
        public static string ErrorTogglingSelection => GetResourceString("ErrorTogglingSelection", "Error toggling selection: {0}") ?? "Error toggling selection: {0}";
        public static string SelectionUpdatedTitle => GetResourceString("SelectionUpdatedTitle", "Selection Updated") ?? "Selection Updated";
        public static string ArchivedFilesFeatureNotImplemented => GetResourceString("ArchivedFilesFeatureNotImplemented", "The archived files feature is not yet implemented in the sidebar. Please use the 'Archived Files' button in the info bar.") ?? "The archived files feature is not yet implemented in the sidebar. Please use the 'Archived Files' button in the info bar.";
        public static string FeatureNotImplementedTitle => GetResourceString("FeatureNotImplementedTitle", "Feature Not Implemented") ?? "Feature Not Implemented";
        
        // Additional strings for Arabization
        public static string SelectFileToLoad => GetResourceString("SelectFileToLoad", "Select a file to load") ?? "Select a file to load";
        public static string FileInsertedSuccessfully => GetResourceString("FileInsertedSuccessfully", "File inserted successfully") ?? "File inserted successfully";
        public static string Success => GetResourceString("Success", "Success") ?? "Success";
        public static string ErrorLoadingFile => GetResourceString("ErrorLoadingFile", "Error loading file: {0}") ?? "Error loading file: {0}";
        public static string FileLoadError => GetResourceString("FileLoadError", "File Load Error") ?? "File Load Error";
        public static string FileDeletedSuccessfully => GetResourceString("FileDeletedSuccessfully", "File deleted successfully") ?? "File deleted successfully";
        public static string FileDeleted => GetResourceString("FileDeleted", "File Deleted") ?? "File Deleted";
        public static string SettingsWindowOpen => GetResourceString("SettingsWindowOpen", "Settings window would open here") ?? "Settings window would open here";
        public static string DarkModeDisabled => GetResourceString("DarkModeDisabled", "Dark mode disabled") ?? "Dark mode disabled";
        public static string ThemeChanged => GetResourceString("ThemeChanged", "Theme Changed") ?? "Theme Changed";
        public static string DataStoredSuccessfully => GetResourceString("DataStoredSuccessfully", "Data stored successfully") ?? "Data stored successfully";
        public static string DataStored => GetResourceString("DataStored", "Data Stored") ?? "Data Stored";
        public static string NoFileLoaded => GetResourceString("NoFileLoaded", "No file loaded. Please insert a file first") ?? "No file loaded. Please insert a file first";
        public static string SearchError => GetResourceString("SearchError", "Search Error") ?? "Search Error";
        
        // Strings for InfoBarViewModel
        public static string NoFileSelected => GetResourceString("NoFileSelected", "No file selected") ?? "No file selected";
        public static string ZeroKB => GetResourceString("ZeroKB", "0 KB") ?? "0 KB";
    }
}