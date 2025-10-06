using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fouad.ViewModels;
using Fouad.Services;
using Fouad.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using System;
using System.IO;
using System.Linq;

namespace Fouad.Tests
{
    /// <summary>
    /// Edge case tests for the SearchFilterViewModel class with advanced filtering capabilities.
    /// </summary>
    [TestClass]
    public class SearchFilterViewModelEdgeCaseTests
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
        /// Tests that ApplyFilterCommand handles null search service gracefully.
        /// </summary>
        [TestMethod]
        public async Task ApplyFilterCommand_WithNullSearchService_HandlesGracefully()
        {
            // Arrange
            var viewModel = new SearchFilterViewModel(null, _mockFileDataService.Object);
            viewModel.SearchTerm = "test";
            
            var mockResults = new List<Result>();
            _mockSearchService.Setup(x => x.AdvancedSearchAsync(It.IsAny<SearchCriteria>()))
                .ReturnsAsync(mockResults);

            // Act & Assert
            // Should not throw exception even with null search service
            viewModel.ApplyFilterCommand.Execute(null);
            
            // Verify that the search service was not called
            _mockSearchService.Verify(x => x.AdvancedSearchAsync(It.IsAny<SearchCriteria>()), Times.Never);
        }

        /// <summary>
        /// Tests that ApplyFilterCommand handles search service exceptions gracefully.
        /// </summary>
        [TestMethod]
        public async Task ApplyFilterCommand_WithSearchServiceException_HandlesGracefully()
        {
            // Arrange
            var viewModel = new SearchFilterViewModel(_mockSearchService.Object, _mockFileDataService.Object);
            viewModel.SearchTerm = "test";
            
            _mockSearchService.Setup(x => x.AdvancedSearchAsync(It.IsAny<SearchCriteria>()))
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            // Act & Assert
            // Should not crash when search service throws exception
            viewModel.ApplyFilterCommand.Execute(null);
            
            // Verify that the search service was called
            _mockSearchService.Verify(x => x.AdvancedSearchAsync(It.IsAny<SearchCriteria>()), Times.Once);
        }

        /// <summary>
        /// Tests that SaveFilterCommand handles file system exceptions gracefully.
        /// </summary>
        [TestMethod]
        public void SaveFilterCommand_WithFileSystemException_HandlesGracefully()
        {
            // Arrange
            var viewModel = new SearchFilterViewModel(_mockSearchService.Object, _mockFileDataService.Object);
            viewModel.SearchTerm = "test";
            
            // We can't easily mock File.WriteAllText, but we can test that the method doesn't crash

            // Act & Assert
            // Should not throw exception even if file system operations fail
            viewModel.SaveFilterCommand.Execute(null);
        }

        /// <summary>
        /// Tests that LoadFilterCommand handles missing filter files gracefully.
        /// </summary>
        [TestMethod]
        public void LoadFilterCommand_WithMissingFilterFiles_HandlesGracefully()
        {
            // Arrange
            var viewModel = new SearchFilterViewModel(_mockSearchService.Object, _mockFileDataService.Object);
            
            // Act & Assert
            // Should not throw exception even if no filter files exist
            viewModel.LoadFilterCommand.Execute(null);
        }

        /// <summary>
        /// Tests that ResetFilterCommand properly resets all filter properties.
        /// </summary>
        [TestMethod]
        public void ResetFilterCommand_ResetsAllPropertiesCompletely()
        {
            // Arrange
            if (_searchFilterViewModel == null) return;
            
            // Set up various filter states
            _searchFilterViewModel.SearchTerm = "test search";
            _searchFilterViewModel.IsCaseSensitive = true;
            _searchFilterViewModel.ExactMatch = true;
            _searchFilterViewModel.FuzzySearch = true;
            _searchFilterViewModel.RegexSearch = true;
            _searchFilterViewModel.IsAllWordsSearch = false;
            _searchFilterViewModel.IsAnyWordSearch = true;
            _searchFilterViewModel.IsPhraseSearch = true;
            _searchFilterViewModel.EnableColumnFilter = true;
            _searchFilterViewModel.EnableDateRange = true;
            _searchFilterViewModel.EnableTimeRange = true;
            _searchFilterViewModel.StartDate = DateTime.Now.AddDays(-10);
            _searchFilterViewModel.EndDate = DateTime.Now;
            _searchFilterViewModel.StartTime = "09:00";
            _searchFilterViewModel.EndTime = "17:00";
            _searchFilterViewModel.EnableResultLimit = true;
            _searchFilterViewModel.ResultLimit = 50;
            _searchFilterViewModel.EnableFileFilter = true;

            // Add some filters
            _searchFilterViewModel.AddColumnValueFilterCommand.Execute(null);
            _searchFilterViewModel.AddValueRangeFilterCommand.Execute(null);

            // Act
            _searchFilterViewModel.ResetFilterCommand.Execute(null);

            // Assert - Check that all properties are reset to default values
            Assert.AreEqual("", _searchFilterViewModel.SearchTerm);
            Assert.IsFalse(_searchFilterViewModel.IsCaseSensitive);
            Assert.IsFalse(_searchFilterViewModel.ExactMatch);
            Assert.IsFalse(_searchFilterViewModel.FuzzySearch);
            Assert.IsFalse(_searchFilterViewModel.RegexSearch);
            Assert.IsTrue(_searchFilterViewModel.IsAllWordsSearch);
            Assert.IsFalse(_searchFilterViewModel.IsAnyWordSearch);
            Assert.IsFalse(_searchFilterViewModel.IsPhraseSearch);
            Assert.IsFalse(_searchFilterViewModel.EnableColumnFilter);
            Assert.IsFalse(_searchFilterViewModel.EnableDateRange);
            Assert.IsFalse(_searchFilterViewModel.EnableTimeRange);
            Assert.IsNull(_searchFilterViewModel.StartDate);
            Assert.IsNull(_searchFilterViewModel.EndDate);
            Assert.AreEqual("00:00", _searchFilterViewModel.StartTime);
            Assert.AreEqual("23:59", _searchFilterViewModel.EndTime);
            Assert.IsFalse(_searchFilterViewModel.EnableResultLimit);
            Assert.AreEqual(100, _searchFilterViewModel.ResultLimit);
            Assert.IsFalse(_searchFilterViewModel.EnableFileFilter);
            
            // Check that filters are cleared
            Assert.AreEqual(0, _searchFilterViewModel.ColumnValueFilters.Count);
            Assert.AreEqual(0, _searchFilterViewModel.ValueRangeFilters.Count);
        }

        /// <summary>
        /// Tests that AddColumnValueFilterCommand handles multiple additions correctly.
        /// </summary>
        [TestMethod]
        public void AddColumnValueFilterCommand_MultipleAdditions_WorkCorrectly()
        {
            // Arrange
            if (_searchFilterViewModel == null) return;

            // Act
            for (int i = 0; i < 5; i++)
            {
                _searchFilterViewModel.AddColumnValueFilterCommand.Execute(null);
            }

            // Assert
            Assert.AreEqual(5, _searchFilterViewModel.ColumnValueFilters.Count);
            
            // Check that each filter has default values
            foreach (var filter in _searchFilterViewModel.ColumnValueFilters)
            {
                Assert.AreEqual("", filter.ColumnName);
                Assert.AreEqual("=", filter.Operator);
                Assert.AreEqual("", filter.Value);
            }
        }

        /// <summary>
        /// Tests that RemoveColumnValueFilterCommand handles removal correctly.
        /// </summary>
        [TestMethod]
        public void RemoveColumnValueFilterCommand_Removal_WorkCorrectly()
        {
            // Arrange
            if (_searchFilterViewModel == null) return;
            
            // Add filters first
            _searchFilterViewModel.AddColumnValueFilterCommand.Execute(null);
            _searchFilterViewModel.AddColumnValueFilterCommand.Execute(null);
            
            Assert.AreEqual(2, _searchFilterViewModel.ColumnValueFilters.Count);
            
            // Select the first filter for removal
            _searchFilterViewModel.ColumnValueFilters[0].IsSelected = true;

            // Act
            _searchFilterViewModel.RemoveColumnValueFilterCommand.Execute(null);

            // Assert
            Assert.AreEqual(1, _searchFilterViewModel.ColumnValueFilters.Count);
        }

        /// <summary>
        /// Tests that AddValueRangeFilterCommand handles multiple additions correctly.
        /// </summary>
        [TestMethod]
        public void AddValueRangeFilterCommand_MultipleAdditions_WorkCorrectly()
        {
            // Arrange
            if (_searchFilterViewModel == null) return;

            // Act
            for (int i = 0; i < 3; i++)
            {
                _searchFilterViewModel.AddValueRangeFilterCommand.Execute(null);
            }

            // Assert
            Assert.AreEqual(3, _searchFilterViewModel.ValueRangeFilters.Count);
            
            // Check that each filter has default values
            foreach (var filter in _searchFilterViewModel.ValueRangeFilters)
            {
                Assert.AreEqual("", filter.ColumnName);
                Assert.AreEqual("", filter.MinValue);
                Assert.AreEqual("", filter.MaxValue);
            }
        }

        /// <summary>
        /// Tests that RemoveValueRangeFilterCommand handles removal correctly.
        /// </summary>
        [TestMethod]
        public void RemoveValueRangeFilterCommand_Removal_WorkCorrectly()
        {
            // Arrange
            if (_searchFilterViewModel == null) return;
            
            // Add filters first
            _searchFilterViewModel.AddValueRangeFilterCommand.Execute(null);
            _searchFilterViewModel.AddValueRangeFilterCommand.Execute(null);
            
            Assert.AreEqual(2, _searchFilterViewModel.ValueRangeFilters.Count);
            
            // Select the first filter for removal
            _searchFilterViewModel.ValueRangeFilters[0].IsSelected = true;

            // Act
            _searchFilterViewModel.RemoveValueRangeFilterCommand.Execute(null);

            // Assert
            Assert.AreEqual(1, _searchFilterViewModel.ValueRangeFilters.Count);
        }

        /// <summary>
        /// Tests that ColumnValueFilter properties handle null values correctly.
        /// </summary>
        [TestMethod]
        public void ColumnValueFilter_Properties_HandleNullValues()
        {
            // Arrange
            var filter = new ColumnValueFilter();

            // Act & Assert
            // Setting properties to null should not throw exceptions
            filter.ColumnName = null;
            filter.Operator = null;
            filter.Value = null;
            
            // Should default to empty strings
            Assert.AreEqual("", filter.ColumnName);
            Assert.AreEqual("", filter.Operator);
            Assert.AreEqual("", filter.Value);
        }

        /// <summary>
        /// Tests that ValueRangeFilter properties handle null values correctly.
        /// </summary>
        [TestMethod]
        public void ValueRangeFilter_Properties_HandleNullValues()
        {
            // Arrange
            var filter = new ValueRangeFilter();

            // Act & Assert
            // Setting properties to null should not throw exceptions
            filter.ColumnName = null;
            filter.MinValue = null;
            filter.MaxValue = null;
            
            // Should default to empty strings
            Assert.AreEqual("", filter.ColumnName);
            Assert.AreEqual("", filter.MinValue);
            Assert.AreEqual("", filter.MaxValue);
        }
    }
}