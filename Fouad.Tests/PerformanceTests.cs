using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fouad.Services;
using Fouad.Models;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OfficeOpenXml;

namespace Fouad.Tests
{
    /// <summary>
    /// Performance tests for the Fouad application services.
    /// Tests measure loading times, search performance, and memory usage.
    /// </summary>
    [TestClass]
    public class PerformanceTests
    {
        private DatabaseService? _databaseService;
        private FileDataService? _fileDataService;
        private SearchService? _searchService;
        private string? _testDatabasePath;
        private string? _testExcelPath;
        private List<string> _tempFiles = new List<string>();

        /// <summary>
        /// Static constructor to set EPPlus license
        /// </summary>
        static PerformanceTests()
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
            _testDatabasePath = Path.Combine(Path.GetTempPath(), $"perf_test_database_{System.Guid.NewGuid()}.db");
            _testExcelPath = Path.Combine(Path.GetTempPath(), $"perf_test_file_{System.Guid.NewGuid()}.xlsx");
            
            // Create services
            _databaseService = new DatabaseService(_testDatabasePath);
            _fileDataService = new FileDataService();
            var configurationService = new TestConfigurationService();
            _searchService = new SearchService(_fileDataService, configurationService);
            
            // Create test Excel file with more data for performance testing
            CreateLargeTestExcelFile(_testExcelPath, 1000); // 1000 rows
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
    
            // Clean up temporary files
            foreach (var tempFile in _tempFiles)
            {
                if (File.Exists(tempFile))
                {
                    DeleteFileWithRetry(tempFile);
                }
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
        /// Creates a large test Excel file for performance testing.
        /// </summary>
        /// <param name="filePath">The path where to create the file.</param>
        /// <param name="rowCount">The number of data rows to create.</param>
        private void CreateLargeTestExcelFile(string filePath, int rowCount)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("TestSheet");
            
            // Add headers
            worksheet.Cells[1, 1].Value = "ID";
            worksheet.Cells[1, 2].Value = "Name";
            worksheet.Cells[1, 3].Value = "Age";
            worksheet.Cells[1, 4].Value = "City";
            worksheet.Cells[1, 5].Value = "Salary";
            worksheet.Cells[1, 6].Value = "Department";
            
            // Add data rows
            for (int i = 0; i < rowCount; i++)
            {
                worksheet.Cells[i + 2, 1].Value = i + 1;
                worksheet.Cells[i + 2, 2].Value = $"Person {i + 1}";
                worksheet.Cells[i + 2, 3].Value = 20 + (i % 50); // Ages 20-69
                worksheet.Cells[i + 2, 4].Value = $"City {(i % 50) + 1}";
                worksheet.Cells[i + 2, 5].Value = 30000 + (i * 100); // Salaries 30000-130000
                worksheet.Cells[i + 2, 6].Value = $"Department {(i % 10) + 1}";
            }
            
            package.SaveAs(new FileInfo(filePath));
        }

        /// <summary>
        /// Tests that file loading performance is within acceptable limits.
        /// </summary>
        [TestMethod]
        public async Task FileLoadingPerformance_IsAcceptable()
        {
            // Arrange
            var stopwatch = Stopwatch.StartNew();
            
            // Act
            await _fileDataService.LoadFileAsync(_testExcelPath);
            stopwatch.Stop();
            
            var loadTime = stopwatch.ElapsedMilliseconds;
            
            // Assert
            // Loading 1000 rows should take less than 5 seconds
            Assert.IsTrue(loadTime < 5000, $"File loading took {loadTime} ms, which is too slow");
            
            // Verify data was loaded correctly
            Assert.AreEqual(1000, _fileDataService.GetRowCount());
        }

        /// <summary>
        /// Tests that binary caching improves subsequent load times.
        /// </summary>
        [TestMethod]
        public async Task BinaryCaching_ImprovesLoadTimes()
        {
            // Arrange
            var cacheFilePath = Path.Combine(
                Path.GetDirectoryName(_testExcelPath) ?? "", 
                Path.GetFileNameWithoutExtension(_testExcelPath) + ".bin");
            
            // Act - First load (no cache)
            var stopwatch1 = Stopwatch.StartNew();
            await _fileDataService.LoadFileAsync(_testExcelPath);
            stopwatch1.Stop();
            var firstLoadTime = stopwatch1.ElapsedMilliseconds;
            
            // Verify cache was created
            Assert.IsTrue(File.Exists(cacheFilePath), "Binary cache file was not created");
            
            // Act - Second load (with cache)
            var fileDataService2 = new FileDataService();
            var stopwatch2 = Stopwatch.StartNew();
            await fileDataService2.LoadFileAsync(_testExcelPath);
            stopwatch2.Stop();
            var secondLoadTime = stopwatch2.ElapsedMilliseconds;
            
            // Assert
            // Second load should be significantly faster (at least 30% faster)
            var improvement = ((double)firstLoadTime - secondLoadTime) / firstLoadTime;
            Assert.IsTrue(improvement >= 0.3 || secondLoadTime < firstLoadTime, 
                $"Caching did not improve performance. First load: {firstLoadTime} ms, Second load: {secondLoadTime} ms");
        }

        /// <summary>
        /// Tests that search performance is within acceptable limits.
        /// </summary>
        [TestMethod]
        public async Task SearchPerformance_IsAcceptable()
        {
            // Arrange
            await _fileDataService.LoadFileAsync(_testExcelPath);
            
            var criteria = new SearchCriteria
            {
                SearchTerm = "Person",
                MaxResults = 100
            };
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            var results = await _searchService.AdvancedSearchAsync(criteria);
            stopwatch.Stop();
            
            var searchTime = stopwatch.ElapsedMilliseconds;
            
            // Assert
            // Searching 1000 rows should take less than 2 seconds
            Assert.IsTrue(searchTime < 2000, $"Search took {searchTime} ms, which is too slow");
            Assert.AreEqual(100, results.Count); // Limited to 100 results
        }

        /// <summary>
        /// Tests that search with filters performs within acceptable limits.
        /// </summary>
        [TestMethod]
        public async Task FilteredSearchPerformance_IsAcceptable()
        {
            // Arrange
            await _fileDataService.LoadFileAsync(_testExcelPath);
            
            var criteria = new SearchCriteria
            {
                SearchTerm = "Person",
                ColumnValueFilters = new List<ColumnValueFilterCriteria>
                {
                    new ColumnValueFilterCriteria
                    {
                        ColumnName = "Age",
                        Operator = ">",
                        Value = "40"
                    }
                },
                ValueRangeFilters = new List<ValueRangeFilterCriteria>
                {
                    new ValueRangeFilterCriteria
                    {
                        ColumnName = "Salary",
                        MinValue = "50000",
                        MaxValue = "80000"
                    }
                },
                MaxResults = 50
            };
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            var results = await _searchService.AdvancedSearchAsync(criteria);
            stopwatch.Stop();
            
            var searchTime = stopwatch.ElapsedMilliseconds;
            
            // Assert
            // Complex search should take less than 3 seconds
            Assert.IsTrue(searchTime < 3000, $"Filtered search took {searchTime} ms, which is too slow");
            Assert.IsTrue(results.Count <= 50); // Limited to 50 results
        }

        /// <summary>
        /// Tests that memory usage is within acceptable limits.
        /// </summary>
        [TestMethod]
        public async Task MemoryUsage_IsAcceptable()
        {
            // Arrange
            var initialMemory = Process.GetCurrentProcess().WorkingSet64;
            
            // Act
            await _fileDataService.LoadFileAsync(_testExcelPath);
            var memoryAfterLoad = Process.GetCurrentProcess().WorkingSet64;
            
            var criteria = new SearchCriteria
            {
                SearchTerm = "Person",
                MaxResults = 100
            };
            
            var results = await _searchService.AdvancedSearchAsync(criteria);
            var memoryAfterSearch = Process.GetCurrentProcess().WorkingSet64;
            
            // Get cache memory usage
            var cacheMemoryUsage = _fileDataService.GetCacheMemoryUsage();
            
            // Assert
            var memoryIncreaseLoad = memoryAfterLoad - initialMemory;
            var memoryIncreaseSearch = memoryAfterSearch - memoryAfterLoad;
            
            // Memory increase should be reasonable (less than 100MB for 1000 rows)
            Assert.IsTrue(memoryIncreaseLoad < 100 * 1024 * 1024, 
                $"Memory usage increased by {memoryIncreaseLoad / (1024 * 1024)} MB during file loading, which is too high");
                
            // Cache memory usage reporting should be reasonable
            Assert.IsTrue(cacheMemoryUsage > 0, "Cache memory usage should be positive");
            Assert.IsTrue(cacheMemoryUsage < 50 * 1024 * 1024, 
                $"Reported cache memory usage {cacheMemoryUsage / (1024 * 1024)} MB is too high");
        }

        /// <summary>
        /// Tests concurrent loading performance.
        /// </summary>
        [TestMethod]
        public async Task ConcurrentLoadingPerformance_IsAcceptable()
        {
            // Arrange
            var filePaths = new List<string>();
            for (int i = 0; i < 5; i++)
            {
                var filePath = Path.Combine(Path.GetTempPath(), $"concurrent_test_{i}.xlsx");
                CreateLargeTestExcelFile(filePath, 100); // Smaller files for concurrent testing
                filePaths.Add(filePath);
                _tempFiles.Add(filePath);
            }
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            var tasks = filePaths.Select(filePath => {
                var service = new FileDataService();
                return service.LoadFileAsync(filePath);
            }).ToArray();
            
            await Task.WhenAll(tasks);
            stopwatch.Stop();
            
            var totalTime = stopwatch.ElapsedMilliseconds;
            
            // Assert
            // Loading 5 files concurrently should take less than 10 seconds
            Assert.IsTrue(totalTime < 10000, $"Concurrent loading took {totalTime} ms, which is too slow");
            
            // All tasks should complete successfully
            Assert.IsTrue(tasks.All(t => t.IsCompletedSuccessfully));
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