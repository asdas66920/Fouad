using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Fouad.Services;

namespace Fouad.Tests
{
    /// <summary>
    /// Unit tests for the ConfigurationService class.
    /// </summary>
    [TestClass]
    public class ConfigurationServiceTests
    {
        private ConfigurationService _configurationService = null!;
        private string _testConfigPath = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _testConfigPath = Path.Combine(Path.GetTempPath(), $"test_config_{Guid.NewGuid()}.json");
            _configurationService = new ConfigurationService(_testConfigPath);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (File.Exists(_testConfigPath))
            {
                File.Delete(_testConfigPath);
            }
        }

        [TestMethod]
        public void ConfigurationService_Constructor_CreatesInstance()
        {
            // Arrange & Act
            var service = new ConfigurationService(_testConfigPath);

            // Assert
            Assert.IsNotNull(service);
        }

        [TestMethod]
        public void Configuration_Persistence_WorksCorrectly()
        {
            // Arrange
            var testValue = 75; // This should be different from the default value
            _configurationService.DefaultPageSize = testValue;

            // Act - Create a new instance to test persistence
            var newService = new ConfigurationService(_testConfigPath);

            // Assert
            Assert.AreEqual(testValue, newService.DefaultPageSize);
        }
    }
}