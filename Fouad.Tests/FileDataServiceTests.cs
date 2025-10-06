using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fouad.Services;
using Fouad.Models;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Fouad.Tests
{
    /// <summary>
    /// Unit tests for the FileDataService class.
    /// Tests cover file loading, searching, and data retrieval functionality.
    /// </summary>
    [TestClass]
    public class FileDataServiceTests
    {
        private FileDataService _fileDataService;
        private string _testCsvPath;
        private string _testExcelPath;

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
        }

        /// <summary>
        /// Tests that the FileDataService can be instantiated correctly.
        /// </summary>
        [TestMethod]
        public void FileDataService_Constructor_CreatesInstance()
        {
            // Arrange & Act
            var service = new FileDataService();

            // Assert
            Assert.IsNotNull(service);
        }

        /// <summary>
        /// Tests that IsFileLoaded returns false when no file has been loaded.
        /// </summary>
        [TestMethod]
        public void IsFileLoaded_ReturnsFalse_WhenNoFileLoaded()
        {
            // Arrange
            var service = new FileDataService();

            // Act
            var result = service.IsFileLoaded();

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that LoadFileAsync correctly loads a CSV file.
        /// </summary>
        [TestMethod]
        public async Task LoadFileAsync_LoadsCsvFile_Successfully()
        {
            // Arrange
            var service = new FileDataService();

            // Act
            await service.LoadFileAsync(_testCsvPath);

            // Assert
            Assert.IsTrue(service.IsFileLoaded());
            Assert.AreEqual(2, service.GetRowCount()); // Header + 2 data rows = 2 data rows
        }

        /// <summary>
        /// Tests that GetColumnHeaders returns the correct column headers from a CSV file.
        /// </summary>
        [TestMethod]
        public async Task GetColumnHeaders_ReturnsCorrectHeaders_ForCsvFile()
        {
            // Arrange
            var service = new FileDataService();
            await service.LoadFileAsync(_testCsvPath);

            // Act
            var headers = service.GetColumnHeaders();

            // Assert
            Assert.IsNotNull(headers);
            Assert.AreEqual(3, headers.Count);
            Assert.AreEqual("Name", headers[0]);
            Assert.AreEqual("Age", headers[1]);
            Assert.AreEqual("City", headers[2]);
        }

        /// <summary>
        /// Tests that Search method returns correct results for a search term.
        /// </summary>
        [TestMethod]
        public async Task Search_ReturnsMatchingRows_ForValidSearchTerm()
        {
            // Arrange
            var service = new FileDataService();
            await service.LoadFileAsync(_testCsvPath);

            // Act
            var results = service.Search("John");

            // Assert
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Count > 0);
        }

        /// <summary>
        /// Tests that GetResultById returns the correct result for a valid ID.
        /// </summary>
        [TestMethod]
        public async Task GetResultById_ReturnsCorrectResult_ForValidId()
        {
            // Arrange
            var service = new FileDataService();
            await service.LoadFileAsync(_testCsvPath);

            // Act
            var result = service.GetResultById(2); // Second data row

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Id);
            // Debug the actual values
            Assert.IsNotNull(result.DynamicColumnValues);
            Assert.IsTrue(result.DynamicColumnValues.Count > 0, $"DynamicColumnValues should not be empty. Actual count: {result.DynamicColumnValues.Count}");
            // Let's check what's actually in the values
            for (int i = 0; i < result.DynamicColumnValues.Count; i++)
            {
                Assert.IsFalse(string.IsNullOrEmpty(result.DynamicColumnValues[i]), $"Value at index {i} should not be null or empty");
            }
            // Check if "John" is in any of the values
            bool containsJohn = result.DynamicColumnValues.Any(v => v.Contains("John"));
            Assert.IsTrue(containsJohn, $"DynamicColumnValues should contain 'John'. Actual values: {string.Join(", ", result.DynamicColumnValues)}");
        }

        /// <summary>
        /// Tests that GetResultById returns null for an invalid ID.
        /// </summary>
        [TestMethod]
        public async Task GetResultById_ReturnsNull_ForInvalidId()
        {
            // Arrange
            var service = new FileDataService();
            await service.LoadFileAsync(_testCsvPath);

            // Act
            var result = service.GetResultById(999);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that UpdateResult correctly updates a result in the cache.
        /// </summary>
        [TestMethod]
        public async Task UpdateResult_UpdatesResultInCache_Successfully()
        {
            // Arrange
            var service = new FileDataService();
            await service.LoadFileAsync(_testCsvPath);
            var originalResult = service.GetResultById(2);
            Assert.IsNotNull(originalResult);
            
            // Modify the result
            originalResult.IsAddedToHistory = true;

            // Act
            service.UpdateResult(originalResult);

            // Assert
            var updatedResult = service.GetResultById(2);
            Assert.IsNotNull(updatedResult);
            Assert.IsTrue(updatedResult.IsAddedToHistory);
        }

        /// <summary>
        /// Creates a sample CSV file with test data.
        /// </summary>
        /// <param name="filePath">The path where the CSV file should be created.</param>
        private void CreateSampleCsvFile(string filePath)
        {
            var lines = new List<string>
            {
                "Name,Age,City",
                "John Doe,30,New York",
                "Jane Smith,25,Los Angeles"
            };

            File.WriteAllLines(filePath, lines);
        }
    }
}