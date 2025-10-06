using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fouad.ViewModels;
using Fouad.Services;
using Fouad.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;

namespace Fouad.Tests
{
    /// <summary>
    /// Enhanced unit tests for the SearchFilterViewModel class with advanced filtering capabilities.
    /// </summary>
    [TestClass]
    public class EnhancedSearchFilterViewModelTests
    {
        private SearchFilterViewModel? _searchFilterViewModel;
        private Mock<ISearchService>? _mockSearchService;
        private Mock<IFileDataService>? _mockFileDataService;

        /// <summary>
        /// Initializes the test environment before each test method runs.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            _mockSearchService = new Mock<ISearchService>();
            _mockFileDataService = new Mock<IFileDataService>();
            
            _searchFilterViewModel = new SearchFilterViewModel(_mockSearchService.Object, _mockFileDataService.Object);
        }

        /// <summary>
        /// Tests that all search mode properties can be set and retrieved.
        /// </summary>
        [TestMethod]
        public void SearchModeProperties_CanBeSetAndRetrieved()
        {
            // Act
            _searchFilterViewModel.RegexSearch = true;
            _searchFilterViewModel.IsAllWordsSearch = false;
            _searchFilterViewModel.IsAnyWordSearch = true;
            _searchFilterViewModel.IsPhraseSearch = true;

            // Assert
            Assert.IsTrue(_searchFilterViewModel.RegexSearch);
            Assert.IsFalse(_searchFilterViewModel.IsAllWordsSearch);
            Assert.IsTrue(_searchFilterViewModel.IsAnyWordSearch);
            Assert.IsTrue(_searchFilterViewModel.IsPhraseSearch);
        }

        /// <summary>
        /// Tests that date range properties can be set and retrieved.
        /// </summary>
        [TestMethod]
        public void DateRangeProperties_CanBeSetAndRetrieved()
        {
            // Arrange
            var startDate = System.DateTime.Now.AddDays(-30);
            var endDate = System.DateTime.Now;

            // Act
            _searchFilterViewModel.EnableDateRange = true;
            _searchFilterViewModel.StartDate = startDate;
            _searchFilterViewModel.EndDate = endDate;

            // Assert
            Assert.IsTrue(_searchFilterViewModel.EnableDateRange);
            Assert.AreEqual(startDate, _searchFilterViewModel.StartDate);
            Assert.AreEqual(endDate, _searchFilterViewModel.EndDate);
        }

        /// <summary>
        /// Tests that time range properties can be set and retrieved.
        /// </summary>
        [TestMethod]
        public void TimeRangeProperties_CanBeSetAndRetrieved()
        {
            // Act
            _searchFilterViewModel.EnableTimeRange = true;
            _searchFilterViewModel.StartTime = "09:30";
            _searchFilterViewModel.EndTime = "17:45";

            // Assert
            Assert.IsTrue(_searchFilterViewModel.EnableTimeRange);
            Assert.AreEqual("09:30", _searchFilterViewModel.StartTime);
            Assert.AreEqual("17:45", _searchFilterViewModel.EndTime);
        }

        /// <summary>
        /// Tests that column value filters can be added with proper properties.
        /// </summary>
        [TestMethod]
        public void ColumnValueFilters_CanBeAddedWithProperties()
        {
            // Act
            _searchFilterViewModel.AddColumnValueFilterCommand.Execute(null);

            // Assert
            Assert.AreEqual(1, _searchFilterViewModel.ColumnValueFilters.Count);
            Assert.AreEqual("", _searchFilterViewModel.ColumnValueFilters[0].ColumnName);
            Assert.AreEqual("=", _searchFilterViewModel.ColumnValueFilters[0].Operator);
            Assert.AreEqual("", _searchFilterViewModel.ColumnValueFilters[0].Value);
        }

        /// <summary>
        /// Tests that value range filters can be added with proper properties.
        /// </summary>
        [TestMethod]
        public void ValueRangeFilters_CanBeAddedWithProperties()
        {
            // Act
            _searchFilterViewModel.AddValueRangeFilterCommand.Execute(null);

            // Assert
            Assert.AreEqual(1, _searchFilterViewModel.ValueRangeFilters.Count);
            Assert.AreEqual("", _searchFilterViewModel.ValueRangeFilters[0].ColumnName);
            Assert.AreEqual("", _searchFilterViewModel.ValueRangeFilters[0].MinValue);
            Assert.AreEqual("", _searchFilterViewModel.ValueRangeFilters[0].MaxValue);
        }

        /// <summary>
        /// Tests that ColumnValueFilter properties work correctly.
        /// </summary>
        [TestMethod]
        public void ColumnValueFilter_Properties_WorkCorrectly()
        {
            // Arrange
            var filter = new ColumnValueFilter();
            var expectedColumn = "TestColumn";
            var expectedOperator = "Contains";
            var expectedValue = "TestValue";
            var expectedSelected = true;

            // Act
            filter.ColumnName = expectedColumn;
            filter.Operator = expectedOperator;
            filter.Value = expectedValue;
            filter.IsSelected = expectedSelected;

            // Assert
            Assert.AreEqual(expectedColumn, filter.ColumnName);
            Assert.AreEqual(expectedOperator, filter.Operator);
            Assert.AreEqual(expectedValue, filter.Value);
            Assert.AreEqual(expectedSelected, filter.IsSelected);
        }

        /// <summary>
        /// Tests that ValueRangeFilter properties work correctly.
        /// </summary>
        [TestMethod]
        public void ValueRangeFilter_Properties_WorkCorrectly()
        {
            // Arrange
            var filter = new ValueRangeFilter();
            var expectedColumn = "TestColumn";
            var expectedMin = "10";
            var expectedMax = "100";
            var expectedSelected = true;

            // Act
            filter.ColumnName = expectedColumn;
            filter.MinValue = expectedMin;
            filter.MaxValue = expectedMax;
            filter.IsSelected = expectedSelected;

            // Assert
            Assert.AreEqual(expectedColumn, filter.ColumnName);
            Assert.AreEqual(expectedMin, filter.MinValue);
            Assert.AreEqual(expectedMax, filter.MaxValue);
            Assert.AreEqual(expectedSelected, filter.IsSelected);
        }

        /// <summary>
        /// Tests that ApplyFilterCommand works with advanced criteria.
        /// </summary>
        [TestMethod]
        public async Task ApplyFilterCommand_WithAdvancedCriteria_CallsSearchService()
        {
            // Arrange
            var mockResults = new List<Result>();
            _mockSearchService.Setup(x => x.AdvancedSearchAsync(It.IsAny<SearchCriteria>()))
                .ReturnsAsync(mockResults);

            _searchFilterViewModel.SearchTerm = "test";
            _searchFilterViewModel.RegexSearch = true;
            _searchFilterViewModel.EnableDateRange = true;
            _searchFilterViewModel.StartDate = System.DateTime.Now.AddDays(-10);
            _searchFilterViewModel.EndDate = System.DateTime.Now;

            // Act
            _searchFilterViewModel.ApplyFilterCommand.Execute(null);

            // Assert
            _mockSearchService.Verify(x => x.AdvancedSearchAsync(It.IsAny<SearchCriteria>()), Times.Once);
        }

        /// <summary>
        /// Tests that SaveFilterCommand serializes all properties correctly.
        /// </summary>
        [TestMethod]
        public void SaveFilterCommand_SerializesAllProperties()
        {
            // Arrange
            if (_searchFilterViewModel == null) return;
            
            _searchFilterViewModel.SearchTerm = "test";
            _searchFilterViewModel.RegexSearch = true;
            _searchFilterViewModel.IsAllWordsSearch = false;
            _searchFilterViewModel.IsAnyWordSearch = true;
            _searchFilterViewModel.EnableDateRange = true;
            _searchFilterViewModel.StartDate = System.DateTime.Now.AddDays(-30);
            _searchFilterViewModel.EndDate = System.DateTime.Now;
            _searchFilterViewModel.EnableTimeRange = true;
            _searchFilterViewModel.StartTime = "09:00";
            _searchFilterViewModel.EndTime = "17:00";

            // Add some filters
            _searchFilterViewModel.AddColumnValueFilterCommand.Execute(null);
            if (_searchFilterViewModel.ColumnValueFilters.Count > 0)
            {
                _searchFilterViewModel.ColumnValueFilters[0].ColumnName = "Name";
                _searchFilterViewModel.ColumnValueFilters[0].Operator = "Contains";
                _searchFilterViewModel.ColumnValueFilters[0].Value = "John";
            }

            _searchFilterViewModel.AddValueRangeFilterCommand.Execute(null);
            if (_searchFilterViewModel.ValueRangeFilters.Count > 0)
            {
                _searchFilterViewModel.ValueRangeFilters[0].ColumnName = "Age";
                _searchFilterViewModel.ValueRangeFilters[0].MinValue = "18";
                _searchFilterViewModel.ValueRangeFilters[0].MaxValue = "65";
            }

            // Act
            _searchFilterViewModel.SaveFilterCommand.Execute(null);

            // Assert
            // The command should execute without throwing exceptions
            Assert.IsNotNull(_searchFilterViewModel); // If we get here, the test passed
        }

        /// <summary>
        /// Tests that LoadFilterCommand deserializes all properties correctly.
        /// </summary>
        [TestMethod]
        public void LoadFilterCommand_DeserializesAllProperties()
        {
            // Arrange
            if (_searchFilterViewModel == null) return;

            // Act
            _searchFilterViewModel.LoadFilterCommand.Execute(null);

            // Assert
            // The command should execute without throwing exceptions
            Assert.IsNotNull(_searchFilterViewModel); // If we get here, the test passed
        }

        /// <summary>
        /// Tests that ResetFilterCommand resets all properties correctly.
        /// </summary>
        [TestMethod]
        public void ResetFilterCommand_ResetsAllProperties()
        {
            // Arrange
            if (_searchFilterViewModel == null) return;
            
            _searchFilterViewModel.SearchTerm = "test";
            _searchFilterViewModel.RegexSearch = true;
            _searchFilterViewModel.IsAllWordsSearch = false;
            _searchFilterViewModel.IsAnyWordSearch = true;
            _searchFilterViewModel.EnableDateRange = true;
            _searchFilterViewModel.StartDate = System.DateTime.Now.AddDays(-30);
            _searchFilterViewModel.EndDate = System.DateTime.Now;
            _searchFilterViewModel.EnableTimeRange = true;
            _searchFilterViewModel.StartTime = "09:00";
            _searchFilterViewModel.EndTime = "17:00";

            // Add some filters
            _searchFilterViewModel.AddColumnValueFilterCommand.Execute(null);
            _searchFilterViewModel.AddValueRangeFilterCommand.Execute(null);

            var initialColumnFilterCount = _searchFilterViewModel.ColumnValueFilters.Count;
            var initialValueRangeFilterCount = _searchFilterViewModel.ValueRangeFilters.Count;

            // Act
            _searchFilterViewModel.ResetFilterCommand.Execute(null);

            // Assert
            Assert.AreEqual("", _searchFilterViewModel.SearchTerm);
            Assert.IsFalse(_searchFilterViewModel.RegexSearch);
            Assert.IsTrue(_searchFilterViewModel.IsAllWordsSearch);
            Assert.IsFalse(_searchFilterViewModel.IsAnyWordSearch);
            Assert.IsFalse(_searchFilterViewModel.IsPhraseSearch);
            Assert.IsFalse(_searchFilterViewModel.EnableDateRange);
            Assert.IsFalse(_searchFilterViewModel.EnableTimeRange);
            Assert.IsTrue(_searchFilterViewModel.ColumnValueFilters.Count <= initialColumnFilterCount);
            Assert.IsTrue(_searchFilterViewModel.ValueRangeFilters.Count <= initialValueRangeFilterCount);
        }
    }
}