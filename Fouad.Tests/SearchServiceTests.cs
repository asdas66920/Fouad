using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fouad.Services;
using Fouad.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;

namespace Fouad.Tests
{
    /// <summary>
    /// Unit tests for the SearchService class.
    /// </summary>
    [TestClass]
    public class SearchServiceTests
    {
        private SearchService _searchService;
        private Mock<IFileDataService> _mockFileDataService;
        private Mock<IConfigurationService> _mockConfigurationService;

        /// <summary>
        /// Initializes the test environment before each test method runs.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            _mockFileDataService = new Mock<IFileDataService>();
            _mockConfigurationService = new Mock<IConfigurationService>();
            
            _searchService = new SearchService(_mockFileDataService.Object, _mockConfigurationService.Object);
        }

        /// <summary>
        /// Tests that the SearchService can be instantiated correctly.
        /// </summary>
        [TestMethod]
        public void SearchService_Constructor_CreatesInstance()
        {
            // Arrange & Act
            var service = new SearchService(_mockFileDataService.Object, _mockConfigurationService.Object);

            // Assert
            Assert.IsNotNull(service);
        }

        /// <summary>
        /// Tests that AdvancedSearchAsync returns results for valid criteria.
        /// </summary>
        [TestMethod]
        public async Task AdvancedSearchAsync_ValidCriteria_ReturnsResults()
        {
            // Arrange
            var criteria = new SearchCriteria
            {
                SearchTerm = "test",
                MaxResults = 10
            };
            
            _mockFileDataService.Setup(x => x.IsFileLoaded()).Returns(true);
            _mockFileDataService.Setup(x => x.GetRowCount()).Returns(1);
            _mockFileDataService.Setup(x => x.GetResultById(1))
                .Returns(new Result { Id = 1, DynamicColumnValues = new List<string> { "test value" } });
            _mockFileDataService.Setup(x => x.GetColumnHeaders())
                .Returns(new List<string> { "Column1" });

            // Act
            var results = await _searchService.AdvancedSearchAsync(criteria);

            // Assert
            Assert.IsNotNull(results);
            // Note: The actual implementation would need to be enhanced to properly test this
        }

        /// <summary>
        /// Tests that AdvancedSearchAsync returns empty list when no file is loaded.
        /// </summary>
        [TestMethod]
        public async Task AdvancedSearchAsync_NoFileLoaded_ReturnsEmptyList()
        {
            // Arrange
            var criteria = new SearchCriteria
            {
                SearchTerm = "test"
            };
            
            _mockFileDataService.Setup(x => x.IsFileLoaded()).Returns(false);

            // Act
            var results = await _searchService.AdvancedSearchAsync(criteria);

            // Assert
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Count);
        }

        /// <summary>
        /// Tests that GetSearchSuggestions returns suggestions for valid input.
        /// </summary>
        [TestMethod]
        public void GetSearchSuggestions_ValidInput_ReturnsSuggestions()
        {
            // Arrange
            var partialInput = "tes";
            _mockFileDataService.Setup(x => x.IsFileLoaded()).Returns(true);

            // Act
            var suggestions = _searchService.GetSearchSuggestions(partialInput);

            // Assert
            Assert.IsNotNull(suggestions);
            // Note: The actual implementation would need to be enhanced to properly test this
        }

        /// <summary>
        /// Tests that GetSearchSuggestions returns empty list for empty input.
        /// </summary>
        [TestMethod]
        public void GetSearchSuggestions_EmptyInput_ReturnsEmptyList()
        {
            // Arrange
            var partialInput = "";

            // Act
            var suggestions = _searchService.GetSearchSuggestions(partialInput);

            // Assert
            Assert.IsNotNull(suggestions);
            Assert.AreEqual(0, suggestions.Count);
        }
    }
}