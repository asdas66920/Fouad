using System;
using System.Collections.Generic;
using Fouad.Models;
using Fouad.Services;

namespace DebugSearchService
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Create a simple implementation of IFileDataService for testing
                var fileDataService = new SimpleFileDataService();
                var configurationService = new SimpleConfigurationService();
                
                var searchService = new SearchService(fileDataService, configurationService);
                
                var criteria = new SearchCriteria
                {
                    SearchTerm = "test",
                    MaxResults = 10
                };
                
                var results = searchService.AdvancedSearchAsync(criteria).Result;
                
                Console.WriteLine($"Found {results.Count} results");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
    
    // Simple implementation of IFileDataService for testing
    class SimpleFileDataService : IFileDataService
    {
        public bool IsFileLoaded() => true;
        
        public int GetRowCount() => 1;
        
        public Result GetResultById(int id) => new Result { Id = id, DynamicColumnValues = new() { "test value" } };
        
        public List<string> GetColumnHeaders() => new() { "Column1" };

        public List<Result> GetAllResults() => new();

        public List<string> SearchResults(string searchTerm, int maxResults = 100) => new();

        public List<Result> GetPagedResults(int pageNumber, int pageSize) => new();

        public void LoadFile(string filePath) { }
        public void LoadFileAsync(string filePath) { }
        public void ClearData() { }
        public bool ContainsData() => true;
        public int GetColumnCount() => 1;
        public string GetColumnHeader(int index) => "Column1";
        public int GetTotalPages(int pageSize) => 1;
        public void OptimizeMemoryUsage() { }
        public void SetMemoryLimit(long limit) { }
    }
    
    // Simple implementation of IConfigurationService for testing
    class SimpleConfigurationService : IConfigurationService
    {
        public T GetSetting<T>(string key, T defaultValue = default(T)) => defaultValue;
        
        public void SaveSetting<T>(string key, T value) { }
    }
}