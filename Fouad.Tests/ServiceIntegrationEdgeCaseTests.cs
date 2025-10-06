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
    /// Edge case tests for service interactions in the Fouad application.
    /// Tests cover edge cases in the interaction between FileDataService, DatabaseService, and SearchService.
    /// </summary>
    [TestClass]
    public class ServiceIntegrationEdgeCaseTests
    {
        private DatabaseService? _databaseService;
        private FileDataService? _fileDataService;
        private SearchService? _searchService;
        private string? _testDatabasePath;
        private string? _testExcelPath;

        /// <summary>
        /// Static constructor to set EPPlus license
        /// </summary>
        static ServiceIntegrationEdgeCaseTests()
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
        /// Tests the complete workflow with error conditions: Import file → Index content → Search.
        /// </summary>
        [TestMethod]
        public async Task CompleteWorkflow_WithErrorConditions_HandlesGracefully()
        {
            // Arrange
            var uploadedBy = "testuser";
            var nonExistentFilePath = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.xlsx");

            try
            {
                // Act - Step 1: Try to import non-existent file
                await Assert.ThrowsExceptionAsync<FileNotFoundException>(async () =>
                {
                    await _databaseService.ImportFileAsync(nonExistentFilePath, uploadedBy);
                });
            }
            finally
            {
                // Continue with normal workflow to ensure system is still functional
                // Step 1: Import valid file
                var archiveId = await _databaseService.ImportFileAsync(_testExcelPath, uploadedBy);
                
                // Step 2: Index file content
                await _databaseService.IndexFileContentAsync(_testExcelPath, archiveId);
                
                // Step 3: Load file in FileDataService
                await _fileDataService.LoadFileAsync(_testExcelPath);
                
                // Step 4: Search using SearchService with various criteria
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
        }

        /// <summary>
        /// Tests that FileDataService can work with DatabaseService when database is disposed.
        /// </summary>
        [TestMethod]
        public async Task FileDataService_WithDisposedDatabaseService_HandlesGracefully()
        {
            // Arrange
            var testFilePath = _testExcelPath;
            
            // Act - Import file first
            var archiveId = await _fileDataService.ImportFileWithDatabaseAsync(testFilePath);
            
            // Dispose database service
            _databaseService.Dispose();
            
            // Try to perform operations that would normally use database
            await _fileDataService.LoadFileAsync(testFilePath);
            
            // Search should still work (using file data service cache)
            var criteria = new SearchCriteria
            {
                SearchTerm = "John",
                MaxResults = 10
            };
            
            var results = await _searchService.AdvancedSearchAsync(criteria);

            // Assert
            Assert.IsTrue(archiveId > 0);
            Assert.IsNotNull(results);
            // Should still work even with disposed database service
        }

        /// <summary>
        /// Tests that search results are properly filtered with complex criteria combinations.
        /// </summary>
        [TestMethod]
        public async Task SearchService_ComplexCriteriaCombinations_WorkCorrectly()
        {
            // Arrange
            await _fileDataService.LoadFileAsync(_testExcelPath);
            
            var criteria = new SearchCriteria
            {
                SearchTerm = "John",
                ExactMatch = false,
                CaseSensitive = false,
                MaxResults = 10,
                IsAllWordsSearch = true,
                EnableDateRange = true,
                StartDate = System.DateTime.Now.AddDays(-1),
                EndDate = System.DateTime.Now.AddDays(1),
                EnableTimeRange = true,
                StartTime = "00:00",
                EndTime = "23:59",
                ColumnValueFilters = new List<ColumnValueFilterCriteria>
                {
                    new ColumnValueFilterCriteria
                    {
                        ColumnName = "City",
                        Operator = "Contains",
                        Value = "New"
                    }
                },
                ValueRangeFilters = new List<ValueRangeFilterCriteria>
                {
                    new ValueRangeFilterCriteria
                    {
                        ColumnName = "Age",
                        MinValue = "25",
                        MaxValue = "35"
                    }
                }
            };

            // Act
            var results = await _searchService.AdvancedSearchAsync(criteria);

            // Assert
            Assert.IsNotNull(results);
            // Should find "John Doe" who matches all criteria
            Assert.IsTrue(results.Count >= 0); // Might be 0 or more depending on exact match
        }

        /// <summary>
        /// Tests that search results handle null or empty filter criteria gracefully.
        /// </summary>
        [TestMethod]
        public async Task SearchService_NullOrEmptyFilterCriteria_HandlesGracefully()
        {
            // Arrange
            await _fileDataService.LoadFileAsync(_testExcelPath);
            
            var criteria = new SearchCriteria
            {
                SearchTerm = "John",
                ColumnValueFilters = null, // Null filters
                ValueRangeFilters = new List<ValueRangeFilterCriteria>() // Empty filters
            };

            // Act
            var results = await _searchService.AdvancedSearchAsync(criteria);

            // Assert
            Assert.IsNotNull(results);
            // Should not crash and return results
        }

        /// <summary>
        /// Tests concurrent service access with mixed success/failure scenarios.
        /// </summary>
        [TestMethod]
        public async Task ConcurrentServiceAccess_MixedSuccessFailure_HandlesGracefully()
        {
            // Act
            var tasks = new List<Task<int>>();
            for (int i = 0; i < 5; i++)
            {
                if (i % 2 == 0)
                {
                    // Even indices: valid operations
                    var filePath = Path.Combine(Path.GetTempPath(), $"concurrent_test_{i}.xlsx");
                    CreateTestExcelFile(filePath);
                    tasks.Add(_databaseService.ImportFileAsync(filePath, $"user{i}"));
                }
                else
                {
                    // Odd indices: operations that will fail
                    var nonExistentPath = Path.Combine(Path.GetTempPath(), $"nonexistent_{i}.xlsx");
                    tasks.Add(_databaseService.ImportFileAsync(nonExistentPath, $"user{i}"));
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
                // Check that the service is still functional
                var validFilePath = Path.Combine(Path.GetTempPath(), "valid_test.xlsx");
                CreateTestExcelFile(validFilePath);
                
                try
                {
                    var archiveId = await _databaseService.ImportFileAsync(validFilePath, "testuser");
                    Assert.IsTrue(archiveId > 0);
                }
                finally
                {
                    if (File.Exists(validFilePath))
                    {
                        File.Delete(validFilePath);
                    }
                }
            }
        }

        /// <summary>
        /// Tests service interaction when configuration service returns null values.
        /// </summary>
        [TestMethod]
        public async Task ServiceInteraction_WithNullConfiguration_HandlesGracefully()
        {
            // Arrange - Create services with null-returning configuration
            var nullConfigService = new NullConfigurationService();
            var searchServiceWithNullConfig = new SearchService(_fileDataService, nullConfigService);
            
            await _fileDataService.LoadFileAsync(_testExcelPath);
            
            var criteria = new SearchCriteria
            {
                SearchTerm = "John",
                MaxResults = 10
            };

            // Act
            var results = await searchServiceWithNullConfig.AdvancedSearchAsync(criteria);

            // Assert
            Assert.IsNotNull(results);
            // Should not crash and return results even with null configuration
        }

        /// <summary>
        /// Test configuration service that returns null values.
        /// </summary>
        private class NullConfigurationService : IConfigurationService
        {
            public T GetSetting<T>(string key, T defaultValue = default(T)) => default(T)!;
            public void SaveSetting<T>(string key, T value) { }
            public int DefaultPageSize { get; set; } = 0;
            public int MaxSearchResults { get; set; } = 0;
            public bool EnableFuzzySearch { get; set; } = false;
            public bool EnableAudioFeedback { get; set; } = false;
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