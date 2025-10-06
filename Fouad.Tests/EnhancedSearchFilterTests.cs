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
    /// Unit tests for the enhanced search filter functionality.
    /// </summary>
    [TestClass]
    public class EnhancedSearchFilterTests
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
        /// Tests that the SearchFilterViewModel can be instantiated correctly.
        /// </summary>
        [TestMethod]
        public void SearchFilterViewModel_Constructor_CreatesInstance()
        {
            // Arrange & Act
            var viewModel = new SearchFilterViewModel(_mockSearchService.Object, _mockFileDataService.Object);

            // Assert
            Assert.IsNotNull(viewModel);
        }

        /// <summary>
        /// Tests that SetAvailableColumns correctly populates the available columns.
        /// </summary>
        [TestMethod]
        public void SetAvailableColumns_PopulatesAvailableColumns()
        {
            // Arrange
            var columnNames = new List<string> { "Name", "Age", "City", "Salary" };

            // Act
            _searchFilterViewModel.SetAvailableColumns(columnNames);

            // Assert
            Assert.AreEqual(4, _searchFilterViewModel.AvailableColumns.Count);
            Assert.AreEqual("Name", _searchFilterViewModel.AvailableColumns[0].Name);
            Assert.AreEqual("Age", _searchFilterViewModel.AvailableColumns[1].Name);
            Assert.AreEqual("City", _searchFilterViewModel.AvailableColumns[2].Name);
            Assert.AreEqual("Salary", _searchFilterViewModel.AvailableColumns[3].Name);
        }

        /// <summary>
        /// Tests that SetAvailableFiles correctly populates the available files.
        /// </summary>
        [TestMethod]
        public void SetAvailableFiles_PopulatesAvailableFiles()
        {
            // Arrange
            var filePaths = new List<string> { 
                @"C:\Test\Sample1.csv", 
                @"C:\Test\Sample2.xlsx" 
            };

            // Act
            _searchFilterViewModel.SetAvailableFiles(filePaths);

            // Assert
            Assert.AreEqual(2, _searchFilterViewModel.AvailableFiles.Count);
            Assert.AreEqual("Sample1.csv", _searchFilterViewModel.AvailableFiles[0].Name);
            Assert.AreEqual(@"C:\Test\Sample1.csv", _searchFilterViewModel.AvailableFiles[0].Path);
            Assert.AreEqual("Sample2.xlsx", _searchFilterViewModel.AvailableFiles[1].Name);
            Assert.AreEqual(@"C:\Test\Sample2.xlsx", _searchFilterViewModel.AvailableFiles[1].Path);
        }

        /// <summary>
        /// Tests that search filter properties can be set and retrieved.
        /// </summary>
        [TestMethod]
        public void SearchFilterProperties_CanBeSetAndRetrieved()
        {
            // Act
            _searchFilterViewModel.SearchTerm = "test";
            _searchFilterViewModel.IsCaseSensitive = true;
            _searchFilterViewModel.ExactMatch = true;
            _searchFilterViewModel.FuzzySearch = true;
            _searchFilterViewModel.RegexSearch = true;
            _searchFilterViewModel.IsAllWordsSearch = false;
            _searchFilterViewModel.IsAnyWordSearch = true;
            _searchFilterViewModel.ResultLimit = 500;

            // Assert
            Assert.AreEqual("test", _searchFilterViewModel.SearchTerm);
            Assert.IsTrue(_searchFilterViewModel.IsCaseSensitive);
            Assert.IsTrue(_searchFilterViewModel.ExactMatch);
            Assert.IsTrue(_searchFilterViewModel.FuzzySearch);
            Assert.IsTrue(_searchFilterViewModel.RegexSearch);
            Assert.IsFalse(_searchFilterViewModel.IsAllWordsSearch);
            Assert.IsTrue(_searchFilterViewModel.IsAnyWordSearch);
            Assert.AreEqual(500, _searchFilterViewModel.ResultLimit);
        }

        /// <summary>
        /// Tests that column value filters can be added and removed.
        /// </summary>
        [TestMethod]
        public void ColumnValueFilters_CanBeAddedAndRemoved()
        {
            // Act
            _searchFilterViewModel.AddColumnValueFilterCommand.Execute(null);
            _searchFilterViewModel.AddColumnValueFilterCommand.Execute(null);

            // Assert
            Assert.AreEqual(2, _searchFilterViewModel.ColumnValueFilters.Count);

            // Act
            _searchFilterViewModel.RemoveColumnValueFilterCommand.Execute(null);

            // Assert
            Assert.AreEqual(1, _searchFilterViewModel.ColumnValueFilters.Count);
        }

        /// <summary>
        /// Tests that value range filters can be added and removed.
        /// </summary>
        [TestMethod]
        public void ValueRangeFilters_CanBeAddedAndRemoved()
        {
            // Act
            _searchFilterViewModel.AddValueRangeFilterCommand.Execute(null);
            _searchFilterViewModel.AddValueRangeFilterCommand.Execute(null);
            _searchFilterViewModel.AddValueRangeFilterCommand.Execute(null);

            // Assert
            Assert.AreEqual(3, _searchFilterViewModel.ValueRangeFilters.Count);

            // Act
            _searchFilterViewModel.RemoveValueRangeFilterCommand.Execute(null);
            _searchFilterViewModel.RemoveValueRangeFilterCommand.Execute(null);

            // Assert
            Assert.AreEqual(1, _searchFilterViewModel.ValueRangeFilters.Count);
        }
    }
}