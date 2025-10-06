using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;
using Fouad.Services;
using Fouad.ViewModels;
using OfficeOpenXml;

namespace Fouad.Tests
{
    /// <summary>
    /// Unit tests for the ReviewViewModel class.
    /// Tests cover review view model operations including initialization and command handling.
    /// </summary>
    [TestClass]
    public class ReviewViewModelTests
    {
        private ReviewService _reviewService = null!;
        private DatabaseService _databaseService = null!;
        private string _testDatabasePath = null!;
        private string _testExcelPath = null!;

        /// <summary>
        /// Static constructor to set EPPlus license
        /// </summary>
        static ReviewViewModelTests()
        {
            ExcelPackage.License.SetNonCommercialPersonal("khaled");
        }

        /// <summary>
        /// Initializes the test environment before each test method runs.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            // Set EPPlus license for non-commercial use
            ExcelPackage.License.SetNonCommercialPersonal("khaled");
            
            // Create a temporary database file for testing
            _testDatabasePath = Path.Combine(Path.GetTempPath(), $"test_database_{Guid.NewGuid()}.db");
            _databaseService = new DatabaseService(_testDatabasePath);
            _reviewService = new ReviewService(_databaseService, $"Data Source={_testDatabasePath};");
            
            // Create a simple test Excel file
            _testExcelPath = Path.Combine(Path.GetTempPath(), $"test_file_{Guid.NewGuid()}.xlsx");
            CreateTestExcelFile(_testExcelPath);
        }

        /// <summary>
        /// Cleans up the test environment after each test method runs.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            // Dispose services to close any open connections
            if (_reviewService != null)
            {
                // ReviewService doesn't implement IDisposable, but it holds a reference to DatabaseService
                _reviewService = null!;
            }
            
            if (_databaseService != null)
            {
                try
                {
                    _databaseService.Dispose();
                }
                catch (Exception ex)
                {
                    // Log but don't throw to avoid breaking the test
                    Console.WriteLine($"Warning: Error disposing DatabaseService: {ex.Message}");
                }
                _databaseService = null!;
            }

            // Add a larger delay to ensure file handles are released
            System.Threading.Thread.Sleep(500);

            // Clean up test files with more robust retry logic
            try
            {
                DeleteFileWithRetry(_testDatabasePath);
                DeleteFileWithRetry(_testExcelPath);
            }
            catch (Exception ex)
            {
                // Log but don't throw to avoid breaking the test
                Console.WriteLine($"Warning: Error deleting test files: {ex.Message}");
            }
            
            // Clean up archive folder if it was created
            try
            {
                var archiveFolderPath = Path.Combine(Path.GetDirectoryName(_testDatabasePath) ?? "", "Excel_Archive");
                if (Directory.Exists(archiveFolderPath))
                {
                    DeleteDirectoryWithRetry(archiveFolderPath);
                }
            }
            catch (Exception ex)
            {
                // Log but don't throw to avoid breaking the test
                Console.WriteLine($"Warning: Error deleting archive folder: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a file with retry logic to handle locking issues.
        /// </summary>
        /// <param name="filePath">The path of the file to delete.</param>
        private void DeleteFileWithRetry(string filePath)
        {
            if (!File.Exists(filePath)) return;
            
            for (int i = 0; i < 15; i++) // Increase retry count
            {
                try
                {
                    File.Delete(filePath);
                    break;
                }
                catch (Exception)
                {
                    if (i == 14) 
                    {
                        // Log the error but don't throw to avoid breaking the test
                        // This is a cleanup method, so we don't want it to fail the test
                        Console.WriteLine($"Warning: Could not delete file {filePath} after 15 attempts");
                        break;
                    }
                    System.Threading.Thread.Sleep(300); // Increase delay between retries
                }
            }
        }
        
        /// <summary>
        /// Deletes a directory with retry logic to handle locking issues.
        /// </summary>
        /// <param name="directoryPath">The path of the directory to delete.</param>
        private void DeleteDirectoryWithRetry(string directoryPath)
        {
            // Check if directory exists before attempting to delete
            if (!Directory.Exists(directoryPath)) return;
            
            for (int i = 0; i < 15; i++) // Increase retry count
            {
                try
                {
                    Directory.Delete(directoryPath, true);
                    break;
                }
                catch (DirectoryNotFoundException)
                {
                    // Directory already deleted, nothing to do
                    break;
                }
                catch (Exception)
                {
                    if (i == 14) 
                    {
                        // Log the error but don't throw to avoid breaking the test
                        // This is a cleanup method, so we don't want it to fail the test
                        Console.WriteLine($"Warning: Could not delete directory {directoryPath} after 15 attempts");
                        break;
                    }
                    System.Threading.Thread.Sleep(300); // Increase delay between retries
                }
            }
        }

        /// <summary>
        /// Tests that the ReviewViewModel can be instantiated correctly.
        /// </summary>
        [TestMethod]
        public void ReviewViewModel_Constructor_CreatesInstance()
        {
            // Arrange & Act
            var reviewViewModel = new ReviewViewModel(_reviewService, 1, "test.xlsx");

            // Assert
            Assert.IsNotNull(reviewViewModel);
            Assert.AreEqual("test.xlsx", reviewViewModel.FileName);
            Assert.AreEqual(1, reviewViewModel.ArchiveId);
        }

        /// <summary>
        /// Tests that the ReviewViewModel initializes correctly.
        /// </summary>
        [TestMethod]
        public async Task ReviewViewModel_Initialize_InitializesSuccessfully()
        {
            // Arrange
            var archiveId = await _databaseService.ImportFileAsync(_testExcelPath, "testuser");
            await _databaseService.IndexFileContentAsync(_testExcelPath, archiveId);
            
            var reviewViewModel = new ReviewViewModel(_reviewService, archiveId, "test.xlsx");

            // Act
            await reviewViewModel.InitializeAsync();

            // Assert
            // The collections should be populated
            Assert.IsNotNull(reviewViewModel.NewRecords);
            Assert.IsNotNull(reviewViewModel.MatchRecords);
            Assert.IsNotNull(reviewViewModel.DisagreementRecords);
        }

        /// <summary>
        /// Tests that commands are initialized correctly.
        /// </summary>
        [TestMethod]
        public void ReviewViewModel_Commands_AreInitialized()
        {
            // Arrange
            var reviewViewModel = new ReviewViewModel(_reviewService, 1, "test.xlsx");

            // Act & Assert
            Assert.IsNotNull(reviewViewModel.ProcessDecisionsCommand);
            Assert.IsNotNull(reviewViewModel.CancelCommand);
        }

        /// <summary>
        /// Tests that the IsProcessing property works correctly.
        /// </summary>
        [TestMethod]
        public async Task ReviewViewModel_IsProcessing_WorksCorrectly()
        {
            // Arrange
            var reviewViewModel = new ReviewViewModel(_reviewService, 1, "test.xlsx");

            // Act & Assert
            Assert.IsFalse(reviewViewModel.IsProcessing);
        }

        /// <summary>
        /// Creates a simple test Excel file for testing.
        /// </summary>
        /// <param name="filePath">The path where to create the test file.</param>
        private void CreateTestExcelFile(string filePath)
        {
            // Create a new Excel package
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("TestSheet");
            
            // Add header row
            worksheet.Cells[1, 1].Value = "Name";
            worksheet.Cells[1, 2].Value = "Age";
            worksheet.Cells[1, 3].Value = "City";
            
            // Add data rows
            worksheet.Cells[2, 1].Value = "John";
            worksheet.Cells[2, 2].Value = 25;
            worksheet.Cells[2, 3].Value = "New York";
            
            worksheet.Cells[3, 1].Value = "Jane";
            worksheet.Cells[3, 2].Value = 30;
            worksheet.Cells[3, 3].Value = "London";
            
            worksheet.Cells[4, 1].Value = "Bob";
            worksheet.Cells[4, 2].Value = 35;
            worksheet.Cells[4, 3].Value = "Paris";
            
            // Save the file
            var file = new FileInfo(filePath);
            package.SaveAs(file);
        }
    }
}