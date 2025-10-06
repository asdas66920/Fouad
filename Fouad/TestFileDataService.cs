using System;
using System.Threading.Tasks;
using Fouad.Services;

namespace Fouad
{
    public class TestFileDataService
    {
        public static async Task Test()
        {
            try
            {
                Console.WriteLine("Testing FileDataService...");
                
                var fileService = new FileDataService();
                
                // Test loading a CSV file
                string testFilePath = @"c:\Users\khaled\OneDrive\Desktop\f\FouadProject\test.csv";
                await fileService.LoadFileAsync(testFilePath);
                
                Console.WriteLine("File loaded successfully.");
                
                // Test if file is loaded
                if (fileService.IsFileLoaded())
                {
                    Console.WriteLine("File is loaded.");
                    
                    // Test getting file info
                    var fileInfo = fileService.GetFileInfo();
                    if (fileInfo != null)
                    {
                        Console.WriteLine($"File name: {fileInfo.Name}");
                        Console.WriteLine($"File size: {fileInfo.Length} bytes");
                    }
                    
                    // Test getting column headers
                    var headers = fileService.GetColumnHeaders();
                    Console.WriteLine("Column headers:");
                    foreach (var header in headers)
                    {
                        Console.WriteLine($"  {header}");
                    }
                    
                    // Test searching
                    var searchResults = fileService.Search("John");
                    Console.WriteLine($"Search results for 'John': {searchResults.Count} matches");
                    foreach (var row in searchResults)
                    {
                        Console.WriteLine($"  Row {row}");
                    }
                    
                    // Test getting row data
                    if (searchResults.Count > 0)
                    {
                        var rowData = await fileService.GetRowDataAsync(searchResults[0]);
                        Console.WriteLine("Row data:");
                        for (int i = 0; i < rowData.Count; i++)
                        {
                            Console.WriteLine($"  Column {i}: {rowData[i]}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("File is not loaded.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during testing: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}