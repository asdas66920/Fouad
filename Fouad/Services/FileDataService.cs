using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using OfficeOpenXml;
using CsvHelper;
using MessagePack;
using Fouad.Models;
using Microsoft.Data.Sqlite;
using System.Threading;
using Fouad.Utilities;
using Fouad.Properties;

namespace Fouad.Services
{
    /// <summary>
    /// Service for loading, processing, and searching data files (Excel or CSV).
    /// Provides caching and indexing for improved performance.
    /// </summary>
    public class FileDataService : IFileDataService
    {
        private Dictionary<string, List<int>> _index = new();
        private string? _filePath;
        private FileType _fileType;
        private List<string> _columnHeaders = new();
        private FileInfo? _fileInfo;
        private bool _isIndexed = false;
        private int _rowCount = 0;
        private List<Result> _cachedData = new(); // Cache for binary data
        private string _binaryCachePath = ""; // Path to binary cache file
        private int _archiveId = -1; // Archive ID for database integration

        /// <summary>
        /// Represents the type of file being processed.
        /// </summary>
        public enum FileType
        {
            /// <summary>
            /// Excel file format (.xlsx or .xls).
            /// </summary>
            Excel,
            
            /// <summary>
            /// CSV file format (.csv).
            /// </summary>
            Csv
        }

        private DatabaseService? _databaseService;
        private string _currentUser = Environment.UserName;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="FileDataService"/> class.
        /// </summary>
        public FileDataService()
        {
            ExcelPackage.License.SetNonCommercialPersonal("khaled");
            try
            {
                LoggingService.LogInfo("FileDataService initialized");
            }
            catch (Exception ex)
            {
                // Handle any exceptions during initialization
                LoggingService.LogError("Error initializing FileDataService", ex);
            }
        }

        /// <summary>
        /// Asynchronously loads a file (Excel or CSV) and creates an index for fast searching.
        /// </summary>
        /// <param name="filePath">Path to the file to load.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task LoadFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            await RetryHelper.ExecuteWithRetryAsync(
                async () =>
                {
                    try
                    {
                        LoggingService.LogInfo($"Starting to load file: {filePath}");
                        _filePath = filePath;
                        _fileInfo = new FileInfo(filePath);
                        
                        // Check for cancellation
                        cancellationToken.ThrowIfCancellationRequested();
                        
                        // Determine file type based on extension
                        var extension = _fileInfo.Extension.ToLower();
                        _fileType = extension == ".xlsx" || extension == ".xls" ? FileType.Excel : FileType.Csv;
                        LoggingService.LogInfo($"File type determined as: {_fileType}");

                        // Generate binary cache path
                        _binaryCachePath = Path.Combine(Path.GetDirectoryName(filePath) ?? "", 
                                                       Path.GetFileNameWithoutExtension(filePath) + ".bin");
                        LoggingService.LogInfo($"Binary cache path: {_binaryCachePath}");

                        // Check if binary cache exists and is newer than the original file
                        if (File.Exists(_binaryCachePath) && IsCacheValid())
                        {
                            LoggingService.LogInfo("Loading from binary cache");
                            // Load from binary cache
                            await LoadFromBinaryCacheAsync(cancellationToken);
                        }
                        else
                        {
                            LoggingService.LogInfo("Loading from original file and creating binary cache");
                            // Load from original file and create binary cache
                            await LoadFromOriginalFileAndCacheAsync(cancellationToken);
                        }

                        // Get row count efficiently
                        _rowCount = _cachedData.Count;
                        LoggingService.LogInfo($"Row count: {_rowCount}");

                        // Create index in background
                        LoggingService.LogInfo("Creating index");
                        await Task.Run(() => CreateIndex(), cancellationToken);
                        
                        _isIndexed = true;
                        LoggingService.LogInfo("File loaded successfully");
                    }
                    catch (OperationCanceledException)
                    {
                        LoggingService.LogInfo("File loading operation was cancelled");
                        throw;
                    }
                    catch (Exception ex)
                    {
                        // Log the exception for debugging
                        LoggingService.LogError("Error in LoadFileAsync", ex);
                        
                        // Re-throw the exception to be handled by the caller
                        throw new InvalidOperationException(string.Format(ErrorMessages.ErrorLoadingFile, ex.Message), ex);
                    }
                },
                maxRetries: 3,
                delayMilliseconds: 1000,
                isTransientException: IsTransientException);
        }

        /// <summary>
        /// Check if the binary cache is valid (newer than the original file).
        /// </summary>
        /// <returns>True if cache is valid, false otherwise.</returns>
        private bool IsCacheValid()
        {
            try
            {
                if (_fileInfo == null) return false;
                
                var cacheInfo = new FileInfo(_binaryCachePath);
                bool isValid = cacheInfo.LastWriteTime >= _fileInfo.LastWriteTime;
                LoggingService.LogInfo($"Cache validity check: {isValid}");
                return isValid;
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error in IsCacheValid", ex);
                return false;
            }
        }

        /// <summary>
        /// Load data from binary cache file.
        /// </summary>
        private async Task LoadFromBinaryCacheAsync(CancellationToken cancellationToken = default)
        {
            await Task.Run(() =>
            {
                try
                {
                    LoggingService.LogInfo("Starting to load from binary cache");
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    using (var fileStream = File.OpenRead(_binaryCachePath))
                    {
                        _cachedData = MessagePackSerializer.Deserialize<List<Result>>(fileStream);
                    }
                    
                    // Check for cancellation
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    // Only generate column headers if they haven't been loaded yet
                    // This preserves the original column headers that were loaded from the file
                    if (_columnHeaders.Count == 0 && _cachedData.Any())
                    {
                        // Load column headers from the original file since we're loading from cache
                        LoadColumnHeaders();
                    }
                    LoggingService.LogInfo($"Loaded {_cachedData.Count} rows from binary cache");
                }
                catch (OperationCanceledException)
                {
                    LoggingService.LogInfo("Binary cache loading operation was cancelled");
                    throw;
                }
                catch (Exception ex)
                {
                    // If there's an error loading from cache, fall back to original file
                    LoggingService.LogError("Error loading from binary cache, falling back to original file", ex);
                    throw new InvalidOperationException(string.Format(ErrorMessages.ErrorLoadingFromCache, ex.Message), ex);
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Load data from original file and create binary cache.
        /// </summary>
        private async Task LoadFromOriginalFileAndCacheAsync(CancellationToken cancellationToken = default)
        {
            await RetryHelper.ExecuteWithRetryAsync(
                async () =>
                {
                    try
                    {
                        LoggingService.LogInfo("Starting to load from original file");
                        // Load column headers
                        await LoadColumnHeadersAsync(cancellationToken);

                        // Check for cancellation
                        cancellationToken.ThrowIfCancellationRequested();

                        // Load data from original file
                        await LoadDataFromOriginalFileAsync(cancellationToken);

                        // Check for cancellation
                        cancellationToken.ThrowIfCancellationRequested();

                        // Save to binary cache
                        await SaveToBinaryCacheAsync(cancellationToken);
                        LoggingService.LogInfo("Finished loading from original file and saved to binary cache");
                    }
                    catch (OperationCanceledException)
                    {
                        LoggingService.LogInfo("Original file loading operation was cancelled");
                        throw;
                    }
                    catch (Exception ex)
                    {
                        LoggingService.LogError("Error in LoadFromOriginalFileAndCacheAsync", ex);
                        throw new InvalidOperationException(string.Format(ErrorMessages.FileProcessingFailed, 3, ex.Message), ex);
                    }
                },
                maxRetries: 3,
                delayMilliseconds: 1000,
                isTransientException: IsTransientException);
        }

        /// <summary>
        /// Save cached data to binary file.
        /// </summary>
        private async Task SaveToBinaryCacheAsync(CancellationToken cancellationToken = default)
        {
            await RetryHelper.ExecuteWithRetryAsync(
                async () =>
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            LoggingService.LogInfo($"Saving to binary cache: {_binaryCachePath}");
                            using (var fileStream = File.Create(_binaryCachePath))
                            {
                                MessagePackSerializer.Serialize(fileStream, _cachedData);
                            }
                            LoggingService.LogInfo("Saved to binary cache successfully");
                        }
                        catch (Exception ex)
                        {
                            LoggingService.LogError("Error saving to binary cache", ex);
                            throw new InvalidOperationException(string.Format(ErrorMessages.ErrorSavingToCache, ex.Message), ex);
                        }
                    }, cancellationToken);
                },
                maxRetries: 3,
                delayMilliseconds: 500,
                isTransientException: IsTransientException);
        }

        /// <summary>
        /// Load data from original file (Excel or CSV).
        /// </summary>
        private async Task LoadDataFromOriginalFileAsync(CancellationToken cancellationToken = default)
        {
            await Task.Run(() =>
            {
                try
                {
                    LoggingService.LogInfo("Starting to load data from original file");
                    if (_fileType == FileType.Excel)
                    {
                        LoggingService.LogInfo("Loading Excel file");
                        LoadDataFromExcel();
                    }
                    else
                    {
                        LoggingService.LogInfo("Loading CSV file");
                        LoadDataFromCsv();
                    }
                    LoggingService.LogInfo($"Loaded {_cachedData.Count} rows from original file");
                }
                catch (Exception ex)
                {
                    // Log the exception for debugging
                    LoggingService.LogError("Error in LoadDataFromOriginalFileAsync", ex);
                    
                    // Re-throw the exception to be handled by the caller
                    if (_fileType == FileType.Excel)
                    {
                        throw new InvalidOperationException(string.Format(ErrorMessages.ErrorLoadingExcelData, ex.Message), ex);
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format(ErrorMessages.ErrorLoadingCsvData, ex.Message), ex);
                    }
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Load data from Excel file.
        /// </summary>
        private void LoadDataFromExcel()
        {
            try
            {
                LoggingService.LogInfo("Starting to load data from Excel file");
                if (_fileInfo != null)
                {
                    using (var package = new ExcelPackage(_fileInfo))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet != null)
                        {
                            var rowCount = worksheet.Dimension?.Rows ?? 0;
                            var colCount = worksheet.Dimension?.Columns ?? 0;
                            LoggingService.LogInfo($"Excel file has {rowCount} rows and {colCount} columns");
                            
                            // Start from row 2 to skip header
                            for (int row = 2; row <= rowCount; row++)
                            {
                                var dynamicColumnValues = new List<string>();
                                for (int col = 1; col <= colCount; col++)
                                {
                                    var cellValue = worksheet.Cells[row, col].Text;
                                    dynamicColumnValues.Add(cellValue ?? "");
                                }

                                var result = new Result
                                {
                                    Id = row,
                                    FileName = _fileInfo.Name,
                                    Content = string.Join(", ", dynamicColumnValues.Take(5)),
                                    SearchDate = DateTime.Now,
                                    MatchCount = 0, // Will be set during search
                                    DynamicColumnValues = dynamicColumnValues,
                                    IsAddedToHistory = false
                                };
                                
                                _cachedData.Add(result);
                            }
                            LoggingService.LogInfo($"Loaded {rowCount - 1} data rows from Excel file");
                        }
                        else
                        {
                            LoggingService.LogWarning("No worksheets found in Excel file");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                LoggingService.LogError("Error in LoadDataFromExcel", ex);
                
                // Re-throw the exception to be handled by the caller
                throw new InvalidOperationException(string.Format(ErrorMessages.ErrorLoadingExcelData, ex.Message), ex);
            }
        }

        /// <summary>
        /// Load data from CSV file.
        /// </summary>
        private void LoadDataFromCsv()
        {
            try
            {
                LoggingService.LogInfo("Starting to load data from CSV file");
                if (_filePath != null)
                {
                    using (var reader = new StreamReader(_filePath))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        csv.Read(); // Skip header
                        int rowNumber = 2; // Start from row 2
                        
                        while (csv.Read())
                        {
                            var dynamicColumnValues = new List<string>();
                            for (int i = 0; i < _columnHeaders.Count; i++)
                            {
                                try
                                {
                                    var fieldValue = csv.GetField(i) ?? "";
                                    dynamicColumnValues.Add(fieldValue);
                                }
                                catch
                                {
                                    dynamicColumnValues.Add("");
                                }
                            }

                            var result = new Result
                            {
                                Id = rowNumber,
                                FileName = Path.GetFileName(_filePath),
                                Content = string.Join(", ", dynamicColumnValues.Take(5)),
                                SearchDate = DateTime.Now,
                                MatchCount = 0, // Will be set during search
                                DynamicColumnValues = dynamicColumnValues,
                                IsAddedToHistory = false
                            };
                            
                            _cachedData.Add(result);
                            rowNumber++;
                        }
                        LoggingService.LogInfo($"Loaded {rowNumber - 2} data rows from CSV file");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                LoggingService.LogError("Error in LoadDataFromCsv", ex);
                
                // Re-throw the exception to be handled by the caller
                throw new InvalidOperationException(string.Format(ErrorMessages.ErrorLoadingCsvData, ex.Message), ex);
            }
        }

        /// <summary>
        /// Synchronously loads column headers from the file.
        /// </summary>
        private void LoadColumnHeaders()
        {
            try
            {
                LoggingService.LogInfo("Loading column headers");
                if (_fileType == FileType.Excel)
                {
                    LoadExcelHeaders();
                }
                else
                {
                    LoadCsvHeaders();
                }
                LoggingService.LogInfo($"Loaded {_columnHeaders.Count} column headers");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error in LoadColumnHeaders", ex);
                throw new InvalidOperationException(string.Format(ErrorMessages.ErrorLoadingColumnHeaders, ex.Message), ex);
            }
        }

        /// <summary>
        /// Asynchronously loads column headers from the file.
        /// </summary>
        /// <returns>Task representing the asynchronous operation.</returns>
        private async Task LoadColumnHeadersAsync(CancellationToken cancellationToken = default)
        {
            await RetryHelper.ExecuteWithRetryAsync(
                async () =>
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            LoggingService.LogInfo("Loading column headers");
                            if (_fileType == FileType.Excel)
                            {
                                LoadExcelHeaders();
                            }
                            else
                            {
                                LoadCsvHeaders();
                            }
                            LoggingService.LogInfo($"Loaded {_columnHeaders.Count} column headers");
                        }
                        catch (Exception ex)
                        {
                            LoggingService.LogError("Error in LoadColumnHeadersAsync", ex);
                            throw new InvalidOperationException(string.Format(ErrorMessages.ErrorLoadingColumnHeaders, ex.Message), ex);
                        }
                    }, cancellationToken);
                },
                maxRetries: 3,
                delayMilliseconds: 500,
                isTransientException: IsTransientException);
        }

        /// <summary>
        /// Loads column headers from an Excel file.
        /// </summary>
        private void LoadExcelHeaders()
        {
            try
            {
                LoggingService.LogInfo("Loading Excel headers");
                _columnHeaders.Clear();
                if (_fileInfo != null)
                {
                    using (var package = new ExcelPackage(_fileInfo))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet != null)
                        {
                            // Read headers from the first row
                            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                            {
                                var header = worksheet.Cells[1, col].Text;
                                LoggingService.LogInfo($"Excel header {col}: '{header}'");
                                _columnHeaders.Add(string.IsNullOrEmpty(header) ? $"Column {col}" : header);
                            }
                        }
                    }
                }
                LoggingService.LogInfo($"Total Excel headers loaded: {_columnHeaders.Count}");
                for (int i = 0; i < _columnHeaders.Count; i++)
                {
                    LoggingService.LogInfo($"Header {i}: '{_columnHeaders[i]}'");
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error in LoadExcelHeaders", ex);
                throw new InvalidOperationException(string.Format(ErrorMessages.ErrorLoadingColumnHeaders, ex.Message), ex);
            }
        }

        /// <summary>
        /// Loads column headers from a CSV file.
        /// </summary>
        private void LoadCsvHeaders()
        {
            try
            {
                LoggingService.LogInfo("Loading CSV headers");
                _columnHeaders.Clear();
                if (_filePath != null)
                {
                    using (var reader = new StreamReader(_filePath))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        csv.Read();
                        csv.ReadHeader();
                        _columnHeaders.AddRange(csv.HeaderRecord ?? new string[0]);
                    }
                }
                LoggingService.LogInfo($"Total CSV headers loaded: {_columnHeaders.Count}");
                for (int i = 0; i < _columnHeaders.Count; i++)
                {
                    LoggingService.LogInfo($"CSV Header {i}: '{_columnHeaders[i]}'");
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error in LoadCsvHeaders", ex);
                throw new InvalidOperationException(string.Format(ErrorMessages.ErrorLoadingColumnHeaders, ex.Message), ex);
            }
        }

        /// <summary>
        /// Creates an index for fast searching.
        /// </summary>
        private void CreateIndex()
        {
            try
            {
                LoggingService.LogInfo("Creating index");
                _index.Clear();

                // Create index from cached data
                for (int i = 0; i < _cachedData.Count; i++)
                {
                    var result = _cachedData[i];
                    
                    // Index all string values in the result
                    var allText = new List<string> { result.FileName ?? "" };
                    allText.AddRange(result.DynamicColumnValues);
                    
                    foreach (var text in allText)
                    {
                        if (!string.IsNullOrEmpty(text))
                        {
                            var lowerText = text.ToLower();
                            if (!_index.ContainsKey(lowerText))
                            {
                                _index[lowerText] = new List<int>();
                            }
                            _index[lowerText].Add(i);
                        }
                    }
                }
                LoggingService.LogInfo($"Index created with {_index.Count} entries");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error in CreateIndex", ex);
                throw new InvalidOperationException(string.Format(ErrorMessages.ErrorCreatingIndex, ex.Message), ex);
            }
        }

        /// <summary>
        /// Gets column headers.
        /// </summary>
        /// <returns>List of column headers.</returns>
        public List<string> GetColumnHeaders()
        {
            return new List<string>(_columnHeaders);
        }

        /// <summary>
        /// Gets the total number of rows in the file (excluding header row).
        /// </summary>
        /// <returns>Number of rows in the file.</returns>
        public int GetRowCount()
        {
            return _cachedData.Count;
        }

        /// <summary>
        /// Searches for a term in all cells of the loaded file.
        /// </summary>
        /// <param name="searchTerm">Term to search for.</param>
        /// <returns>List of row numbers where the term is found.</returns>
        public List<int> Search(string searchTerm)
        {
            if (!_isIndexed || string.IsNullOrEmpty(searchTerm))
                return new List<int>();

            var matchingRows = new HashSet<int>();
            var lowerSearchTerm = searchTerm.ToLower();

            // Use a more efficient search algorithm
            // First, try exact match in the index
            if (_index.ContainsKey(lowerSearchTerm))
            {
                foreach (var rowIndex in _index[lowerSearchTerm])
                {
                    matchingRows.Add(_cachedData[rowIndex].Id);
                }
            }

            // Then, try partial matches
            foreach (var kvp in _index)
            {
                if (kvp.Key.Contains(lowerSearchTerm))
                {
                    foreach (var rowIndex in kvp.Value)
                    {
                        matchingRows.Add(_cachedData[rowIndex].Id);
                    }
                }
            }

            // If no matches found and search term is long enough, try fuzzy matching
            if (matchingRows.Count == 0 && searchTerm.Length > 2)
            {
                foreach (var kvp in _index)
                {
                    // Simple fuzzy matching - check if the Levenshtein distance is small
                    if (CalculateLevenshteinDistance(kvp.Key, lowerSearchTerm) <= 2)
                    {
                        foreach (var rowIndex in kvp.Value)
                        {
                            matchingRows.Add(_cachedData[rowIndex].Id);
                        }
                    }
                }
            }

            return matchingRows.ToList();
        }

        /// <summary>
        /// Calculates the Levenshtein distance between two strings.
        /// </summary>
        /// <param name="source">Source string</param>
        /// <param name="target">Target string</param>
        /// <returns>Levenshtein distance</returns>
        private int CalculateLevenshteinDistance(string source, string target)
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
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[source.Length, target.Length];
        }

        /// <summary>
        /// Searches for a term in cached data.
        /// </summary>
        /// <param name="searchTerm">Term to search for.</param>
        /// <param name="matchingRows">Set to populate with matching row numbers.</param>
        private void SearchCachedData(string searchTerm, HashSet<int> matchingRows)
        {
            for (int i = 0; i < _cachedData.Count; i++)
            {
                var result = _cachedData[i];
                
                // Check all fields for the search term
                var allFields = new List<string> { result.FileName ?? "", result.Content ?? "" };
                allFields.AddRange(result.DynamicColumnValues);
                
                foreach (var field in allFields)
                {
                    if (!string.IsNullOrEmpty(field) && 
                        field.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        matchingRows.Add(result.Id);
                        break; // Found in this row, no need to check other fields
                    }
                }
            }
        }

        /// <summary>
        /// Gets file information.
        /// </summary>
        /// <returns>File information.</returns>
        public FileInfo? GetFileInfo()
        {
            return _fileInfo;
        }

        /// <summary>
        /// Checks if a file is loaded.
        /// </summary>
        /// <returns>True if a file is loaded, false otherwise.</returns>
        public bool IsFileLoaded()
        {
            return !string.IsNullOrEmpty(_filePath) && _fileInfo != null && _fileInfo.Exists;
        }

        /// <summary>
        /// Gets data for a specific row using cached data.
        /// </summary>
        /// <param name="rowNumber">Row number to retrieve.</param>
        /// <returns>List of cell values for the specified row.</returns>
        public async Task<List<string>> GetRowDataAsync(int rowNumber)
        {
            return await Task.Run(() =>
            {
                // Find the result with the matching ID
                var result = _cachedData.FirstOrDefault(r => r.Id == rowNumber);
                if (result != null)
                {
                    return result.DynamicColumnValues;
                }
                
                return new List<string>();
            });
        }

        /// <summary>
        /// Gets cached result by ID.
        /// </summary>
        /// <param name="id">ID of the result to retrieve.</param>
        /// <returns>Result object or null if not found.</returns>
        public Result? GetResultById(int id)
        {
            return _cachedData.FirstOrDefault(r => r.Id == id);
        }

        /// <summary>
        /// Updates a result in the cache.
        /// </summary>
        /// <param name="result">Result to update.</param>
        public void UpdateResult(Result result)
        {
            var index = _cachedData.FindIndex(r => r.Id == result.Id);
            if (index >= 0)
            {
                _cachedData[index] = result;
                
                // Save updated cache to binary file
                Task.Run(() => SaveToBinaryCacheAsync());
            }
        }
        
        /// <summary>
        /// Sets the database service for database integration.
        /// </summary>
        /// <param name="databaseService">The database service to use.</param>
        public void SetDatabaseService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }
        
        /// <summary>
        /// Imports a file using the database service.
        /// </summary>
        /// <param name="filePath">Path to the file to import.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task<int> ImportFileWithDatabaseAsync(string filePath)
        {
            return await RetryHelper.ExecuteWithRetryAsync(
                async () =>
                {
                    if (_databaseService == null)
                    {
                        throw new InvalidOperationException("Database service not initialized");
                    }
                    
                    // Import file and get archive ID
                    _archiveId = await _databaseService.ImportFileAsync(filePath, _currentUser);
                    
                    // Index file content
                    await _databaseService.IndexFileContentAsync(filePath, _archiveId);
                    
                    return _archiveId;
                },
                maxRetries: 3,
                delayMilliseconds: 1000,
                isTransientException: IsTransientException);
        }

        /// <summary>
        /// Gets the archive ID of the last imported file.
        /// </summary>
        /// <returns>The archive ID.</returns>
        public int GetArchiveId()
        {
            return _archiveId;
        }
        
        /// <summary>
        /// Clears the cached data and index to free up memory.
        /// </summary>
        public void ClearCache()
        {
            _cachedData.Clear();
            _index.Clear();
            _columnHeaders.Clear();
            _filePath = null;
            _fileInfo = null;
            _isIndexed = false;
            _rowCount = 0;
            
            // Delete binary cache file if it exists
            if (!string.IsNullOrEmpty(_binaryCachePath) && File.Exists(_binaryCachePath))
            {
                try
                {
                    File.Delete(_binaryCachePath);
                }
                catch (Exception ex)
                {
                    LoggingService.LogError("Error deleting binary cache file", ex);
                }
            }
            
            LoggingService.LogInfo("Cache cleared successfully");
        }
        
        /// <summary>
        /// Gets the current memory usage of the cache.
        /// </summary>
        /// <returns>Approximate memory usage in bytes</returns>
        public long GetCacheMemoryUsage()
        {
            // Rough estimation of memory usage
            long usage = 0;
            
            // Estimate for _cachedData
            usage += _cachedData.Count * 1000; // Rough estimate per Result object
            
            // Estimate for _index
            foreach (var kvp in _index)
            {
                usage += kvp.Key.Length * 2; // String length * 2 bytes per char
                usage += kvp.Value.Count * 4; // Integers are 4 bytes each
            }
            
            // Estimate for _columnHeaders
            foreach (var header in _columnHeaders)
            {
                usage += header.Length * 2;
            }
            
            return usage;
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
    }
}