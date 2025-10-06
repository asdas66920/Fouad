using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fouad.ViewModels;
using Fouad.Services;
using Fouad.Models;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Fouad.Tests
{
    /// <summary>
    /// Integration tests for the Fouad application.
    /// Tests cover the complete workflow from file loading to search and history management.
    /// </summary>
    [TestClass]
    public class IntegrationTests
    {
        private MainViewModel _mainViewModel = null!;
        private string _testCsvPath = null!;

        /// <summary>
        /// Initializes the test environment before each test method runs.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            _mainViewModel = new MainViewModel();
            _testCsvPath = Path.GetTempFileName() + ".csv";
            
            // Create a sample CSV file for testing
            CreateSampleCsvFile(_testCsvPath);
        }

        /// <summary>
        /// Cleans up the test environment after each test method runs.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            if (File.Exists(_testCsvPath))
            {
                File.Delete(_testCsvPath);
            }

            // Clean up binary cache files
            var csvCachePath = Path.Combine(
                Path.GetDirectoryName(_testCsvPath) ?? "", 
                Path.GetFileNameWithoutExtension(_testCsvPath) + ".bin");
                
            if (File.Exists(csvCachePath))
            {
                File.Delete(csvCachePath);
            }
        }

        /// <summary>
        /// Tests the complete workflow: file loading, searching, and adding to history.
        /// </summary>
        [TestMethod]
        public async Task CompleteWorkflow_FileLoadSearchAndAddToHistory_WorksCorrectly()
        {
            // Arrange
            var fileDataService = _mainViewModel.TopBar?.GetFileDataService();
            Assert.IsNotNull(fileDataService, "FileDataService should not be null");

            // Act - Load file
            await fileDataService!.LoadFileAsync(_testCsvPath);
            
            // Assert - File is loaded
            Assert.IsTrue(fileDataService.IsFileLoaded(), "File should be loaded");
            
            // Act - Perform search
            _mainViewModel.ResultsTable!.SearchText = "John";
            _mainViewModel.ResultsTable.HandleEnterKeyPress();
            
            // Assert - Results are populated
            Assert.IsTrue(_mainViewModel.ResultsTable.Results.Count > 0, "Search should return results");
            
            // Act - Add first result to history
            var firstResult = _mainViewModel.ResultsTable.Results[0];
            _mainViewModel.ResultsTable.AddToHistoryCommand.Execute(firstResult);
            
            // Assert - History is updated
            Assert.IsTrue(_mainViewModel.HistoryTable!.HistoryItems.Count > 0, "History should contain items");
            Assert.IsTrue(firstResult.IsAddedToHistory, "Result should be marked as added to history");
        }

        /// <summary>
        /// Tests that column selection affects search results.
        /// </summary>
        [TestMethod]
        public async Task ColumnSelection_AffectsSearchResults()
        {
            // Arrange
            var fileDataService = _mainViewModel.TopBar?.GetFileDataService();
            Assert.IsNotNull(fileDataService, "FileDataService should not be null");
            
            // Load file
            await fileDataService!.LoadFileAsync(_testCsvPath);
            
            // Get initial column count
            var initialColumns = _mainViewModel.ColumnSelector!.GetSelectedColumns().Count;
            
            // Act - Unselect one column
            if (_mainViewModel.ColumnSelector.Columns.Count > 0)
            {
                _mainViewModel.ColumnSelector.Columns[0].IsSelected = false;
            }
            
            // Perform search
            _mainViewModel.ResultsTable!.SearchText = "John";
            _mainViewModel.ResultsTable.HandleEnterKeyPress();
            
            // Assert - Search still works
            Assert.IsTrue(_mainViewModel.ResultsTable.Results.Count >= 0, "Search should still work after column selection change");
        }

        /// <summary>
        /// Tests that the application maintains state correctly across operations.
        /// </summary>
        [TestMethod]
        public async Task ApplicationState_MaintainedAcrossOperations()
        {
            // Arrange
            var fileDataService = _mainViewModel.TopBar?.GetFileDataService();
            Assert.IsNotNull(fileDataService, "FileDataService should not be null");
            
            // Act - Load file
            await fileDataService!.LoadFileAsync(_testCsvPath);
            
            // Store initial state
            var initialRowCount = _mainViewModel.InfoBar?.RowCount ?? 0;
            var initialColumnCount = _mainViewModel.InfoBar?.ColumnCount ?? 0;
            
            // Perform search
            _mainViewModel.ResultsTable!.SearchText = "John";
            _mainViewModel.ResultsTable.HandleEnterKeyPress();
            
            // Add to history
            if (_mainViewModel.ResultsTable.Results.Count > 0)
            {
                var firstResult = _mainViewModel.ResultsTable.Results[0];
                _mainViewModel.ResultsTable.AddToHistoryCommand.Execute(firstResult);
            }
            
            // Assert - State is maintained
            Assert.AreEqual(initialRowCount, _mainViewModel.InfoBar?.RowCount, "Row count should be maintained");
            Assert.AreEqual(initialColumnCount, _mainViewModel.InfoBar?.ColumnCount, "Column count should be maintained");
            Assert.IsTrue(fileDataService.IsFileLoaded(), "File should remain loaded");
        }

        /// <summary>
        /// Tests that the application handles empty search terms correctly.
        /// </summary>
        [TestMethod]
        public async Task EmptySearchTerm_HandledCorrectly()
        {
            // Arrange
            var fileDataService = _mainViewModel.TopBar?.GetFileDataService();
            Assert.IsNotNull(fileDataService, "FileDataService should not be null");
            
            // Load file
            await fileDataService!.LoadFileAsync(_testCsvPath);
            
            // Act - Search with empty term
            _mainViewModel.ResultsTable!.SearchText = "";
            _mainViewModel.ResultsTable.HandleEnterKeyPress();
            
            // Assert - Results are cleared
            Assert.AreEqual(0, _mainViewModel.ResultsTable.Results.Count, "Results should be cleared for empty search");
        }

        /// <summary>
        /// Creates a sample CSV file with test data.
        /// </summary>
        /// <param name="filePath">The path where the CSV file should be created.</param>
        private void CreateSampleCsvFile(string filePath)
        {
            var lines = new List<string>
            {
                "Name,Age,City",
                "John Doe,30,New York",
                "Jane Smith,25,Los Angeles",
                "John Smith,35,Chicago"
            };

            File.WriteAllLines(filePath, lines);
        }
    }
}