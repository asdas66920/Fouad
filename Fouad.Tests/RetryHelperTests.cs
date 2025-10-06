using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fouad.Utilities;

namespace Fouad.Tests
{
    [TestClass]
    public class RetryHelperTests
    {
        [TestMethod]
        public async Task ExecuteWithRetryAsync_SuccessOnFirstAttempt_ReturnsResult()
        {
            // Arrange
            var expectedResult = "Success";
            var attemptCount = 0;
            
            // Act
            var result = await RetryHelper.ExecuteWithRetryAsync(async () =>
            {
                attemptCount++;
                await Task.Delay(10); // Simulate some work
                return expectedResult;
            });

            // Assert
            Assert.AreEqual(expectedResult, result);
            Assert.AreEqual(1, attemptCount);
        }

        [TestMethod]
        public async Task ExecuteWithRetryAsync_SuccessOnRetry_ReturnsResult()
        {
            // Arrange
            var expectedResult = "Success";
            var attemptCount = 0;
            
            // Act
            var result = await RetryHelper.ExecuteWithRetryAsync(async () =>
            {
                attemptCount++;
                await Task.Delay(10); // Simulate some work
                
                if (attemptCount < 3)
                {
                    throw new InvalidOperationException("Transient error");
                }
                
                return expectedResult;
            }, maxRetries: 3, delayMilliseconds: 50);

            // Assert
            Assert.AreEqual(expectedResult, result);
            Assert.AreEqual(3, attemptCount);
        }

        [TestMethod]
        public async Task ExecuteWithRetryAsync_MaxRetriesExceeded_ThrowsException()
        {
            // Arrange
            var attemptCount = 0;
            
            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await RetryHelper.ExecuteWithRetryAsync(async () =>
                {
                    attemptCount++;
                    await Task.Delay(10); // Simulate some work
                    throw new InvalidOperationException("Persistent error");
                }, maxRetries: 2, delayMilliseconds: 50);
            });
            
            // Verify that it retried the expected number of times
            Assert.AreEqual(3, attemptCount); // 1 initial attempt + 2 retries
        }

        [TestMethod]
        public async Task ExecuteWithRetryAsync_NonTransientException_ThrowsImmediately()
        {
            // Arrange
            var isTransientException = new Func<Exception, bool>(ex => ex is InvalidOperationException);
            var attemptCount = 0;
            
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            {
                await RetryHelper.ExecuteWithRetryAsync(async () =>
                {
                    attemptCount++;
                    await Task.Delay(10); // Simulate some work
                    throw new ArgumentException("Non-transient error");
                }, maxRetries: 3, delayMilliseconds: 50, isTransientException: isTransientException);
            });
            
            // Verify that it only attempted once (no retries for non-transient exceptions)
            Assert.AreEqual(1, attemptCount);
        }

        [TestMethod]
        public async Task ExecuteWithRetryAsync_Void_SuccessOnFirstAttempt()
        {
            // Arrange
            var attemptCount = 0;
            var success = false;
            
            // Act
            await RetryHelper.ExecuteWithRetryAsync(async () =>
            {
                attemptCount++;
                await Task.Delay(10); // Simulate some work
                success = true;
            });

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual(1, attemptCount);
        }

        [TestMethod]
        public async Task ExecuteWithRetryAsync_Void_SuccessOnRetry()
        {
            // Arrange
            var attemptCount = 0;
            var success = false;
            
            // Act
            await RetryHelper.ExecuteWithRetryAsync(async () =>
            {
                attemptCount++;
                await Task.Delay(10); // Simulate some work
                
                if (attemptCount < 3)
                {
                    throw new InvalidOperationException("Transient error");
                }
                
                success = true;
            }, maxRetries: 3, delayMilliseconds: 50);

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual(3, attemptCount);
        }

        [TestMethod]
        public async Task ExecuteWithRetryAsync_Void_MaxRetriesExceeded_ThrowsException()
        {
            // Arrange
            var attemptCount = 0;
            
            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await RetryHelper.ExecuteWithRetryAsync(async () =>
                {
                    attemptCount++;
                    await Task.Delay(10); // Simulate some work
                    throw new InvalidOperationException("Persistent error");
                }, maxRetries: 2, delayMilliseconds: 50);
            });
            
            // Verify that it retried the expected number of times
            Assert.AreEqual(3, attemptCount); // 1 initial attempt + 2 retries
        }

        [TestMethod]
        public async Task ExecuteWithRetryAsync_ExponentialBackoff_WaitsIncreasingly()
        {
            // Arrange
            var attemptCount = 0;
            var delays = new System.Collections.Generic.List<long>();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Act
            await RetryHelper.ExecuteWithRetryAsync(async () =>
            {
                attemptCount++;
                var elapsed = stopwatch.ElapsedMilliseconds;
                delays.Add(elapsed);
                
                await Task.Delay(10); // Simulate some work
                
                if (attemptCount < 3)
                {
                    throw new InvalidOperationException("Transient error");
                }
            }, maxRetries: 3, delayMilliseconds: 100);

            // Assert
            Assert.AreEqual(3, attemptCount);
            // The delays should be approximately 100ms, 200ms, 300ms (plus execution time)
            Assert.IsTrue(delays[1] > delays[0]);
            Assert.IsTrue(delays[2] > delays[1]);
        }
    }
}