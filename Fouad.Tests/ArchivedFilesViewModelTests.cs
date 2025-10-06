using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Fouad.Services;
using Fouad.ViewModels;
using Fouad.Models;

namespace Fouad.Tests
{
    /// <summary>
    /// Unit tests for the ArchivedFilesViewModel class.
    /// Tests cover loading archived files, working with selected files, and other functionality.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class ArchivedFilesViewModelTests
    {
        private IDatabaseService _databaseService = null!;
        private FileDataService _fileDataService = null!;
        private ArchivedFilesViewModel _viewModel = null!;
        private string _testDatabasePath = null!;

        /// <summary>
        /// Initializes the test environment before each test method runs.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            // Create a temporary database file for testing
            _testDatabasePath = Path.Combine(Path.GetTempPath(), $"test_database_{Guid.NewGuid()}.db");
            _databaseService = new DatabaseService(_testDatabasePath);
            _fileDataService = new FileDataService();
            _viewModel = new ArchivedFilesViewModel(_databaseService, _fileDataService);
        }

        /// <summary>
        /// Cleans up the test environment after each test method runs.
        /// </summary>
        [TestCleanup]
        public async Task TestCleanup()
        {
            // Wait for any background operations to complete
            await Task.Delay(100);
            
            // Dispose the view model to close any open connections
            if (_viewModel != null)
            {
                try
                {
                    _viewModel.Dispose();
                }
                catch (Exception ex)
                {
                    // Log but don't throw to avoid breaking the test
                    Console.WriteLine($"Warning: Error disposing ArchivedFilesViewModel: {ex.Message}");
                }
                _viewModel = null!;
            }
            
            // Dispose the file data service to clear cache and close any open files
            if (_fileDataService != null)
            {
                try
                {
                    _fileDataService.ClearCache();
                }
                catch (Exception ex)
                {
                    // Log but don't throw to avoid breaking the test
                    Console.WriteLine($"Warning: Error clearing FileDataService cache: {ex.Message}");
                }
                _fileDataService = null!;
            }

            // Dispose the database service to close any open connections
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

            // Force garbage collection to ensure all file handles are released
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Add a larger delay to ensure file handles are released
            await Task.Delay(3000);

            // Try to delete the test database file with more robust retry logic
            if (!string.IsNullOrEmpty(_testDatabasePath))
            {
                DeleteFileWithRetry(_testDatabasePath);
            }
            
            // Also try to delete any binary cache files that might have been created
            try
            {
                var directory = Path.GetDirectoryName(_testDatabasePath);
                if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                {
                    var cacheFiles = Directory.GetFiles(directory, "*.bin");
                    foreach (var cacheFile in cacheFiles)
                    {
                        DeleteFileWithRetry(cacheFile);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log but don't throw to avoid breaking the test
                Console.WriteLine($"Warning: Error deleting cache files: {ex.Message}");
            }
            
            // Also try to delete the archive folder if it was created
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
        /// Deletes a file with retry logic.
        /// </summary>
        /// <param name="filePath">Path to the file to delete.</param>
        private void DeleteFileWithRetry(string filePath)
        {
            if (!File.Exists(filePath)) return;
            
            for (int i = 0; i < 25; i++) // Try up to 25 times
            {
                try
                {
                    File.Delete(filePath);
                    Console.WriteLine($"Successfully deleted file: {filePath}");
                    break; // Success, exit the loop
                }
                catch (UnauthorizedAccessException)
                {
                    // File might be read-only, try to change attributes
                    try
                    {
                        File.SetAttributes(filePath, FileAttributes.Normal);
                        File.Delete(filePath);
                        Console.WriteLine($"Successfully deleted file after changing attributes: {filePath}");
                        break;
                    }
                    catch
                    {
                        if (i < 24) // Don't sleep on the last iteration
                        {
                            System.Threading.Thread.Sleep(1000);
                        }
                        else
                        {
                            Console.WriteLine($"Warning: Could not delete file {filePath} after 25 attempts (UnauthorizedAccessException)");
                        }
                    }
                }
                catch (IOException ioEx)
                {
                    // File is still locked, wait a bit and try again
                    if (i < 24) // Don't sleep on the last iteration
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Could not delete file {filePath} after 25 attempts (IOException): {ioEx.Message}");
                    }
                }
                catch (Exception ex)
                {
                    // Log other exceptions but don't throw to avoid breaking the test
                    Console.WriteLine($"Warning: Error deleting file {filePath}: {ex.Message}");
                    break;
                }
            }
        }
        
        /// <summary>
        /// Deletes a directory with retry logic.
        /// </summary>
        /// <param name="directoryPath">Path to the directory to delete.</param>
        private void DeleteDirectoryWithRetry(string directoryPath)
        {
            if (!Directory.Exists(directoryPath)) return;
            
            for (int i = 0; i < 20; i++)
            {
                try
                {
                    Directory.Delete(directoryPath, true);
                    Console.WriteLine($"Successfully deleted directory: {directoryPath}");
                    break;
                }
                catch (UnauthorizedAccessException)
                {
                    // Try to change attributes of files in directory
                    try
                    {
                        var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            File.SetAttributes(file, FileAttributes.Normal);
                        }
                        Directory.Delete(directoryPath, true);
                        Console.WriteLine($"Successfully deleted directory after changing attributes: {directoryPath}");
                        break;
                    }
                    catch
                    {
                        if (i < 19)
                        {
                            System.Threading.Thread.Sleep(1000);
                        }
                        else
                        {
                            Console.WriteLine($"Warning: Could not delete directory {directoryPath} after 20 attempts (UnauthorizedAccessException)");
                        }
                    }
                }
                catch (IOException)
                {
                    if (i < 19)
                    {
                        System.Threading.Thread.Sleep(1500);
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Could not delete directory {directoryPath} after 20 attempts (IOException)");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Error deleting directory {directoryPath}: {ex.Message}");
                    break;
                }
            }
        }

        /// <summary>
        /// Tests that the ArchivedFilesViewModel can be instantiated correctly.
        /// </summary>
        [TestMethod]
        public async Task ArchivedFilesViewModel_Constructor_CreatesInstance()
        {
            // Arrange & Act
            // Wait for any background loading to complete
            await Task.Delay(500);
            var viewModel = new ArchivedFilesViewModel(_databaseService, _fileDataService);

            // Assert
            Assert.IsNotNull(viewModel);
            Assert.IsNotNull(viewModel.ArchivedFiles);
            Assert.IsNotNull(viewModel.SelectedFiles);
        }

        /// <summary>
        /// Tests that the work on selected file command is enabled when one file is selected.
        /// </summary>
        [TestMethod]
        public async Task ArchivedFilesViewModel_WorkOnSelectedFileCommand_EnablesWithOneFile()
        {
            // Wait for any background loading to complete
            await Task.Delay(500);
            
            // Arrange
            var testFile = new ArchivedFile
            {
                ArchiveId = 1,
                FileName = "TestFile.xlsx",
                UploadDate = DateTime.Now,
                UploadedBy = "TestUser",
                FilePath = "C:\\Test\\TestFile.xlsx"
            };
            
            _viewModel.SelectedFiles.Add(testFile);

            // Act
            var canExecute = _viewModel.WorkOnSelectedFileCommand.CanExecute(null);

            // Assert
            Assert.IsTrue(canExecute);
        }

        /// <summary>
        /// Tests that the work on multiple files command is enabled when multiple files are selected.
        /// </summary>
        [TestMethod]
        public async Task ArchivedFilesViewModel_WorkOnMultipleFilesCommand_EnablesWithMultipleFiles()
        {
            // Wait for any background loading to complete
            await Task.Delay(500);
            
            // Arrange
            var testFile1 = new ArchivedFile
            {
                ArchiveId = 1,
                FileName = "TestFile1.xlsx",
                UploadDate = DateTime.Now,
                UploadedBy = "TestUser",
                FilePath = "C:\\Test\\TestFile1.xlsx"
            };
            
            var testFile2 = new ArchivedFile
            {
                ArchiveId = 2,
                FileName = "TestFile2.xlsx",
                UploadDate = DateTime.Now,
                UploadedBy = "TestUser",
                FilePath = "C:\\Test\\TestFile2.xlsx"
            };
            
            _viewModel.SelectedFiles.Add(testFile1);
            _viewModel.SelectedFiles.Add(testFile2);

            // Act
            var canExecute = _viewModel.WorkOnMultipleFilesCommand.CanExecute(null);

            // Assert
            Assert.IsTrue(canExecute);
        }

        /// <summary>
        /// Tests that the compare files command is enabled when at least two files are selected.
        /// </summary>
        [TestMethod]
        public async Task ArchivedFilesViewModel_CompareFilesCommand_EnablesWithTwoOrMoreFiles()
        {
            // Wait for any background loading to complete
            await Task.Delay(500);
            
            // Arrange
            var testFile1 = new ArchivedFile
            {
                ArchiveId = 1,
                FileName = "TestFile1.xlsx",
                UploadDate = DateTime.Now,
                UploadedBy = "TestUser",
                FilePath = "C:\\Test\\TestFile1.xlsx"
            };
            
            var testFile2 = new ArchivedFile
            {
                ArchiveId = 2,
                FileName = "TestFile2.xlsx",
                UploadDate = DateTime.Now,
                UploadedBy = "TestUser",
                FilePath = "C:\\Test\\TestFile2.xlsx"
            };
            
            _viewModel.SelectedFiles.Add(testFile1);
            _viewModel.SelectedFiles.Add(testFile2);

            // Act
            var canExecute = _viewModel.CompareSelectedFilesCommand.CanExecute(null);

            // Assert
            Assert.IsTrue(canExecute);
        }

        /// <summary>
        /// Tests that the view latest info command is enabled when one file is selected.
        /// </summary>
        [TestMethod]
        public async Task ArchivedFilesViewModel_ViewLatestInfoCommand_EnablesWithOneFile()
        {
            // Wait for any background loading to complete
            await Task.Delay(500);
            
            // Arrange
            var testFile = new ArchivedFile
            {
                ArchiveId = 1,
                FileName = "TestFile.xlsx",
                UploadDate = DateTime.Now,
                UploadedBy = "TestUser",
                FilePath = "C:\\Test\\TestFile.xlsx"
            };
            
            _viewModel.SelectedFiles.Add(testFile);

            // Act
            var canExecute = _viewModel.ViewLatestInfoCommand.CanExecute(null);

            // Assert
            Assert.IsTrue(canExecute);
        }
        
        /// <summary>
        /// Tests that the archived files can be loaded successfully.
        /// </summary>
        [TestMethod]
        public async Task ArchivedFilesViewModel_LoadArchivedFiles_LoadsSuccessfully()
        {
            // Wait for initial background loading to complete
            await Task.Delay(1000);
            
            // Ensure the ArchivedFiles collection is populated
            Assert.IsNotNull(_viewModel.ArchivedFiles);
            
            // We're not adding any test data, so the collection should be empty
            Assert.AreEqual(0, _viewModel.ArchivedFiles.Count);
        }
    }
}