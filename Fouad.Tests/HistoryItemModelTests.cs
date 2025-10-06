using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fouad.Models;
using System;
using System.Collections.Generic;

namespace Fouad.Tests
{
    /// <summary>
    /// Unit tests for the HistoryItem model class.
    /// Tests cover property access and default values.
    /// </summary>
    [TestClass]
    public class HistoryItemModelTests
    {
        /// <summary>
        /// Tests that the HistoryItem model can be instantiated correctly.
        /// </summary>
        [TestMethod]
        public void HistoryItem_Constructor_CreatesInstance()
        {
            // Arrange & Act
            var historyItem = new HistoryItem();

            // Assert
            Assert.IsNotNull(historyItem);
        }

        /// <summary>
        /// Tests that all properties of the HistoryItem model can be set and retrieved.
        /// </summary>
        [TestMethod]
        public void HistoryItem_Properties_SetAndGet_WorkCorrectly()
        {
            // Arrange
            var historyItem = new HistoryItem();
            var expectedId = 1;
            var expectedFileName = "test-file.xlsx";
            var expectedSearchDate = DateTime.Now;
            var expectedSearchTerm = "test search";
            var expectedResultCount = 5;
            var expectedIsSelected = true;
            var expectedIsAddedToHistory = true;
            var expectedDynamicColumnValues = new List<string> { "value1", "value2", "value3" };

            // Act
            historyItem.Id = expectedId;
            historyItem.FileName = expectedFileName;
            historyItem.SearchDate = expectedSearchDate;
            historyItem.SearchTerm = expectedSearchTerm;
            historyItem.ResultCount = expectedResultCount;
            historyItem.IsSelected = expectedIsSelected;
            historyItem.IsAddedToHistory = expectedIsAddedToHistory;
            historyItem.DynamicColumnValues = expectedDynamicColumnValues;

            // Assert
            Assert.AreEqual(expectedId, historyItem.Id);
            Assert.AreEqual(expectedFileName, historyItem.FileName);
            Assert.AreEqual(expectedSearchDate, historyItem.SearchDate);
            Assert.AreEqual(expectedSearchTerm, historyItem.SearchTerm);
            Assert.AreEqual(expectedResultCount, historyItem.ResultCount);
            Assert.AreEqual(expectedIsSelected, historyItem.IsSelected);
            Assert.AreEqual(expectedIsAddedToHistory, historyItem.IsAddedToHistory);
            Assert.AreEqual(expectedDynamicColumnValues, historyItem.DynamicColumnValues);
        }

        /// <summary>
        /// Tests that default values are correctly applied to the HistoryItem model.
        /// </summary>
        [TestMethod]
        public void HistoryItem_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var historyItem = new HistoryItem();

            // Assert
            Assert.AreEqual(0, historyItem.Id);
            Assert.IsNull(historyItem.FileName);
            Assert.AreEqual(default(DateTime), historyItem.SearchDate);
            Assert.IsNull(historyItem.SearchTerm);
            Assert.AreEqual(0, historyItem.ResultCount);
            Assert.IsFalse(historyItem.IsSelected);
            Assert.IsFalse(historyItem.IsAddedToHistory);
            Assert.IsNotNull(historyItem.DynamicColumnValues);
            Assert.AreEqual(0, historyItem.DynamicColumnValues.Count);
        }
    }
}