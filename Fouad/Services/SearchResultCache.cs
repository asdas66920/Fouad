using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Fouad.Models;

namespace Fouad.Services
{
    /// <summary>
    /// Provides caching for search results to improve performance.
    /// </summary>
    public class SearchResultCache
    {
        private readonly ConcurrentDictionary<string, CacheEntry> _cache;
        private readonly int _maxCacheSize;
        private readonly TimeSpan _cacheExpiration;

        public SearchResultCache(int maxCacheSize = 1000, int cacheExpirationMinutes = 30)
        {
            _cache = new ConcurrentDictionary<string, CacheEntry>();
            _maxCacheSize = maxCacheSize;
            _cacheExpiration = TimeSpan.FromMinutes(cacheExpirationMinutes);
        }

        /// <summary>
        /// Gets cached search results for the specified search term and criteria.
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <param name="criteria">The search criteria.</param>
        /// <returns>Cached results if available and not expired, otherwise null.</returns>
        public List<Result>? GetCachedResults(string searchTerm, SearchCriteria criteria)
        {
            var key = GenerateCacheKey(searchTerm, criteria);
            
            if (_cache.TryGetValue(key, out var entry))
            {
                // Check if the entry is expired
                if (DateTime.UtcNow - entry.Timestamp < _cacheExpiration)
                {
                    return entry.Results;
                }
                else
                {
                    // Remove expired entry
                    _cache.TryRemove(key, out _);
                }
            }
            
            return null;
        }

        /// <summary>
        /// Caches search results for the specified search term and criteria.
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <param name="criteria">The search criteria.</param>
        /// <param name="results">The search results to cache.</param>
        public void CacheResults(string searchTerm, SearchCriteria criteria, List<Result> results)
        {
            var key = GenerateCacheKey(searchTerm, criteria);
            
            // If cache is at maximum size, remove the oldest entry
            if (_cache.Count >= _maxCacheSize)
            {
                var oldestKey = _cache.OrderBy(kvp => kvp.Value.Timestamp).First().Key;
                _cache.TryRemove(oldestKey, out _);
            }
            
            var entry = new CacheEntry
            {
                Results = results,
                Timestamp = DateTime.UtcNow
            };
            
            _cache[key] = entry;
        }

        /// <summary>
        /// Clears all cached results.
        /// </summary>
        public void ClearCache()
        {
            _cache.Clear();
        }

        /// <summary>
        /// Removes expired entries from the cache.
        /// </summary>
        public void CleanupExpiredEntries()
        {
            var expiredKeys = _cache
                .Where(kvp => DateTime.UtcNow - kvp.Value.Timestamp >= _cacheExpiration)
                .Select(kvp => kvp.Key)
                .ToList();
                
            foreach (var key in expiredKeys)
            {
                _cache.TryRemove(key, out _);
            }
        }

        /// <summary>
        /// Generates a cache key based on the search term and criteria.
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <param name="criteria">The search criteria.</param>
        /// <returns>A unique cache key.</returns>
        private string GenerateCacheKey(string searchTerm, SearchCriteria criteria)
        {
            // Create a hash-based key that uniquely identifies the search
            var key = $"{searchTerm}|{criteria.CaseSensitive}|{criteria.ExactMatch}|{criteria.FuzzySearch}|" +
                     $"{criteria.IsAllWordsSearch}|{criteria.IsAnyWordSearch}|{criteria.IsPhraseSearch}|" +
                     $"{criteria.EnableDateRange}|{criteria.StartDate}|{criteria.EndDate}|" +
                     $"{criteria.EnableTimeRange}|{criteria.StartTime}|{criteria.EndTime}|" +
                     $"{criteria.MaxResults}";
            
            // Add column filters to the key
            if (criteria.ColumnValueFilters != null)
            {
                foreach (var filter in criteria.ColumnValueFilters.OrderBy(f => f.ColumnName))
                {
                    key += $"|CF:{filter.ColumnName}:{filter.Operator}:{filter.Value}";
                }
            }
            
            // Add value range filters to the key
            if (criteria.ValueRangeFilters != null)
            {
                foreach (var filter in criteria.ValueRangeFilters.OrderBy(f => f.ColumnName))
                {
                    key += $"|VF:{filter.ColumnName}:{filter.MinValue}:{filter.MaxValue}";
                }
            }
            
            // Add selected columns to the key
            if (criteria.Columns != null)
            {
                key += $"|Cols:{string.Join(",", criteria.Columns.OrderBy(c => c))}";
            }
            
            return key;
        }

        /// <summary>
        /// Represents a cached search result entry.
        /// </summary>
        private class CacheEntry
        {
            public List<Result> Results { get; set; } = new List<Result>();
            public DateTime Timestamp { get; set; }
        }
    }
}