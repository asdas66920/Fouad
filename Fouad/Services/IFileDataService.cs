using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Fouad.Models;

namespace Fouad.Services
{
    /// <summary>
    /// Interface for file data service operations.
    /// </summary>
    public interface IFileDataService
    {
        /// <summary>
        /// Asynchronously loads a file (Excel or CSV) and creates an index for fast searching.
        /// </summary>
        /// <param name="filePath">Path to the file to load.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task LoadFileAsync(string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets column headers.
        /// </summary>
        /// <returns>List of column headers.</returns>
        List<string> GetColumnHeaders();

        /// <summary>
        /// Gets the total number of rows in the file (excluding header row).
        /// </summary>
        /// <returns>Number of rows in the file.</returns>
        int GetRowCount();

        /// <summary>
        /// Searches for a term in all cells of the loaded file.
        /// </summary>
        /// <param name="searchTerm">Term to search for.</param>
        /// <returns>List of row numbers where the term is found.</returns>
        List<int> Search(string searchTerm);

        /// <summary>
        /// Gets file information.
        /// </summary>
        /// <returns>File information.</returns>
        FileInfo? GetFileInfo();

        /// <summary>
        /// Checks if a file is loaded.
        /// </summary>
        /// <returns>True if a file is loaded, false otherwise.</returns>
        bool IsFileLoaded();

        /// <summary>
        /// Gets data for a specific row using cached data.
        /// </summary>
        /// <param name="rowNumber">Row number to retrieve.</param>
        /// <returns>List of cell values for the specified row.</returns>
        Task<List<string>> GetRowDataAsync(int rowNumber);

        /// <summary>
        /// Gets cached result by ID.
        /// </summary>
        /// <param name="id">ID of the result to retrieve.</param>
        /// <returns>Result object or null if not found.</returns>
        Result? GetResultById(int id);

        /// <summary>
        /// Updates a result in the cache.
        /// </summary>
        /// <param name="result">Result to update.</param>
        void UpdateResult(Result result);
        
        /// <summary>
        /// Sets the database service for database integration.
        /// </summary>
        /// <param name="databaseService">The database service to use.</param>
        void SetDatabaseService(DatabaseService databaseService);
        
        /// <summary>
        /// Imports a file using the database service.
        /// </summary>
        /// <param name="filePath">Path to the file to import.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task<int> ImportFileWithDatabaseAsync(string filePath);
        
        /// <summary>
        /// Gets the archive ID of the last imported file.
        /// </summary>
        /// <returns>The archive ID.</returns>
        int GetArchiveId();
        
        /// <summary>
        /// Clears the cached data and index to free up memory.
        /// </summary>
        void ClearCache();
        
        /// <summary>
        /// Gets the current memory usage of the cache.
        /// </summary>
        /// <returns>Approximate memory usage in bytes</returns>
        long GetCacheMemoryUsage();
    }
}