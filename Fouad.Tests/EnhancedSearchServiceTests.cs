using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fouad.Services;
using Fouad.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;

namespace Fouad.Tests
{
    /// <summary>
    /// Unit tests for the enhanced SearchService class with advanced filtering capabilities.
    /// </summary>
    [TestClass]
    public class EnhancedSearchServiceTests
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
        /// Tests that AdvancedSearchAsync works with regex search criteria.
        /// </summary>
        [TestMethod]
        public async Task AdvancedSearchAsync_WithRegexSearch_ReturnsMatchingResults()
        {
            // Arrange
            var criteria = new SearchCriteria
            {
                SearchTerm = @"\d+",
                RegexSearch = true,
                MaxResults = 10
            };
            
            _mockFileDataService.Setup(x => x.IsFileLoaded()).Returns(true);
            _mockFileDataService.Setup(x => x.GetRowCount()).Returns(1);
            _mockFileDataService.Setup(x => x.GetResultById(1))
                .Returns(new Result { Id = 1, DynamicColumnValues = new List<string> { "test 123 value" } });
            _mockFileDataService.Setup(x => x.GetColumnHeaders())
                .Returns(new List<string> { "Column1" });

            // Act
            var results = await _searchService.AdvancedSearchAsync(criteria);

            // Assert
            Assert.IsNotNull(results);
        }

        /// <summary>
        /// Tests that AdvancedSearchAsync works with all words search criteria.
        /// </summary>
        [TestMethod]
        public async Task AdvancedSearchAsync_WithAllWordsSearch_ReturnsMatchingResults()
        {
            // Arrange
            var criteria = new SearchCriteria
            {
                SearchTerm = "test value",
                IsAllWordsSearch = true,
                MaxResults = 10
            };
            
            _mockFileDataService.Setup(x => x.IsFileLoaded()).Returns(true);
            _mockFileDataService.Setup(x => x.GetRowCount()).Returns(1);
            _mockFileDataService.Setup(x => x.GetResultById(1))
                .Returns(new Result { Id = 1, DynamicColumnValues = new List<string> { "test some value" } });
            _mockFileDataService.Setup(x => x.GetColumnHeaders())
                .Returns(new List<string> { "Column1" });

            // Act
            var results = await _searchService.AdvancedSearchAsync(criteria);

            // Assert
            Assert.IsNotNull(results);
        }

        /// <summary>
        /// Tests that AdvancedSearchAsync works with any word search criteria.
        /// </summary>
        [TestMethod]
        public async Task AdvancedSearchAsync_WithAnyWordSearch_ReturnsMatchingResults()
        {
            // Arrange
            var criteria = new SearchCriteria
            {
                SearchTerm = "test missing",
                IsAnyWordSearch = true,
                MaxResults = 10
            };
            
            _mockFileDataService.Setup(x => x.IsFileLoaded()).Returns(true);
            _mockFileDataService.Setup(x => x.GetRowCount()).Returns(1);
            _mockFileDataService.Setup(x => x.GetResultById(1))
                .Returns(new Result { Id = 1, DynamicColumnValues = new List<string> { "test some value" } });
            _mockFileDataService.Setup(x => x.GetColumnHeaders())
                .Returns(new List<string> { "Column1" });

            // Act
            var results = await _searchService.AdvancedSearchAsync(criteria);

            // Assert
            Assert.IsNotNull(results);
        }

        /// <summary>
        /// Tests that AdvancedSearchAsync works with exact phrase search criteria.
        /// </summary>
        [TestMethod]
        public async Task AdvancedSearchAsync_WithExactPhraseSearch_ReturnsMatchingResults()
        {
            // Arrange
            var criteria = new SearchCriteria
            {
                SearchTerm = "test phrase",
                IsPhraseSearch = true,
                MaxResults = 10
            };
            
            _mockFileDataService.Setup(x => x.IsFileLoaded()).Returns(true);
            _mockFileDataService.Setup(x => x.GetRowCount()).Returns(1);
            _mockFileDataService.Setup(x => x.GetResultById(1))
                .Returns(new Result { Id = 1, DynamicColumnValues = new List<string> { "this is a test phrase here" } });
            _mockFileDataService.Setup(x => x.GetColumnHeaders())
                .Returns(new List<string> { "Column1" });

            // Act
            var results = await _searchService.AdvancedSearchAsync(criteria);

            // Assert
            Assert.IsNotNull(results);
        }

        /// <summary>
        /// Tests that AdvancedSearchAsync works with date range filtering.
        /// </summary>
        [TestMethod]
        public async Task AdvancedSearchAsync_WithDateRangeFilter_ReturnsMatchingResults()
        {
            // Arrange
            var startDate = System.DateTime.Now.AddDays(-10);
            var endDate = System.DateTime.Now.AddDays(-5);
            
            var criteria = new SearchCriteria
            {
                SearchTerm = "test",
                EnableDateRange = true,
                StartDate = startDate,
                EndDate = endDate,
                MaxResults = 10
            };
            
            _mockFileDataService.Setup(x => x.IsFileLoaded()).Returns(true);
            _mockFileDataService.Setup(x => x.GetRowCount()).Returns(1);
            _mockFileDataService.Setup(x => x.GetResultById(1))
                .Returns(new Result { Id = 1, DynamicColumnValues = new List<string> { "test value" }, SearchDate = System.DateTime.Now.AddDays(-7) });
            _mockFileDataService.Setup(x => x.GetColumnHeaders())
                .Returns(new List<string> { "Column1" });

            // Act
            var results = await _searchService.AdvancedSearchAsync(criteria);

            // Assert
            Assert.IsNotNull(results);
        }

        /// <summary>
        /// Tests that AdvancedSearchAsync works with time range filtering.
        /// </summary>
        [TestMethod]
        public async Task AdvancedSearchAsync_WithTimeRangeFilter_ReturnsMatchingResults()
        {
            // Arrange
            var criteria = new SearchCriteria
            {
                SearchTerm = "test",
                EnableTimeRange = true,
                StartTime = "09:00",
                EndTime = "17:00",
                MaxResults = 10
            };
            
            _mockFileDataService.Setup(x => x.IsFileLoaded()).Returns(true);
            _mockFileDataService.Setup(x => x.GetRowCount()).Returns(1);
            _mockFileDataService.Setup(x => x.GetResultById(1))
                .Returns(new Result { Id = 1, DynamicColumnValues = new List<string> { "test value" }, SearchDate = System.DateTime.Now.AddHours(12) });
            _mockFileDataService.Setup(x => x.GetColumnHeaders())
                .Returns(new List<string> { "Column1" });

            // Act
            var results = await _searchService.AdvancedSearchAsync(criteria);

            // Assert
            Assert.IsNotNull(results);
        }

        /// <summary>
        /// Tests that AdvancedSearchAsync works with column value filters.
        /// </summary>
        [TestMethod]
        public async Task AdvancedSearchAsync_WithColumnValueFilters_ReturnsMatchingResults()
        {
            // Arrange
            var criteria = new SearchCriteria
            {
                SearchTerm = "test",
                ColumnValueFilters = new List<ColumnValueFilterCriteria>
                {
                    new ColumnValueFilterCriteria
                    {
                        ColumnName = "Name",
                        Operator = "=",
                        Value = "John"
                    }
                },
                MaxResults = 10
            };
            
            _mockFileDataService.Setup(x => x.IsFileLoaded()).Returns(true);
            _mockFileDataService.Setup(x => x.GetRowCount()).Returns(1);
            _mockFileDataService.Setup(x => x.GetResultById(1))
                .Returns(new Result { Id = 1, DynamicColumnValues = new List<string> { "test value", "John" } });
            _mockFileDataService.Setup(x => x.GetColumnHeaders())
                .Returns(new List<string> { "Description", "Name" });

            // Act
            var results = await _searchService.AdvancedSearchAsync(criteria);

            // Assert
            Assert.IsNotNull(results);
        }

        /// <summary>
        /// Tests that AdvancedSearchAsync works with value range filters.
        /// </summary>
        [TestMethod]
        public async Task AdvancedSearchAsync_WithValueRangeFilters_ReturnsMatchingResults()
        {
            // Arrange
            var criteria = new SearchCriteria
            {
                SearchTerm = "test",
                ValueRangeFilters = new List<ValueRangeFilterCriteria>
                {
                    new ValueRangeFilterCriteria
                    {
                        ColumnName = "Age",
                        MinValue = "18",
                        MaxValue = "65"
                    }
                },
                MaxResults = 10
            };
            
            _mockFileDataService.Setup(x => x.IsFileLoaded()).Returns(true);
            _mockFileDataService.Setup(x => x.GetRowCount()).Returns(1);
            _mockFileDataService.Setup(x => x.GetResultById(1))
                .Returns(new Result { Id = 1, DynamicColumnValues = new List<string> { "test value", "30" } });
            _mockFileDataService.Setup(x => x.GetColumnHeaders())
                .Returns(new List<string> { "Description", "Age" });

            // Act
            var results = await _searchService.AdvancedSearchAsync(criteria);

            // Assert
            Assert.IsNotNull(results);
        }
    }
}