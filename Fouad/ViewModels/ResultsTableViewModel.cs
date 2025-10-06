using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Fouad.Models;
using Fouad.Commands;
using Fouad.Services;
using Fouad.Views;
using System.Windows.Controls;
using System.Threading;

namespace Fouad.ViewModels
{
    /// <summary>
    /// ViewModel for the results table view.
    /// Manages search results, filtering, and interaction with the history table.
    /// </summary>
    public class ResultsTableViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Result> _results = new ObservableCollection<Result>();
        private ObservableCollection<Result> _filteredResults = new ObservableCollection<Result>();
        private ObservableCollection<Result> _pagedResults = new ObservableCollection<Result>();
        private string _searchText = "";
        private int _searchResultCount = 0;
        private bool _isHighlightingEnabled = false; // New property to control highlighting
        private FileDataService? _fileDataService; // This will be set from TopBarViewModel
        private ColumnSelectorViewModel? _columnSelector;
        private HistoryTableViewModel? _historyTable; // Reference to HistoryTableViewModel
        private AudioService _audioService; // Audio service for playing sounds
        private ISearchService _searchService; // Enhanced search service
        private bool _isSearching = false; // New property to track search status
        private string _searchStatus = ""; // New property for search status message
        
        // Pagination properties
        private int _currentPage = 1;
        private int _pageSize = 50;
        
        // Configuration service
        private ConfigurationService? _configurationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultsTableViewModel"/> class.
        /// </summary>
        /// <param name="columnSelector">The column selector view model.</param>
        public ResultsTableViewModel(ColumnSelectorViewModel columnSelector)
        {
            _columnSelector = columnSelector;
            _audioService = new AudioService();
            _searchService = App.ServiceContainer.Resolve<ISearchService>(); // Use enhanced search service
            
            // Try to get configuration service
            try
            {
                _configurationService = App.ServiceContainer.Resolve<ConfigurationService>();
                _pageSize = _configurationService.DefaultPageSize;
            }
            catch
            {
                // Fallback to default page size
                _pageSize = 50;
            }
            
            AdvancedSearchCommand = new RelayCommand(PerformAdvancedSearch);
            AddToHistoryCommand = new RelayCommand(AddResultToHistory);
            SearchFilterCommand = new RelayCommand(OpenSearchFilter);
            // Pagination commands
            NextPageCommand = new RelayCommand(GoToNextPage, CanGoToNextPage);
            PreviousPageCommand = new RelayCommand(GoToPreviousPage, CanGoToPreviousPage);
            FirstPageCommand = new RelayCommand(GoToFirstPage, CanGoToFirstPage);
            LastPageCommand = new RelayCommand(GoToLastPage, CanGoToLastPage);
            // Subscribe to column selection changes
            _columnSelector.ColumnSelectionChanged += OnColumnSelectionChanged;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultsTableViewModel"/> class.
        /// </summary>
        /// <param name="fileDataService">The file data service.</param>
        /// <param name="columnSelector">The column selector view model.</param>
        public ResultsTableViewModel(FileDataService fileDataService, ColumnSelectorViewModel columnSelector) : this(columnSelector)
        {
            _fileDataService = fileDataService;
        }

        /// <summary>
        /// Sets the HistoryTableViewModel reference.
        /// </summary>
        /// <param name="historyTable">The history table view model.</param>
        public void SetHistoryTableViewModel(HistoryTableViewModel historyTable)
        {
            _historyTable = historyTable;
        }

        /// <summary>
        /// Gets the audio service for playing sounds.
        /// </summary>
        public AudioService AudioService 
        { 
            get { return _audioService; } 
        }

        /// <summary>
        /// Gets or sets the collection of results.
        /// </summary>
        public ObservableCollection<Result> Results
        {
            get { return _pagedResults; }
            set
            {
                _pagedResults = value ?? new ObservableCollection<Result>();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the search text used for filtering results.
        /// </summary>
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value ?? "";
                OnPropertyChanged();
                FilterResults();
            }
        }
        
        /// <summary>
        /// Gets or sets whether highlighting is enabled for search results.
        /// </summary>
        public bool IsHighlightingEnabled
        {
            get { return _isHighlightingEnabled; }
            set
            {
                _isHighlightingEnabled = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets whether a search operation is currently in progress.
        /// </summary>
        public bool IsSearching
        {
            get { return _isSearching; }
            set
            {
                _isSearching = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets the search status message.
        /// </summary>
        public string SearchStatus
        {
            get { return _searchStatus; }
            set
            {
                _searchStatus = value;
                OnPropertyChanged();
            }
        }
        
        // Pagination properties
        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int CurrentPage
        {
            get { return _currentPage; }
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    OnPropertyChanged();
                    UpdatePagedResults();
                    OnPropertyChanged(nameof(CanGoToPreviousPage));
                    OnPropertyChanged(nameof(CanGoToNextPage));
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize
        {
            get { return _pageSize; }
            set
            {
                if (_pageSize != value)
                {
                    _pageSize = value;
                    OnPropertyChanged();
                    UpdatePagedResults();
                    OnPropertyChanged(nameof(TotalPages));
                    
                    // Save to configuration
                    if (_configurationService != null)
                    {
                        _configurationService.DefaultPageSize = value;
                    }
                }
            }
        }
        
        /// <summary>
        /// Gets the total number of pages.
        /// </summary>
        public int TotalPages
        {
            get { return (int)Math.Ceiling((double)_filteredResults.Count / _pageSize); }
        }
        
        /// <summary>
        /// Gets or sets the count of search results.
        /// </summary>
        public int SearchResultCount
        {
            get { return _searchResultCount; }
            set
            {
                _searchResultCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ResultsTitleWithCount));
                OnPropertyChanged(nameof(TotalPages));
            }
        }

        /// <summary>
        /// Gets the results title with count.
        /// </summary>
        public string ResultsTitleWithCount
        {
            get
            {
                var baseTitle = Application.Current?.FindResource("Results")?.ToString() ?? "Results";
                return SearchResultCount > 0 ? $"{baseTitle} ({SearchResultCount})" : baseTitle;
            }
        }

        // Commands
        /// <summary>
        /// Gets the advanced search command.
        /// </summary>
        public RelayCommand AdvancedSearchCommand { get; private set; }
        
        /// <summary>
        /// Gets the add to history command.
        /// </summary>
        public RelayCommand AddToHistoryCommand { get; private set; } // New command for adding to history
        
        /// <summary>
        /// Gets the search filter command for opening the advanced search filter window.
        /// </summary>
        public RelayCommand SearchFilterCommand { get; private set; }
        
        // Pagination commands
        /// <summary>
        /// Gets the next page command.
        /// </summary>
        public RelayCommand NextPageCommand { get; private set; }
        
        /// <summary>
        /// Gets the previous page command.
        /// </summary>
        public RelayCommand PreviousPageCommand { get; private set; }
        
        /// <summary>
        /// Gets the first page command.
        /// </summary>
        public RelayCommand FirstPageCommand { get; private set; }
        
        /// <summary>
        /// Gets the last page command.
        /// </summary>
        public RelayCommand LastPageCommand { get; private set; }

        // Command implementations
        /// <summary>
        /// Performs an advanced search using the FileDataService.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private async void PerformAdvancedSearch(object? parameter)
        {
            // Implementation for advanced search using the enhanced search service
            if (!string.IsNullOrWhiteSpace(_searchText) && _fileDataService != null && _fileDataService.IsFileLoaded())
            {
                try
                {
                    IsSearching = true;
                    SearchStatus = "Searching...";

                    // Create search criteria with basic settings
                    var criteria = new SearchCriteria
                    {
                        SearchTerm = _searchText,
                        CaseSensitive = _configurationService?.EnableCaseSensitiveSearch ?? false,
                        MaxResults = _configurationService?.MaxSearchResults ?? 1000,
                        FuzzySearch = _configurationService?.EnableFuzzySearch ?? false,
                        Columns = new List<string>(), // Search all columns
                        ColumnValueFilters = new List<ColumnValueFilterCriteria>(),
                        ValueRangeFilters = new List<ValueRangeFilterCriteria>()
                    };

                    // Get selected columns for dynamic column values
                    var selectedColumns = _columnSelector?.GetSelectedColumns() ?? new List<string>();

                    // Perform the search using the enhanced search service
                    var searchResults = await _searchService.AdvancedSearchAsync(criteria);

                    // Update the search result count
                    SearchResultCount = searchResults.Count;

                    // Clear existing results
                    _results.Clear();
                    _filteredResults.Clear();

                    // Load data for matching results
                    foreach (var result in searchResults.Take(1000)) // Limit to 1000 results for performance
                    {
                        // Create dynamic column values based on selected columns ONLY
                        var dynamicColumnValues = new List<string>();
                        var headers = _fileDataService.GetColumnHeaders();
                        
                        if (headers != null)
                        {
                            foreach (var columnName in selectedColumns)
                            {
                                // Find the index of the column in the rowData
                                var columnIndex = headers.IndexOf(columnName);
                                if (columnIndex >= 0 && result.DynamicColumnValues != null && columnIndex < result.DynamicColumnValues.Count)
                                {
                                    dynamicColumnValues.Add(result.DynamicColumnValues[columnIndex]);
                                }
                                else
                                {
                                    dynamicColumnValues.Add("");
                                }
                            }
                        }

                        var enhancedResult = new Result
                        {
                            Id = result.Id,
                            FileName = result.FileName ?? "Unknown",
                            Content = string.Join(", ", result.DynamicColumnValues?.Take(5) ?? new List<string>()), // Show first 5 columns
                            SearchDate = result.SearchDate,
                            MatchCount = result.MatchCount,
                            DynamicColumnValues = dynamicColumnValues,
                            IsAddedToHistory = result.IsAddedToHistory // Preserve the added status
                        };
                        _results.Add(enhancedResult);
                    }

                    // Update filtered results
                    _filteredResults = new ObservableCollection<Result>(_results);
                    OnPropertyChanged(nameof(Results));
                    OnPropertyChanged(nameof(ResultsTitleWithCount));
                    OnPropertyChanged(nameof(TotalPages));

                    // Reset to first page
                    CurrentPage = 1;
                    
                    // Update paged results
                    UpdatePagedResults();

                    // Notify that columns may have changed
                    ColumnsChanged?.Invoke(this, EventArgs.Empty);

                    // Enable highlighting for search results
                    IsHighlightingEnabled = true;

                    // Play success sound if results found
                    if (searchResults.Count > 0)
                    {
                        _audioService.PlaySound("Success");
                        SearchStatus = $"Found {searchResults.Count} results";
                    }
                    else
                    {
                        _audioService.PlaySound("NoResults");
                        SearchStatus = "No results found";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error performing search: {ex.Message}", "Search Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    _audioService.PlaySound("Error");
                    SearchStatus = "Search failed";
                }
                finally
                {
                    IsSearching = false;
                }
            }
            else if (string.IsNullOrWhiteSpace(_searchText))
            {
                // Clear results and count when search text is empty
                _results.Clear();
                _filteredResults.Clear();
                _pagedResults.Clear();
                SearchResultCount = 0;
                OnPropertyChanged(nameof(Results));
                OnPropertyChanged(nameof(ResultsTitleWithCount));
                OnPropertyChanged(nameof(TotalPages));
                
                // Disable highlighting when search text is cleared
                IsHighlightingEnabled = false;
                SearchStatus = "";
            }
            else
            {
                MessageBox.Show(Properties.Resources.NoFileLoaded, Properties.Resources.SearchError, MessageBoxButton.OK, MessageBoxImage.Warning);
                _audioService.PlaySound("Error");
                SearchStatus = "No file loaded";
            }
        }

        /// <summary>
        /// Opens the search filter window for advanced search filtering options.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void OpenSearchFilter(object? parameter)
        {
            try
            {
                // Get services from the service container
                var searchService = App.ServiceContainer.Resolve<ISearchService>();
                var fileDataService = App.ServiceContainer.Resolve<IFileDataService>();
                
                // Create and show the search filter window
                var searchFilterWindow = new SearchFilterWindow();
                var searchFilterViewModel = new SearchFilterViewModel(searchService, fileDataService);
                
                // Set available columns if we have them
                if (fileDataService != null && fileDataService.IsFileLoaded())
                {
                    var columnHeaders = fileDataService.GetColumnHeaders();
                    searchFilterViewModel.SetAvailableColumns(columnHeaders);
                    
                    // Set available files (in a real implementation, you would get this from a file service)
                    // For now, we'll just use the currently loaded file
                    var fileInfo = fileDataService.GetFileInfo();
                    if (fileInfo != null)
                    {
                        searchFilterViewModel.SetAvailableFiles(new List<string> { fileInfo.FullName });
                    }
                }
                
                searchFilterWindow.DataContext = searchFilterViewModel;
                searchFilterWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening search filter: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _audioService.PlaySound("Error");
            }
        }

        /// <summary>
        /// Adds a result to the history table.
        /// </summary>
        /// <param name="parameter">The result to add to history.</param>
        private void AddResultToHistory(object? parameter)
        {
            if (parameter is Result result && _historyTable != null)
            {
                // Check if the result has already been added
                if (result.IsAddedToHistory)
                {
                    // Play warning sound for duplicate addition
                    _audioService.PlaySound("AlreadyAdded");
                    // Notify that this is a duplicate for UI feedback
                    OnDuplicateResultAdded(result);
                    return;
                }

                // Mark the result as added
                result.IsAddedToHistory = true;

                // Update the cached result
                if (_fileDataService != null)
                {
                    _fileDataService.UpdateResult(result);
                }

                // Create a HistoryItem from the Result
                var historyItem = new HistoryItem
                {
                    Id = result.Id,
                    FileName = result.FileName,
                    SearchDate = result.SearchDate,
                    SearchTerm = _searchText,
                    ResultCount = 1,
                    IsSelected = false,
                    IsAddedToHistory = true, // Mark as added to history
                    DynamicColumnValues = new List<string>(result.DynamicColumnValues)
                };

                // Add to history table
                _historyTable.HistoryItems.Add(historyItem);

                // Play success sound
                _audioService.PlaySound("Added");
                
                // Notify that a result was added successfully
                OnResultAddedSuccessfully(result);
            }
        }

        /// <summary>
        /// Handles the Enter key press for search.
        /// </summary>
        public void HandleEnterKeyPress()
        {
            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                PerformAdvancedSearch(null);
            }
        }

        /// <summary>
        /// Handles the Enter key press for adding a result.
        /// </summary>
        /// <param name="result">The result to add.</param>
        public void HandleAddEnterKeyPress(Result result)
        {
            AddResultToHistory(result);
        }

        /// <summary>
        /// Clears the search text and focuses the search box.
        /// </summary>
        public void ClearSearchAndFocus()
        {
            SearchText = "";
        }

        /// <summary>
        /// Filters the results based on the search text.
        /// </summary>
        private void FilterResults()
        {
            if (string.IsNullOrWhiteSpace(_searchText))
            {
                _filteredResults = new ObservableCollection<Result>(_results);
            }
            else
            {
                var filtered = _results.Where(r =>
                    (r.FileName?.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.Content?.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
                _filteredResults = new ObservableCollection<Result>(filtered);
            }
            
            // Reset to first page when filtering
            CurrentPage = 1;
            UpdatePagedResults();
            OnPropertyChanged(nameof(TotalPages));
        }
        
        /// <summary>
        /// Updates the paged results based on current page and page size.
        /// </summary>
        private void UpdatePagedResults()
        {
            var startIndex = (CurrentPage - 1) * PageSize;
            var endIndex = Math.Min(startIndex + PageSize, _filteredResults.Count);
            
            _pagedResults.Clear();
            
            if (startIndex < _filteredResults.Count)
            {
                for (int i = startIndex; i < endIndex; i++)
                {
                    _pagedResults.Add(_filteredResults[i]);
                }
            }
            
            OnPropertyChanged(nameof(Results));
        }

        /// <summary>
        /// Called when column selection changes.
        /// </summary>
        private void OnColumnSelectionChanged(object? sender, EventArgs e)
        {
            // Refresh the search results to reflect the new column selection
            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                PerformAdvancedSearch(null);
            }
            else
            {
                // Even if there's no search text, we still need to notify that columns changed
                ColumnsChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        
        // Pagination command methods
        private void GoToNextPage(object? parameter)
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
            }
        }
        
        private bool CanGoToNextPage(object? parameter)
        {
            return CurrentPage < TotalPages;
        }
        
        private void GoToPreviousPage(object? parameter)
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
            }
        }
        
        private bool CanGoToPreviousPage(object? parameter)
        {
            return CurrentPage > 1;
        }
        
        private void GoToFirstPage(object? parameter)
        {
            CurrentPage = 1;
        }
        
        private bool CanGoToFirstPage(object? parameter)
        {
            return CurrentPage > 1;
        }
        
        private void GoToLastPage(object? parameter)
        {
            CurrentPage = TotalPages;
        }
        
        private bool CanGoToLastPage(object? parameter)
        {
            return CurrentPage < TotalPages;
        }

        /// <summary>
        /// Event raised when a duplicate result is added.
        /// </summary>
        public event EventHandler<Result>? DuplicateResultAdded;

        /// <summary>
        /// Event raised when a result is added successfully.
        /// </summary>
        public event EventHandler<Result>? ResultAddedSuccessfully;

        /// <summary>
        /// Called when a duplicate result is added.
        /// </summary>
        private void OnDuplicateResultAdded(Result result)
        {
            DuplicateResultAdded?.Invoke(this, result);
        }

        /// <summary>
        /// Called when a result is added successfully.
        /// </summary>
        private void OnResultAddedSuccessfully(Result result)
        {
            ResultAddedSuccessfully?.Invoke(this, result);
        }

        /// <summary>
        /// Event raised when columns change.
        /// </summary>
        public event EventHandler? ColumnsChanged;

        /// <summary>
        /// Event raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}