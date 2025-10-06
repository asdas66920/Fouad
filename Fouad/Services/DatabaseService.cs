using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fouad.Models;
using OfficeOpenXml;
using Fouad.Utilities;
using Fouad.Properties;

namespace Fouad.Services
{
    /// <summary>
    /// Service for handling database operations including file import, indexing, and storage.
    /// </summary>
    public class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString;
        private readonly string _archiveFolderPath;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseService"/> class.
        /// </summary>
        /// <param name="databasePath">Path to the SQLite database file.</param>
        public DatabaseService(string databasePath)
        {
            LoggingService.LogInfo($"Initializing DatabaseService with path: {databasePath}");
            _connectionString = $"Data Source={databasePath};";
            _archiveFolderPath = Path.Combine(Path.GetDirectoryName(databasePath) ?? "", "Excel_Archive");
            
            // Ensure archive folder exists
            LoggingService.LogInfo($"Ensuring archive folder exists: {_archiveFolderPath}");
            try
            {
                // Make sure the parent directory exists first
                var parentDirectory = Path.GetDirectoryName(databasePath);
                if (!string.IsNullOrEmpty(parentDirectory) && !Directory.Exists(parentDirectory))
                {
                    LoggingService.LogInfo($"Creating parent directory: {parentDirectory}");
                    Directory.CreateDirectory(parentDirectory);
                }
                
                if (!Directory.Exists(_archiveFolderPath))
                {
                    LoggingService.LogInfo("Creating archive folder");
                    Directory.CreateDirectory(_archiveFolderPath);
                }
                
                LoggingService.LogInfo($"Archive folder ready: {_archiveFolderPath}");
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Error creating archive folder: {_archiveFolderPath}", ex);
                throw new InvalidOperationException($"Failed to create archive folder: {_archiveFolderPath}", ex);
            }
            
            // Initialize database
            LoggingService.LogInfo("Initializing database");
            InitializeDatabase();
            LoggingService.LogInfo("DatabaseService initialized successfully");
        }

        /// <summary>
        /// Initializes the database with required tables.
        /// </summary>
        private void InitializeDatabase()
        {
            try
            {
                LoggingService.LogInfo("Starting database initialization");
                using var connection = new SqliteConnection(_connectionString);
                LoggingService.LogInfo("Opening database connection");
                connection.Open();
                LoggingService.LogInfo("Database connection opened successfully");

                // Create Archive_Log table
                string createArchiveLogTable = @"
                    CREATE TABLE IF NOT EXISTS Archive_Log (
                        ArchiveId INTEGER PRIMARY KEY AUTOINCREMENT,
                        FileName TEXT NOT NULL,
                        UploadDate DATETIME NOT NULL,
                        UploadedBy TEXT NOT NULL,
                        FilePath TEXT NOT NULL
                    );";
                
                LoggingService.LogInfo("Creating Archive_Log table");
                using var command = new SqliteCommand(createArchiveLogTable, connection);
                command.ExecuteNonQuery();
                LoggingService.LogInfo("Archive_Log table created successfully");

                // Create Content_Index table
                string createContentIndexTable = @"
                    CREATE TABLE IF NOT EXISTS Content_Index (
                        ContentIndexId INTEGER PRIMARY KEY AUTOINCREMENT,
                        ArchiveId INTEGER NOT NULL,
                        SheetName TEXT,
                        ColumnName TEXT,
                        RowNumber INTEGER NOT NULL,
                        CellValue TEXT,
                        FOREIGN KEY (ArchiveId) REFERENCES Archive_Log(ArchiveId)
                    );";
                
                LoggingService.LogInfo("Creating Content_Index table");
                command.CommandText = createContentIndexTable;
                command.ExecuteNonQuery();
                LoggingService.LogInfo("Content_Index table created successfully");
                
                // Create Master_Data table
                string createMasterDataTable = @"
                    CREATE TABLE IF NOT EXISTS Master_Data (
                        MasterId INTEGER PRIMARY KEY AUTOINCREMENT,
                        UniqueKey TEXT UNIQUE NOT NULL,
                        Data JSON NOT NULL,
                        CreatedDate DATETIME NOT NULL,
                        LastUpdated DATETIME NOT NULL
                    );";
                
                LoggingService.LogInfo("Creating Master_Data table");
                command.CommandText = createMasterDataTable;
                command.ExecuteNonQuery();
                LoggingService.LogInfo("Master_Data table created successfully");
                
                LoggingService.LogInfo("Database initialized successfully");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error initializing database", ex);
                throw;
            }
        }

        /// <summary>
        /// Executes a database operation with proper connection management.
        /// </summary>
        /// <param name="operation">The operation to execute.</param>
        private async Task ExecuteDatabaseOperationAsync(Func<SqliteConnection, Task> operation)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            await operation(connection);
            // Connection is automatically closed when disposed
        }

        /// <summary>
        /// Executes a database operation with proper connection management and returns a result.
        /// </summary>
        /// <param name="operation">The operation to execute.</param>
        private async Task<T> ExecuteDatabaseOperationAsync<T>(Func<SqliteConnection, Task<T>> operation)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            var result = await operation(connection);
            // Connection is automatically closed when disposed
            return result;
        }

        /// <summary>
        /// Imports a file and stores its metadata in the Archive_Log table.
        /// </summary>
        /// <param name="filePath">Path to the file to import.</param>
        /// <param name="uploadedBy">Username of the person uploading the file.</param>
        /// <returns>The ArchiveId of the imported file.</returns>
        public async Task<int> ImportFileAsync(string filePath, string uploadedBy)
        {
            return await RetryHelper.ExecuteWithRetryAsync(
                async () =>
                {
                    try
                    {
                        // Step 1: Initial Verification
                        if (!File.Exists(filePath))
                        {
                            throw new FileNotFoundException(string.Format(ErrorMessages.FileNotFound, filePath), filePath);
                        }

                        var fileInfo = new FileInfo(filePath);
                        var extension = fileInfo.Extension.ToLower();
                        
                        if (extension != ".xlsx" && extension != ".xls")
                        {
                            throw new InvalidOperationException(ErrorMessages.UnsupportedFileType);
                        }

                        // Step 2: Archiving and Storing Metadata
                        // File Archive: Create a copy of the original file and save it in the archive folder
                        var archiveFileName = $"{Guid.NewGuid()}_{fileInfo.Name}";
                        var archiveFilePath = Path.Combine(_archiveFolderPath, archiveFileName);
                        
                        // Ensure the archive folder exists before copying
                        if (!Directory.Exists(_archiveFolderPath))
                        {
                            LoggingService.LogWarning($"Archive folder does not exist, attempting to create: {_archiveFolderPath}");
                            // Make sure all parent directories exist
                            try
                            {
                                Directory.CreateDirectory(_archiveFolderPath);
                                LoggingService.LogInfo($"Created archive folder and all necessary parent directories: {_archiveFolderPath}");
                            }
                            catch (Exception ex)
                            {
                                LoggingService.LogError($"Failed to create archive folder: {_archiveFolderPath}", ex);
                                throw new InvalidOperationException(string.Format(ErrorMessages.FailedToCreateArchiveFolder, _archiveFolderPath), ex);
                            }
                        }
                        
                        // Verify that the archive folder exists
                        if (!Directory.Exists(_archiveFolderPath))
                        {
                            LoggingService.LogError($"Archive folder does not exist after creation attempt: {_archiveFolderPath}", null);
                            throw new DirectoryNotFoundException(string.Format(ErrorMessages.FailedToCreateArchiveFolder, _archiveFolderPath));
                        }
                        
                        LoggingService.LogInfo($"Copying file from {filePath} to {archiveFilePath}");
                        LoggingService.LogInfo($"Archive folder exists: {Directory.Exists(_archiveFolderPath)}");
                        File.Copy(filePath, archiveFilePath, true);

                        // Archive Log: Add a new record in the Archive_Log table
                        return await ExecuteDatabaseOperationAsync<int>(async (connection) =>
                        {
                            string insertArchiveLog = @"
                                INSERT INTO Archive_Log (FileName, UploadDate, UploadedBy, FilePath)
                                VALUES (@FileName, @UploadDate, @UploadedBy, @FilePath);
                                SELECT last_insert_rowid();";

                            using var command = new SqliteCommand(insertArchiveLog, connection);
                            command.Parameters.AddWithValue("@FileName", fileInfo.Name);
                            command.Parameters.AddWithValue("@UploadDate", DateTime.Now);
                            command.Parameters.AddWithValue("@UploadedBy", uploadedBy);
                            command.Parameters.AddWithValue("@FilePath", archiveFilePath);

                            var result = await command.ExecuteScalarAsync();
                            var archiveId = Convert.ToInt32(result);
                            
                            LoggingService.LogInfo($"File imported successfully with ArchiveId: {archiveId}");
                            return archiveId;
                        });
                    }
                    catch (Exception ex)
                    {
                        LoggingService.LogError("Error importing file", ex);
                        throw new InvalidOperationException(string.Format(ErrorMessages.ErrorImportingFile, ex.Message), ex);
                    }
                },
                maxRetries: 3,
                delayMilliseconds: 1000,
                isTransientException: IsTransientException);
        }

        /// <summary>
        /// Indexes the content of an Excel file and stores it in the Content_Index table.
        /// </summary>
        /// <param name="filePath">Path to the Excel file to index.</param>
        /// <param name="archiveId">The ArchiveId of the file.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task IndexFileContentAsync(string filePath, int archiveId)
        {
            await RetryHelper.ExecuteWithRetryAsync(
                async () =>
                {
                    try
                    {
                        // Step 3: Read and Index the Content
                        using var package = new ExcelPackage(new FileInfo(filePath));
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        
                        if (worksheet == null)
                        {
                            LoggingService.LogWarning("No worksheets found in Excel file");
                            return;
                        }

                        var rowCount = worksheet.Dimension?.Rows ?? 0;
                        var colCount = worksheet.Dimension?.Columns ?? 0;
                        
                        await ExecuteDatabaseOperationAsync(async (connection) =>
                        {
                            using var transaction = connection.BeginTransaction();
                            
                            try
                            {
                                for (int row = 1; row <= rowCount; row++)
                                {
                                    for (int col = 1; col <= colCount; col++)
                                    {
                                        var cellValue = worksheet.Cells[row, col].Text;
                                        var columnName = col <= colCount ? 
                                            worksheet.Cells[1, col].Text : $"Column {col}";

                                        string insertContentIndex = @"
                                            INSERT INTO Content_Index (ArchiveId, SheetName, ColumnName, RowNumber, CellValue)
                                            VALUES (@ArchiveId, @SheetName, @ColumnName, @RowNumber, @CellValue);";

                                        using var command = new SqliteCommand(insertContentIndex, connection, transaction);
                                        command.Parameters.AddWithValue("@ArchiveId", archiveId);
                                        command.Parameters.AddWithValue("@SheetName", worksheet.Name);
                                        command.Parameters.AddWithValue("@ColumnName", columnName);
                                        command.Parameters.AddWithValue("@RowNumber", row);
                                        command.Parameters.AddWithValue("@CellValue", cellValue ?? "");
                                        
                                        await command.ExecuteNonQueryAsync();
                                    }
                                }
                                
                                await transaction.CommitAsync();
                                LoggingService.LogInfo($"File content indexed successfully for ArchiveId: {archiveId}");
                            }
                            catch
                            {
                                await transaction.RollbackAsync();
                                throw;
                            }
                            return Task.CompletedTask;
                        });
                    }
                    catch (Exception ex)
                    {
                        LoggingService.LogError("Error indexing file content", ex);
                        throw new InvalidOperationException(string.Format(ErrorMessages.ErrorIndexingFileContent, ex.Message), ex);
                    }
                },
                maxRetries: 3,
                delayMilliseconds: 1000,
                isTransientException: IsTransientException);
        }

        /// <summary>
        /// Gets all indexed content for a specific archive.
        /// </summary>
        /// <param name="archiveId">The ArchiveId to retrieve content for.</param>
        /// <returns>List of ContentIndex entries.</returns>
        public async Task<List<ContentIndex>> GetIndexedContentAsync(int archiveId)
        {
            return await RetryHelper.ExecuteWithRetryAsync(
                async () =>
                {
                    try
                    {
                        return await ExecuteDatabaseOperationAsync<List<ContentIndex>>(async (connection) =>
                        {
                            var contentList = new List<ContentIndex>();

                            string selectContent = @"
                                SELECT ContentIndexId, ArchiveId, SheetName, ColumnName, RowNumber, CellValue
                                FROM Content_Index
                                WHERE ArchiveId = @ArchiveId
                                ORDER BY RowNumber, ColumnName;";

                            using var command = new SqliteCommand(selectContent, connection);
                            command.Parameters.AddWithValue("@ArchiveId", archiveId);

                            using var reader = await command.ExecuteReaderAsync();
                            while (await reader.ReadAsync())
                            {
                                contentList.Add(new ContentIndex
                                {
                                    ContentIndexId = reader.GetInt32("ContentIndexId"),
                                    ArchiveId = reader.GetInt32("ArchiveId"),
                                    SheetName = reader.IsDBNull("SheetName") ? null : reader.GetString("SheetName"),
                                    ColumnName = reader.IsDBNull("ColumnName") ? null : reader.GetString("ColumnName"),
                                    RowNumber = reader.GetInt32("RowNumber"),
                                    CellValue = reader.IsDBNull("CellValue") ? null : reader.GetString("CellValue")
                                });
                            }
                            
                            return contentList;
                        });
                    }
                    catch (Exception ex)
                    {
                        LoggingService.LogError("Error retrieving indexed content", ex);
                        throw new InvalidOperationException(string.Format(ErrorMessages.ErrorRetrievingIndexedContent, ex.Message), ex);
                    }
                },
                maxRetries: 3,
                delayMilliseconds: 500,
                isTransientException: IsTransientException);
        }

        /// <summary>
        /// Gets archive log entry by ID.
        /// </summary>
        /// <param name="archiveId">The ArchiveId to retrieve.</param>
        /// <returns>ArchiveLog entry or null if not found.</returns>
        public async Task<ArchiveLog?> GetArchiveLogAsync(int archiveId)
        {
            return await RetryHelper.ExecuteWithRetryAsync(
                async () =>
                {
                    try
                    {
                        return await ExecuteDatabaseOperationAsync<ArchiveLog?>(async (connection) =>
                        {
                            string selectArchive = @"
                                SELECT ArchiveId, FileName, UploadDate, UploadedBy, FilePath
                                FROM Archive_Log
                                WHERE ArchiveId = @ArchiveId;";

                            using var command = new SqliteCommand(selectArchive, connection);
                            command.Parameters.AddWithValue("@ArchiveId", archiveId);

                            using var reader = await command.ExecuteReaderAsync();
                            if (await reader.ReadAsync())
                            {
                                return new ArchiveLog
                                {
                                    ArchiveId = reader.GetInt32("ArchiveId"),
                                    FileName = reader.GetString("FileName"),
                                    UploadDate = reader.GetDateTime("UploadDate"),
                                    UploadedBy = reader.GetString("UploadedBy"),
                                    FilePath = reader.GetString("FilePath")
                                };
                            }
                            
                            return null;
                        });
                    }
                    catch (Exception ex)
                    {
                        LoggingService.LogError("Error retrieving archive log", ex);
                        throw new InvalidOperationException(string.Format(ErrorMessages.ErrorRetrievingArchiveLog, ex.Message), ex);
                    }
                },
                maxRetries: 3,
                delayMilliseconds: 500,
                isTransientException: IsTransientException);
        }

        /// <summary>
        /// Gets all archive logs.
        /// </summary>
        /// <returns>List of ArchiveLog entries.</returns>
        public async Task<List<ArchiveLog>> GetAllArchiveLogsAsync()
        {
            return await RetryHelper.ExecuteWithRetryAsync(
                async () =>
                {
                    try
                    {
                        return await ExecuteDatabaseOperationAsync<List<ArchiveLog>>(async (connection) =>
                        {
                            var archiveLogs = new List<ArchiveLog>();

                            string selectArchives = @"
                                SELECT ArchiveId, FileName, UploadDate, UploadedBy, FilePath
                                FROM Archive_Log
                                ORDER BY UploadDate DESC;";

                            using var command = new SqliteCommand(selectArchives, connection);

                            using var reader = await command.ExecuteReaderAsync();
                            while (await reader.ReadAsync())
                            {
                                archiveLogs.Add(new ArchiveLog
                                {
                                    ArchiveId = reader.GetInt32("ArchiveId"),
                                    FileName = reader.GetString("FileName"),
                                    UploadDate = reader.GetDateTime("UploadDate"),
                                    UploadedBy = reader.GetString("UploadedBy"),
                                    FilePath = reader.GetString("FilePath")
                                });
                            }
                            
                            return archiveLogs;
                        });
                    }
                    catch (Exception ex)
                    {
                        LoggingService.LogError("Error retrieving archive logs", ex);
                        throw new InvalidOperationException(string.Format(ErrorMessages.ErrorRetrievingArchiveLogs, ex.Message), ex);
                    }
                },
                maxRetries: 3,
                delayMilliseconds: 500,
                isTransientException: IsTransientException);
        }

        /// <summary>
        /// Gets a master record by unique key.
        /// </summary>
        /// <param name="uniqueKey">The unique key to search for.</param>
        /// <returns>Master record data or null if not found.</returns>
        public async Task<(int MasterId, string UniqueKey, string Data, DateTime CreatedDate, DateTime LastUpdated)?> GetMasterRecordAsync(string uniqueKey)
        {
            return await RetryHelper.ExecuteWithRetryAsync(
                async () =>
                {
                    try
                    {
                        return await ExecuteDatabaseOperationAsync<(int MasterId, string UniqueKey, string Data, DateTime CreatedDate, DateTime LastUpdated)?>(async (connection) =>
                        {
                            string selectMasterData = @"
                                SELECT MasterId, UniqueKey, Data, CreatedDate, LastUpdated
                                FROM Master_Data
                                WHERE UniqueKey = @UniqueKey;";

                            using var command = new SqliteCommand(selectMasterData, connection);
                            command.Parameters.AddWithValue("@UniqueKey", uniqueKey);

                            using var reader = await command.ExecuteReaderAsync();
                            if (await reader.ReadAsync())
                            {
                                return (
                                    reader.GetInt32("MasterId"),
                                    reader.GetString("UniqueKey"),
                                    reader.GetString("Data"),
                                    reader.GetDateTime("CreatedDate"),
                                    reader.GetDateTime("LastUpdated")
                                );
                            }
                            
                            return null;
                        });
                    }
                    catch (Exception ex)
                    {
                        LoggingService.LogError("Error retrieving master record", ex);
                        throw new InvalidOperationException(string.Format(ErrorMessages.ErrorRetrievingMasterRecord, ex.Message), ex);
                    }
                },
                maxRetries: 3,
                delayMilliseconds: 500,
                isTransientException: IsTransientException);
        }

        /// <summary>
        /// Adds a new record to the master data table.
        /// </summary>
        /// <param name="uniqueKey">The unique key for the record.</param>
        /// <param name="data">The data to store.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task AddToMasterDataAsync(string uniqueKey, string data)
        {
            await RetryHelper.ExecuteWithRetryAsync(
                async () =>
                {
                    try
                    {
                        await ExecuteDatabaseOperationAsync(async (connection) =>
                        {
                            string insertMasterData = @"
                                INSERT INTO Master_Data (UniqueKey, Data, CreatedDate, LastUpdated)
                                VALUES (@UniqueKey, @Data, @CreatedDate, @LastUpdated);";

                            using var command = new SqliteCommand(insertMasterData, connection);
                            command.Parameters.AddWithValue("@UniqueKey", uniqueKey);
                            command.Parameters.AddWithValue("@Data", data);
                            command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                            command.Parameters.AddWithValue("@LastUpdated", DateTime.Now);

                            await command.ExecuteNonQueryAsync();
                            return Task.CompletedTask;
                        });
                    }
                    catch (Exception ex)
                    {
                        LoggingService.LogError("Error adding to master data", ex);
                        throw new InvalidOperationException(string.Format(ErrorMessages.ErrorAddingToMasterData, ex.Message), ex);
                    }
                },
                maxRetries: 3,
                delayMilliseconds: 1000,
                isTransientException: IsTransientException);
        }

        /// <summary>
        /// Updates an existing record in the master data table.
        /// </summary>
        /// <param name="uniqueKey">The unique key for the record.</param>
        /// <param name="data">The updated data.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task UpdateMasterDataAsync(string uniqueKey, string data)
        {
            await RetryHelper.ExecuteWithRetryAsync(
                async () =>
                {
                    try
                    {
                        await ExecuteDatabaseOperationAsync(async (connection) =>
                        {
                            string updateMasterData = @"
                                UPDATE Master_Data
                                SET Data = @Data, LastUpdated = @LastUpdated
                                WHERE UniqueKey = @UniqueKey;";

                            using var command = new SqliteCommand(updateMasterData, connection);
                            command.Parameters.AddWithValue("@UniqueKey", uniqueKey);
                            command.Parameters.AddWithValue("@Data", data);
                            command.Parameters.AddWithValue("@LastUpdated", DateTime.Now);

                            await command.ExecuteNonQueryAsync();
                            return Task.CompletedTask;
                        });
                    }
                    catch (Exception ex)
                    {
                        LoggingService.LogError("Error updating master data", ex);
                        throw new InvalidOperationException(string.Format(ErrorMessages.ErrorUpdatingMasterData, ex.Message), ex);
                    }
                },
                maxRetries: 3,
                delayMilliseconds: 1000,
                isTransientException: IsTransientException);
        }

        /// <summary>
        /// Deletes indexed content for a specific archive.
        /// </summary>
        /// <param name="archiveId">The ArchiveId to delete indexed content for.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task DeleteIndexedContentAsync(int archiveId)
        {
            await RetryHelper.ExecuteWithRetryAsync(
                async () =>
                {
                    try
                    {
                        await ExecuteDatabaseOperationAsync(async (connection) =>
                        {
                            string deleteContentIndex = @"
                                DELETE FROM Content_Index
                                WHERE ArchiveId = @ArchiveId;";

                            using var command = new SqliteCommand(deleteContentIndex, connection);
                            command.Parameters.AddWithValue("@ArchiveId", archiveId);

                            await command.ExecuteNonQueryAsync();
                            return Task.CompletedTask;
                        });
                    }
                    catch (Exception ex)
                    {
                        LoggingService.LogError("Error deleting indexed content", ex);
                        throw new InvalidOperationException(string.Format(ErrorMessages.ErrorDeletingIndexedContent, ex.Message), ex);
                    }
                },
                maxRetries: 3,
                delayMilliseconds: 1000,
                isTransientException: IsTransientException);
        }

        /// <summary>
        /// Determines if an exception is transient and should be retried.
        /// </summary>
        /// <param name="ex">The exception to check.</param>
        /// <returns>True if the exception is transient, false otherwise.</returns>
        private static bool IsTransientException(Exception ex)
        {
            // Check for common transient database exceptions
            if (ex is SqliteException sqliteEx)
            {
                // Database is locked or busy
                if (sqliteEx.SqliteErrorCode == 5 || sqliteEx.SqliteErrorCode == 6)
                {
                    return true;
                }
                
                // Disk I/O error
                if (sqliteEx.SqliteErrorCode == 10)
                {
                    return true;
                }
            }
            
            // Check for file system related transient exceptions
            if (ex is IOException ioEx)
            {
                // File is being used by another process
                if (ioEx.Message.Contains("being used by another process"))
                {
                    return true;
                }
                
                // Disk is full or other I/O issues
                if (ioEx.Message.Contains("disk") || ioEx.Message.Contains("I/O"))
                {
                    return true;
                }
            }
            
            // Network related exceptions (if database is remote in future)
            if (ex is System.Net.Sockets.SocketException)
            {
                return true;
            }
            
            // Timeout exceptions
            if (ex is TimeoutException)
            {
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Gets a user-friendly error message with recovery suggestions.
        /// </summary>
        /// <param name="ex">The exception to analyze.</param>
        /// <returns>A user-friendly error message with recovery suggestions.</returns>
        private static string GetUserFriendlyErrorMessage(Exception ex)
        {
            var message = ex.Message;
            
            // Check for specific error types and provide recovery suggestions
            if (ex is SqliteException sqliteEx)
            {
                // Database is locked or busy
                if (sqliteEx.SqliteErrorCode == 5 || sqliteEx.SqliteErrorCode == 6)
                {
                    return $"Database is currently locked. Please try again in a moment.\n\nRecovery suggestion: Close any other applications using the database and try again.";
                }
                
                // Disk I/O error
                if (sqliteEx.SqliteErrorCode == 10)
                {
                    return $"Insufficient disk space to complete the operation.\n\nRecovery suggestion: Free up disk space and try again.";
                }
            }
            
            // Check for file system related exceptions
            if (ex is IOException ioEx)
            {
                // File is being used by another process
                if (ioEx.Message.Contains("being used by another process"))
                {
                    return $"Unable to access file: {message}. The file may be in use by another process.\n\nRecovery suggestion: Close any applications using the file and try again.";
                }
                
                // Disk is full or other I/O issues
                if (ioEx.Message.Contains("disk") || ioEx.Message.Contains("I/O"))
                {
                    return $"Insufficient disk space to complete the operation.\n\nRecovery suggestion: Free up disk space and try again.";
                }
            }
            
            // Network related exceptions
            if (ex is System.Net.Sockets.SocketException)
            {
                return $"Network error occurred while accessing database: {message}\n\nRecovery suggestion: Check your network connection and try again.";
            }
            
            // Timeout exceptions
            if (ex is TimeoutException)
            {
                return $"Operation timed out. Please try again.\n\nRecovery suggestion: Try again. If the problem persists, restart the application.";
            }
            
            // Generic error message
            return $"Operation failed: {message}";
        }

        /// <summary>
        /// Releases all resources used by the DatabaseService.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the DatabaseService and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources here if any
                }
                
                // Free unmanaged resources here if any
                
                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizer for the DatabaseService.
        /// </summary>
        ~DatabaseService()
        {
            Dispose(false);
        }
    }
}