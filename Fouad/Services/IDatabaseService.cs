using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fouad.Models;

namespace Fouad.Services
{
    /// <summary>
    /// Interface for database service operations.
    /// </summary>
    public interface IDatabaseService : IDisposable
    {
        /// <summary>
        /// Imports a file and stores its metadata in the Archive_Log table.
        /// </summary>
        /// <param name="filePath">Path to the file to import.</param>
        /// <param name="uploadedBy">Username of the person uploading the file.</param>
        /// <returns>The ArchiveId of the imported file.</returns>
        Task<int> ImportFileAsync(string filePath, string uploadedBy);

        /// <summary>
        /// Indexes the content of an Excel file and stores it in the Content_Index table.
        /// </summary>
        /// <param name="filePath">Path to the Excel file to index.</param>
        /// <param name="archiveId">The ArchiveId of the file.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task IndexFileContentAsync(string filePath, int archiveId);

        /// <summary>
        /// Gets all indexed content for a specific archive.
        /// </summary>
        /// <param name="archiveId">The ArchiveId to retrieve content for.</param>
        /// <returns>List of ContentIndex entries.</returns>
        Task<List<ContentIndex>> GetIndexedContentAsync(int archiveId);

        /// <summary>
        /// Gets archive log entry by ID.
        /// </summary>
        /// <param name="archiveId">The ArchiveId to retrieve.</param>
        /// <returns>ArchiveLog entry or null if not found.</returns>
        Task<ArchiveLog?> GetArchiveLogAsync(int archiveId);
        
        /// <summary>
        /// Gets all archive logs.
        /// </summary>
        /// <returns>List of ArchiveLog entries.</returns>
        Task<List<ArchiveLog>> GetAllArchiveLogsAsync();
        
        /// <summary>
        /// Gets a master record by unique key.
        /// </summary>
        /// <param name="uniqueKey">The unique key to search for.</param>
        /// <returns>Master record data or null if not found.</returns>
        Task<(int MasterId, string UniqueKey, string Data, DateTime CreatedDate, DateTime LastUpdated)?> GetMasterRecordAsync(string uniqueKey);
        
        /// <summary>
        /// Adds a new record to the master data table.
        /// </summary>
        /// <param name="uniqueKey">The unique key for the record.</param>
        /// <param name="data">The JSON data for the record.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task AddToMasterDataAsync(string uniqueKey, string data);
        
        /// <summary>
        /// Updates a record in the master data table.
        /// </summary>
        /// <param name="uniqueKey">The unique key for the record.</param>
        /// <param name="data">The JSON data for the record.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task UpdateMasterDataAsync(string uniqueKey, string data);
        
        /// <summary>
        /// Deletes indexed content for a specific archive.
        /// </summary>
        /// <param name="archiveId">The ArchiveId to delete indexed content for.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task DeleteIndexedContentAsync(int archiveId);
    }
}