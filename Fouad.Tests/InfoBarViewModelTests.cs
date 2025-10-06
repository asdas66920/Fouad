using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fouad.ViewModels;
using System;

namespace Fouad.Tests
{
    /// <summary>
    /// Unit tests for the InfoBarViewModel class.
    /// Tests cover info bar functionality and property management.
    /// </summary>
    [TestClass]
    public class InfoBarViewModelTests
    {
        private InfoBarViewModel _viewModel = null!;

        /// <summary>
        /// Initializes the test environment before each test method runs.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            _viewModel = new InfoBarViewModel();
        }

        /// <summary>
        /// Tests that the InfoBarViewModel can be instantiated correctly.
        /// </summary>
        [TestMethod]
        public void InfoBarViewModel_Constructor_CreatesInstance()
        {
            // Arrange & Act
            var viewModel = new InfoBarViewModel();

            // Assert
            Assert.IsNotNull(viewModel);
        }

        /// <summary>
        /// Tests that FileName property correctly updates and notifies changes.
        /// </summary>
        [TestMethod]
        public void FileName_Property_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var expectedName = "test-file.xlsx";

            // Act
            _viewModel.FileName = expectedName;

            // Assert
            Assert.AreEqual(expectedName, _viewModel.FileName);
        }

        /// <summary>
        /// Tests that FileSize property correctly updates and notifies changes.
        /// </summary>
        [TestMethod]
        public void FileSize_Property_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var expectedSize = "1.5 MB";

            // Act
            _viewModel.FileSize = expectedSize;

            // Assert
            Assert.AreEqual(expectedSize, _viewModel.FileSize);
        }

        /// <summary>
        /// Tests that RowCount property correctly updates and notifies changes.
        /// </summary>
        [TestMethod]
        public void RowCount_Property_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var expectedCount = 100;

            // Act
            _viewModel.RowCount = expectedCount;

            // Assert
            Assert.AreEqual(expectedCount, _viewModel.RowCount);
        }

        /// <summary>
        /// Tests that ColumnCount property correctly updates and notifies changes.
        /// </summary>
        [TestMethod]
        public void ColumnCount_Property_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var expectedCount = 5;

            // Act
            _viewModel.ColumnCount = expectedCount;

            // Assert
            Assert.AreEqual(expectedCount, _viewModel.ColumnCount);
        }

        /// <summary>
        /// Tests that LastModified property correctly updates and notifies changes.
        /// </summary>
        [TestMethod]
        public void LastModified_Property_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var expectedDate = DateTime.Now;

            // Act
            _viewModel.LastModified = expectedDate;

            // Assert
            Assert.AreEqual(expectedDate, _viewModel.LastModified);
        }

        /// <summary>
        /// Tests that Reset method clears all properties.
        /// </summary>
        [TestMethod]
        public void Reset_ClearsAllProperties()
        {
            // Arrange
            _viewModel.FileName = "test-file.xlsx";
            _viewModel.FileSize = "1.5 MB";
            _viewModel.RowCount = 100;
            _viewModel.ColumnCount = 5;
            _viewModel.LastModified = DateTime.Now;

            // Act
            _viewModel.Reset();

            // Assert
            Assert.AreEqual("No file selected", _viewModel.FileName);
            Assert.AreEqual("0 KB", _viewModel.FileSize);
            Assert.AreEqual(0, _viewModel.RowCount);
            Assert.AreEqual(0, _viewModel.ColumnCount);
        }

        /// <summary>
        /// Tests that FormattedRowCount property returns correctly formatted string.
        /// </summary>
        [TestMethod]
        public void FormattedRowCount_ReturnsCorrectFormat()
        {
            // Arrange
            _viewModel.RowCount = 1000;

            // Act
            var formatted = _viewModel.FormattedRowCount;

            // Assert
            Assert.AreEqual("1,000", formatted);
        }

        /// <summary>
        /// Tests that FormattedColumnCount property returns correctly formatted string.
        /// </summary>
        [TestMethod]
        public void FormattedColumnCount_ReturnsCorrectFormat()
        {
            // Arrange
            _viewModel.ColumnCount = 5;

            // Act
            var formatted = _viewModel.FormattedColumnCount;

            // Assert
            Assert.AreEqual("5", formatted);
        }
    }
}