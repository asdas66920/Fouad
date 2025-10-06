using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Fouad.Services;
using OfficeOpenXml;

namespace Fouad.Tests
{
    /// <summary>
    /// Unit tests for the ExportService class.
    /// Tests cover Excel and PDF export functionality.
    /// </summary>
    [TestClass]
    public class ExportServiceTests
    {
        private ExportService _exportService = null!;
        private string _testExcelPath = null!;
        private string _testPdfPath = null!;

        /// <summary>
        /// Initializes the test environment before each test method runs.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            // Set EPPlus license for non-commercial use
            ExcelPackage.License.SetNonCommercialPersonal("khaled");
            
            _exportService = new ExportService();
            _testExcelPath = Path.Combine(Path.GetTempPath(), $"test_export_{Guid.NewGuid()}.xlsx");
            _testPdfPath = Path.Combine(Path.GetTempPath(), $"test_export_{Guid.NewGuid()}.pdf");
        }

        /// <summary>
        /// Cleans up the test environment after each test method runs.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            // Retry logic for file deletion with proper disposal
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    if (File.Exists(_testExcelPath))
                    {
                        File.Delete(_testExcelPath);
                    }

                    if (File.Exists(_testPdfPath))
                    {
                        File.Delete(_testPdfPath);
                    }
                    break; // Success, exit the loop
                }
                catch (Exception)
                {
                    if (i == 2) // Last attempt
                        throw;
                    System.Threading.Thread.Sleep(100); // Wait before retry
                }
            }
        }

        /// <summary>
        /// Tests that the ExportService can be instantiated correctly.
        /// </summary>
        [TestMethod]
        public void ExportService_Constructor_CreatesInstance()
        {
            // Arrange & Act
            var service = new ExportService();

            // Assert
            Assert.IsNotNull(service);
        }

        /// <summary>
        /// Tests that ExportToExcel correctly exports data to an Excel file.
        /// </summary>
        [TestMethod]
        public void ExportToExcel_ExportDataToExcelFile_Successfully()
        {
            // Arrange
            var testData = new List<TestData>
            {
                new TestData { Name = "John", Age = 30, City = "New York" },
                new TestData { Name = "Jane", Age = 25, City = "Los Angeles" }
            };

            // Act
            _exportService.ExportToExcel(testData, _testExcelPath);

            // Assert
            Assert.IsTrue(File.Exists(_testExcelPath));
        }

        /// <summary>
        /// Tests that ExportToPdf correctly exports data to a PDF file.
        /// </summary>
        [TestMethod]
        public void ExportToPdf_ExportDataToPdfFile_Successfully()
        {
            // Arrange
            var testData = new List<TestData>
            {
                new TestData { Name = "John", Age = 30, City = "New York" },
                new TestData { Name = "Jane", Age = 25, City = "Los Angeles" }
            };

            // Act
            _exportService.ExportToPdf(testData, _testPdfPath);

            // Assert
            Assert.IsTrue(File.Exists(_testPdfPath));
        }

        /// <summary>
        /// Tests that ExportToExcelAsync correctly exports data to an Excel file asynchronously.
        /// </summary>
        [TestMethod]
        public async Task ExportToExcelAsync_ExportDataToExcelFile_Successfully()
        {
            // Arrange
            var testData = new List<TestData>
            {
                new TestData { Name = "John", Age = 30, City = "New York" },
                new TestData { Name = "Jane", Age = 25, City = "Los Angeles" }
            };

            // Act
            await _exportService.ExportToExcelAsync(testData, _testExcelPath);

            // Assert
            Assert.IsTrue(File.Exists(_testExcelPath));
        }

        /// <summary>
        /// Tests that ExportToPdfAsync correctly exports data to a PDF file asynchronously.
        /// </summary>
        [TestMethod]
        public async Task ExportToPdfAsync_ExportDataToPdfFile_Successfully()
        {
            // Arrange
            var testData = new List<TestData>
            {
                new TestData { Name = "John", Age = 30, City = "New York" },
                new TestData { Name = "Jane", Age = 25, City = "Los Angeles" }
            };

            // Act
            await _exportService.ExportToPdfAsync(testData, _testPdfPath);

            // Assert
            Assert.IsTrue(File.Exists(_testPdfPath));
        }
    }

    /// <summary>
    /// Test data class for export tests.
    /// </summary>
    public class TestData
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string City { get; set; } = string.Empty;
    }
}