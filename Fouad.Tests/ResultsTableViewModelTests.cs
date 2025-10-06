using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fouad.ViewModels;
using Fouad.Services;
using Fouad.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace Fouad.Tests
{
    /// <summary>
    /// Unit tests for the ResultsTableViewModel class.
    /// Tests cover search functionality, result management, and command execution.
    /// </summary>
    [TestClass]
    public class ResultsTableViewModelTests
    {
        private ResultsTableViewModel _viewModel;
        private ColumnSelectorViewModel _columnSelector;

        /// <summary>
        /// Initializes the test environment before each test method runs.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            _columnSelector = new ColumnSelectorViewModel();
            _viewModel = new ResultsTableViewModel(_columnSelector);
        }

        /// <summary>
        /// Tests that the ResultsTableViewModel can be instantiated correctly.
        /// </summary>
        [TestMethod]
        public void ResultsTableViewModel_Constructor_CreatesInstance()
        {
            // Arrange & Act
            var viewModel = new ResultsTableViewModel(_columnSelector);

            // Assert
            Assert.IsNotNull(viewModel);
            Assert.IsNotNull(viewModel.Results);
            Assert.IsNotNull(viewModel.AdvancedSearchCommand);
            Assert.IsNotNull(viewModel.AddToHistoryCommand);
        }

        /// <summary>
        /// Tests that SearchText property correctly updates and notifies changes.
        /// </summary>
        [TestMethod]
        public void SearchText_Property_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var expectedText = "test search";

            // Act
            _viewModel.SearchText = expectedText;

            // Assert
            Assert.AreEqual(expectedText, _viewModel.SearchText);
        }

        /// <summary>
        /// Tests that SearchResultCount property correctly updates and notifies changes.
        /// </summary>
        [TestMethod]
        public void SearchResultCount_Property_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var expectedCount = 5;

            // Act
            _viewModel.SearchResultCount = expectedCount;

            // Assert
            Assert.AreEqual(expectedCount, _viewModel.SearchResultCount);
        }

        /// <summary>
        /// Tests that ResultsTitleWithCount returns correct title when there are results.
        /// </summary>
        [TestMethod]
        public void ResultsTitleWithCount_ReturnsTitleWithCount_WhenResultsExist()
        {
            // Arrange
            _viewModel.SearchResultCount = 3;

            // Act
            var title = _viewModel.ResultsTitleWithCount;

            // Assert
            Assert.IsTrue(title.Contains("(3)"));
        }

        /// <summary>
        /// Tests that ResultsTitleWithCount returns correct title when there are no results.
        /// </summary>
        [TestMethod]
        public void ResultsTitleWithCount_ReturnsTitleWithoutCount_WhenNoResults()
        {
            // Arrange
            _viewModel.SearchResultCount = 0;

            // Act
            var title = _viewModel.ResultsTitleWithCount;

            // Assert
            Assert.IsFalse(title.Contains("("));
        }

        /// <summary>
        /// Tests that ClearSearchAndFocus clears the search text.
        /// </summary>
        [TestMethod]
        public void ClearSearchAndFocus_ClearsSearchText()
        {
            // Arrange
            _viewModel.SearchText = "test search";

            // Act
            _viewModel.ClearSearchAndFocus();

            // Assert
            Assert.AreEqual("", _viewModel.SearchText);
        }

        /// <summary>
        /// Tests that HandleEnterKeyPress method exists and can be called.
        /// </summary>
        [TestMethod]
        public void HandleEnterKeyPress_Method_ExistsAndCallable()
        {
            // Arrange
            _viewModel.SearchText = "test";

            // Act & Assert
            // This should not throw an exception
            _viewModel.HandleEnterKeyPress();
        }

        /// <summary>
        /// Tests that HandleAddEnterKeyPress method exists and can be called.
        /// </summary>
        [TestMethod]
        public void HandleAddEnterKeyPress_Method_ExistsAndCallable()
        {
            // Arrange
            var result = new Result { Id = 1, IsAddedToHistory = false };

            // Act & Assert
            // This should not throw an exception
            _viewModel.HandleAddEnterKeyPress(result);
        }

        /// <summary>
        /// Tests that SetHistoryTableViewModel correctly sets the history table reference.
        /// </summary>
        [TestMethod]
        public void SetHistoryTableViewModel_SetsReference_Correctly()
        {
            // Arrange
            var historyTable = new HistoryTableViewModel(_columnSelector);

            // Act
            _viewModel.SetHistoryTableViewModel(historyTable);

            // Assert
            // The method exists and can be called without exception
            // In a more comprehensive test, we would verify the internal state was set
        }

        /// <summary>
        /// Tests that AudioService property returns a valid instance.
        /// </summary>
        [TestMethod]
        public void AudioService_Property_ReturnsValidInstance()
        {
            // Act
            var audioService = _viewModel.AudioService;

            // Assert
            Assert.IsNotNull(audioService);
        }
    }
}