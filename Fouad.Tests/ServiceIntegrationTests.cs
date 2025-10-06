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
    /// Integration tests for service interactions in the Fouad application.
    /// Tests cover the interaction between FileDataService, DatabaseService, and SearchService.
    /// </summary>
    [TestClass]
    public class ServiceIntegrationTests
    {
        private DatabaseService? _databaseService;
        private FileDataService? _fileDataService;
        private SearchService? _searchService;
        private string? _testDatabasePath;
        private string? _testExcelPath;

        /// <summary>
        /// Static constructor to set EPPlus license
        /// </summary>
        static ServiceIntegrationTests()
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
            
            // Create test files
            _testDatabasePath = Path.Combine(Path.GetTempPath(), $"test_database_{System.Guid.NewGuid()}.db");
            _testExcelPath = Path.Combine(Path.GetTempPath(), $"test_file_{System.Guid.NewGuid()}.xlsx");
            
            // Create services
            _databaseService = new DatabaseService(_testDatabasePath);
            _fileDataService = new FileDataService();
            var configurationService = new TestConfigurationService();
            _searchService = new SearchService(_fileDataService, configurationService);
            
            // Set up service dependencies
            _fileDataService.SetDatabaseService(_databaseService);
            
            // Create test Excel file
            CreateTestExcelFile(_testExcelPath);
        }

        /// <summary>
        /// Cleans up the test environment after each test method runs.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            // Dispose services
            if (_databaseService != null)
            {
                try
                {
                    _databaseService.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Error disposing DatabaseService: {ex.Message}");
                }
                _databaseService = null;
            }
            
            // Add delay to ensure file handles are released
            System.Threading.Thread.Sleep(100);
            
            // Clean up test files with retry logic
            if (!string.IsNullOrEmpty(_testDatabasePath) && File.Exists(_testDatabasePath))
            {
                DeleteFileWithRetry(_testDatabasePath);
            }
            
            if (!string.IsNullOrEmpty(_testExcelPath) && File.Exists(_testExcelPath))
            {
                DeleteFileWithRetry(_testExcelPath);
            }
            
            // Clean up archive folder
            if (!string.IsNullOrEmpty(_testDatabasePath))
            {
                var archiveFolderPath = Path.Combine(Path.GetDirectoryName(_testDatabasePath) ?? "", "Excel_Archive");
                if (Directory.Exists(archiveFolderPath))
                {
                    try
                    {
                        Directory.Delete(archiveFolderPath, true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: Error deleting archive folder: {ex.Message}");
                    }
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
            
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    File.Delete(filePath);
                    break;
                }
                catch (Exception)
                {
                    if (i == 9) 
                    {
                        Console.WriteLine($"Warning: Could not delete file {filePath} after 10 attempts");
                        break;
                    }
                    System.Threading.Thread.Sleep(200);
                }
            }
        }

        /// <summary>
        /// Creates a test Excel file.
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
        /// Tests the complete workflow: Import file → Index content → Search.
        /// </summary>
        [TestMethod]
        public async Task CompleteWorkflow_ImportIndexSearch_WorksCorrectly()
        {
            // Arrange
            var uploadedBy = "testuser";
            
            // Act - Step 1: Import file
            var archiveId = await _databaseService.ImportFileAsync(_testExcelPath, uploadedBy);
            
            // Step 2: Index file content
            await _databaseService.IndexFileContentAsync(_testExcelPath, archiveId);
            
            // Step 3: Load file in FileDataService
            await _fileDataService.LoadFileAsync(_testExcelPath);
            
            // Step 4: Search using SearchService
            var criteria = new SearchCriteria
            {
                SearchTerm = "John",
                MaxResults = 10
            };
            
            var results = await _searchService.AdvancedSearchAsync(criteria);

            // Assert
            Assert.IsTrue(archiveId > 0);
            Assert.IsNotNull(results);
            // At least one result should match "John"
            Assert.IsTrue(results.Count > 0);
        }

        /// <summary>
        /// Tests that FileDataService can work with DatabaseService for file import.
        /// </summary>
        [TestMethod]
        public async Task FileDataService_WithDatabaseService_ImportWorksCorrectly()
        {
            // Arrange
            var testFilePath = _testExcelPath;
            
            // Act
            var archiveId = await _fileDataService.ImportFileWithDatabaseAsync(testFilePath);
            
            // Assert
            Assert.IsTrue(archiveId > 0);
            
            // Verify the file was archived
            var archiveLog = await _databaseService.GetArchiveLogAsync(archiveId);
            Assert.IsNotNull(archiveLog);
            Assert.AreEqual("test_file_" + Path.GetFileNameWithoutExtension(testFilePath).Split('_').Last() + ".xlsx", archiveLog.FileName);
        }

        /// <summary>
        /// Tests that search results are properly filtered by column values.
        /// </summary>
        [TestMethod]
        public async Task SearchService_ColumnValueFiltering_WorksCorrectly()
        {
            // Arrange
            await _fileDataService.LoadFileAsync(_testExcelPath);
            
            var criteria = new SearchCriteria
            {
                SearchTerm = "John",
                ColumnValueFilters = new List<ColumnValueFilterCriteria>
                {
                    new ColumnValueFilterCriteria
                    {
                        ColumnName = "City",
                        Operator = "Contains",
                        Value = "New"
                    }
                },
                MaxResults = 10
            };

            // Act
            var results = await _searchService.AdvancedSearchAsync(criteria);

            // Assert
            Assert.IsNotNull(results);
            // Should find "John Doe" who lives in "New York"
            Assert.IsTrue(results.Count > 0);
        }

        /// <summary>
        /// Tests that search results are properly filtered by value ranges.
        /// </summary>
        [TestMethod]
        public async Task SearchService_ValueRangeFiltering_WorksCorrectly()
        {
            // Arrange
            await _fileDataService.LoadFileAsync(_testExcelPath);
            
            var criteria = new SearchCriteria
            {
                SearchTerm = "",
                ValueRangeFilters = new List<ValueRangeFilterCriteria>
                {
                    new ValueRangeFilterCriteria
                    {
                        ColumnName = "Age",
                        MinValue = "25",
                        MaxValue = "35"
                    }
                },
                MaxResults = 10
            };

            // Act
            var results = await _searchService.AdvancedSearchAsync(criteria);

            // Assert
            Assert.IsNotNull(results);
            // Should find people with ages between 25 and 35
            Assert.IsTrue(results.Count >= 3); // Jane (25), John (30), Bob (35)
        }

        /// <summary>
        /// Tests that search results are properly filtered by date ranges.
        /// </summary>
        [TestMethod]
        public async Task SearchService_DateRangeFiltering_WorksCorrectly()
        {
            // Arrange
            await _fileDataService.LoadFileAsync(_testExcelPath);
            
            var criteria = new SearchCriteria
            {
                SearchTerm = "",
                EnableDateRange = true,
                StartDate = System.DateTime.Now.AddDays(-1),
                EndDate = System.DateTime.Now.AddDays(1),
                MaxResults = 10
            };

            // Act
            var results = await _searchService.AdvancedSearchAsync(criteria);

            // Assert
            Assert.IsNotNull(results);
            // All results should be within the date range (since they were just loaded)
            Assert.IsTrue(results.Count > 0);
        }

        /// <summary>
        /// Tests concurrent access to services.
        /// </summary>
        [TestMethod]
        public async Task ConcurrentServiceAccess_WorksCorrectly()
        {
            // Act
            var tasks = new List<Task<int>>();
            for (int i = 0; i < 3; i++)
            {
                var filePath = Path.Combine(Path.GetTempPath(), $"concurrent_test_{i}.xlsx");
                CreateTestExcelFile(filePath);
                
                tasks.Add(_databaseService.ImportFileAsync(filePath, $"user{i}"));
            }
            
            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.AreEqual(3, results.Length);
            foreach (var result in results)
            {
                Assert.IsTrue(result > 0);
            }
        }

        /// <summary>
        /// Test configuration service for SearchService.
        /// </summary>
        private class TestConfigurationService : IConfigurationService
        {
            public T GetSetting<T>(string key, T defaultValue = default(T)) => defaultValue!;
            public void SaveSetting<T>(string key, T value) { }
            public int DefaultPageSize { get; set; } = 10;
            public int MaxSearchResults { get; set; } = 100;
            public bool EnableFuzzySearch { get; set; } = false;
            public bool EnableAudioFeedback { get; set; } = false;
        }
    }
}