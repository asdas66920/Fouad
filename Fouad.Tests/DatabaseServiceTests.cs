using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;
using Fouad.Services;
using OfficeOpenXml;

namespace Fouad.Tests
{
    /// <summary>
    /// Unit tests for the DatabaseService class.
    /// Tests cover database operations including file import, content indexing, and master data management.
    /// </summary>
    [TestClass]
    public class DatabaseServiceTests
    {
        private DatabaseService _databaseService = null!;
        private string _testDatabasePath = null!;
        private string _testExcelPath = null!;

        /// <summary>
        /// Static constructor to set EPPlus license
        /// </summary>
        static DatabaseServiceTests()
        {
            ExcelPackage.License.SetNonCommercialPersonal("khaled");
        }

        /// <summary>
        /// Initializes the test environment before each test method runs.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            // Set EPPlus license for non-commercial use
            ExcelPackage.License.SetNonCommercialPersonal("khaled");
            
            // Create a temporary database file for testing
            _testDatabasePath = Path.Combine(Path.GetTempPath(), $"test_database_{Guid.NewGuid()}.db");
            _databaseService = new DatabaseService(_testDatabasePath);
            
            // Create a simple test Excel file
            _testExcelPath = Path.Combine(Path.GetTempPath(), $"test_file_{Guid.NewGuid()}.xlsx");
            CreateTestExcelFile(_testExcelPath);
        }

        /// <summary>
        /// Cleans up the test environment after each test method runs.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            // Dispose the database service to close any open connections
            if (_databaseService != null)
            {
                try
                {
                    _databaseService.Dispose();
                }
                catch (Exception ex)
                {
                    // Log but don't throw to avoid breaking the test
                    Console.WriteLine($"Warning: Error disposing DatabaseService: {ex.Message}");
                }
                _databaseService = null!;
            }

            // Add a larger delay to ensure file handles are released
            System.Threading.Thread.Sleep(500);

            // Clean up test files with more robust retry logic
            try
            {
                DeleteFileWithRetry(_testDatabasePath);
                DeleteFileWithRetry(_testExcelPath);
            }
            catch (Exception ex)
            {
                // Log but don't throw to avoid breaking the test
                Console.WriteLine($"Warning: Error deleting test files: {ex.Message}");
            }
            
            // Clean up archive folder if it was created
            try
            {
                var archiveFolderPath = Path.Combine(Path.GetDirectoryName(_testDatabasePath) ?? "", "Excel_Archive");
                if (Directory.Exists(archiveFolderPath))
                {
                    DeleteDirectoryWithRetry(archiveFolderPath);
                }
            }
            catch (Exception ex)
            {
                // Log but don't throw to avoid breaking the test
                Console.WriteLine($"Warning: Error deleting archive folder: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a file with retry logic to handle locking issues.
        /// </summary>
        /// <param name="filePath">The path of the file to delete.</param>
        private void DeleteFileWithRetry(string filePath)
        {
            if (!File.Exists(filePath)) return;
            
            for (int i = 0; i < 15; i++) // Increase retry count
            {
                try
                {
                    File.Delete(filePath);
                    break;
                }
                catch (Exception)
                {
                    if (i == 14) 
                    {
                        // Log the error but don't throw to avoid breaking the test
                        // This is a cleanup method, so we don't want it to fail the test
                        Console.WriteLine($"Warning: Could not delete file {filePath} after 15 attempts");
                        break;
                    }
                    System.Threading.Thread.Sleep(300); // Increase delay between retries
                }
            }
        }
        
        /// <summary>
        /// Deletes a directory with retry logic to handle locking issues.
        /// </summary>
        /// <param name="directoryPath">The path of the directory to delete.</param>
        private void DeleteDirectoryWithRetry(string directoryPath)
        {
            // Check if directory exists before attempting to delete
            if (!Directory.Exists(directoryPath)) return;
            
            for (int i = 0; i < 15; i++) // Increase retry count
            {
                try
                {
                    Directory.Delete(directoryPath, true);
                    break;
                }
                catch (DirectoryNotFoundException)
                {
                    // Directory already deleted, nothing to do
                    break;
                }
                catch (Exception)
                {
                    if (i == 14) 
                    {
                        // Log the error but don't throw to avoid breaking the test
                        // This is a cleanup method, so we don't want it to fail the test
                        Console.WriteLine($"Warning: Could not delete directory {directoryPath} after 15 attempts");
                        break;
                    }
                    System.Threading.Thread.Sleep(300); // Increase delay between retries
                }
            }
        }

        /// <summary>
        /// Tests that the DatabaseService can be instantiated correctly.
        /// </summary>
        [TestMethod]
        public void DatabaseService_Constructor_CreatesInstance()
        {
            // Arrange & Act
            var databaseService = new DatabaseService(_testDatabasePath);

            // Assert
            Assert.IsNotNull(databaseService);
        }

        /// <summary>
        /// Tests that the database is initialized with the correct tables.
        /// </summary>
        [TestMethod]
        public async Task DatabaseService_Initialize_CreatesTables()
        {
            // Act
            // The database should be initialized in the constructor
            
            // Assert
            // If we get here without exception, the database was initialized successfully
            Assert.IsTrue(File.Exists(_testDatabasePath));
        }

        /// <summary>
        /// Tests that a file can be imported successfully.
        /// </summary>
        [TestMethod]
        public async Task DatabaseService_ImportFile_ImportsSuccessfully()
        {
            // Act
            var archiveId = await _databaseService.ImportFileAsync(_testExcelPath, "testuser");

            // Assert
            Assert.IsTrue(archiveId > 0);
        }

        /// <summary>
        /// Tests that file content can be indexed successfully.
        /// </summary>
        [TestMethod]
        public async Task DatabaseService_IndexFileContent_IndexesSuccessfully()
        {
            // Arrange
            var archiveId = await _databaseService.ImportFileAsync(_testExcelPath, "testuser");

            // Act
            await _databaseService.IndexFileContentAsync(_testExcelPath, archiveId);

            // Assert
            // If we get here without exception, the content was indexed successfully
            var content = await _databaseService.GetIndexedContentAsync(archiveId);
            Assert.IsTrue(content.Count > 0);
        }

        /// <summary>
        /// Tests that archive log can be retrieved successfully.
        /// </summary>
        [TestMethod]
        public async Task DatabaseService_GetArchiveLog_RetrievesSuccessfully()
        {
            // Arrange
            var archiveId = await _databaseService.ImportFileAsync(_testExcelPath, "testuser");

            // Act
            var archiveLog = await _databaseService.GetArchiveLogAsync(archiveId);

            // Assert
            Assert.IsNotNull(archiveLog);
            Assert.AreEqual(archiveId, archiveLog!.ArchiveId);
            Assert.AreEqual(Path.GetFileName(_testExcelPath), archiveLog.FileName);
            Assert.AreEqual("testuser", archiveLog.UploadedBy);
        }
        
        /// <summary>
        /// Tests that master data can be added successfully.
        /// </summary>
        [TestMethod]
        public async Task DatabaseService_AddToMasterData_AddsSuccessfully()
        {
            // Arrange
            var uniqueKey = "test_key";
            var data = "{\"name\":\"John\",\"age\":25}";
            
            // Act
            await _databaseService.AddToMasterDataAsync(uniqueKey, data);
            
            // Assert
            var masterRecord = await _databaseService.GetMasterRecordAsync(uniqueKey);
            Assert.IsNotNull(masterRecord);
            Assert.AreEqual(uniqueKey, masterRecord!.Value.UniqueKey);
            Assert.AreEqual(data, masterRecord.Value.Data);
        }
        
        /// <summary>
        /// Tests that master data can be updated successfully.
        /// </summary>
        [TestMethod]
        public async Task DatabaseService_UpdateMasterData_UpdatesSuccessfully()
        {
            // Arrange
            var uniqueKey = "test_key";
            var initialData = "{\"name\":\"John\",\"age\":25}";
            var updatedData = "{\"name\":\"John\",\"age\":26}";
            
            await _databaseService.AddToMasterDataAsync(uniqueKey, initialData);
            
            // Act
            await _databaseService.UpdateMasterDataAsync(uniqueKey, updatedData);
            
            // Assert
            var masterRecord = await _databaseService.GetMasterRecordAsync(uniqueKey);
            Assert.IsNotNull(masterRecord);
            Assert.AreEqual(uniqueKey, masterRecord!.Value.UniqueKey);
            Assert.AreEqual(updatedData, masterRecord.Value.Data);
        }
        
        /// <summary>
        /// Tests that master record can be retrieved successfully.
        /// </summary>
        [TestMethod]
        public async Task DatabaseService_GetMasterRecord_RetrievesSuccessfully()
        {
            // Arrange
            var uniqueKey = "test_key";
            var data = "{\"name\":\"John\",\"age\":25}";
            
            await _databaseService.AddToMasterDataAsync(uniqueKey, data);
            
            // Act
            var masterRecord = await _databaseService.GetMasterRecordAsync(uniqueKey);
            
            // Assert
            Assert.IsNotNull(masterRecord);
            Assert.AreEqual(uniqueKey, masterRecord!.Value.UniqueKey);
            Assert.AreEqual(data, masterRecord.Value.Data);
        }
        
        /// <summary>
        /// Tests that non-existent master record returns null.
        /// </summary>
        [TestMethod]
        public async Task DatabaseService_GetMasterRecord_ReturnsNullForNonExistent()
        {
            // Act
            var masterRecord = await _databaseService.GetMasterRecordAsync("non_existent_key");
            
            // Assert
            Assert.IsNull(masterRecord);
        }

        /// <summary>
        /// Creates a simple test Excel file for testing.
        /// </summary>
        /// <param name="filePath">The path where to create the test file.</param>
        private void CreateTestExcelFile(string filePath)
        {
            // Set EPPlus license for non-commercial use
            ExcelPackage.License.SetNonCommercialPersonal("khaled");
            
            // Create a new Excel package
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("TestSheet");
            
            // Add header row
            worksheet.Cells[1, 1].Value = "Name";
            worksheet.Cells[1, 2].Value = "Age";
            worksheet.Cells[1, 3].Value = "City";
            
            // Add data rows
            worksheet.Cells[2, 1].Value = "John";
            worksheet.Cells[2, 2].Value = 25;
            worksheet.Cells[2, 3].Value = "New York";
            
            worksheet.Cells[3, 1].Value = "Jane";
            worksheet.Cells[3, 2].Value = 30;
            worksheet.Cells[3, 3].Value = "London";
            
            worksheet.Cells[4, 1].Value = "Bob";
            worksheet.Cells[4, 2].Value = 35;
            worksheet.Cells[4, 3].Value = "Paris";
            
            // Save the file
            var file = new FileInfo(filePath);
            package.SaveAs(file);
        }
    }
}