using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fouad.Services;
using System.IO;

namespace Fouad.Tests
{
    /// <summary>
    /// Unit tests for the AudioService class.
    /// Tests cover audio playback functionality.
    /// </summary>
    [TestClass]
    public class AudioServiceTests
    {
        private AudioService _audioService = null!;

        /// <summary>
        /// Initializes the test environment before each test method runs.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            _audioService = new AudioService();
        }

        /// <summary>
        /// Tests that the AudioService can be instantiated correctly.
        /// </summary>
        [TestMethod]
        public void AudioService_Constructor_CreatesInstance()
        {
            // Arrange & Act
            var audioService = new AudioService();

            // Assert
            Assert.IsNotNull(audioService);
        }

        /// <summary>
        /// Tests that PlaySound method exists and can be called without throwing exceptions.
        /// </summary>
        [TestMethod]
        public void PlaySound_Method_ExistsAndCallable()
        {
            // Act & Assert
            // This should not throw an exception even if the sound file doesn't exist
            _audioService.PlaySound("Success");
        }

        /// <summary>
        /// Tests that StopSound method exists and can be called without throwing exceptions.
        /// </summary>
        [TestMethod]
        public void StopSound_Method_ExistsAndCallable()
        {
            // Act & Assert
            // This should not throw an exception
            _audioService.StopSound();
        }

        /// <summary>
        /// Tests that Dispose method exists and can be called without throwing exceptions.
        /// </summary>
        [TestMethod]
        public void Dispose_Method_ExistsAndCallable()
        {
            // Act & Assert
            // This should not throw an exception
            _audioService.Dispose();
        }

        /// <summary>
        /// Tests that PlaySound method handles invalid sound names gracefully.
        /// </summary>
        [TestMethod]
        public void PlaySound_HandlesInvalidSoundName_Gracefully()
        {
            // Act & Assert
            // This should not throw an exception for an invalid sound name
            _audioService.PlaySound("NonExistentSound");
        }
    }
}