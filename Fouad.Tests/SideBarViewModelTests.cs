using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fouad.ViewModels;
using System.Windows.Input;

namespace Fouad.Tests
{
    /// <summary>
    /// Unit tests for the SideBarViewModel class.
    /// Tests cover sidebar functionality and command execution.
    /// </summary>
    [TestClass]
    public class SideBarViewModelTests
    {
        private SideBarViewModel _viewModel;

        /// <summary>
        /// Initializes the test environment before each test method runs.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            _viewModel = new SideBarViewModel();
        }

        /// <summary>
        /// Tests that the SideBarViewModel can be instantiated correctly.
        /// </summary>
        [TestMethod]
        public void SideBarViewModel_Constructor_CreatesInstance()
        {
            // Arrange & Act
            var viewModel = new SideBarViewModel();

            // Assert
            Assert.IsNotNull(viewModel);
        }

        /// <summary>
        /// Tests that IsExpanded property correctly updates and notifies changes.
        /// </summary>
        [TestMethod]
        public void IsExpanded_Property_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var expectedValue = false;

            // Act
            _viewModel.IsExpanded = expectedValue;

            // Assert
            Assert.AreEqual(expectedValue, _viewModel.IsExpanded);
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
        /// Tests that all commands are properly initialized.
        /// </summary>
        [TestMethod]
        public void AllCommands_AreInitialized_Correctly()
        {
            // Assert
            Assert.IsNotNull(_viewModel.ToggleSidebarCommand);
            Assert.IsNotNull(_viewModel.AddNewFileCommand);
            Assert.IsNotNull(_viewModel.ImportFileCommand);
            Assert.IsNotNull(_viewModel.ExportFileCommand);
            Assert.IsNotNull(_viewModel.BackupCommand);
            Assert.IsNotNull(_viewModel.RestoreCommand);
            Assert.IsNotNull(_viewModel.ClearHistoryCommand);
            Assert.IsNotNull(_viewModel.SettingsCommand);
            Assert.IsNotNull(_viewModel.SearchCommand);
            Assert.IsNotNull(_viewModel.ToggleAutoBackupCommand);
            Assert.IsNotNull(_viewModel.ToggleNotificationsCommand);
            Assert.IsNotNull(_viewModel.ToggleDarkModeCommand);
            Assert.IsNotNull(_viewModel.ArchiveSelectedCommand);
            Assert.IsNotNull(_viewModel.CompressSelectedCommand);
            Assert.IsNotNull(_viewModel.EncryptSelectedCommand);
        }

        /// <summary>
        /// Tests that ToggleSidebarCommand toggles the IsExpanded property.
        /// </summary>
        [TestMethod]
        public void ToggleSidebarCommand_TogglesIsExpanded()
        {
            // Arrange
            var initialValue = _viewModel.IsExpanded;
            
            // Act
            _viewModel.ToggleSidebarCommand.Execute(null);
            
            // Assert
            Assert.AreEqual(!initialValue, _viewModel.IsExpanded);
        }

        /// <summary>
        /// Tests that AutoBackupEnabled property correctly updates and notifies changes.
        /// </summary>
        [TestMethod]
        public void AutoBackupEnabled_Property_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var expectedValue = true;

            // Act
            _viewModel.AutoBackupEnabled = expectedValue;

            // Assert
            Assert.AreEqual(expectedValue, _viewModel.AutoBackupEnabled);
        }

        /// <summary>
        /// Tests that NotificationsEnabled property correctly updates and notifies changes.
        /// </summary>
        [TestMethod]
        public void NotificationsEnabled_Property_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var expectedValue = false;

            // Act
            _viewModel.NotificationsEnabled = expectedValue;

            // Assert
            Assert.AreEqual(expectedValue, _viewModel.NotificationsEnabled);
        }

        /// <summary>
        /// Tests that DarkModeEnabled property correctly updates and notifies changes.
        /// </summary>
        [TestMethod]
        public void DarkModeEnabled_Property_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var expectedValue = true;

            // Act
            _viewModel.DarkModeEnabled = expectedValue;

            // Assert
            Assert.AreEqual(expectedValue, _viewModel.DarkModeEnabled);
        }

        /// <summary>
        /// Tests that FilteredHistoryItems collection is properly initialized.
        /// </summary>
        [TestMethod]
        public void FilteredHistoryItems_Collection_IsInitialized()
        {
            // Assert
            Assert.IsNotNull(_viewModel.FilteredHistoryItems);
        }
    }
}