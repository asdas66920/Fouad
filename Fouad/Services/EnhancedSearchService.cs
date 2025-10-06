using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using Fouad.Models;
using Fouad.Utilities;
using Fouad.Properties;
using System.IO;
using Microsoft.Data.Sqlite;

namespace Fouad.Services
{
    /// <summary>
    /// Enhanced service for advanced search operations with improved performance.
    /// </summary>
    public class EnhancedSearchService : ISearchService
    {
        private readonly IFileDataService _fileDataService;
        private readonly IConfigurationService _configurationService;
        private readonly SearchResultCache _searchCache;
        private readonly object _searchLock = new object();
        private CancellationTokenSource? _currentSearchCancellationTokenSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnhancedSearchService"/> class.
        /// </summary>
        /// <param name="fileDataService">File data service</param>
        /// <param name="configurationService">Configuration service</param>
        public EnhancedSearchService(IFileDataService fileDataService, IConfigurationService configurationService)
        {
            _fileDataService = fileDataService;
            _configurationService = configurationService;
            _searchCache = new SearchResultCache();
        }

        /// <summary>
        /// Performs an advanced search with multiple criteria using improved algorithms.
        /// </summary>
        /// <param name="criteria">Search criteria</param>
        /// <returns>List of matching results</returns>
        public async Task<List<Result>> AdvancedSearchAsync(SearchCriteria criteria)
        {
            return await RetryHelper.ExecuteWithRetryAsync(
                async () =>
                {
                    // Check if results are cached
                    if (!string.IsNullOrEmpty(criteria.SearchTerm))
                    {
                        var cachedResults = _searchCache.GetCachedResults(criteria.SearchTerm, criteria);
                        if (cachedResults != null)
                        {
                            LoggingService.LogInfo($"Returning {cachedResults.Count} cached results for search term: {criteria.SearchTerm}");
                            return cachedResults.Take(criteria.MaxResults).ToList();
                        }
                    }

                    if (!_fileDataService.IsFileLoaded() || 
                        (string.IsNullOrEmpty(criteria.SearchTerm) && !criteria.EnableDateRange && 
                         !criteria.EnableTimeRange && (criteria.ColumnValueFilters?.Count == 0) && 
                         (criteria.ValueRangeFilters?.Count == 0)))
                    {
                        return new List<Result>();
                    }

                    // Create a new cancellation token for this search
                    lock (_searchLock)
                    {
                        _currentSearchCancellationTokenSource?.Cancel();
                        _currentSearchCancellationTokenSource = new CancellationTokenSource();
                    }

                    var cancellationToken = _currentSearchCancellationTokenSource.Token;

                    try
                    {
                        // Get all results from file data service with progress reporting
                        var allResults = await GetAllResultsWithProgressAsync(cancellationToken);
                        
                        // Apply search criteria with parallel processing
                        var filteredResults = await ApplySearchCriteriaAsync(allResults, criteria, cancellationToken);
                        
                        // Limit results
                        var finalResults = filteredResults.Take(criteria.MaxResults).ToList();
                        
                        // Cache the results if there's a search term
                        if (!string.IsNullOrEmpty(criteria.SearchTerm))
                        {
                            _searchCache.CacheResults(criteria.SearchTerm, criteria, finalResults);
                        }
                        
                        return finalResults;
                    }
                    finally
                    {
                        lock (_searchLock)
                        {
                            _currentSearchCancellationTokenSource?.Dispose();
                            _currentSearchCancellationTokenSource = null;
                        }
                    }
                },
                maxRetries: 3,
                delayMilliseconds: 500,
                isTransientException: IsTransientException);
        }

        /// <summary>
        /// Gets search suggestions based on partial input using a trie-based approach.
        /// </summary>
        /// <param name="partialInput">Partial input string</param>
        /// <param name="maxSuggestions">Maximum number of suggestions to return</param>
        /// <returns>List of search suggestions</returns>
        public List<string> GetSearchSuggestions(string partialInput, int maxSuggestions = 10)
        {
            try
            {
                if (string.IsNullOrEmpty(partialInput) || !_fileDataService.IsFileLoaded())
                {
                    return new List<string>();
                }

                // In a real implementation, this would use a trie or similar data structure
                // for efficient prefix matching. For now, we'll implement a basic version.
                var suggestions = new HashSet<string>();
                var lowerPartialInput = partialInput.ToLower();

                // Get all unique words from the data
                var allResults = GetAllResults();
                var allWords = new HashSet<string>();

                foreach (var result in allResults)
                {
                    // Add words from all fields
                    var allText = string.Join(" ", 
                        new[] { result.FileName ?? "", result.Content ?? "" }
                        .Concat(result.DynamicColumnValues ?? new List<string>()));
                    
                    var words = Regex.Split(allText, @"\W+")
                        .Where(w => !string.IsNullOrEmpty(w))
                        .Select(w => w.ToLower());
                    
                    foreach (var word in words)
                    {
                        allWords.Add(word);
                    }
                }

                // Find words that start with the partial input
                foreach (var word in allWords)
                {
                    if (word.StartsWith(lowerPartialInput) && word.Length > partialInput.Length)
                    {
                        suggestions.Add(word);
                        if (suggestions.Count >= maxSuggestions)
                            break;
                    }
                }

                return suggestions.Take(maxSuggestions).ToList();
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error getting search suggestions", ex);
                throw new InvalidOperationException(string.Format(ErrorMessages.ErrorGettingSearchSuggestions, ex.Message), ex);
            }
        }

        /// <summary>
        /// Gets all results from the file data service with progress reporting.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of all results</returns>
        private async Task<List<Result>> GetAllResultsWithProgressAsync(CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                var results = new List<Result>();
                
                try
                {
                    var rowCount = _fileDataService.GetRowCount();
                    var batchSize = Math.Max(100, rowCount / 10); // Process in batches
                    
                    for (int i = 1; i <= rowCount; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        
                        var result = _fileDataService.GetResultById(i);
                        if (result != null)
                        {
                            results.Add(result);
                        }
                        
                        // Report progress every batch
                        if (i % batchSize == 0)
                        {
                            LoggingService.LogInfo($"Loaded {i}/{rowCount} results");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    LoggingService.LogInfo("Search operation was cancelled");
                    throw;
                }
                catch (Exception ex)
                {
                    LoggingService.LogError($"Error in GetAllResultsWithProgressAsync: {ex.Message}", ex);
                    throw new InvalidOperationException(string.Format(ErrorMessages.ErrorPerformingSearch, ex.Message), ex);
                }
                
                return results;
            }, cancellationToken);
        }

        /// <summary>
        /// Gets all results from the file data service.
        /// </summary>
        /// <returns>List of all results</returns>
        private List<Result> GetAllResults()
        {
            var results = new List<Result>();
            
            try
            {
                var rowCount = _fileDataService.GetRowCount();
                
                // Use parallel processing for better performance on large datasets
                var resultIds = Enumerable.Range(1, rowCount).ToList();
                var parallelResults = new List<Result>();
                
                Parallel.ForEach(resultIds, id =>
                {
                    var result = _fileDataService.GetResultById(id);
                    if (result != null)
                    {
                        lock (parallelResults)
                        {
                            parallelResults.Add(result);
                        }
                    }
                });
                
                results.AddRange(parallelResults.OrderBy(r => r.Id));
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Error in GetAllResults: {ex.Message}", ex);
                throw new InvalidOperationException(string.Format(ErrorMessages.ErrorPerformingSearch, ex.Message), ex);
            }
            
            return results;
        }

        /// <summary>
        /// Applies search criteria to filter results using parallel processing.
        /// </summary>
        /// <param name="results">List of results to filter</param>
        /// <param name="criteria">Search criteria</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Filtered list of results</returns>
        private async Task<List<Result>> ApplySearchCriteriaAsync(List<Result> results, SearchCriteria criteria, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Use parallel processing for better performance
                    var matchingResults = new ConcurrentBag<Result>();
                    
                    Parallel.ForEach(results, result =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        
                        if (MatchesCriteria(result, criteria))
                        {
                            matchingResults.Add(result);
                        }
                    });
                    
                    return matchingResults.ToList();
                }
                catch (OperationCanceledException)
                {
                    LoggingService.LogInfo("Search criteria application was cancelled");
                    throw;
                }
                catch (Exception ex)
                {
                    LoggingService.LogError($"Error in ApplySearchCriteriaAsync: {ex.Message}", ex);
                    throw new InvalidOperationException(string.Format(ErrorMessages.ErrorPerformingSearch, ex.Message), ex);
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Determines whether a result matches the specified search criteria.
        /// </summary>
        /// <param name="result">The result to check.</param>
        /// <param name="criteria">The search criteria to match against.</param>
        /// <returns>True if the result matches the criteria, false otherwise.</returns>
        private bool MatchesCriteria(Result result, SearchCriteria criteria)
        {
            try
            {
                // Check if result is null
                if (result == null)
                    return false;
                    
                // Check if DynamicColumnValues is null
                if (result.DynamicColumnValues == null)
                    return false;
                
                // Apply date range filter
                if (criteria.EnableDateRange && criteria.StartDate.HasValue && criteria.EndDate.HasValue)
                {
                    if (result.SearchDate < criteria.StartDate.Value || result.SearchDate > criteria.EndDate.Value)
                    {
                        return false;
                    }
                }
                
                // Apply time range filter
                if (criteria.EnableTimeRange && !string.IsNullOrEmpty(criteria.StartTime) && !string.IsNullOrEmpty(criteria.EndTime))
                {
                    var resultTime = result.SearchDate.TimeOfDay;
                    if (TimeSpan.TryParse(criteria.StartTime, out var startTime) && TimeSpan.TryParse(criteria.EndTime, out var endTime))
                    {
                        if (resultTime < startTime || resultTime > endTime)
                        {
                            return false;
                        }
                    }
                }
                
                // If no search term and no other filters, match everything
                if (string.IsNullOrEmpty(criteria.SearchTerm) && 
                    (criteria.ColumnValueFilters?.Count == 0 || criteria.ColumnValueFilters == null) &&
                    (criteria.ValueRangeFilters?.Count == 0 || criteria.ValueRangeFilters == null))
                {
                    return true;
                }
                
                // Apply column value filters
                var headers = _fileDataService.GetColumnHeaders();
                if (headers != null && criteria.ColumnValueFilters != null && criteria.ColumnValueFilters.Count > 0)
                {
                    foreach (var filter in criteria.ColumnValueFilters)
                    {
                        var columnIndex = headers.IndexOf(filter.ColumnName);
                        if (columnIndex >= 0 && columnIndex < result.DynamicColumnValues.Count)
                        {
                            var cellValue = result.DynamicColumnValues[columnIndex];
                            if (!MatchesColumnValueFilter(cellValue, filter.Operator, filter.Value))
                            {
                                return false;
                            }
                        }
                    }
                }
                
                // Apply value range filters
                if (headers != null && criteria.ValueRangeFilters != null && criteria.ValueRangeFilters.Count > 0)
                {
                    foreach (var filter in criteria.ValueRangeFilters)
                    {
                        var columnIndex = headers.IndexOf(filter.ColumnName);
                        if (columnIndex >= 0 && columnIndex < result.DynamicColumnValues.Count)
                        {
                            var cellValue = result.DynamicColumnValues[columnIndex];
                            if (!MatchesValueRangeFilter(cellValue, filter.MinValue, filter.MaxValue))
                            {
                                return false;
                            }
                        }
                    }
                }
                
                // If no search term, and we've passed all other filters, match everything
                if (string.IsNullOrEmpty(criteria.SearchTerm))
                {
                    return true;
                }
                
                // Determine which columns to search in
                var columnsToSearch = criteria.Columns.Count > 0 ? criteria.Columns : 
                                     _fileDataService.GetColumnHeaders();
                
                // Check if columnsToSearch is null
                if (columnsToSearch == null)
                    return false;
                
                // Get the values from the specified columns
                var valuesToSearch = new List<string>();
                
                // Check if headers is null
                if (headers == null)
                    return false;
                
                // Make sure we don't exceed the bounds of the arrays
                var maxColumns = Math.Min(headers.Count, result.DynamicColumnValues.Count);
                
                foreach (var columnName in columnsToSearch)
                {
                    var columnIndex = headers.IndexOf(columnName);
                    if (columnIndex >= 0 && columnIndex < maxColumns)
                    {
                        valuesToSearch.Add(result.DynamicColumnValues[columnIndex]);
                    }
                }
                
                // If no specific columns were specified, also search the FileName
                if (criteria.Columns.Count == 0 && !string.IsNullOrEmpty(result.FileName))
                {
                    valuesToSearch.Add(result.FileName);
                }
                
                // Combine all values to search in
                var combinedText = string.Join(" ", valuesToSearch);
                
                // Apply different search modes
                if (criteria.ExactMatch)
                {
                    return ExactMatchSearch(combinedText, criteria.SearchTerm, criteria.CaseSensitive);
                }
                else if (criteria.FuzzySearch)
                {
                    return FuzzySearch(combinedText, criteria.SearchTerm);
                }
                else if (criteria.RegexSearch)
                {
                    return RegexSearch(combinedText, criteria.SearchTerm, criteria.CaseSensitive);
                }
                else if (criteria.IsAllWordsSearch)
                {
                    return AllWordsSearch(combinedText, criteria.SearchTerm, criteria.CaseSensitive);
                }
                else if (criteria.IsAnyWordSearch)
                {
                    return AnyWordSearch(combinedText, criteria.SearchTerm, criteria.CaseSensitive);
                }
                else if (criteria.IsPhraseSearch)
                {
                    return ExactPhraseSearch(combinedText, criteria.SearchTerm, criteria.CaseSensitive);
                }
                else if (!string.IsNullOrEmpty(criteria.SearchTerm))
                {
                    return RegularSearch(combinedText, criteria.SearchTerm, criteria.CaseSensitive);
                }
                
                return true; // If no search term, match everything
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Error in MatchesCriteria: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Checks if a cell value matches a column value filter.
        /// </summary>
        /// <param name="cellValue">The cell value to check.</param>
        /// <param name="operator">The operator to use.</param>
        /// <param name="filterValue">The filter value to compare against.</param>
        /// <returns>True if the cell value matches the filter, false otherwise.</returns>
        private static bool MatchesColumnValueFilter(string cellValue, string @operator, string filterValue)
        {
            try
            {
                switch (@operator)
                {
                    case "=":
                        return cellValue.Equals(filterValue, StringComparison.OrdinalIgnoreCase);
                    case "!=":
                        return !cellValue.Equals(filterValue, StringComparison.OrdinalIgnoreCase);
                    case ">":
                        if (double.TryParse(cellValue, out var cellValueDouble) && double.TryParse(filterValue, out var filterValueDouble))
                        {
                            return cellValueDouble > filterValueDouble;
                        }
                        return string.Compare(cellValue, filterValue, StringComparison.OrdinalIgnoreCase) > 0;
                    case "<":
                        if (double.TryParse(cellValue, out cellValueDouble) && double.TryParse(filterValue, out filterValueDouble))
                        {
                            return cellValueDouble < filterValueDouble;
                        }
                        return string.Compare(cellValue, filterValue, StringComparison.OrdinalIgnoreCase) < 0;
                    case ">=":
                        if (double.TryParse(cellValue, out cellValueDouble) && double.TryParse(filterValue, out filterValueDouble))
                        {
                            return cellValueDouble >= filterValueDouble;
                        }
                        return string.Compare(cellValue, filterValue, StringComparison.OrdinalIgnoreCase) >= 0;
                    case "<=":
                        if (double.TryParse(cellValue, out cellValueDouble) && double.TryParse(filterValue, out filterValueDouble))
                        {
                            return cellValueDouble <= filterValueDouble;
                        }
                        return string.Compare(cellValue, filterValue, StringComparison.OrdinalIgnoreCase) <= 0;
                    case "Contains":
                        return cellValue.IndexOf(filterValue, StringComparison.OrdinalIgnoreCase) >= 0;
                    case "Starts With":
                        return cellValue.StartsWith(filterValue, StringComparison.OrdinalIgnoreCase);
                    case "Ends With":
                        return cellValue.EndsWith(filterValue, StringComparison.OrdinalIgnoreCase);
                    default:
                        return cellValue.Equals(filterValue, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Error in MatchesColumnValueFilter: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Checks if a cell value matches a value range filter.
        /// </summary>
        /// <param name="cellValue">The cell value to check.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <returns>True if the cell value is within the range, false otherwise.</returns>
        private static bool MatchesValueRangeFilter(string cellValue, string minValue, string maxValue)
        {
            try
            {
                if (double.TryParse(cellValue, out var cellValueDouble))
                {
                    var hasMin = double.TryParse(minValue, out var minDouble);
                    var hasMax = double.TryParse(maxValue, out var maxDouble);
                    
                    if (hasMin && hasMax)
                    {
                        return cellValueDouble >= minDouble && cellValueDouble <= maxDouble;
                    }
                    else if (hasMin)
                    {
                        return cellValueDouble >= minDouble;
                    }
                    else if (hasMax)
                    {
                        return cellValueDouble <= maxDouble;
                    }
                }
                
                return true; // If we can't parse as numbers, don't filter
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Error in MatchesValueRangeFilter: {ex.Message}", ex);
                return true; // Don't filter on error
            }
        }

        /// <summary>
        /// Performs an exact match search.
        /// </summary>
        /// <param name="text">Text to search in</param>
        /// <param name="searchTerm">Term to search for</param>
        /// <param name="caseSensitive">Whether to perform case sensitive search</param>
        /// <returns>True if exact match found, false otherwise</returns>
        private static bool ExactMatchSearch(string text, string searchTerm, bool caseSensitive)
        {
            if (caseSensitive)
            {
                return text.Equals(searchTerm);
            }
            else
            {
                return text.Equals(searchTerm, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Performs a regular search.
        /// </summary>
        /// <param name="text">Text to search in</param>
        /// <param name="searchTerm">Term to search for</param>
        /// <param name="caseSensitive">Whether to perform case sensitive search</param>
        /// <returns>True if match found, false otherwise</returns>
        private static bool RegularSearch(string text, string searchTerm, bool caseSensitive)
        {
            if (caseSensitive)
            {
                return text.Contains(searchTerm);
            }
            else
            {
                return text.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Performs a fuzzy search.
        /// </summary>
        /// <param name="text">Text to search in</param>
        /// <param name="searchTerm">Term to search for</param>
        /// <returns>True if fuzzy match found, false otherwise</returns>
        private static bool FuzzySearch(string text, string searchTerm)
        {
            // Simple fuzzy matching - check if the Levenshtein distance is small
            return CalculateLevenshteinDistance(text.ToLower(), searchTerm.ToLower()) <= 2;
        }

        /// <summary>
        /// Performs a regex search.
        /// </summary>
        /// <param name="text">Text to search in</param>
        /// <param name="searchTerm">Term to search for</param>
        /// <param name="caseSensitive">Whether to perform case sensitive search</param>
        /// <returns>True if regex match found, false otherwise</returns>
        private static bool RegexSearch(string text, string searchTerm, bool caseSensitive)
        {
            try
            {
                var options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                return Regex.IsMatch(text, searchTerm, options);
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Error in RegexSearch: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Performs an all words search.
        /// </summary>
        /// <param name="text">Text to search in</param>
        /// <param name="searchTerm">Term to search for</param>
        /// <param name="caseSensitive">Whether to perform case sensitive search</param>
        /// <returns>True if all words found, false otherwise</returns>
        private static bool AllWordsSearch(string text, string searchTerm, bool caseSensitive)
        {
            var words = searchTerm.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            
            foreach (var word in words)
            {
                if (text.IndexOf(word, comparison) < 0)
                {
                    return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// Performs an any word search.
        /// </summary>
        /// <param name="text">Text to search in</param>
        /// <param name="searchTerm">Term to search for</param>
        /// <param name="caseSensitive">Whether to perform case sensitive search</param>
        /// <returns>True if any word found, false otherwise</returns>
        private static bool AnyWordSearch(string text, string searchTerm, bool caseSensitive)
        {
            var words = searchTerm.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            
            foreach (var word in words)
            {
                if (text.IndexOf(word, comparison) >= 0)
                {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Performs an exact phrase search.
        /// </summary>
        /// <param name="text">Text to search in</param>
        /// <param name="searchTerm">Term to search for</param>
        /// <param name="caseSensitive">Whether to perform case sensitive search</param>
        /// <returns>True if exact phrase found, false otherwise</returns>
        private static bool ExactPhraseSearch(string text, string searchTerm, bool caseSensitive)
        {
            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            return text.IndexOf(searchTerm, comparison) >= 0;
        }

        /// <summary>
        /// Calculates the Levenshtein distance between two strings.
        /// </summary>
        /// <param name="source">Source string</param>
        /// <param name="target">Target string</param>
        /// <returns>Levenshtein distance</returns>
        private static int CalculateLevenshteinDistance(string source, string target)
        {
            if (string.IsNullOrEmpty(source))
                return string.IsNullOrEmpty(target) ? 0 : target.Length;

            if (string.IsNullOrEmpty(target))
                return source.Length;

            // Create matrix
            var matrix = new int[source.Length + 1, target.Length + 1];

            // Initialize first row and column
            for (int i = 0; i <= source.Length; i++)
                matrix[i, 0] = i;
            for (int j = 0; j <= target.Length; j++)
                matrix[0, j] = j;

            // Fill matrix
            for (int i = 1; i <= source.Length; i++)
            {
                for (int j = 1; j <= target.Length; j++)
                {
                    var cost = source[i - 1] == target[j - 1] ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(
                            matrix[i - 1, j] + 1,      // deletion
                            matrix[i, j - 1] + 1),     // insertion
                        matrix[i - 1, j - 1] + cost);  // substitution
                }
            }

            return matrix[source.Length, target.Length];
        }

        /// <summary>
        /// Determines if an exception is transient and should be retried.
        /// </summary>
        /// <param name="ex">The exception to check.</param>
        /// <returns>True if the exception is transient, false otherwise.</returns>
        private static bool IsTransientException(Exception ex)
        {
            // Check for file system related transient exceptions
            if (ex is IOException ioEx)
            {
                // File is being used by another process
                if (ioEx.Message.Contains("being used by another process"))
                {
                    return true;
                }
                
                // Disk is full or other I/O issues
                if (ioEx.Message.Contains("disk") || ioEx.Message.Contains("I/O"))
                {
                    return true;
                }
            }
            
            // Network related exceptions (if file is remote)
            if (ex is System.Net.Sockets.SocketException)
            {
                return true;
            }
            
            // Timeout exceptions
            if (ex is TimeoutException)
            {
                return true;
            }
            
            // Check for database related transient exceptions
            if (ex is SqliteException sqliteEx)
            {
                // Database is locked or busy
                if (sqliteEx.SqliteErrorCode == 5 || sqliteEx.SqliteErrorCode == 6)
                {
                    return true;
                }
                
                // Disk I/O error
                if (sqliteEx.SqliteErrorCode == 10)
                {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Clears the search result cache.
        /// </summary>
        public void ClearCache()
        {
            _searchCache.ClearCache();
        }
    }
}