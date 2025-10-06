using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fouad.Services;
using Fouad.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using System;

namespace Fouad.Tests
{
    /// <summary>
    /// Edge case tests for the SearchService class with advanced filtering capabilities.
    /// </summary>
    [TestClass]
    public class SearchServiceEdgeCaseTests
    {
        private SearchService? _searchService;
        private Mock<IFileDataService>? _mockFileDataService;
        private Mock<IConfigurationService>? _mockConfigurationService;

        /// <summary>
        /// Initializes the test environment before each test method runs.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            _mockFileDataService = new Mock<IFileDataService>();
            _mockConfigurationService = new Mock<IConfigurationService>();
            
            _searchService = new SearchService(_mockFileDataService.Object, _mockConfigurationService.Object);
        }

        /// <summary>
        /// Tests that AdvancedSearchAsync handles null DynamicColumnValues gracefully.
        /// </summary>
        [TestMethod]
        public async Task AdvancedSearchAsync_WithNullDynamicColumnValues_HandlesGracefully()
        {
            // Arrange
            var criteria = new SearchCriteria
            {
                SearchTerm = "test",
                MaxResults = 10
            };
            
            _mockFileDataService.Setup(x => x.IsFileLoaded()).Returns(true);
            _mockFileDataService.Setup(x => x.GetRowCount()).Returns(1);
            _mockFileDataService.Setup(x => x.GetResultById(1))
                .Returns(new Result { Id = 1, DynamicColumnValues = null }); // Null DynamicColumnValues
            _mockFileDataService.Setup(x => x.GetColumnHeaders())
                .Returns(new List<string> { "Column1" });

            // Act
            var results = await _searchService.AdvancedSearchAsync(criteria);

            // Assert
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Count); // Should not crash and return empty results
        }

        /// <summary>
        /// Tests that AdvancedSearchAsync handles empty DynamicColumnValues gracefully.
        /// </summary>
        [TestMethod]
        public async Task AdvancedSearchAsync_WithEmptyDynamicColumnValues_HandlesGracefully()
        {
            // Arrange
            var criteria = new SearchCriteria
            {
                SearchTerm = "test",
                MaxResults = 10
            };
            
            _mockFileDataService.Setup(x => x.IsFileLoaded()).Returns(true);
            _mockFileDataService.Setup(x => x.GetRowCount()).Returns(1);
            _mockFileDataService.Setup(x => x.GetResultById(1))
                .Returns(new Result { Id = 1, DynamicColumnValues = new List<string>() }); // Empty DynamicColumnValues
            _mockFileDataService.Setup(x => x.GetColumnHeaders())
                .Returns(new List<string> { "Column1" });

            // Act
            var results = await _searchService.AdvancedSearchAsync(criteria);

            // Assert
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Count); // Should not crash and return empty results
        }

        /// <summary>
        /// Tests that AdvancedSearchAsync handles regex with invalid pattern gracefully.
        /// </summary>
        [TestMethod]
        public async Task AdvancedSearchAsync_WithInvalidRegexPattern_HandlesGracefully()
        {
            // Arrange
            var criteria = new SearchCriteria
            {
                SearchTerm = "[", // Invalid regex pattern
                RegexSearch = true,
                MaxResults = 10
            };
            
            _mockFileDataService.Setup(x => x.IsFileLoaded()).Returns(true);
            _mockFileDataService.Setup(x => x.GetRowCount()).Returns(1);
            _mockFileDataService.Setup(x => x.GetResultById(1))
                .Returns(new Result { Id = 1, DynamicColumnValues = new List<string> { "test value" } });
            _mockFileDataService.Setup(x => x.GetColumnHeaders())
                .Returns(new List<string> { "Column1" });

            // Act
            var results = await _searchService.AdvancedSearchAsync(criteria);

            // Assert
            Assert.IsNotNull(results);
            // Should not crash and return results (regex search will fail but not throw)
        }

        /// <summary>
        /// Tests that AdvancedSearchAsync handles null result gracefully.
        /// </summary>
        [TestMethod]
        public async Task AdvancedSearchAsync_WithNullResult_HandlesGracefully()
        {
            // Arrange
            var criteria = new SearchCriteria
            {
                SearchTerm = "test",
                MaxResults = 10
            };
            
            _mockFileDataService.Setup(x => x.IsFileLoaded()).Returns(true);
            _mockFileDataService.Setup(x => x.GetRowCount()).Returns(1);
            _mockFileDataService.Setup(x => x.GetResultById(1))
                .Returns((Result)null); // Null result
            _mockFileDataService.Setup(x => x.GetColumnHeaders())
                .Returns(new List<string> { "Column1" });

            // Act
            var results = await _searchService.AdvancedSearchAsync(criteria);

            // Assert
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Count); // Should not crash and return empty results
        }

        /// <summary>
        /// Tests that AdvancedSearchAsync handles date range with null dates gracefully.
        /// </summary>
        [TestMethod]
        public async Task AdvancedSearchAsync_WithNullDateRange_HandlesGracefully()
        {
            // Arrange
            var criteria = new SearchCriteria
            {
                SearchTerm = "test",
                EnableDateRange = true,
                StartDate = null, // Null start date
                EndDate = null,   // Null end date
                MaxResults = 10
            };
            
            _mockFileDataService.Setup(x => x.IsFileLoaded()).Returns(true);
            _mockFileDataService.Setup(x => x.GetRowCount()).Returns(1);
            _mockFileDataService.Setup(x => x.GetResultById(1))
                .Returns(new Result { Id = 1, DynamicColumnValues = new List<string> { "test value" }, SearchDate = DateTime.Now });
            _mockFileDataService.Setup(x => x.GetColumnHeaders())
                .Returns(new List<string> { "Column1" });

            // Act
            var results = await _searchService.AdvancedSearchAsync(criteria);

            // Assert
            Assert.IsNotNull(results);
            // Should not crash and return results
        }

        /// <summary>
        /// Tests that AdvancedSearchAsync handles time range with invalid times gracefully.
        /// </summary>
        [TestMethod]
        public async Task AdvancedSearchAsync_WithInvalidTimeRange_HandlesGracefully()
        {
            // Arrange
            var criteria = new SearchCriteria
            {
                SearchTerm = "test",
                EnableTimeRange = true,
                StartTime = "invalid", // Invalid time format
                EndTime = "also invalid", // Invalid time format
                MaxResults = 10
            };
            
            _mockFileDataService.Setup(x => x.IsFileLoaded()).Returns(true);
            _mockFileDataService.Setup(x => x.GetRowCount()).Returns(1);
            _mockFileDataService.Setup(x => x.GetResultById(1))
                .Returns(new Result { Id = 1, DynamicColumnValues = new List<string> { "test value" }, SearchDate = DateTime.Now });
            _mockFileDataService.Setup(x => x.GetColumnHeaders())
                .Returns(new List<string> { "Column1" });

            // Act
            var results = await _searchService.AdvancedSearchAsync(criteria);

            // Assert
            Assert.IsNotNull(results);
            // Should not crash and return results
        }

        /// <summary>
        /// Tests that AdvancedSearchAsync handles column value filters with non-existent columns gracefully.
        /// </summary>
        [TestMethod]
        public async Task AdvancedSearchAsync_WithNonExistentColumnValueFilters_HandlesGracefully()
        {
            // Arrange
            var criteria = new SearchCriteria
            {
                SearchTerm = "test",
                ColumnValueFilters = new List<ColumnValueFilterCriteria>
                {
                    new ColumnValueFilterCriteria
                    {
                        ColumnName = "NonExistentColumn",
                        Operator = "=",
                        Value = "TestValue"
                    }
                },
                MaxResults = 10
            };
            
            _mockFileDataService.Setup(x => x.IsFileLoaded()).Returns(true);
            _mockFileDataService.Setup(x => x.GetRowCount()).Returns(1);
            _mockFileDataService.Setup(x => x.GetResultById(1))
                .Returns(new Result { Id = 1, DynamicColumnValues = new List<string> { "test value" } });
            _mockFileDataService.Setup(x => x.GetColumnHeaders())
                .Returns(new List<string> { "ExistingColumn" }); // Different column name

            // Act
            var results = await _searchService.AdvancedSearchAsync(criteria);

            // Assert
            Assert.IsNotNull(results);
            // Should not crash and return results
        }

        /// <summary>
        /// Tests that AdvancedSearchAsync handles value range filters with non-numeric values gracefully.
        /// </summary>
        [TestMethod]
        public async Task AdvancedSearchAsync_WithValueRangeFiltersNonNumeric_HandlesGracefully()
        {
            // Arrange
            var criteria = new SearchCriteria
            {
                SearchTerm = "test",
                ValueRangeFilters = new List<ValueRangeFilterCriteria>
                {
                    new ValueRangeFilterCriteria
                    {
                        ColumnName = "TextColumn",
                        MinValue = "abc", // Non-numeric min value
                        MaxValue = "xyz"  // Non-numeric max value
                    }
                },
                MaxResults = 10
            };
            
            _mockFileDataService.Setup(x => x.IsFileLoaded()).Returns(true);
            _mockFileDataService.Setup(x => x.GetRowCount()).Returns(1);
            _mockFileDataService.Setup(x => x.GetResultById(1))
                .Returns(new Result { Id = 1, DynamicColumnValues = new List<string> { "test value", "text data" } });
            _mockFileDataService.Setup(x => x.GetColumnHeaders())
                .Returns(new List<string> { "Description", "TextColumn" });

            // Act
            var results = await _searchService.AdvancedSearchAsync(criteria);

            // Assert
            Assert.IsNotNull(results);
            // Should not crash and return results
        }
    }
}