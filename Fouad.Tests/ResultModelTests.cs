using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fouad.Models;
using System;
using System.Collections.Generic;

namespace Fouad.Tests
{
    /// <summary>
    /// Unit tests for the Result model class.
    /// Tests cover property access and default values.
    /// </summary>
    [TestClass]
    public class ResultModelTests
    {
        /// <summary>
        /// Tests that the Result model can be instantiated correctly.
        /// </summary>
        [TestMethod]
        public void Result_Constructor_CreatesInstance()
        {
            // Arrange & Act
            var result = new Result();

            // Assert
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Tests that all properties of the Result model can be set and retrieved.
        /// </summary>
        [TestMethod]
        public void Result_Properties_SetAndGet_WorkCorrectly()
        {
            // Arrange
            var result = new Result();
            var expectedId = 1;
            var expectedFileName = "test-file.xlsx";
            var expectedContent = "test content";
            var expectedSearchDate = DateTime.Now;
            var expectedMatchCount = 5;
            var expectedDynamicColumnValues = new List<string> { "value1", "value2", "value3" };
            var expectedIsAddedToHistory = true;

            // Act
            result.Id = expectedId;
            result.FileName = expectedFileName;
            result.Content = expectedContent;
            result.SearchDate = expectedSearchDate;
            result.MatchCount = expectedMatchCount;
            result.DynamicColumnValues = expectedDynamicColumnValues;
            result.IsAddedToHistory = expectedIsAddedToHistory;

            // Assert
            Assert.AreEqual(expectedId, result.Id);
            Assert.AreEqual(expectedFileName, result.FileName);
            Assert.AreEqual(expectedContent, result.Content);
            Assert.AreEqual(expectedSearchDate, result.SearchDate);
            Assert.AreEqual(expectedMatchCount, result.MatchCount);
            Assert.AreEqual(expectedDynamicColumnValues, result.DynamicColumnValues);
            Assert.AreEqual(expectedIsAddedToHistory, result.IsAddedToHistory);
        }

        /// <summary>
        /// Tests that default values are correctly applied to the Result model.
        /// </summary>
        [TestMethod]
        public void Result_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var result = new Result();

            // Assert
            Assert.AreEqual(0, result.Id);
            Assert.IsNull(result.FileName);
            Assert.IsNull(result.Content);
            Assert.AreEqual(default(DateTime), result.SearchDate);
            Assert.AreEqual(0, result.MatchCount);
            Assert.IsNotNull(result.DynamicColumnValues);
            Assert.AreEqual(0, result.DynamicColumnValues.Count);
            Assert.IsFalse(result.IsAddedToHistory);
        }
    }
}