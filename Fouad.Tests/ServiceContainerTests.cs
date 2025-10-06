using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fouad.Services;
using System;

namespace Fouad.Tests
{
    /// <summary>
    /// Unit tests for the ServiceContainer class.
    /// </summary>
    [TestClass]
    public class ServiceContainerTests
    {
        private ServiceContainer _serviceContainer;

        /// <summary>
        /// Initializes the test environment before each test method runs.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            _serviceContainer = new ServiceContainer();
        }

        /// <summary>
        /// Tests that the ServiceContainer can be instantiated correctly.
        /// </summary>
        [TestMethod]
        public void ServiceContainer_Constructor_CreatesInstance()
        {
            // Arrange & Act
            var container = new ServiceContainer();

            // Assert
            Assert.IsNotNull(container);
        }

        /// <summary>
        /// Tests that Register and Resolve work correctly for instances.
        /// </summary>
        [TestMethod]
        public void RegisterAndResolve_Instance_WorksCorrectly()
        {
            // Arrange
            var testService = new TestService();
            _serviceContainer.Register<ITestService>(testService);

            // Act
            var resolvedService = _serviceContainer.Resolve<ITestService>();

            // Assert
            Assert.IsNotNull(resolvedService);
            Assert.AreSame(testService, resolvedService);
        }

        /// <summary>
        /// Tests that Register and Resolve work correctly for factories.
        /// </summary>
        [TestMethod]
        public void RegisterAndResolve_Factory_WorksCorrectly()
        {
            // Arrange
            _serviceContainer.Register<ITestService>(() => new TestService());

            // Act
            var resolvedService1 = _serviceContainer.Resolve<ITestService>();
            var resolvedService2 = _serviceContainer.Resolve<ITestService>();

            // Assert
            Assert.IsNotNull(resolvedService1);
            Assert.IsNotNull(resolvedService2);
            // Factory should return the same instance after first call (singleton behavior)
            Assert.AreSame(resolvedService1, resolvedService2);
        }

        /// <summary>
        /// Tests that resolving an unregistered service throws an exception.
        /// </summary>
        [TestMethod]
        public void Resolve_UnregisteredService_ThrowsException()
        {
            // Arrange - No service registered
            
            // Act & Assert
            try
            {
                _serviceContainer.Resolve<ITestService>();
                Assert.Fail("Expected InvalidOperationException to be thrown");
            }
            catch (InvalidOperationException ex)
            {
                // Expected exception
                Assert.IsTrue(ex.Message.Contains("not registered"));
            }
        }

        /// <summary>
        /// Tests that RegisterDefaultServices registers all expected services.
        /// </summary>
        [TestMethod]
        public void RegisterDefaultServices_RegistersAllServices()
        {
            // Arrange
            var databasePath = System.IO.Path.GetTempFileName();
            System.IO.File.Delete(databasePath); // We just need the path, not the file

            // Act
            _serviceContainer.RegisterDefaultServices(databasePath);

            // Assert
            Assert.IsNotNull(_serviceContainer.Resolve<IDatabaseService>());
            Assert.IsNotNull(_serviceContainer.Resolve<IFileDataService>());
            Assert.IsNotNull(_serviceContainer.Resolve<IAudioService>());
            Assert.IsNotNull(_serviceContainer.Resolve<IExportService>());
            Assert.IsNotNull(_serviceContainer.Resolve<IReviewService>());
            Assert.IsNotNull(_serviceContainer.Resolve<ConfigurationService>());
            Assert.IsNotNull(_serviceContainer.Resolve<ISearchService>());
        }
    }

    /// <summary>
    /// Test interface for service container tests.
    /// </summary>
    public interface ITestService
    {
        string GetData();
    }

    /// <summary>
    /// Test implementation for service container tests.
    /// </summary>
    public class TestService : ITestService
    {
        public string GetData()
        {
            return "Test data";
        }
    }
}