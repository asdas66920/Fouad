using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fouad.Models;

namespace Fouad.Services
{
    /// <summary>
    /// Service for handling the manual review process of imported data.
    /// </summary>
    public class ReviewService : IReviewService
    {
        private readonly IDatabaseService _databaseService;
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReviewService"/> class.
        /// </summary>
        /// <param name="databaseService">The database service to use.</param>
        /// <param name="connectionString">The database connection string.</param>
        public ReviewService(IDatabaseService databaseService, string connectionString)
        {
            _databaseService = databaseService;
            _connectionString = connectionString;
        }

        /// <summary>
        /// Identifies matching records by comparing data in the Content_Index table with the global master table.
        /// </summary>
        /// <param name="archiveId">The ArchiveId to review.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task<(List<NewRecord> newRecords, List<MatchRecord> matchRecords, List<DisagreementRecord> disagreementRecords)> IdentifyMatchingRecordsAsync(int archiveId)
        {
            try
            {
                // Get all indexed content for this archive
                var contentIndex = await _databaseService.GetIndexedContentAsync(archiveId);
                
                // Get the file name for this archive
                var fileName = await GetFileNameAsync(archiveId) ?? "Unknown File";
                
                // Group content by row number to reconstruct records
                var rows = contentIndex.GroupBy(c => c.RowNumber).ToList();
                
                // Lists to hold our results
                var newRecords = new List<NewRecord>();
                var matchRecords = new List<MatchRecord>();
                var disagreementRecords = new List<DisagreementRecord>();
                
                // Process each row (skip header row)
                foreach (var rowGroup in rows.Skip(1))
                {
                    // Create a list of cell values for this row
                    var rowData = rowGroup.Select(c => c.CellValue ?? "").ToList();
                    
                    // Determine a unique key for this record (using first column as key for now)
                    // In a real implementation, this would be based on business logic
                    var uniqueKey = rowData.FirstOrDefault() ?? $"Row{rowGroup.Key}";
                    
                    // Check if this record exists in the master table
                    var masterRecord = await GetMasterRecordAsync(uniqueKey);
                    
                    if (masterRecord == null)
                    {
                        // This is a new record
                        var newRecord = new NewRecord
                        {
                            Id = rowGroup.Key,
                            ArchiveId = archiveId,
                            FileName = fileName,
                            DynamicColumnValues = rowData
                        };
                        
                        newRecords.Add(newRecord);
                    }
                    else
                    {
                        // This record exists in the master table
                        // Check for discrepancies
                        var hasDiscrepancies = false;
                        var discrepancyColumns = new List<int>();
                        
                        // Compare each field (simplified comparison for now)
                        for (int i = 0; i < rowData.Count; i++)
                        {
                            // In a real implementation, we would deserialize the master data and compare
                            // For now, we'll simulate this by checking if the values are different
                            if (!masterRecord.Value.Data.Contains(rowData[i]))
                            {
                                // Values don't match - this is a discrepancy
                                hasDiscrepancies = true;
                                discrepancyColumns.Add(i);
                            }
                        }
                        
                        if (hasDiscrepancies)
                        {
                            // Create disagreement record
                            var disagreementRecord = new DisagreementRecord
                            {
                                Id = rowGroup.Key,
                                ArchiveId = archiveId,
                                FileName = fileName,
                                NewData = rowData,
                                ExistingData = new List<string>(), // Would be populated from master record in real implementation
                                DiscrepancyColumns = discrepancyColumns
                            };
                            
                            disagreementRecords.Add(disagreementRecord);
                        }
                        else
                        {
                            // Create match record
                            var matchRecord = new MatchRecord
                            {
                                Id = rowGroup.Key,
                                ArchiveId = archiveId,
                                FileName = fileName,
                                NewData = rowData,
                                ExistingData = new List<string>() // Would be populated from master record in real implementation
                            };
                            
                            matchRecords.Add(matchRecord);
                        }
                    }
                }
                
                LoggingService.LogInfo($"Identified {newRecords.Count} new records, {matchRecords.Count} matches, and {disagreementRecords.Count} disagreements for ArchiveId: {archiveId}");
                return (newRecords, matchRecords, disagreementRecords);
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error identifying matching records", ex);
                throw;
            }
        }

        /// <summary>
        /// Gets the file name for an archive.
        /// </summary>
        /// <param name="archiveId">The ArchiveId to get the file name for.</param>
        /// <returns>The file name.</returns>
        private async Task<string?> GetFileNameAsync(int archiveId)
        {
            try
            {
                var archiveLog = await _databaseService.GetArchiveLogAsync(archiveId);
                return archiveLog?.FileName;
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error getting file name", ex);
                return null;
            }
        }

        /// <summary>
        /// Gets a master record by unique key.
        /// </summary>
        /// <param name="uniqueKey">The unique key to search for.</param>
        /// <returns>Master record data or null if not found.</returns>
        private async Task<(int MasterId, string UniqueKey, string Data, DateTime CreatedDate, DateTime LastUpdated)?> GetMasterRecordAsync(string uniqueKey)
        {
            try
            {
                return await _databaseService.GetMasterRecordAsync(uniqueKey);
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Error retrieving master record for key: {uniqueKey}", ex);
                return null;
            }
        }

        /// <summary>
        /// Processes user decisions for the reviewed records.
        /// </summary>
        /// <param name="newRecords">The new records with user decisions.</param>
        /// <param name="matchRecords">The match records with user decisions.</param>
        /// <param name="disagreementRecords">The disagreement records with user decisions.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task ProcessUserDecisionsAsync(
            List<(NewRecord record, UserDecision decision)> newRecords,
            List<(MatchRecord record, UserDecision decision)> matchRecords,
            List<(DisagreementRecord record, UserDecision decision)> disagreementRecords)
        {
            try
            {
                // Process new records
                foreach (var (record, decision) in newRecords)
                {
                    await ProcessNewRecordDecisionAsync(record, decision);
                }
                
                // Process match records
                foreach (var (record, decision) in matchRecords)
                {
                    await ProcessMatchRecordDecisionAsync(record, decision);
                }
                
                // Process disagreement records
                foreach (var (record, decision) in disagreementRecords)
                {
                    await ProcessDisagreementRecordDecisionAsync(record, decision);
                }
                
                LoggingService.LogInfo($"Processed {newRecords.Count} new records, {matchRecords.Count} match records, and {disagreementRecords.Count} disagreement records");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error processing user decisions", ex);
                throw;
            }
        }

        /// <summary>
        /// Processes a new record decision.
        /// </summary>
        /// <param name="record">The new record.</param>
        /// <param name="decision">The user decision.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        private async Task ProcessNewRecordDecisionAsync(NewRecord record, UserDecision decision)
        {
            if (decision == UserDecision.AddAsNew)
            {
                // Convert record data to JSON
                var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(record.DynamicColumnValues);
                
                // Add to master data
                await _databaseService.AddToMasterDataAsync($"Record_{record.Id}", jsonData);
            }
            // For other decisions (Ignore, Update), do nothing for new records
        }

        /// <summary>
        /// Processes a match record decision.
        /// </summary>
        /// <param name="record">The match record.</param>
        /// <param name="decision">The user decision.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        private async Task ProcessMatchRecordDecisionAsync(MatchRecord record, UserDecision decision)
        {
            if (decision == UserDecision.Update)
            {
                // Convert record data to JSON
                var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(record.NewData);
                
                // Update master data
                await _databaseService.UpdateMasterDataAsync($"Record_{record.Id}", jsonData);
            }
            // For other decisions (Ignore, AddAsNew), do nothing for match records
        }

        /// <summary>
        /// Processes a disagreement record decision.
        /// </summary>
        /// <param name="record">The disagreement record.</param>
        /// <param name="decision">The user decision.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        private async Task ProcessDisagreementRecordDecisionAsync(DisagreementRecord record, UserDecision decision)
        {
            switch (decision)
            {
                case UserDecision.AddAsNew:
                    // Convert record data to JSON
                    var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(record.NewData);
                    
                    // Add to master data
                    await _databaseService.AddToMasterDataAsync($"Record_{record.Id}", jsonData);
                    break;
                    
                case UserDecision.Update:
                    // Convert record data to JSON
                    var updateData = Newtonsoft.Json.JsonConvert.SerializeObject(record.NewData);
                    
                    // Update master data
                    await _databaseService.UpdateMasterDataAsync($"Record_{record.Id}", updateData);
                    break;
                    
                case UserDecision.Ignore:
                    // Do nothing
                    break;
            }
        }

        /// <summary>
        /// Cleans up indexed content for an archive after review is complete.
        /// </summary>
        /// <param name="archiveId">The ArchiveId to clean up.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task CleanupIndexedContentAsync(int archiveId)
        {
            try
            {
                // Use the database service to delete indexed content
                await _databaseService.DeleteIndexedContentAsync(archiveId);
                LoggingService.LogInfo($"Indexed content cleaned up for ArchiveId: {archiveId}");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error cleaning up indexed content", ex);
                throw;
            }
        }
    }
}