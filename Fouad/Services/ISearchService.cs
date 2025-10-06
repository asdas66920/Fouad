using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fouad.Models;

namespace Fouad.Services
{
    /// <summary>
    /// Interface for search service operations.
    /// </summary>
    public interface ISearchService
    {
        /// <summary>
        /// Performs an advanced search with multiple criteria.
        /// </summary>
        /// <param name="criteria">Search criteria</param>
        /// <returns>List of matching results</returns>
        Task<List<Result>> AdvancedSearchAsync(SearchCriteria criteria);
        
        /// <summary>
        /// Gets search suggestions based on partial input.
        /// </summary>
        /// <param name="partialInput">Partial input string</param>
        /// <param name="maxSuggestions">Maximum number of suggestions to return</param>
        /// <returns>List of search suggestions</returns>
        List<string> GetSearchSuggestions(string partialInput, int maxSuggestions = 10);
    }
    
    /// <summary>
    /// Search criteria for advanced search.
    /// </summary>
    public class SearchCriteria
    {
        /// <summary>
        /// Gets or sets the search term.
        /// </summary>
        public string SearchTerm { get; set; } = "";
        
        /// <summary>
        /// Gets or sets the column names to search in.
        /// </summary>
        public List<string> Columns { get; set; } = new List<string>();
        
        /// <summary>
        /// Gets or sets whether to perform exact match search.
        /// </summary>
        public bool ExactMatch { get; set; } = false;
        
        /// <summary>
        /// Gets or sets whether to perform case sensitive search.
        /// </summary>
        public bool CaseSensitive { get; set; } = false;
        
        /// <summary>
        /// Gets or sets the maximum number of results to return.
        /// </summary>
        public int MaxResults { get; set; } = 1000;
        
        /// <summary>
        /// Gets or sets whether to enable fuzzy search.
        /// </summary>
        public bool FuzzySearch { get; set; } = false;
        
        /// <summary>
        /// Gets or sets whether to enable regex search.
        /// </summary>
        public bool RegexSearch { get; set; } = false;
        
        /// <summary>
        /// Gets or sets whether to search for all words.
        /// </summary>
        public bool IsAllWordsSearch { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to search for any word.
        /// </summary>
        public bool IsAnyWordSearch { get; set; } = false;
        
        /// <summary>
        /// Gets or sets whether to search for exact phrase.
        /// </summary>
        public bool IsPhraseSearch { get; set; } = false;
        
        /// <summary>
        /// Gets or sets whether to enable date range filtering.
        /// </summary>
        public bool EnableDateRange { get; set; } = false;
        
        /// <summary>
        /// Gets or sets the start date for date range filtering.
        /// </summary>
        public DateTime? StartDate { get; set; }
        
        /// <summary>
        /// Gets or sets the end date for date range filtering.
        /// </summary>
        public DateTime? EndDate { get; set; }
        
        /// <summary>
        /// Gets or sets whether to enable time range filtering.
        /// </summary>
        public bool EnableTimeRange { get; set; } = false;
        
        /// <summary>
        /// Gets or sets the start time for time range filtering.
        /// </summary>
        public string StartTime { get; set; } = "00:00";
        
        /// <summary>
        /// Gets or sets the end time for time range filtering.
        /// </summary>
        public string EndTime { get; set; } = "23:59";
        
        /// <summary>
        /// Gets or sets the column value filters.
        /// </summary>
        public List<ColumnValueFilterCriteria> ColumnValueFilters { get; set; } = new List<ColumnValueFilterCriteria>();
        
        /// <summary>
        /// Gets or sets the value range filters.
        /// </summary>
        public List<ValueRangeFilterCriteria> ValueRangeFilters { get; set; } = new List<ValueRangeFilterCriteria>();
    }
    
    /// <summary>
    /// Represents a column value filter criteria.
    /// </summary>
    public class ColumnValueFilterCriteria
    {
        /// <summary>
        /// Gets or sets the column name.
        /// </summary>
        public string ColumnName { get; set; } = "";
        
        /// <summary>
        /// Gets or sets the operator.
        /// </summary>
        public string Operator { get; set; } = "=";
        
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string Value { get; set; } = "";
    }
    
    /// <summary>
    /// Represents a value range filter criteria.
    /// </summary>
    public class ValueRangeFilterCriteria
    {
        /// <summary>
        /// Gets or sets the column name.
        /// </summary>
        public string ColumnName { get; set; } = "";
        
        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        public string MinValue { get; set; } = "";
        
        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        public string MaxValue { get; set; } = "";
    }
}