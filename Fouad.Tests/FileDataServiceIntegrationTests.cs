using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fouad.Services;
using Fouad.Models;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;

namespace Fouad.Tests
{
    /// <summary>
    /// Integration tests for the FileDataService class with advanced search functionality.
    /// </summary>
    [TestClass]
    public class FileDataServiceIntegrationTests
    {
        private FileDataService? _fileDataService;
        private string? _testCsvPath;
        private string? _testExcelPath;

        /// <summary>
        /// Static constructor to set EPPlus license
        /// </summary>
        static FileDataServiceIntegrationTests()
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
                "Bob Johnson,35,Chicago,55000",
                "Alice Brown,28,Boston,52000",
                "Charlie Wilson,32,San Francisco,65000"
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
            
            worksheet.Cells[5, 1].Value = "Alice Brown";
            worksheet.Cells[5, 2].Value = 28;
            worksheet.Cells[5, 3].Value = "Boston";
            worksheet.Cells[5, 4].Value = 52000;
            
            worksheet.Cells[6, 1].Value = "Charlie Wilson";
            worksheet.Cells[6, 2].Value = 32;
            worksheet.Cells[6, 3].Value = "San Francisco";
            worksheet.Cells[6, 4].Value = 65000;
            
            package.SaveAs(new FileInfo(filePath));
        }

        /// <summary>
        /// Tests that LoadFileAsync correctly loads an Excel file.
        /// </summary>
        [TestMethod]
        public async Task LoadFileAsync_LoadsExcelFile_Successfully()
        {
            // Arrange
            var service = new FileDataService();

            // Act
            await service.LoadFileAsync(_testExcelPath);

            // Assert
            Assert.IsTrue(service.IsFileLoaded());
            Assert.AreEqual(5, service.GetRowCount()); // 5 data rows
        }

        /// <summary>
        /// Tests that GetColumnHeaders returns the correct column headers from an Excel file.
        /// </summary>
        [TestMethod]
        public async Task GetColumnHeaders_ReturnsCorrectHeaders_ForExcelFile()
        {
            // Arrange
            var service = new FileDataService();
            await service.LoadFileAsync(_testExcelPath);

            // Act
            var headers = service.GetColumnHeaders();

            // Assert
            Assert.IsNotNull(headers);
            Assert.AreEqual(4, headers.Count);
            Assert.AreEqual("Name", headers[0]);
            Assert.AreEqual("Age", headers[1]);
            Assert.AreEqual("City", headers[2]);
            Assert.AreEqual("Salary", headers[3]);
        }

        /// <summary>
        /// Tests that Search method returns correct results for a search term in Excel file.
        /// </summary>
        [TestMethod]
        public async Task Search_ReturnsMatchingRows_ForValidSearchTermInExcel()
        {
            // Arrange
            var service = new FileDataService();
            await service.LoadFileAsync(_testExcelPath);

            // Act
            var results = service.Search("John");

            // Assert
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Count > 0);
        }

        /// <summary>
        /// Tests that GetResultById returns the correct result for a valid ID in Excel file.
        /// </summary>
        [TestMethod]
        public async Task GetResultById_ReturnsCorrectResult_ForValidIdInExcel()
        {
            // Arrange
            var service = new FileDataService();
            await service.LoadFileAsync(_testExcelPath);

            // Act
            var result = service.GetResultById(2); // Second data row (Jane Smith)

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Id);
            Assert.IsNotNull(result.DynamicColumnValues);
            Assert.IsTrue(result.DynamicColumnValues.Count >= 4); // At least 4 columns
            // Check that one of the values contains "Jane" (the actual data might vary)
            Assert.IsTrue(result.DynamicColumnValues.Any(v => v.Contains("Jane")));
        }

        /// <summary>
        /// Tests that binary caching works correctly for Excel files.
        /// </summary>
        [TestMethod]
        public async Task BinaryCaching_WorksCorrectly_ForExcelFiles()
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
            
            // Act - Load from cache
            var service2 = new FileDataService();
            await service2.LoadFileAsync(_testExcelPath);

            // Assert
            Assert.IsTrue(service2.IsFileLoaded());
            Assert.AreEqual(5, service2.GetRowCount());
        }

        /// <summary>
        /// Tests that UpdateResult correctly updates a result in the cache.
        /// </summary>
        [TestMethod]
        public async Task UpdateResult_UpdatesResultInCache_Successfully()
        {
            // Arrange
            var service = new FileDataService();
            await service.LoadFileAsync(_testExcelPath);
            var originalResult = service.GetResultById(2);
            
            // Modify the result
            var modifiedResult = new Result
            {
                Id = originalResult.Id,
                FileName = originalResult.FileName,
                Content = "Modified content",
                SearchDate = originalResult.SearchDate,
                MatchCount = originalResult.MatchCount,
                DynamicColumnValues = new List<string> { "Modified Name", "30", "Modified City", "60000" },
                IsAddedToHistory = originalResult.IsAddedToHistory
            };

            // Act
            service.UpdateResult(modifiedResult);
            var updatedResult = service.GetResultById(2);

            // Assert
            Assert.IsNotNull(updatedResult);
            Assert.AreEqual("Modified content", updatedResult.Content);
            Assert.AreEqual("Modified Name", updatedResult.DynamicColumnValues[0]);
        }

        /// <summary>
        /// Tests that GetCacheMemoryUsage returns a positive value.
        /// </summary>
        [TestMethod]
        public async Task GetCacheMemoryUsage_ReturnsPositiveValue()
        {
            // Arrange
            var service = new FileDataService();
            await service.LoadFileAsync(_testExcelPath);

            // Act
            var memoryUsage = service.GetCacheMemoryUsage();

            // Assert
            Assert.IsTrue(memoryUsage > 0);
        }

        /// <summary>
        /// Tests concurrent loading of multiple files.
        /// </summary>
        [TestMethod]
        public async Task ConcurrentFileLoading_WorksCorrectly()
        {
            // Act
            var tasks = new List<Task>();
            for (int i = 0; i < 3; i++)
            {
                var service = new FileDataService();
                tasks.Add(service.LoadFileAsync(_testExcelPath));
            }
            
            await Task.WhenAll(tasks);

            // Assert
            Assert.IsTrue(tasks.All(t => t.IsCompletedSuccessfully));
        }
    }
}