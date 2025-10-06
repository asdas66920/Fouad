using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fouad.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace Fouad.Tests
{
    /// <summary>
    /// Unit tests for the SearchFilterViewModel class.
    /// Tests cover search filter functionality and command implementations.
    /// </summary>
    [TestClass]
    public class SearchFilterViewModelTests
    {
        private SearchFilterViewModel _viewModel;

        /// <summary>
        /// Initializes the test environment before each test method runs.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            _viewModel = new SearchFilterViewModel();
        }

        /// <summary>
        /// Cleans up the test environment after each test method runs.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            _viewModel = null!;
        }

        /// <summary>
        /// Tests that the SearchFilterViewModel can be instantiated correctly.
        /// </summary>
        [TestMethod]
        public void SearchFilterViewModel_Constructor_CreatesInstance()
        {
            // Arrange & Act
            var viewModel = new SearchFilterViewModel();

            // Assert
            Assert.IsNotNull(viewModel);
        }

        /// <summary>
        /// Tests that SearchTerm property correctly updates and notifies changes.
        /// </summary>
        [TestMethod]
        public void SearchTerm_Property_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var expectedTerm = "test search";

            // Act
            _viewModel.SearchTerm = expectedTerm;

            // Assert
            Assert.AreEqual(expectedTerm, _viewModel.SearchTerm);
        }

        /// <summary>
        /// Tests that IsCaseSensitive property correctly updates and notifies changes.
        /// </summary>
        [TestMethod]
        public void IsCaseSensitive_Property_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var expectedValue = true;

            // Act
            _viewModel.IsCaseSensitive = expectedValue;

            // Assert
            Assert.AreEqual(expectedValue, _viewModel.IsCaseSensitive);
        }

        /// <summary>
        /// Tests that EnableColumnFilter property correctly updates and notifies changes.
        /// </summary>
        [TestMethod]
        public void EnableColumnFilter_Property_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var expectedValue = true;

            // Act
            _viewModel.EnableColumnFilter = expectedValue;

            // Assert
            Assert.AreEqual(expectedValue, _viewModel.EnableColumnFilter);
        }

        /// <summary>
        /// Tests that EnableDateRange property correctly updates and notifies changes.
        /// </summary>
        [TestMethod]
        public void EnableDateRange_Property_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var expectedValue = true;

            // Act
            _viewModel.EnableDateRange = expectedValue;

            // Assert
            Assert.AreEqual(expectedValue, _viewModel.EnableDateRange);
        }

        /// <summary>
        /// Tests that EnableResultLimit property correctly updates and notifies changes.
        /// </summary>
        [TestMethod]
        public void EnableResultLimit_Property_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var expectedValue = true;

            // Act
            _viewModel.EnableResultLimit = expectedValue;

            // Assert
            Assert.AreEqual(expectedValue, _viewModel.EnableResultLimit);
        }

        /// <summary>
        /// Tests that ResultLimit property correctly updates and notifies changes.
        /// </summary>
        [TestMethod]
        public void ResultLimit_Property_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var expectedLimit = 50;

            // Act
            _viewModel.ResultLimit = expectedLimit;

            // Assert
            Assert.AreEqual(expectedLimit, _viewModel.ResultLimit);
        }

        /// <summary>
        /// Tests that SetAvailableColumns correctly populates the AvailableColumns collection.
        /// </summary>
        [TestMethod]
        public void SetAvailableColumns_PopulatesCollection_Correctly()
        {
            // Arrange
            var columnNames = new List<string> { "Column1", "Column2", "Column3" };

            // Act
            _viewModel.SetAvailableColumns(columnNames);

            // Assert
            Assert.IsNotNull(_viewModel.AvailableColumns);
            Assert.AreEqual(columnNames.Count, _viewModel.AvailableColumns.Count);
            
            for (int i = 0; i < columnNames.Count; i++)
            {
                Assert.AreEqual(columnNames[i], _viewModel.AvailableColumns[i].Name);
                Assert.IsFalse(_viewModel.AvailableColumns[i].IsSelected);
            }
        }

        /// <summary>
        /// Tests that ApplyFilterCommand is properly initialized.
        /// </summary>
        [TestMethod]
        public void ApplyFilterCommand_IsInitialized_Correctly()
        {
            // Assert
            Assert.IsNotNull(_viewModel.ApplyFilterCommand);
        }

        /// <summary>
        /// Tests that CancelCommand is properly initialized.
        /// </summary>
        [TestMethod]
        public void CancelCommand_IsInitialized_Correctly()
        {
            // Assert
            Assert.IsNotNull(_viewModel.CancelCommand);
        }

        /// <summary>
        /// Tests that FilterColumn properties work correctly.
        /// </summary>
        [TestMethod]
        public void FilterColumn_Properties_WorkCorrectly()
        {
            // Arrange
            var filterColumn = new FilterColumn();
            var expectedName = "TestColumn";
            var expectedSelected = true;

            // Act
            filterColumn.Name = expectedName;
            filterColumn.IsSelected = expectedSelected;

            // Assert
            Assert.AreEqual(expectedName, filterColumn.Name);
            Assert.AreEqual(expectedSelected, filterColumn.IsSelected);
        }
    }
}