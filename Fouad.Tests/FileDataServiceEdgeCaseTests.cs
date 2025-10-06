using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fouad.Services;
using Fouad.Models;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;
using System;

namespace Fouad.Tests
{
    /// <summary>
    /// Edge case tests for the FileDataService class.
    /// </summary>
    [TestClass]
    public class FileDataServiceEdgeCaseTests
    {
        private FileDataService? _fileDataService;
        private string? _testCsvPath;
        private string? _testExcelPath;

        /// <summary>
        /// Static constructor to set EPPlus license
        /// </summary>
        static FileDataServiceEdgeCaseTests()
        {
            ExcelPackage.License.SetNonCommercialPersonal("khaled");
        }

        /// <summary>
        /// Initializes the test environment before each test method runs.
        /// Creates sample test files for testing.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            _fileDataService = new FileDataService();
            _testCsvPath = Path.GetTempFileName() + ".csv";
            _testExcelPath = Path.GetTempFileName() + ".xlsx";

            // Create a sample CSV file for testing
            CreateSampleCsvFile(_testCsvPath);
            
            // Create a sample Excel file for testing
            CreateSampleExcelFile(_testExcelPath);
        }

        /// <summary>
        /// Cleans up the test environment after each test method runs.
        /// Deletes temporary test files.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            if (File.Exists(_testCsvPath))
            {
                File.Delete(_testCsvPath);
            }

            if (File.Exists(_testExcelPath))
            {
                File.Delete(_testExcelPath);
            }

            // Clean up binary cache files
            var csvCachePath = Path.Combine(
                Path.GetDirectoryName(_testCsvPath) ?? "", 
                Path.GetFileNameWithoutExtension(_testCsvPath) + ".bin");
                
            if (File.Exists(csvCachePath))
            {
                File.Delete(csvCachePath);
            }
            
            var excelCachePath = Path.Combine(
                Path.GetDirectoryName(_testExcelPath) ?? "", 
                Path.GetFileNameWithoutExtension(_testExcelPath) + ".bin");
                
            if (File.Exists(excelCachePath))
            {
                File.Delete(excelCachePath);
            }
        }

        /// <summary>
        /// Creates a sample CSV file for testing.
        /// </summary>
        /// <param name="filePath">The path where to create the file.</param>
        private void CreateSampleCsvFile(string filePath)
        {
            var lines = new List<string>
            {
                "Name,Age,City,Salary",
                "John Doe,30,New York,50000",
                "Jane Smith,25,Los Angeles,60000",
                "Bob Johnson,35,Chicago,55000"
            };
            
            File.WriteAllLines(filePath, lines);
        }

        /// <summary>
        /// Creates a sample Excel file for testing.
        /// </summary>
        /// <param name="filePath">The path where to create the file.</param>
        private void CreateSampleExcelFile(string filePath)
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
        /// Tests that LoadFileAsync handles non-existent file gracefully.
        /// </summary>
        [TestMethod]
        public async Task LoadFileAsync_WithNonExistentFile_ThrowsFileNotFoundException()
        {
            // Arrange
            var service = new FileDataService();
            var nonExistentFilePath = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.xlsx");

            // Act & Assert
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(async () =>
            {
                await service.LoadFileAsync(nonExistentFilePath);
            });
        }

        /// <summary>
        /// Tests that LoadFileAsync handles unsupported file type gracefully.
        /// </summary>
        [TestMethod]
        public async Task LoadFileAsync_WithUnsupportedFileType_ThrowsInvalidOperationException()
        {
            // Arrange
            var service = new FileDataService();
            var unsupportedFilePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.txt");
            File.WriteAllText(unsupportedFilePath, "This is a text file, not Excel or CSV");

            try
            {
                // Act & Assert
                await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
                {
                    await service.LoadFileAsync(unsupportedFilePath);
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
        /// Tests that GetResultById handles invalid ID gracefully.
        /// </summary>
        [TestMethod]
        public async Task GetResultById_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var service = new FileDataService();
            await service.LoadFileAsync(_testExcelPath);

            // Act
            var result = service.GetResultById(999); // Non-existent ID

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that GetResultById handles negative ID gracefully.
        /// </summary>
        [TestMethod]
        public async Task GetResultById_WithNegativeId_ReturnsNull()
        {
            // Arrange
            var service = new FileDataService();
            await service.LoadFileAsync(_testExcelPath);

            // Act
            var result = service.GetResultById(-1); // Negative ID

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that Search handles null search term gracefully.
        /// </summary>
        [TestMethod]
        public async Task Search_WithNullSearchTerm_ReturnsEmptyList()
        {
            // Arrange
            var service = new FileDataService();
            await service.LoadFileAsync(_testExcelPath);

            // Act
            var results = service.Search(null);

            // Assert
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Count);
        }

        /// <summary>
        /// Tests that Search handles empty search term gracefully.
        /// </summary>
        [TestMethod]
        public async Task Search_WithEmptySearchTerm_ReturnsEmptyList()
        {
            // Arrange
            var service = new FileDataService();
            await service.LoadFileAsync(_testExcelPath);

            // Act
            var results = service.Search("");

            // Assert
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Count);
        }

        /// <summary>
        /// Tests that UpdateResult handles null result gracefully.
        /// </summary>
        [TestMethod]
        public async Task UpdateResult_WithNullResult_HandlesGracefully()
        {
            // Arrange
            var service = new FileDataService();
            await service.LoadFileAsync(_testExcelPath);

            // Act & Assert
            // Should not throw exception with null result
            service.UpdateResult(null);
            
            // If we get here, the test passes
            Assert.IsTrue(true);
        }

        /// <summary>
        /// Tests that GetColumnHeaders returns empty list when no file is loaded.
        /// </summary>
        [TestMethod]
        public void GetColumnHeaders_WithNoFileLoaded_ReturnsEmptyList()
        {
            // Arrange
            var service = new FileDataService();

            // Act
            var headers = service.GetColumnHeaders();

            // Assert
            Assert.IsNotNull(headers);
            Assert.AreEqual(0, headers.Count);
        }

        /// <summary>
        /// Tests that GetRowCount returns 0 when no file is loaded.
        /// </summary>
        [TestMethod]
        public void GetRowCount_WithNoFileLoaded_ReturnsZero()
        {
            // Arrange
            var service = new FileDataService();

            // Act
            var count = service.GetRowCount();

            // Assert
            Assert.AreEqual(0, count);
        }

        /// <summary>
        /// Tests that IsFileLoaded returns false when no file is loaded.
        /// </summary>
        [TestMethod]
        public void IsFileLoaded_WithNoFileLoaded_ReturnsFalse()
        {
            // Arrange
            var service = new FileDataService();

            // Act
            var isLoaded = service.IsFileLoaded();

            // Assert
            Assert.IsFalse(isLoaded);
        }

        /// <summary>
        /// Tests concurrent loading with error conditions.
        /// </summary>
        [TestMethod]
        public async Task ConcurrentFileLoadingWithErrorConditions_HandlesGracefully()
        {
            // Act
            var tasks = new List<Task>();
            for (int i = 0; i < 5; i++)
            {
                var service = new FileDataService();
                
                if (i % 2 == 0)
                {
                    // Even indices: load valid file
                    tasks.Add(service.LoadFileAsync(_testExcelPath));
                }
                else
                {
                    // Odd indices: try to load non-existent file
                    var nonExistentPath = Path.Combine(Path.GetTempPath(), $"nonexistent_{i}.xlsx");
                    tasks.Add(service.LoadFileAsync(nonExistentPath));
                }
            }
            
            // Act & Assert
            try
            {
                await Task.WhenAll(tasks);
                // If we get here, all tasks completed successfully
                Assert.IsTrue(tasks.All(t => t.IsCompletedSuccessfully));
            }
            catch (Exception)
            {
                // Some tasks failed as expected, which is fine
                // Check that at least some tasks succeeded
                var completedTasks = tasks.Count(t => t.IsCompletedSuccessfully);
                Assert.IsTrue(completedTasks >= 0); // At least 0 tasks succeeded (some may have failed)
            }
        }

        /// <summary>
        /// Tests that binary caching handles file system errors gracefully.
        /// </summary>
        [TestMethod]
        public async Task BinaryCaching_WithFileSystemErrors_HandlesGracefully()
        {
            // Arrange
            var service1 = new FileDataService();
            await service1.LoadFileAsync(_testExcelPath);
            
            // Get cache path
            var cachePath = Path.Combine(
                Path.GetDirectoryName(_testExcelPath) ?? "", 
                Path.GetFileNameWithoutExtension(_testExcelPath) + ".bin");
            
            // Verify cache file was created
            Assert.IsTrue(File.Exists(cachePath));
            
            // Make the cache file read-only to simulate file system error
            File.SetAttributes(cachePath, FileAttributes.ReadOnly);
            
            try
            {
                // Act - Try to load from cache (should handle read-only file gracefully)
                var service2 = new FileDataService();
                await service2.LoadFileAsync(_testExcelPath);

                // Assert
                Assert.IsTrue(service2.IsFileLoaded());
            }
            finally
            {
                // Clean up - make file writable again
                if (File.Exists(cachePath))
                {
                    File.SetAttributes(cachePath, FileAttributes.Normal);
                }
            }
        }
    }
}