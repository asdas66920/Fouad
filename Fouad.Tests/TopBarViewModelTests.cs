using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fouad.ViewModels;
using Fouad.Services;
using System.IO;

namespace Fouad.Tests
{
    /// <summary>
    /// Unit tests for the TopBarViewModel class.
    /// Tests cover top bar functionality and command execution.
    /// </summary>
    [TestClass]
    public class TopBarViewModelTests
    {
        private TopBarViewModel _viewModel = null!;
        private InfoBarViewModel _infoBarViewModel = null!;
        private ColumnSelectorViewModel _columnSelectorViewModel = null!;

        /// <summary>
        /// Initializes the test environment before each test method runs.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            _infoBarViewModel = new InfoBarViewModel();
            _columnSelectorViewModel = new ColumnSelectorViewModel();
            _viewModel = new TopBarViewModel(_infoBarViewModel, _columnSelectorViewModel);
        }

        /// <summary>
        /// Tests that the TopBarViewModel can be instantiated correctly.
        /// </summary>
        [TestMethod]
        public void TopBarViewModel_Constructor_CreatesInstance()
        {
            // Arrange & Act
            var viewModel = new TopBarViewModel(_infoBarViewModel, _columnSelectorViewModel);

            // Assert
            Assert.IsNotNull(viewModel);
        }

        /// <summary>
        /// Tests that Title property correctly updates and notifies changes.
        /// </summary>
        [TestMethod]
        public void Title_Property_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var expectedTitle = "Test Title";

            // Act
            _viewModel.Title = expectedTitle;

            // Assert
            Assert.AreEqual(expectedTitle, _viewModel.Title);
        }

        /// <summary>
        /// Tests that IsDarkMode property correctly updates and notifies changes.
        /// </summary>
        [TestMethod]
        public void IsDarkMode_Property_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var expectedValue = true;

            // Act
            _viewModel.IsDarkMode = expectedValue;

            // Assert
            Assert.AreEqual(expectedValue, _viewModel.IsDarkMode);
        }

        /// <summary>
        /// Tests that LanguageButtonText property correctly updates and notifies changes.
        /// </summary>
        [TestMethod]
        public void LanguageButtonText_Property_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var expectedText = "Test Language";

            // Act
            _viewModel.LanguageButtonText = expectedText;

            // Assert
            Assert.AreEqual(expectedText, _viewModel.LanguageButtonText);
        }

        /// <summary>
        /// Tests that IsArabic property correctly updates and notifies changes.
        /// </summary>
        [TestMethod]
        public void IsArabic_Property_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var expectedValue = true;

            // Act
            _viewModel.IsArabic = expectedValue;

            // Assert
            Assert.AreEqual(expectedValue, _viewModel.IsArabic);
        }

        /// <summary>
        /// Tests that all commands are properly initialized.
        /// </summary>
        [TestMethod]
        public void AllCommands_AreInitialized_Correctly()
        {
            // Assert
            Assert.IsNotNull(_viewModel.InsertFileCommand);
            Assert.IsNotNull(_viewModel.DeleteFileCommand);
            Assert.IsNotNull(_viewModel.SettingsCommand);
            Assert.IsNotNull(_viewModel.ToggleDarkModeCommand);
            Assert.IsNotNull(_viewModel.StoreCommand);
            Assert.IsNotNull(_viewModel.ToggleLanguageCommand);
            Assert.IsNotNull(_viewModel.PlayTestSoundCommand);
        }

        /// <summary>
        /// Tests that GetFileDataService returns a valid FileDataService instance.
        /// </summary>
        [TestMethod]
        public void GetFileDataService_ReturnsValidInstance()
        {
            // Act
            var fileDataService = _viewModel.GetFileDataService();

            // Assert
            Assert.IsNotNull(fileDataService);
            Assert.IsInstanceOfType(fileDataService, typeof(FileDataService));
        }

        /// <summary>
        /// Tests that FormatFileSize correctly formats file sizes.
        /// </summary>
        [TestMethod]
        public void FormatFileSize_FormatsSizes_Correctly()
        {
            // Act & Assert
            Assert.AreEqual("1 B", _viewModel.FormatFileSize(1));
            Assert.AreEqual("1 KB", _viewModel.FormatFileSize(1024));
            Assert.AreEqual("1 MB", _viewModel.FormatFileSize(1024 * 1024));
            Assert.AreEqual("1 GB", _viewModel.FormatFileSize(1024 * 1024 * 1024));
        }

        /// <summary>
        /// Tests that StopTitleAnimation method exists and can be called.
        /// </summary>
        [TestMethod]
        public void StopTitleAnimation_Method_ExistsAndCallable()
        {
            // Act & Assert
            // This should not throw an exception
            _viewModel.StopTitleAnimation();
        }
    }
}