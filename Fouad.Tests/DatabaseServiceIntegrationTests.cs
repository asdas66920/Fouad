using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;
using Fouad.Services;
using OfficeOpenXml;
using System.Collections.Generic;
using Fouad.Models;

namespace Fouad.Tests
{
    /// <summary>
    /// Integration tests for the DatabaseService class with advanced search functionality.
    /// </summary>
    [TestClass]
    public class DatabaseServiceIntegrationTests
    {
        private DatabaseService? _databaseService = null!;
        private string? _testDatabasePath = null!;
        private string? _testExcelPath = null!;

        /// <summary>
        /// Static constructor to set EPPlus license
        /// </summary>
        static DatabaseServiceIntegrationTests()
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
                _databaseService = null;
            }

            // Add a larger delay to ensure file handles are released
            System.Threading.Thread.Sleep(200);

            // Clean up test files with more robust retry logic
            if (!string.IsNullOrEmpty(_testDatabasePath))
            {
                DeleteFileWithRetry(_testDatabasePath);
            }
            
            if (!string.IsNullOrEmpty(_testExcelPath))
            {
                DeleteFileWithRetry(_testExcelPath);
            }
            
            // Clean up archive folder if it was created
            if (!string.IsNullOrEmpty(_testDatabasePath))
            {
                var archiveFolderPath = Path.Combine(Path.GetDirectoryName(_testDatabasePath) ?? "", "Excel_Archive");
                if (Directory.Exists(archiveFolderPath))
                {
                    DeleteDirectoryWithRetry(archiveFolderPath);
                }
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
        /// Creates a simple test Excel file for testing.
        /// </summary>
        /// <param name="filePath">The path where to create the file.</param>
        private void CreateTestExcelFile(string filePath)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("TestSheet");
            
            // Add headers
            worksheet.Cells[1, 1].Value = "Name";
            worksheet.Cells[1, 2].Value = "Age";
            worksheet.Cells[1, 3].Value = "City";
            worksheet.Cells[1, 4].Value = "Salary";
            
            // Add data rows
            worksheet.Cells[2, 1].Value = "John Doe";
            worksheet.Cells[2, 2].Value = 30;
            worksheet.Cells[2, 3].Value = "New York";
            worksheet.Cells[2, 4].Value = 50000;
            
            worksheet.Cells[3, 1].Value = "Jane Smith";
            worksheet.Cells[3, 2].Value = 25;
            worksheet.Cells[3, 3].Value = "Los Angeles";
            worksheet.Cells[3, 4].Value = 60000;
            
            worksheet.Cells[4, 1].Value = "Bob Johnson";
            worksheet.Cells[4, 2].Value = 35;
            worksheet.Cells[4, 3].Value = "Chicago";
            worksheet.Cells[4, 4].Value = 55000;
            
            package.SaveAs(new FileInfo(filePath));
        }

        /// <summary>
        /// Tests that content can be indexed and retrieved successfully.
        /// </summary>
        [TestMethod]
        public async Task DatabaseService_IndexAndRetrieveContent_WorksCorrectly()
        {
            // Arrange
            var archiveId = await _databaseService.ImportFileAsync(_testExcelPath, "testuser");
            
            // Act
            await _databaseService.IndexFileContentAsync(_testExcelPath, archiveId);
            var content = await _databaseService.GetIndexedContentAsync(archiveId);

            // Assert
            Assert.IsNotNull(content);
            Assert.IsTrue(content.Count > 0);
            Assert.AreEqual(archiveId, content[0].ArchiveId);
        }

        /// <summary>
        /// Tests that archive logs can be retrieved successfully.
        /// </summary>
        [TestMethod]
        public async Task DatabaseService_GetArchiveLogs_WorksCorrectly()
        {
            // Arrange
            var archiveId = await _databaseService.ImportFileAsync(_testExcelPath, "testuser");
            
            // Act
            var archiveLogs = await _databaseService.GetAllArchiveLogsAsync();

            // Assert
            Assert.IsNotNull(archiveLogs);
            Assert.IsTrue(archiveLogs.Count > 0);
            Assert.AreEqual(archiveId, archiveLogs[0].ArchiveId);
            Assert.AreEqual("testuser", archiveLogs[0].UploadedBy);
        }

        /// <summary>
        /// Tests that master data can be added and retrieved successfully.
        /// </summary>
        [TestMethod]
        public async Task DatabaseService_MasterData_AddAndRetrieve_WorksCorrectly()
        {
            // Arrange
            var uniqueKey = "test_key";
            var data = "{\"name\":\"John Doe\",\"age\":30}";
            
            // Act
            await _databaseService.AddToMasterDataAsync(uniqueKey, data);
            var result = await _databaseService.GetMasterRecordAsync(uniqueKey);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(uniqueKey, result.Value.UniqueKey);
            Assert.AreEqual(data, result.Value.Data);
        }

        /// <summary>
        /// Tests that master data can be updated successfully.
        /// </summary>
        [TestMethod]
        public async Task DatabaseService_MasterData_Update_WorksCorrectly()
        {
            // Arrange
            var uniqueKey = "test_key";
            var initialData = "{\"name\":\"John Doe\",\"age\":30}";
            var updatedData = "{\"name\":\"John Doe\",\"age\":31}";
            
            await _databaseService.AddToMasterDataAsync(uniqueKey, initialData);
            
            // Act
            await _databaseService.UpdateMasterDataAsync(uniqueKey, updatedData);
            var result = await _databaseService.GetMasterRecordAsync(uniqueKey);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(uniqueKey, result.Value.UniqueKey);
            Assert.AreEqual(updatedData, result.Value.Data);
        }

        /// <summary>
        /// Tests concurrent access to the database.
        /// </summary>
        [TestMethod]
        public async Task DatabaseService_ConcurrentAccess_WorksCorrectly()
        {
            // Act
            var tasks = new List<Task<int>>();
            for (int i = 0; i < 5; i++)
            {
                var fileName = $"test_file_{i}.xlsx";
                var filePath = Path.Combine(Path.GetTempPath(), fileName);
                CreateTestExcelFile(filePath);
                
                tasks.Add(_databaseService.ImportFileAsync(filePath, $"user{i}"));
            }
            
            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.AreEqual(5, results.Length);
            foreach (var result in results)
            {
                Assert.IsTrue(result > 0);
            }
        }
    }
}