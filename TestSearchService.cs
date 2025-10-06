using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fouad.Services;
using Fouad.Models;

public class TestFileDataService : IFileDataService
{
    public bool IsFileLoaded() => true;
    public int GetRowCount() => 1;
    public Result GetResultById(int id) => new Result { Id = id, DynamicColumnValues = new List<string> { "test value" } };
    public List<string> GetColumnHeaders() => new List<string> { "Column1" };
    
    // Implement other required methods with minimal implementations
    public void LoadFile(string filePath) { }
    public void LoadFileAsync(string filePath) { }
    public List<Result> GetAllResults() => new List<Result>();
    public List<string> SearchResults(string searchTerm, int maxResults = 100) => new List<string>();
    public void ClearData() { }
    public bool ContainsData() => true;
    public int GetColumnCount() => 1;
    public string GetColumnHeader(int index) => "Column1";
    public List<Result> GetPagedResults(int pageNumber, int pageSize) => new List<Result>();
    public int GetTotalPages(int pageSize) => 1;
    public void OptimizeMemoryUsage() { }
    public void SetMemoryLimit(long limit) { }
}

public class TestConfigurationService : IConfigurationService
{
    public T GetSetting<T>(string key, T defaultValue = default(T)) => defaultValue;
    public void SaveSetting<T>(string key, T value) { }
    public int DefaultPageSize { get; set; } = 10;
    public int MaxSearchResults { get; set; } = 100;
    public bool EnableFuzzySearch { get; set; } = false;
    public bool EnableAudioFeedback { get; set; } = false;
}

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            var fileDataService = new TestFileDataService();
            var configurationService = new TestConfigurationService();
            
            var searchService = new SearchService(fileDataService, configurationService);
            
            var criteria = new SearchCriteria
            {
                SearchTerm = "test",
                MaxResults = 10
            };
            
            Console.WriteLine("Calling AdvancedSearchAsync...");
            var results = await searchService.AdvancedSearchAsync(criteria);
            
            Console.WriteLine($"Found {results.Count} results");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}