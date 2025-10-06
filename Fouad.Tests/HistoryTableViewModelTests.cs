using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fouad.ViewModels;
using Fouad.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace Fouad.Tests
{
    /// <summary>
    /// Unit tests for the HistoryTableViewModel class.
    /// Tests cover history management functionality and command execution.
    /// </summary>
    [TestClass]
    public class HistoryTableViewModelTests
    {
        private HistoryTableViewModel _viewModel;
        private ColumnSelectorViewModel _columnSelector;

        /// <summary>
        /// Initializes the test environment before each test method runs.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            _columnSelector = new ColumnSelectorViewModel();
            _viewModel = new HistoryTableViewModel(_columnSelector);
        }

        /// <summary>
        /// Tests that the HistoryTableViewModel can be instantiated correctly.
        /// </summary>
        [TestMethod]
        public void HistoryTableViewModel_Constructor_CreatesInstance()
        {
            // Arrange & Act
            var viewModel = new HistoryTableViewModel(_columnSelector);

            // Assert
            Assert.IsNotNull(viewModel);
            Assert.IsNotNull(viewModel.HistoryItems);
            Assert.IsNotNull(viewModel.ToggleAllCommand);
            Assert.IsNotNull(viewModel.DeleteCommand);
            Assert.IsNotNull(viewModel.EditCommand);
            Assert.IsNotNull(viewModel.CopyCommand);
            Assert.IsNotNull(viewModel.ExportToExcelCommand);
            Assert.IsNotNull(viewModel.ExportToPdfCommand);
            Assert.IsNotNull(viewModel.BackupSelectedCommand);
            Assert.IsNotNull(viewModel.ShareCommand);
            Assert.IsNotNull(viewModel.ViewCommand);
            Assert.IsNotNull(viewModel.DeleteItemCommand);
        }

        /// <summary>
        /// Tests that HistoryItems collection is properly initialized with sample data.
        /// </summary>
        [TestMethod]
        public void HistoryItems_Collection_IsInitializedWithSampleData()
        {
            // Assert
            Assert.IsNotNull(_viewModel.HistoryItems);
            Assert.IsTrue(_viewModel.HistoryItems.Count > 0);
        }

        /// <summary>
        /// Tests that IsAllSelected property correctly updates and notifies changes.
        /// </summary>
        [TestMethod]
        public void IsAllSelected_Property_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var expectedValue = true;

            // Act
            _viewModel.IsAllSelected = expectedValue;

            // Assert
            Assert.AreEqual(expectedValue, _viewModel.IsAllSelected);
        }

        /// <summary>
        /// Tests that SelectAllItems method correctly selects all items.
        /// </summary>
        [TestMethod]
        public void SelectAllItems_SelectsAllItems_Correctly()
        {
            // Arrange
            // Add a few items to test with
            _viewModel.HistoryItems.Clear();
            _viewModel.HistoryItems.Add(new HistoryItem { Id = 1, IsSelected = false });
            _viewModel.HistoryItems.Add(new HistoryItem { Id = 2, IsSelected = false });
            _viewModel.HistoryItems.Add(new HistoryItem { Id = 3, IsSelected = false });

            // Act
            _viewModel.IsAllSelected = true; // This should call SelectAllItems internally

            // Assert
            Assert.IsTrue(_viewModel.HistoryItems.All(item => item.IsSelected));
        }

        /// <summary>
        /// Tests that SelectAllItems method correctly unselects all items.
        /// </summary>
        [TestMethod]
        public void SelectAllItems_UnselectsAllItems_Correctly()
        {
            // Arrange
            // Add a few items to test with and select them all
            _viewModel.HistoryItems.Clear();
            _viewModel.HistoryItems.Add(new HistoryItem { Id = 1, IsSelected = true });
            _viewModel.HistoryItems.Add(new HistoryItem { Id = 2, IsSelected = true });
            _viewModel.HistoryItems.Add(new HistoryItem { Id = 3, IsSelected = true });

            // Act
            _viewModel.IsAllSelected = false; // This should call SelectAllItems internally

            // Assert
            Assert.IsTrue(_viewModel.HistoryItems.All(item => !item.IsSelected));
        }

        /// <summary>
        /// Tests that all commands are properly initialized.
        /// </summary>
        [TestMethod]
        public void AllCommands_AreInitialized_Correctly()
        {
            // Assert
            Assert.IsNotNull(_viewModel.ToggleAllCommand);
            Assert.IsNotNull(_viewModel.DeleteCommand);
            Assert.IsNotNull(_viewModel.EditCommand);
            Assert.IsNotNull(_viewModel.CopyCommand);
            Assert.IsNotNull(_viewModel.ExportToExcelCommand);
            Assert.IsNotNull(_viewModel.ExportToPdfCommand);
            Assert.IsNotNull(_viewModel.BackupSelectedCommand);
            Assert.IsNotNull(_viewModel.ShareCommand);
            Assert.IsNotNull(_viewModel.ViewCommand);
            Assert.IsNotNull(_viewModel.DeleteItemCommand);
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

        /// <summary>
        /// Tests that InitializeSampleData method adds sample data to the collection.
        /// </summary>
        [TestMethod]
        public void InitializeSampleData_AddsSampleData_ToCollection()
        {
            // Arrange
            var viewModel = new HistoryTableViewModel(_columnSelector);
            
            // Act - InitializeSampleData is called in constructor
            
            // Assert
            Assert.IsTrue(viewModel.HistoryItems.Count > 0);
            Assert.IsTrue(viewModel.HistoryItems.Any(item => item.Id == 1));
            Assert.IsTrue(viewModel.HistoryItems.Any(item => item.Id == 2));
            Assert.IsTrue(viewModel.HistoryItems.Any(item => item.Id == 3));
        }
    }
}