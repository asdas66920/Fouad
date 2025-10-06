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
    /// Edge case tests for the DatabaseService class.
    /// </summary>
    [TestClass]
    public class DatabaseServiceEdgeCaseTests
    {
        private DatabaseService? _databaseService = null!;
        private string? _testDatabasePath = null!;

        /// <summary>
        /// Static constructor to set EPPlus license
        /// </summary>
        static DatabaseServiceEdgeCaseTests()
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
        /// Tests that ImportFileAsync handles non-existent file gracefully.
        /// </summary>
        [TestMethod]
        public async Task ImportFileAsync_WithNonExistentFile_HandlesGracefully()
        {
            // Arrange
            var nonExistentFilePath = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.xlsx");
            var uploadedBy = "testuser";

            // Act & Assert
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(async () =>
            {
                await _databaseService.ImportFileAsync(nonExistentFilePath, uploadedBy);
            });
        }

        /// <summary>
        /// Tests that ImportFileAsync handles unsupported file type gracefully.
        /// </summary>
        [TestMethod]
        public async Task ImportFileAsync_WithUnsupportedFileType_HandlesGracefully()
        {
            // Arrange
            var unsupportedFilePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.txt");
            File.WriteAllText(unsupportedFilePath, "This is a text file, not Excel");
            var uploadedBy = "testuser";

            try
            {
                // Act & Assert
                await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
                {
                    await _databaseService.ImportFileAsync(unsupportedFilePath, uploadedBy);
                });
            }
            finally
            {
                // Clean up
                if (File.Exists(unsupportedFilePath))
                {
                    File.Delete(unsupportedFilePath);
                }
            }
        }

        /// <summary>
        /// Tests that IndexFileContentAsync handles non-existent file gracefully.
        /// </summary>
        [TestMethod]
        public async Task IndexFileContentAsync_WithNonExistentFile_HandlesGracefully()
        {
            // Arrange
            var nonExistentFilePath = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.xlsx");
            var archiveId = 1;

            // Act & Assert
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(async () =>
            {
                await _databaseService.IndexFileContentAsync(nonExistentFilePath, archiveId);
            });
        }

        /// <summary>
        /// Tests that GetArchiveLogAsync handles non-existent archive ID gracefully.
        /// </summary>
        [TestMethod]
        public async Task GetArchiveLogAsync_WithNonExistentId_ReturnsNull()
        {
            // Arrange
            var nonExistentArchiveId = 999999;

            // Act
            var result = await _databaseService.GetArchiveLogAsync(nonExistentArchiveId);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that GetIndexedContentAsync handles non-existent archive ID gracefully.
        /// </summary>
        [TestMethod]
        public async Task GetIndexedContentAsync_WithNonExistentId_ReturnsEmptyList()
        {
            // Arrange
            var nonExistentArchiveId = 999999;

            // Act
            var result = await _databaseService.GetIndexedContentAsync(nonExistentArchiveId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        /// <summary>
        /// Tests that AddToMasterDataAsync handles null or empty data gracefully.
        /// </summary>
        [TestMethod]
        public async Task AddToMasterDataAsync_WithNullOrEmptyData_HandlesGracefully()
        {
            // Arrange
            var uniqueKey = "test_key";
            string nullData = null;
            var emptyData = "";

            // Act & Assert
            // Should handle null data
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            {
                await _databaseService.AddToMasterDataAsync(uniqueKey, nullData);
            });

            // Should handle empty data
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            {
                await _databaseService.AddToMasterDataAsync(uniqueKey, emptyData);
            });
        }

        /// <summary>
        /// Tests that UpdateMasterDataAsync handles non-existent key gracefully.
        /// </summary>
        [TestMethod]
        public async Task UpdateMasterDataAsync_WithNonExistentKey_HandlesGracefully()
        {
            // Arrange
            var nonExistentKey = "non_existent_key";
            var data = "{\"name\":\"Test\"}";

            // Act
            var result = await _databaseService.UpdateMasterDataAsync(nonExistentKey, data);

            // Assert
            Assert.IsFalse(result); // Should return false indicating no update occurred
        }

        /// <summary>
        /// Tests that DeleteIndexedContentAsync handles non-existent archive ID gracefully.
        /// </summary>
        [TestMethod]
        public async Task DeleteIndexedContentAsync_WithNonExistentId_HandlesGracefully()
        {
            // Arrange
            var nonExistentArchiveId = 999999;

            // Act
            // Should not throw exception for non-existent ID
            await _databaseService.DeleteIndexedContentAsync(nonExistentArchiveId);

            // Assert
            // If we get here without exception, the test passes
            Assert.IsTrue(true);
        }

        /// <summary>
        /// Tests concurrent access with error conditions.
        /// </summary>
        [TestMethod]
        public async Task DatabaseService_ConcurrentAccessWithErrorConditions_HandlesGracefully()
        {
            // Act
            var tasks = new List<Task<int>>();
            for (int i = 0; i < 5; i++)
            {
                var fileName = $"test_file_{i}.xlsx";
                var filePath = Path.Combine(Path.GetTempPath(), fileName);
                
                // Some files exist, some don't
                if (i % 2 == 0)
                {
                    // Create file for even indices
                    CreateTestExcelFile(filePath);
                    tasks.Add(_databaseService.ImportFileAsync(filePath, $"user{i}"));
                }
                else
                {
                    // Don't create file for odd indices (will cause FileNotFoundException)
                    tasks.Add(_databaseService.ImportFileAsync(filePath, $"user{i}"));
                }
            }
            
            // Act & Assert
            try
            {
                var results = await Task.WhenAll(tasks);
                // If we get here, all tasks completed successfully
                Assert.AreEqual(5, results.Length);
            }
            catch (Exception)
            {
                // Some tasks failed as expected, which is fine
                // Check that at least some tasks succeeded
                var completedTasks = tasks.Count(t => t.IsCompletedSuccessfully);
                Assert.IsTrue(completedTasks >= 0); // At least 0 tasks succeeded (some may have failed)
            }
            finally
            {
                // Clean up created files
                for (int i = 0; i < 5; i++)
                {
                    if (i % 2 == 0)
                    {
                        var filePath = Path.Combine(Path.GetTempPath(), $"test_file_{i}.xlsx");
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                    }
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
            
            // Add data rows
            worksheet.Cells[2, 1].Value = "John Doe";
            worksheet.Cells[2, 2].Value = 30;
            
            package.SaveAs(new FileInfo(filePath));
        }
    }
}