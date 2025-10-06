#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Fouad.Properties
{
    /// <summary>
    /// Provides localized error messages for the application.
    /// </summary>
    public static class ErrorMessages
    {
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
        
        // Database Service Error Messages
        public static string FileNotFound => GetResourceString("FileNotFound", "File not found: {0}") ?? "File not found: {0}";
        public static string UnsupportedFileType => GetResourceString("UnsupportedFileType", "Unsupported file type. Only Excel files (.xlsx or .ls) are supported") ?? "Unsupported file type. Only Excel files (.xlsx or .ls) are supported";
        public static string FailedToCreateArchiveFolder => GetResourceString("FailedToCreateArchiveFolder", "Failed to create archive folder: {0}") ?? "Failed to create archive folder: {0}";
        public static string ErrorImportingFile => GetResourceString("ErrorImportingFile", "Error importing file: {0}") ?? "Error importing file: {0}";
        public static string ErrorIndexingFileContent => GetResourceString("ErrorIndexingFileContent", "Error indexing file content: {0}") ?? "Error indexing file content: {0}";
        public static string ErrorRetrievingIndexedContent => GetResourceString("ErrorRetrievingIndexedContent", "Error retrieving indexed content: {0}") ?? "Error retrieving indexed content: {0}";
        public static string ErrorRetrievingArchiveLog => GetResourceString("ErrorRetrievingArchiveLog", "Error retrieving archive log: {0}") ?? "Error retrieving archive log: {0}";
        public static string ErrorRetrievingArchiveLogs => GetResourceString("ErrorRetrievingArchiveLogs", "Error retrieving archive logs: {0}") ?? "Error retrieving archive logs: {0}";
        public static string ErrorRetrievingMasterRecord => GetResourceString("ErrorRetrievingMasterRecord", "Error retrieving master record: {0}") ?? "Error retrieving master record: {0}";
        public static string ErrorAddingToMasterData => GetResourceString("ErrorAddingToMasterData", "Error adding to master data: {0}") ?? "Error adding to master data: {0}";
        public static string ErrorUpdatingMasterData => GetResourceString("ErrorUpdatingMasterData", "Error updating master data: {0}") ?? "Error updating master data: {0}";
        public static string ErrorDeletingIndexedContent => GetResourceString("ErrorDeletingIndexedContent", "Error deleting indexed content: {0}") ?? "Error deleting indexed content: {0}";
        public static string DatabaseOperationFailed => GetResourceString("DatabaseOperationFailed", "Database operation failed after {0} attempts. Last error: {1}") ?? "Database operation failed after {0} attempts. Last error: {1}";
        public static string DatabaseLocked => GetResourceString("DatabaseLocked", "Database is currently locked. Please try again in a moment.") ?? "Database is currently locked. Please try again in a moment.";
        public static string DatabaseBusy => GetResourceString("DatabaseBusy", "Database is busy processing another request. Please wait and try again.") ?? "Database is busy processing another request. Please wait and try again.";
        public static string DiskFull => GetResourceString("DiskFull", "Insufficient disk space to complete the operation.") ?? "Insufficient disk space to complete the operation.";
        public static string FileAccessError => GetResourceString("FileAccessError", "Unable to access file: {0}. The file may be in use by another process.") ?? "Unable to access file: {0}. The file may be in use by another process.";
        public static string NetworkError => GetResourceString("NetworkError", "Network error occurred while accessing database: {0}") ?? "Network error occurred while accessing database: {0}";
        public static string TimeoutError => GetResourceString("TimeoutError", "Operation timed out. Please try again.") ?? "Operation timed out. Please try again.";
        
        // File Data Service Error Messages
        public static string ErrorLoadingFile => GetResourceString("ErrorLoadingFile", "Error loading file: {0}") ?? "Error loading file: {0}";
        public static string ErrorLoadingFromCache => GetResourceString("ErrorLoadingFromCache", "Error loading from cache: {0}") ?? "Error loading from cache: {0}";
        public static string ErrorSavingToCache => GetResourceString("ErrorSavingToCache", "Error saving to cache: {0}") ?? "Error saving to cache: {0}";
        public static string ErrorLoadingColumnHeaders => GetResourceString("ErrorLoadingColumnHeaders", "Error loading column headers: {0}") ?? "Error loading column headers: {0}";
        public static string ErrorLoadingExcelData => GetResourceString("ErrorLoadingExcelData", "Error loading Excel data: {0}") ?? "Error loading Excel data: {0}";
        public static string ErrorLoadingCsvData => GetResourceString("ErrorLoadingCsvData", "Error loading CSV data: {0}") ?? "Error loading CSV data: {0}";
        public static string ErrorCreatingIndex => GetResourceString("ErrorCreatingIndex", "Error creating index: {0}") ?? "Error creating index: {0}";
        public static string FileProcessingFailed => GetResourceString("FileProcessingFailed", "File processing failed after {0} attempts. Last error: {1}") ?? "File processing failed after {0} attempts. Last error: {1}";
        
        // Search Service Error Messages
        public static string ErrorPerformingSearch => GetResourceString("ErrorPerformingSearch", "Error performing search: {0}") ?? "Error performing search: {0}";
        public static string ErrorGettingSearchSuggestions => GetResourceString("ErrorGettingSearchSuggestions", "Error getting search suggestions: {0}") ?? "Error getting search suggestions: {0}";
        public static string SearchOperationFailed => GetResourceString("SearchOperationFailed", "Search operation failed after {0} attempts. Last error: {1}") ?? "Search operation failed after {0} attempts. Last error: {1}";
        
        // General Error Messages
        public static string OperationFailed => GetResourceString("OperationFailed", "Operation failed: {0}") ?? "Operation failed: {0}";
        public static string RetryAttempt => GetResourceString("RetryAttempt", "Attempt {0} failed. Retrying in {1}ms...") ?? "Attempt {0} failed. Retrying in {1}ms...";
        public static string MaxRetriesExceeded => GetResourceString("MaxRetriesExceeded", "Maximum retry attempts ({0}) exceeded. Operation failed.") ?? "Maximum retry attempts ({0}) exceeded. Operation failed.";
        public static string RecoverySuggestionDatabaseLocked => GetResourceString("RecoverySuggestionDatabaseLocked", "Recovery suggestion: Close any other applications using the database and try again.") ?? "Recovery suggestion: Close any other applications using the database and try again.";
        public static string RecoverySuggestionDiskFull => GetResourceString("RecoverySuggestionDiskFull", "Recovery suggestion: Free up disk space and try again.") ?? "Recovery suggestion: Free up disk space and try again.";
        public static string RecoverySuggestionFileInUse => GetResourceString("RecoverySuggestionFileInUse", "Recovery suggestion: Close any applications using the file and try again.") ?? "Recovery suggestion: Close any applications using the file and try again.";
        public static string RecoverySuggestionNetwork => GetResourceString("RecoverySuggestionNetwork", "Recovery suggestion: Check your network connection and try again.") ?? "Recovery suggestion: Check your network connection and try again.";
        public static string RecoverySuggestionTimeout => GetResourceString("RecoverySuggestionTimeout", "Recovery suggestion: Try again. If the problem persists, restart the application.") ?? "Recovery suggestion: Try again. If the problem persists, restart the application.";
    }
}