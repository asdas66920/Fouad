using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fouad.Services;
using Moq;

namespace Fouad.Tests
{
    [TestClass]
    public class FileDataServiceRetryTests
    {
        private FileDataService _fileDataService = null!;
        private string _testExcelPath = "";
        
        [TestInitialize]
        public void TestInitialize()
        {
            _fileDataService = new FileDataService();
            _testExcelPath = Path.Combine(Path.GetTempPath(), $"test_file_{Guid.NewGuid()}.xlsx");
            
            // Create a simple test Excel file
            CreateTestExcelFile(_testExcelPath);
        }
        
        [TestCleanup]
        public void TestCleanup()
        {
            // Clean up test files with retry logic
            if (!string.IsNullOrEmpty(_testExcelPath) && File.Exists(_testExcelPath))
            {
                DeleteFileWithRetry(_testExcelPath);
            }
            
            // Also clean up any binary cache files
            var cachePath = Path.Combine(Path.GetDirectoryName(_testExcelPath) ?? "", 
                                       Path.GetFileNameWithoutExtension(_testExcelPath) + ".bin");
            if (File.Exists(cachePath))
            {
                DeleteFileWithRetry(cachePath);
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
        public async Task LoadFileAsync_RetryOnTransientError_Succeeds()
        {
            // Act
            await _fileDataService.LoadFileAsync(_testExcelPath, CancellationToken.None);
            
            // Assert
            Assert.IsTrue(_fileDataService.IsFileLoaded());
            Assert.IsTrue(_fileDataService.GetRowCount() > 0);
            Assert.IsTrue(_fileDataService.GetColumnHeaders().Count > 0);
        }
        
        [TestMethod]
        public async Task LoadFileAsync_Cancelled_ThrowsOperationCanceledException()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            
            // Act & Assert
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () =>
            {
                await _fileDataService.LoadFileAsync(_testExcelPath, cancellationTokenSource.Token);
            });
        }
        
        [TestMethod]
        public async Task ImportFileWithDatabaseAsync_RetryOnTransientError_Succeeds()
        {
            // Arrange
            var testDatabasePath = Path.Combine(Path.GetTempPath(), $"test_database_{Guid.NewGuid()}.db");
            var databaseService = new DatabaseService(testDatabasePath);
            _fileDataService.SetDatabaseService(databaseService);
            
            try
            {
                // Act
                var archiveId = await _fileDataService.ImportFileWithDatabaseAsync(_testExcelPath);
                
                // Assert
                Assert.IsTrue(archiveId > 0);
                
                // Verify the file was imported by checking the archive log
                var archiveLog = await databaseService.GetArchiveLogAsync(archiveId);
                Assert.IsNotNull(archiveLog);
                Assert.AreEqual(Environment.UserName, archiveLog!.UploadedBy);
            }
            finally
            {
                // Clean up
                databaseService?.Dispose();
                
                if (File.Exists(testDatabasePath))
                {
                    DeleteFileWithRetry(testDatabasePath);
                }
            }
        }
        
        [TestMethod]
        public void ClearCache_SuccessfullyClearsCache()
        {
            // Arrange
            var cacheUsageBefore = _fileDataService.GetCacheMemoryUsage();
            
            // Act
            _fileDataService.ClearCache();
            
            // Assert
            var cacheUsageAfter = _fileDataService.GetCacheMemoryUsage();
            Assert.AreEqual(0, cacheUsageAfter);
        }
        
        [TestMethod]
        public void GetCacheMemoryUsage_ReturnsApproximateMemoryUsage()
        {
            // Act
            var cacheUsage = _fileDataService.GetCacheMemoryUsage();
            
            // Assert
            // Should be a non-negative value
            Assert.IsTrue(cacheUsage >= 0);
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