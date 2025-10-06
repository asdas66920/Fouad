using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fouad.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace Fouad.Tests
{
    /// <summary>
    /// Unit tests for the ColumnSelectorViewModel class.
    /// Tests cover column selection functionality and UI interaction.
    /// </summary>
    [TestClass]
    public class ColumnSelectorViewModelTests
    {
        private ColumnSelectorViewModel _viewModel;

        /// <summary>
        /// Initializes the test environment before each test method runs.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            _viewModel = new ColumnSelectorViewModel();
        }

        /// <summary>
        /// Tests that the ColumnSelectorViewModel can be instantiated correctly.
        /// </summary>
        [TestMethod]
        public void ColumnSelectorViewModel_Constructor_CreatesInstance()
        {
            // Arrange & Act
            var viewModel = new ColumnSelectorViewModel();

            // Assert
            Assert.IsNotNull(viewModel);
            Assert.IsNotNull(viewModel.Columns);
            Assert.IsNotNull(viewModel.ToggleAllColumnsCommand);
        }

        /// <summary>
        /// Tests that UpdateColumns correctly updates the column list.
        /// </summary>
        [TestMethod]
        public void UpdateColumns_UpdatesColumnList_Correctly()
        {
            // Arrange
            var headers = new List<string> { "Column1", "Column2", "Column3" };

            // Act
            _viewModel.UpdateColumns(headers);

            // Assert
            Assert.AreEqual(3, _viewModel.Columns.Count);
            Assert.AreEqual("Column1", _viewModel.Columns[0].Name);
            Assert.AreEqual("Column2", _viewModel.Columns[1].Name);
            Assert.AreEqual("Column3", _viewModel.Columns[2].Name);
        }

        /// <summary>
        /// Tests that GetSelectedColumns returns the correct selected columns.
        /// </summary>
        [TestMethod]
        public void GetSelectedColumns_ReturnsSelectedColumns_Correctly()
        {
            // Arrange
            var headers = new List<string> { "Column1", "Column2", "Column3" };
            _viewModel.UpdateColumns(headers);
            
            // Unselect the second column
            _viewModel.Columns[1].IsSelected = false;

            // Act
            var selectedColumns = _viewModel.GetSelectedColumns();

            // Assert
            Assert.AreEqual(2, selectedColumns.Count);
            Assert.IsTrue(selectedColumns.Contains("Column1"));
            Assert.IsTrue(selectedColumns.Contains("Column3"));
            Assert.IsFalse(selectedColumns.Contains("Column2"));
        }

        /// <summary>
        /// Tests that Reset clears the column list.
        /// </summary>
        [TestMethod]
        public void Reset_ClearsColumnList()
        {
            // Arrange
            var headers = new List<string> { "Column1", "Column2", "Column3" };
            _viewModel.UpdateColumns(headers);
            Assert.AreEqual(3, _viewModel.Columns.Count);

            // Act
            _viewModel.Reset();

            // Assert
            Assert.AreEqual(0, _viewModel.Columns.Count);
        }

        /// <summary>
        /// Tests that AreAllSelected property is true when all columns are selected.
        /// </summary>
        [TestMethod]
        public void AreAllSelected_ReturnsTrue_WhenAllColumnsSelected()
        {
            // Arrange
            var headers = new List<string> { "Column1", "Column2", "Column3" };
            _viewModel.UpdateColumns(headers);
            
            // Ensure all columns are selected
            foreach (var column in _viewModel.Columns)
            {
                column.IsSelected = true;
            }

            // Act
            var result = _viewModel.AreAllSelected;

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests that AreAllSelected property is false when not all columns are selected.
        /// </summary>
        [TestMethod]
        public void AreAllSelected_ReturnsFalse_WhenNotAllColumnsSelected()
        {
            // Arrange
            var headers = new List<string> { "Column1", "Column2", "Column3" };
            _viewModel.UpdateColumns(headers);
            
            // Unselect one column
            _viewModel.Columns[1].IsSelected = false;

            // Act
            var result = _viewModel.AreAllSelected;

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that ToggleAllColumnsCommand toggles all columns when they are all selected.
        /// </summary>
        [TestMethod]
        public void ToggleAllColumnsCommand_TogglesAllColumns_UnselectAll()
        {
            // Arrange
            var headers = new List<string> { "Column1", "Column2", "Column3" };
            _viewModel.UpdateColumns(headers);
            
            // Ensure all columns are initially selected
            foreach (var column in _viewModel.Columns)
            {
                column.IsSelected = true;
            }
            _viewModel.AreAllSelected = true;

            // Act
            _viewModel.ToggleAllColumnsCommand.Execute(null);

            // Assert
            Assert.IsFalse(_viewModel.AreAllSelected);
            Assert.IsTrue(_viewModel.Columns.All(c => !c.IsSelected));
        }

        /// <summary>
        /// Tests that ToggleAllColumnsCommand toggles all columns when they are not all selected.
        /// </summary>
        [TestMethod]
        public void ToggleAllColumnsCommand_TogglesAllColumns_SelectAll()
        {
            // Arrange
            var headers = new List<string> { "Column1", "Column2", "Column3" };
            _viewModel.UpdateColumns(headers);
            
            // Unselect one column
            _viewModel.Columns[1].IsSelected = false;
            _viewModel.AreAllSelected = false;

            // Act
            _viewModel.ToggleAllColumnsCommand.Execute(null);

            // Assert
            Assert.IsTrue(_viewModel.AreAllSelected);
            Assert.IsTrue(_viewModel.Columns.All(c => c.IsSelected));
        }
    }
}