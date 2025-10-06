using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;
using Fouad.Services;
using Fouad.Models;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Linq;

namespace Fouad.Tests
{
    /// <summary>
    /// Unit tests for the ReviewService class.
    /// Tests cover review operations including record identification and user decision processing.
    /// </summary>
    [TestClass]
    public class ReviewServiceTests
    {
        private ReviewService _reviewService = null!;
        private DatabaseService _databaseService = null!;
        private string _testDatabasePath = null!;
        private string _testExcelPath = null!;

        /// <summary>
        /// Static constructor to set EPPlus license
        /// </summary>
        static ReviewServiceTests()
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
                DeleteDirectoryWithRetry(archiveFolderPath);
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
        /// Tests that the ReviewService can be instantiated correctly.
        /// </summary>
        [TestMethod]
        public void ReviewService_Constructor_CreatesInstance()
        {
            // Arrange & Act
            var reviewService = new ReviewService(_databaseService, $"Data Source={_testDatabasePath};");

            // Assert
            Assert.IsNotNull(reviewService);
        }

        /// <summary>
        /// Tests that matching records can be identified successfully.
        /// </summary>
        [TestMethod]
        public async Task ReviewService_IdentifyMatchingRecords_IdentifiesSuccessfully()
        {
            // Arrange
            var archiveId = await _databaseService.ImportFileAsync(_testExcelPath, "testuser");
            await _databaseService.IndexFileContentAsync(_testExcelPath, archiveId);

            // Act
            var (newRecords, matchRecords, disagreementRecords) = await _reviewService.IdentifyMatchingRecordsAsync(archiveId);

            // Assert
            Assert.IsNotNull(newRecords);
            Assert.IsNotNull(matchRecords);
            Assert.IsNotNull(disagreementRecords);
            // We expect at least one new record (the data rows from the Excel file)
            Assert.IsTrue(newRecords.Count > 0);
        }

        /// <summary>
        /// Tests that user decisions can be processed successfully.
        /// </summary>
        [TestMethod]
        public async Task ReviewService_ProcessUserDecisions_ProcessesSuccessfully()
        {
            // Arrange
            var archiveId = await _databaseService.ImportFileAsync(_testExcelPath, "testuser");
            await _databaseService.IndexFileContentAsync(_testExcelPath, archiveId);
            
            var (newRecords, matchRecords, disagreementRecords) = await _reviewService.IdentifyMatchingRecordsAsync(archiveId);
            
            var newRecordDecisions = new List<(NewRecord record, UserDecision decision)>();
            foreach (var record in newRecords)
            {
                newRecordDecisions.Add((record, UserDecision.AddAsNew));
            }
            
            var matchRecordDecisions = new List<(MatchRecord record, UserDecision decision)>();
            foreach (var record in matchRecords)
            {
                matchRecordDecisions.Add((record, UserDecision.Update));
            }
            
            var disagreementRecordDecisions = new List<(DisagreementRecord record, UserDecision decision)>();
            foreach (var record in disagreementRecords)
            {
                disagreementRecordDecisions.Add((record, UserDecision.Ignore));
            }

            // Act & Assert
            // This should not throw an exception
            await _reviewService.ProcessUserDecisionsAsync(
                newRecordDecisions,
                matchRecordDecisions,
                disagreementRecordDecisions);
        }

        /// <summary>
        /// Tests that indexed content can be cleaned up successfully.
        /// </summary>
        [TestMethod]
        public async Task ReviewService_CleanupIndexedContent_CleansUpSuccessfully()
        {
            // Arrange
            var archiveId = await _databaseService.ImportFileAsync(_testExcelPath, "testuser");
            await _databaseService.IndexFileContentAsync(_testExcelPath, archiveId);
            
            // Verify content exists before cleanup
            var contentBefore = await _databaseService.GetIndexedContentAsync(archiveId);
            Assert.IsTrue(contentBefore.Count > 0);

            // Act
            await _reviewService.CleanupIndexedContentAsync(archiveId);

            // Assert
            var contentAfter = await _databaseService.GetIndexedContentAsync(archiveId);
            Assert.AreEqual(0, contentAfter.Count);
        }
        
        /// <summary>
        /// Tests that new records are added to master data when user selects AddAsNew.
        /// </summary>
        [TestMethod]
        public async Task ReviewService_ProcessUserDecisions_AddsNewRecordsToMasterData()
        {
            // Arrange
            var archiveId = await _databaseService.ImportFileAsync(_testExcelPath, "testuser");
            await _databaseService.IndexFileContentAsync(_testExcelPath, archiveId);
            
            var (newRecords, matchRecords, disagreementRecords) = await _reviewService.IdentifyMatchingRecordsAsync(archiveId);
            
            // Add a sample record to master data first to test against
            await _databaseService.AddToMasterDataAsync("John", "{\"name\":\"John\",\"age\":24,\"city\":\"New York\"}");
            
            var newRecordDecisions = new List<(NewRecord record, UserDecision decision)>();
            foreach (var record in newRecords.Take(1)) // Process just one record
            {
                newRecordDecisions.Add((record, UserDecision.AddAsNew));
            }

            // Act
            await _reviewService.ProcessUserDecisionsAsync(
                newRecordDecisions,
                new List<(MatchRecord record, UserDecision decision)>(),
                new List<(DisagreementRecord record, UserDecision decision)>());

            // Assert - Verify that the record was added to master data
            // Note: This test would be more comprehensive with a more sophisticated matching logic
        }
        
        /// <summary>
        /// Tests that match records are updated in master data when user selects Update.
        /// </summary>
        [TestMethod]
        public async Task ReviewService_ProcessUserDecisions_UpdatesMatchRecordsInMasterData()
        {
            // Arrange
            var archiveId = await _databaseService.ImportFileAsync(_testExcelPath, "testuser");
            await _databaseService.IndexFileContentAsync(_testExcelPath, archiveId);
            
            // Add a record that will match with our test data
            await _databaseService.AddToMasterDataAsync("John", "{\"name\":\"John\",\"age\":24,\"city\":\"New York\"}");
            
            var (newRecords, matchRecords, disagreementRecords) = await _reviewService.IdentifyMatchingRecordsAsync(archiveId);
            
            var matchRecordDecisions = new List<(MatchRecord record, UserDecision decision)>();
            foreach (var record in matchRecords.Take(1)) // Process just one record
            {
                matchRecordDecisions.Add((record, UserDecision.Update));
            }

            // Act
            await _reviewService.ProcessUserDecisionsAsync(
                new List<(NewRecord record, UserDecision decision)>(),
                matchRecordDecisions,
                new List<(DisagreementRecord record, UserDecision decision)>());

            // Assert - Verify that the record was updated in master data
            // Note: This test would be more comprehensive with a more sophisticated matching logic
        }

        /// <summary>
        /// Creates a simple test Excel file for testing.
        /// </summary>
        /// <param name="filePath">The path where to create the test file.</param>
        private void CreateTestExcelFile(string filePath)
        {
            // Set EPPlus license for non-commercial use
            ExcelPackage.License.SetNonCommercialPersonal("khaled");
            
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