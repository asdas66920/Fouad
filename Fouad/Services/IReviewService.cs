using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fouad.Models;

namespace Fouad.Services
{
    /// <summary>
    /// Interface for review service operations.
    /// </summary>
    public interface IReviewService
    {
        /// <summary>
        /// Identifies matching records by comparing data in the Content_Index table with the global master table.
        /// </summary>
        /// <param name="archiveId">The ArchiveId to review.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task<(List<NewRecord> newRecords, List<MatchRecord> matchRecords, List<DisagreementRecord> disagreementRecords)> IdentifyMatchingRecordsAsync(int archiveId);

        /// <summary>
        /// Processes user decisions for the reviewed records.
        /// </summary>
        /// <param name="newRecords">The new records with user decisions.</param>
        /// <param name="matchRecords">The match records with user decisions.</param>
        /// <param name="disagreementRecords">The disagreement records with user decisions.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task ProcessUserDecisionsAsync(
            List<(NewRecord record, UserDecision decision)> newRecords,
            List<(MatchRecord record, UserDecision decision)> matchRecords,
            List<(DisagreementRecord record, UserDecision decision)> disagreementRecords);

        /// <summary>
        /// Cleans up indexed content after processing is complete.
        /// </summary>
        /// <param name="archiveId">The ArchiveId to clean up.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task CleanupIndexedContentAsync(int archiveId);
    }
}