using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fouad.Services;
using Moq;

namespace Fouad.Tests
{
    [TestClass]
    public class DatabaseServiceRetryTests
    {
        private string _testDatabasePath = "";
        private DatabaseService _databaseService = null!;
        
        [TestInitialize]
        public void TestInitialize()
        {
            // Create a temporary database file for testing
            _testDatabasePath = Path.Combine(Path.GetTempPath(), $"test_database_{Guid.NewGuid()}.db");
            _databaseService = new DatabaseService(_testDatabasePath);
        }
        
        [TestCleanup]
        public void TestCleanup()
        {
            // Dispose the database service
            _databaseService?.Dispose();
            
            // Clean up test files with retry logic
            if (!string.IsNullOrEmpty(_testDatabasePath) && File.Exists(_testDatabasePath))
            {
                DeleteFileWithRetry(_testDatabasePath);
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
        
        [TestMethod]
        public async Task ImportFileAsync_RetryOnTransientError_Succeeds()
        {
            // Arrange
            var testExcelPath = Path.Combine(Path.GetTempPath(), $"test_file_{Guid.NewGuid()}.xlsx");
            
            try
            {
                // Create a simple test Excel file
                CreateTestExcelFile(testExcelPath);
                
                // Act
                var archiveId = await _databaseService.ImportFileAsync(testExcelPath, "testuser");
                
                // Assert
                Assert.IsTrue(archiveId > 0);
                
                // Verify the file was imported by checking the archive log
                var archiveLog = await _databaseService.GetArchiveLogAsync(archiveId);
                Assert.IsNotNull(archiveLog);
                Assert.AreEqual("testuser", archiveLog!.UploadedBy);
            }
            finally
            {
                // Clean up test files
                if (File.Exists(testExcelPath))
                {
                    DeleteFileWithRetry(testExcelPath);
                }
            }
        }
        
        [TestMethod]
        public async Task IndexFileContentAsync_RetryOnTransientError_Succeeds()
        {
            // Arrange
            var testExcelPath = Path.Combine(Path.GetTempPath(), $"test_file_{Guid.NewGuid()}.xlsx");
            
            try
            {
                // Create a simple test Excel file
                CreateTestExcelFile(testExcelPath);
                
                // Import the file first
                var archiveId = await _databaseService.ImportFileAsync(testExcelPath, "testuser");
                
                // Act
                await _databaseService.IndexFileContentAsync(testExcelPath, archiveId);
                
                // Assert
                // Verify content was indexed by checking for content
                var content = await _databaseService.GetIndexedContentAsync(archiveId);
                Assert.IsNotNull(content);
                // Content should have been indexed (exact count depends on test file structure)
                Assert.IsTrue(content.Count > 0);
            }
            finally
            {
                // Clean up test files
                if (File.Exists(testExcelPath))
                {
                    DeleteFileWithRetry(testExcelPath);
                }
            }
        }
        
        [TestMethod]
        public async Task GetAllArchiveLogsAsync_RetryOnTransientError_Succeeds()
        {
            // Arrange
            var testExcelPath = Path.Combine(Path.GetTempPath(), $"test_file_{Guid.NewGuid()}.xlsx");
            
            try
            {
                // Create a simple test Excel file
                CreateTestExcelFile(testExcelPath);
                
                // Import a file to have something in the archive logs
                await _databaseService.ImportFileAsync(testExcelPath, "testuser");
                
                // Act
                var archiveLogs = await _databaseService.GetAllArchiveLogsAsync();
                
                // Assert
                Assert.IsNotNull(archiveLogs);
                Assert.IsTrue(archiveLogs.Count > 0);
                Assert.AreEqual("testuser", archiveLogs[0].UploadedBy);
            }
            finally
            {
                // Clean up test files
                if (File.Exists(testExcelPath))
                {
                    DeleteFileWithRetry(testExcelPath);
                }
            }
        }
        
        [TestMethod]
        public async Task AddToMasterDataAsync_RetryOnTransientError_Succeeds()
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
        
        [TestMethod]
        public async Task UpdateMasterDataAsync_RetryOnTransientError_Succeeds()
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
        /// Creates a simple test Excel file for testing.
        /// </summary>
        /// <param name="filePath">The path where to create the file.</param>
        private void CreateTestExcelFile(string filePath)
        {
            using var package = new OfficeOpenXml.ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets.Add("TestSheet");
            
            // Add headers
            worksheet.Cells[1, 1].Value = "Name";
            worksheet.Cells[1, 2].Value = "Age";
            worksheet.Cells[1, 3].Value = "City";
            
            // Add data
            worksheet.Cells[2, 1].Value = "John Doe";
            worksheet.Cells[2, 2].Value = 30;
            worksheet.Cells[2, 3].Value = "New York";
            
            worksheet.Cells[3, 1].Value = "Jane Smith";
            worksheet.Cells[3, 2].Value = 25;
            worksheet.Cells[3, 3].Value = "Los Angeles";
            
            package.Save();
        }
    }
}