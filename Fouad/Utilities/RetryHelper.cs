#nullable enable

using System;
using System.Threading.Tasks;
using Fouad.Services;

namespace Fouad.Utilities
{
    /// <summary>
    /// Provides retry functionality for operations that might fail transiently.
    /// </summary>
    public static class RetryHelper
    {
        /// <summary>
        /// Executes an operation with retry logic.
        /// </summary>
        /// <param name="operation">The operation to execute.</param>
        /// <param name="maxRetries">Maximum number of retry attempts.</param>
        /// <param name="delayMilliseconds">Delay between retries in milliseconds.</param>
        /// <param name="isTransientException">Function to determine if an exception is transient and should be retried.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public static async Task ExecuteWithRetryAsync(
            Func<Task> operation,
            int maxRetries = 3,
            int delayMilliseconds = 1000,
            Func<Exception, bool>? isTransientException = null)
        {
            isTransientException ??= (ex => true); // By default, retry on all exceptions
            
            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    await operation();
                    return; // Success, exit the loop
                }
                catch (Exception ex)
                {
                    // If this is the last attempt or the exception is not transient, re-throw
                    if (attempt == maxRetries || !isTransientException(ex))
                    {
                        LoggingService.LogError($"Operation failed after {attempt + 1} attempts", ex);
                        throw;
                    }
                    
                    // Log the retry attempt
                    LoggingService.LogWarning($"Operation failed on attempt {attempt + 1}, retrying in {delayMilliseconds}ms: {ex.Message}");
                    
                    // Wait before retrying
                    await Task.Delay(delayMilliseconds * (attempt + 1)); // Exponential backoff
                }
            }
        }

        /// <summary>
        /// Executes an operation with retry logic and returns a result.
        /// </summary>
        /// <typeparam name="T">The type of result.</typeparam>
        /// <param name="operation">The operation to execute.</param>
        /// <param name="maxRetries">Maximum number of retry attempts.</param>
        /// <param name="delayMilliseconds">Delay between retries in milliseconds.</param>
        /// <param name="isTransientException">Function to determine if an exception is transient and should be retried.</param>
        /// <returns>The result of the operation.</returns>
        public static async Task<T> ExecuteWithRetryAsync<T>(
            Func<Task<T>> operation,
            int maxRetries = 3,
            int delayMilliseconds = 1000,
            Func<Exception, bool>? isTransientException = null)
        {
            isTransientException ??= (ex => true); // By default, retry on all exceptions
            
            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex)
                {
                    // If this is the last attempt or the exception is not transient, re-throw
                    if (attempt == maxRetries || !isTransientException(ex))
                    {
                        LoggingService.LogError($"Operation failed after {attempt + 1} attempts", ex);
                        throw;
                    }
                    
                    // Log the retry attempt
                    LoggingService.LogWarning($"Operation failed on attempt {attempt + 1}, retrying in {delayMilliseconds}ms: {ex.Message}");
                    
                    // Wait before retrying
                    await Task.Delay(delayMilliseconds * (attempt + 1)); // Exponential backoff
                }
            }
            
            // This should never be reached, but compiler requires it
            throw new InvalidOperationException("Retry logic failed unexpectedly");
        }
    }
}