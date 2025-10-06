using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Fouad.Commands;
using Fouad.Models;
using Fouad.Services;

namespace Fouad.ViewModels
{
    /// <summary>
    /// ViewModel for the search filter window.
    /// Manages advanced search filter options.
    /// </summary>
    public class SearchFilterViewModel : INotifyPropertyChanged
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
        private string _searchTerm = "";
        private bool _isCaseSensitive = false;
        private bool _exactMatch = false;
        private bool _fuzzySearch = false;
        private bool _regexSearch = false;
        private bool _isAllWordsSearch = true;
        private bool _isAnyWordSearch = false;
        private bool _isPhraseSearch = false;
        private bool _enableColumnFilter = false;
        private bool _enableDateRange = false;
        private bool _enableTimeRange = false;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private string _startTime = "00:00";
        private string _endTime = "23:59";
        private bool _enableResultLimit = false;
        private int _resultLimit = 100;
        private bool _enableFileFilter = false;
        
        private ObservableCollection<FilterColumn> _availableColumns = new ObservableCollection<FilterColumn>();
        private ObservableCollection<FilterColumn> _selectedColumns = new ObservableCollection<FilterColumn>();
        private ObservableCollection<ColumnValueFilter> _columnValueFilters = new ObservableCollection<ColumnValueFilter>();
        private ObservableCollection<ValueRangeFilter> _valueRangeFilters = new ObservableCollection<ValueRangeFilter>();
        private ObservableCollection<FilterFile> _availableFiles = new ObservableCollection<FilterFile>();
        private ObservableCollection<string> _filterOperators = new ObservableCollection<string> { "=", "!=", ">", "<", ">=", "<=", "Contains", "Starts With", "Ends With" };
        private ObservableCollection<string> _numericColumns = new ObservableCollection<string>();
        private RelayCommand _applyFilterCommand;
        private RelayCommand _saveFilterCommand;
        private RelayCommand _loadFilterCommand;
        private RelayCommand _resetFilterCommand;
        private RelayCommand _cancelCommand;
        private RelayCommand _addColumnCommand;
        private RelayCommand _removeColumnCommand;
        private RelayCommand _addColumnValueFilterCommand;
        private RelayCommand _removeColumnValueFilterCommand;
        private RelayCommand _addValueRangeFilterCommand;
        private RelayCommand _removeValueRangeFilterCommand;
        private Window? _window;
        private ISearchService? _searchService;
        private IFileDataService? _fileDataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchFilterViewModel"/> class.
        /// </summary>
        public SearchFilterViewModel()
        {
            _applyFilterCommand = new RelayCommand(ApplyFilter);
            _saveFilterCommand = new RelayCommand(SaveFilter);
            _loadFilterCommand = new RelayCommand(LoadFilter);
            _resetFilterCommand = new RelayCommand(ResetFilter);
            _cancelCommand = new RelayCommand(Cancel);
            _addColumnCommand = new RelayCommand(AddColumn);
            _removeColumnCommand = new RelayCommand(RemoveColumn);
            _addColumnValueFilterCommand = new RelayCommand(AddColumnValueFilter);
            _removeColumnValueFilterCommand = new RelayCommand(RemoveColumnValueFilter);
            _addValueRangeFilterCommand = new RelayCommand(AddValueRangeFilter);
            _removeValueRangeFilterCommand = new RelayCommand(RemoveValueRangeFilter);
            
            // Initialize default dates
            _startDate = DateTime.Now.AddDays(-30);
            _endDate = DateTime.Now;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchFilterViewModel"/> class with services.
        /// </summary>
        /// <param name="searchService">The search service.</param>
        /// <param name="fileDataService">The file data service.</param>
        public SearchFilterViewModel(ISearchService searchService, IFileDataService fileDataService) : this()
        {
            _searchService = searchService;
            _fileDataService = fileDataService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchFilterViewModel"/> class with a window reference.
        /// </summary>
        /// <param name="window">The window to control.</param>
        public SearchFilterViewModel(Window window) : this()
        {
            _window = window;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchFilterViewModel"/> class with a window reference and services.
        /// </summary>
        /// <param name="window">The window to control.</param>
        /// <param name="searchService">The search service.</param>
        /// <param name="fileDataService">The file data service.</param>
        public SearchFilterViewModel(Window window, ISearchService searchService, IFileDataService fileDataService) : this()
        {
            _window = window;
            _searchService = searchService;
            _fileDataService = fileDataService;
        }

        /// <summary>
        /// Gets or sets the search term.
        /// </summary>
        public string SearchTerm
        {
            get { return _searchTerm; }
            set
            {
                _searchTerm = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the search is case sensitive.
        /// </summary>
        public bool IsCaseSensitive
        {
            get { return _isCaseSensitive; }
            set
            {
                _isCaseSensitive = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to perform exact match search.
        /// </summary>
        public bool ExactMatch
        {
            get { return _exactMatch; }
            set
            {
                _exactMatch = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to perform fuzzy search.
        /// </summary>
        public bool FuzzySearch
        {
            get { return _fuzzySearch; }
            set
            {
                _fuzzySearch = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to perform regex search.
        /// </summary>
        public bool RegexSearch
        {
            get { return _regexSearch; }
            set
            {
                _regexSearch = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to search for all words.
        /// </summary>
        public bool IsAllWordsSearch
        {
            get { return _isAllWordsSearch; }
            set
            {
                _isAllWordsSearch = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to search for any word.
        /// </summary>
        public bool IsAnyWordSearch
        {
            get { return _isAnyWordSearch; }
            set
            {
                _isAnyWordSearch = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to search for exact phrase.
        /// </summary>
        public bool IsPhraseSearch
        {
            get { return _isPhraseSearch; }
            set
            {
                _isPhraseSearch = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether column filtering is enabled.
        /// </summary>
        public bool EnableColumnFilter
        {
            get { return _enableColumnFilter; }
            set
            {
                _enableColumnFilter = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether date range filtering is enabled.
        /// </summary>
        public bool EnableDateRange
        {
            get { return _enableDateRange; }
            set
            {
                _enableDateRange = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether time range filtering is enabled.
        /// </summary>
        public bool EnableTimeRange
        {
            get { return _enableTimeRange; }
            set
            {
                _enableTimeRange = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the start date for date range filtering.
        /// </summary>
        public DateTime? StartDate
        {
            get { return _startDate; }
            set
            {
                _startDate = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the end date for date range filtering.
        /// </summary>
        public DateTime? EndDate
        {
            get { return _endDate; }
            set
            {
                _endDate = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the start time for time range filtering.
        /// </summary>
        public string StartTime
        {
            get { return _startTime; }
            set
            {
                _startTime = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the end time for time range filtering.
        /// </summary>
        public string EndTime
        {
            get { return _endTime; }
            set
            {
                _endTime = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether result limiting is enabled.
        /// </summary>
        public bool EnableResultLimit
        {
            get { return _enableResultLimit; }
            set
            {
                _enableResultLimit = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the result limit.
        /// </summary>
        public int ResultLimit
        {
            get { return _resultLimit; }
            set
            {
                _resultLimit = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether file filtering is enabled.
        /// </summary>
        public bool EnableFileFilter
        {
            get { return _enableFileFilter; }
            set
            {
                _enableFileFilter = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the available columns for filtering.
        /// </summary>
        public ObservableCollection<FilterColumn> AvailableColumns
        {
            get { return _availableColumns; }
            set
            {
                _availableColumns = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the selected columns for filtering.
        /// </summary>
        public ObservableCollection<FilterColumn> SelectedColumns
        {
            get { return _selectedColumns; }
            set
            {
                _selectedColumns = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the column value filters.
        /// </summary>
        public ObservableCollection<ColumnValueFilter> ColumnValueFilters
        {
            get { return _columnValueFilters; }
            set
            {
                _columnValueFilters = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the value range filters.
        /// </summary>
        public ObservableCollection<ValueRangeFilter> ValueRangeFilters
        {
            get { return _valueRangeFilters; }
            set
            {
                _valueRangeFilters = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the available files for filtering.
        /// </summary>
        public ObservableCollection<FilterFile> AvailableFiles
        {
            get { return _availableFiles; }
            set
            {
                _availableFiles = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the filter operators.
        /// </summary>
        public ObservableCollection<string> FilterOperators
        {
            get { return _filterOperators; }
        }

        /// <summary>
        /// Gets the numeric columns.
        /// </summary>
        public ObservableCollection<string> NumericColumns
        {
            get { return _numericColumns; }
        }

        /// <summary>
        /// Gets the apply filter command.
        /// </summary>
        public RelayCommand ApplyFilterCommand
        {
            get { return _applyFilterCommand; }
        }

        /// <summary>
        /// Gets the save filter command.
        /// </summary>
        public RelayCommand SaveFilterCommand
        {
            get { return _saveFilterCommand; }
        }

        /// <summary>
        /// Gets the load filter command.
        /// </summary>
        public RelayCommand LoadFilterCommand
        {
            get { return _loadFilterCommand; }
        }

        /// <summary>
        /// Gets the reset filter command.
        /// </summary>
        public RelayCommand ResetFilterCommand
        {
            get { return _resetFilterCommand; }
        }

        /// <summary>
        /// Gets the cancel command.
        /// </summary>
        public RelayCommand CancelCommand
        {
            get { return _cancelCommand; }
        }

        /// <summary>
        /// Gets the add column command.
        /// </summary>
        public RelayCommand AddColumnCommand
        {
            get { return _addColumnCommand; }
        }

        /// <summary>
        /// Gets the remove column command.
        /// </summary>
        public RelayCommand RemoveColumnCommand
        {
            get { return _removeColumnCommand; }
        }

        /// <summary>
        /// Gets the add column value filter command.
        /// </summary>
        public RelayCommand AddColumnValueFilterCommand
        {
            get { return _addColumnValueFilterCommand; }
        }

        /// <summary>
        /// Gets the remove column value filter command.
        /// </summary>
        public RelayCommand RemoveColumnValueFilterCommand
        {
            get { return _removeColumnValueFilterCommand; }
        }

        /// <summary>
        /// Gets the add value range filter command.
        /// </summary>
        public RelayCommand AddValueRangeFilterCommand
        {
            get { return _addValueRangeFilterCommand; }
        }

        /// <summary>
        /// Gets the remove value range filter command.
        /// </summary>
        public RelayCommand RemoveValueRangeFilterCommand
        {
            get { return _removeValueRangeFilterCommand; }
        }

        /// <summary>
        /// Applies the filter settings.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private async void ApplyFilter(object? parameter)
        {
            try
            {
                // Create search criteria from current settings
                var criteria = new SearchCriteria
                {
                    SearchTerm = _searchTerm,
                    Columns = _selectedColumns.Where(c => c.IsSelected).Select(c => c.Name).ToList(),
                    ExactMatch = _exactMatch,
                    CaseSensitive = _isCaseSensitive,
                    MaxResults = _enableResultLimit ? _resultLimit : int.MaxValue,
                    FuzzySearch = _fuzzySearch,
                    RegexSearch = _regexSearch,
                    IsAllWordsSearch = _isAllWordsSearch,
                    IsAnyWordSearch = _isAnyWordSearch,
                    IsPhraseSearch = _isPhraseSearch,
                    EnableDateRange = _enableDateRange,
                    StartDate = _startDate,
                    EndDate = _endDate,
                    EnableTimeRange = _enableTimeRange,
                    StartTime = _startTime,
                    EndTime = _endTime,
                    ColumnValueFilters = _columnValueFilters.Select(f => new ColumnValueFilterCriteria 
                    { 
                        ColumnName = f.ColumnName, 
                        Operator = f.Operator, 
                        Value = f.Value 
                    }).ToList(),
                    ValueRangeFilters = _valueRangeFilters.Select(f => new ValueRangeFilterCriteria 
                    { 
                        ColumnName = f.ColumnName, 
                        MinValue = f.MinValue, 
                        MaxValue = f.MaxValue 
                    }).ToList()
                };

                // If we have a search service, perform the search
                if (_searchService != null)
                {
                    var results = await _searchService.AdvancedSearchAsync(criteria);
                    
                    // In a real implementation, this would update the results in the main UI
                    // For now, we'll show a message with the result count
                    MessageBox.Show($"Search completed successfully! Found {results.Count} matching results.", "Search Results", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // In a real implementation, this would apply the filter settings
                    // and perform the advanced search with these options
                    MessageBox.Show("Search filter applied successfully!", "Search Filter", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
                // Close the window
                _window?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying filter: {ex.Message}", "Search Filter Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Saves the current filter settings to a file.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void SaveFilter(object? parameter)
        {
            try
            {
                var filterSettings = new FilterSettings
                {
                    SearchTerm = _searchTerm,
                    IsCaseSensitive = _isCaseSensitive,
                    ExactMatch = _exactMatch,
                    FuzzySearch = _fuzzySearch,
                    RegexSearch = _regexSearch,
                    IsAllWordsSearch = _isAllWordsSearch,
                    IsAnyWordSearch = _isAnyWordSearch,
                    IsPhraseSearch = _isPhraseSearch,
                    EnableColumnFilter = _enableColumnFilter,
                    EnableDateRange = _enableDateRange,
                    EnableTimeRange = _enableTimeRange,
                    StartDate = _startDate,
                    EndDate = _endDate,
                    StartTime = _startTime,
                    EndTime = _endTime,
                    EnableResultLimit = _enableResultLimit,
                    ResultLimit = _resultLimit,
                    EnableFileFilter = _enableFileFilter,
                    SelectedColumns = _selectedColumns.Where(c => c.IsSelected).Select(c => c.Name).ToList(),
                    ColumnValueFilters = _columnValueFilters.Select(f => new ColumnValueFilterCriteria 
                    { 
                        ColumnName = f.ColumnName, 
                        Operator = f.Operator, 
                        Value = f.Value 
                    }).ToList(),
                    ValueRangeFilters = _valueRangeFilters.Select(f => new ValueRangeFilterCriteria 
                    { 
                        ColumnName = f.ColumnName, 
                        MinValue = f.MinValue, 
                        MaxValue = f.MaxValue 
                    }).ToList()
                };

                var fileName = $"SearchFilter_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var folderPath = Path.Combine(documentsPath, "Fouad", "Filters");
                var filePath = Path.Combine(folderPath, fileName);
                
                // Ensure the directory exists
                if (!string.IsNullOrEmpty(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                
                var json = JsonSerializer.Serialize(filterSettings, JsonOptions);
                File.WriteAllText(filePath, json);
                
                MessageBox.Show($"Filter saved successfully to {filePath}", "Save Filter", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving filter: {ex.Message}", "Save Filter Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Loads filter settings from a file.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void LoadFilter(object? parameter)
        {
            try
            {
                var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Fouad", "Filters");
                if (!Directory.Exists(folderPath))
                {
                    MessageBox.Show("No saved filters found.", "Load Filter", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var filterFiles = Directory.GetFiles(folderPath, "*.json");
                if (filterFiles.Length == 0)
                {
                    MessageBox.Show("No saved filters found.", "Load Filter", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // For simplicity, we'll load the most recent filter
                var mostRecentFile = filterFiles.OrderByDescending(f => File.GetCreationTime(f)).First();
                var json = File.ReadAllText(mostRecentFile);
                var filterSettings = JsonSerializer.Deserialize<FilterSettings>(json);

                if (filterSettings != null)
                {
                    // Apply loaded settings
                    _searchTerm = filterSettings.SearchTerm;
                    _isCaseSensitive = filterSettings.IsCaseSensitive;
                    _exactMatch = filterSettings.ExactMatch;
                    _fuzzySearch = filterSettings.FuzzySearch;
                    _regexSearch = filterSettings.RegexSearch;
                    _isAllWordsSearch = filterSettings.IsAllWordsSearch;
                    _isAnyWordSearch = filterSettings.IsAnyWordSearch;
                    _isPhraseSearch = filterSettings.IsPhraseSearch;
                    _enableColumnFilter = filterSettings.EnableColumnFilter;
                    _enableDateRange = filterSettings.EnableDateRange;
                    _enableTimeRange = filterSettings.EnableTimeRange;
                    _startDate = filterSettings.StartDate;
                    _endDate = filterSettings.EndDate;
                    _startTime = filterSettings.StartTime;
                    _endTime = filterSettings.EndTime;
                    _enableResultLimit = filterSettings.EnableResultLimit;
                    _resultLimit = filterSettings.ResultLimit;
                    _enableFileFilter = filterSettings.EnableFileFilter;

                    // Update selected columns
                    foreach (var column in _availableColumns)
                    {
                        column.IsSelected = filterSettings.SelectedColumns.Contains(column.Name);
                    }

                    // Update column value filters
                    _columnValueFilters.Clear();
                    foreach (var filter in filterSettings.ColumnValueFilters)
                    {
                        _columnValueFilters.Add(new ColumnValueFilter 
                        { 
                            ColumnName = filter.ColumnName, 
                            Operator = filter.Operator, 
                            Value = filter.Value 
                        });
                    }

                    // Update value range filters
                    _valueRangeFilters.Clear();
                    foreach (var filter in filterSettings.ValueRangeFilters)
                    {
                        _valueRangeFilters.Add(new ValueRangeFilter 
                        { 
                            ColumnName = filter.ColumnName, 
                            MinValue = filter.MinValue, 
                            MaxValue = filter.MaxValue 
                        });
                    }

                    // Notify property changes
                    OnPropertyChanged(nameof(SearchTerm));
                    OnPropertyChanged(nameof(IsCaseSensitive));
                    OnPropertyChanged(nameof(ExactMatch));
                    OnPropertyChanged(nameof(FuzzySearch));
                    OnPropertyChanged(nameof(RegexSearch));
                    OnPropertyChanged(nameof(IsAllWordsSearch));
                    OnPropertyChanged(nameof(IsAnyWordSearch));
                    OnPropertyChanged(nameof(IsPhraseSearch));
                    OnPropertyChanged(nameof(EnableColumnFilter));
                    OnPropertyChanged(nameof(EnableDateRange));
                    OnPropertyChanged(nameof(EnableTimeRange));
                    OnPropertyChanged(nameof(StartDate));
                    OnPropertyChanged(nameof(EndDate));
                    OnPropertyChanged(nameof(StartTime));
                    OnPropertyChanged(nameof(EndTime));
                    OnPropertyChanged(nameof(EnableResultLimit));
                    OnPropertyChanged(nameof(ResultLimit));
                    OnPropertyChanged(nameof(EnableFileFilter));
                    OnPropertyChanged(nameof(AvailableColumns));
                    OnPropertyChanged(nameof(ColumnValueFilters));
                    OnPropertyChanged(nameof(ValueRangeFilters));

                    MessageBox.Show("Filter loaded successfully!", "Load Filter", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading filter: {ex.Message}", "Load Filter Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Resets all filter settings to default values.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void ResetFilter(object? parameter)
        {
            _searchTerm = "";
            _isCaseSensitive = false;
            _exactMatch = false;
            _fuzzySearch = false;
            _regexSearch = false;
            _isAllWordsSearch = true;
            _isAnyWordSearch = false;
            _isPhraseSearch = false;
            _enableColumnFilter = false;
            _enableDateRange = false;
            _enableTimeRange = false;
            _startDate = DateTime.Now.AddDays(-30);
            _endDate = DateTime.Now;
            _startTime = "00:00";
            _endTime = "23:59";
            _enableResultLimit = false;
            _resultLimit = 100;
            _enableFileFilter = false;

            // Reset column selections
            foreach (var column in _availableColumns)
            {
                column.IsSelected = false;
            }

            // Clear filters
            _columnValueFilters.Clear();
            _valueRangeFilters.Clear();

            // Notify property changes
            OnPropertyChanged(nameof(SearchTerm));
            OnPropertyChanged(nameof(IsCaseSensitive));
            OnPropertyChanged(nameof(ExactMatch));
            OnPropertyChanged(nameof(FuzzySearch));
            OnPropertyChanged(nameof(RegexSearch));
            OnPropertyChanged(nameof(IsAllWordsSearch));
            OnPropertyChanged(nameof(IsAnyWordSearch));
            OnPropertyChanged(nameof(IsPhraseSearch));
            OnPropertyChanged(nameof(EnableColumnFilter));
            OnPropertyChanged(nameof(EnableDateRange));
            OnPropertyChanged(nameof(EnableTimeRange));
            OnPropertyChanged(nameof(StartDate));
            OnPropertyChanged(nameof(EndDate));
            OnPropertyChanged(nameof(StartTime));
            OnPropertyChanged(nameof(EndTime));
            OnPropertyChanged(nameof(EnableResultLimit));
            OnPropertyChanged(nameof(ResultLimit));
            OnPropertyChanged(nameof(EnableFileFilter));
            OnPropertyChanged(nameof(AvailableColumns));
            OnPropertyChanged(nameof(ColumnValueFilters));
            OnPropertyChanged(nameof(ValueRangeFilters));
        }

        /// <summary>
        /// Cancels the filter operation.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void Cancel(object? parameter)
        {
            // Close the window without applying filters
            _window?.Close();
        }

        /// <summary>
        /// Adds selected columns to the selected columns list.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void AddColumn(object? parameter)
        {
            var selectedItems = AvailableColumns.Where(c => c.IsSelected).ToList();
            foreach (var item in selectedItems)
            {
                if (!SelectedColumns.Any(c => c.Name == item.Name))
                {
                    SelectedColumns.Add(new FilterColumn { Name = item.Name, IsSelected = true });
                }
            }
            OnPropertyChanged(nameof(SelectedColumns));
        }

        /// <summary>
        /// Removes selected columns from the selected columns list.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void RemoveColumn(object? parameter)
        {
            var selectedItems = SelectedColumns.Where(c => c.IsSelected).ToList();
            foreach (var item in selectedItems)
            {
                SelectedColumns.Remove(item);
            }
            OnPropertyChanged(nameof(SelectedColumns));
        }

        /// <summary>
        /// Adds a new column value filter.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void AddColumnValueFilter(object? parameter)
        {
            ColumnValueFilters.Add(new ColumnValueFilter());
            OnPropertyChanged(nameof(ColumnValueFilters));
        }

        /// <summary>
        /// Removes selected column value filters.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void RemoveColumnValueFilter(object? parameter)
        {
            var selectedItems = ColumnValueFilters.Where(f => f.IsSelected).ToList();
            if (selectedItems.Count > 0)
            {
                foreach (var item in selectedItems)
                {
                    ColumnValueFilters.Remove(item);
                }
                OnPropertyChanged(nameof(ColumnValueFilters));
            }
            else if (ColumnValueFilters.Count > 0)
            {
                // If no items are selected, remove the last item
                ColumnValueFilters.RemoveAt(ColumnValueFilters.Count - 1);
                OnPropertyChanged(nameof(ColumnValueFilters));
            }
        }

        /// <summary>
        /// Adds a new value range filter.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void AddValueRangeFilter(object? parameter)
        {
            ValueRangeFilters.Add(new ValueRangeFilter());
            OnPropertyChanged(nameof(ValueRangeFilters));
        }

        /// <summary>
        /// Removes selected value range filters.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        private void RemoveValueRangeFilter(object? parameter)
        {
            var selectedItems = ValueRangeFilters.Where(f => f.IsSelected).ToList();
            if (selectedItems.Count > 0)
            {
                foreach (var item in selectedItems)
                {
                    ValueRangeFilters.Remove(item);
                }
                OnPropertyChanged(nameof(ValueRangeFilters));
            }
            else if (ValueRangeFilters.Count > 0)
            {
                // If no items are selected, remove the last item
                ValueRangeFilters.RemoveAt(ValueRangeFilters.Count - 1);
                OnPropertyChanged(nameof(ValueRangeFilters));
            }
        }

        /// <summary>
        /// Sets the available columns for filtering.
        /// </summary>
        /// <param name="columnNames">The column names to set.</param>
        public void SetAvailableColumns(IEnumerable<string> columnNames)
        {
            _availableColumns.Clear();
            _selectedColumns.Clear();
            _numericColumns.Clear();
            
            foreach (var columnName in columnNames)
            {
                _availableColumns.Add(new FilterColumn { Name = columnName, IsSelected = false });
                
                // Add to numeric columns if it might contain numeric data
                // In a real implementation, you would analyze the data to determine this
                if (columnName.Contains("count", StringComparison.OrdinalIgnoreCase) ||
                    columnName.Contains("number", StringComparison.OrdinalIgnoreCase) ||
                    columnName.Contains("amount", StringComparison.OrdinalIgnoreCase) ||
                    columnName.Contains("price", StringComparison.OrdinalIgnoreCase) ||
                    columnName.Contains("value", StringComparison.OrdinalIgnoreCase))
                {
                    _numericColumns.Add(columnName);
                }
            }
            
            OnPropertyChanged(nameof(AvailableColumns));
            OnPropertyChanged(nameof(NumericColumns));
        }

        /// <summary>
        /// Sets the available files for filtering.
        /// </summary>
        /// <param name="filePaths">The file paths to set.</param>
        public void SetAvailableFiles(IEnumerable<string> filePaths)
        {
            _availableFiles.Clear();
            foreach (var filePath in filePaths)
            {
                _availableFiles.Add(new FilterFile { Name = Path.GetFileName(filePath), Path = filePath, IsSelected = false });
            }
            OnPropertyChanged(nameof(AvailableFiles));
        }


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

    /// <summary>
    /// Represents a column that can be filtered.
    /// </summary>
    public class FilterColumn : INotifyPropertyChanged
    {
        private string _name = "";
        private bool _isSelected = false;

        /// <summary>
        /// Gets or sets the name of the column.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the column is selected for filtering.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

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

    /// <summary>
    /// Represents a column value filter.
    /// </summary>
    public class ColumnValueFilter : INotifyPropertyChanged
    {
        private string _columnName = "";
        private string _operator = "=";
        private string _value = "";
        private bool _isSelected = false;

        /// <summary>
        /// Gets or sets the column name.
        /// </summary>
        public string ColumnName
        {
            get { return _columnName; }
            set
            {
                _columnName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the operator.
        /// </summary>
        public string Operator
        {
            get { return _operator; }
            set
            {
                _operator = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the filter is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

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

    /// <summary>
    /// Represents a value range filter.
    /// </summary>
    public class ValueRangeFilter : INotifyPropertyChanged
    {
        private string _columnName = "";
        private string _minValue = "";
        private string _maxValue = "";
        private bool _isSelected = false;

        /// <summary>
        /// Gets or sets the column name.
        /// </summary>
        public string ColumnName
        {
            get { return _columnName; }
            set
            {
                _columnName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        public string MinValue
        {
            get { return _minValue; }
            set
            {
                _minValue = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        public string MaxValue
        {
            get { return _maxValue; }
            set
            {
                _maxValue = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the filter is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

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

    /// <summary>
    /// Represents a file that can be filtered.
    /// </summary>
    public class FilterFile : INotifyPropertyChanged
    {
        private string _name = "";
        private string _path = "";
        private bool _isSelected = false;

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the path of the file.
        /// </summary>
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the file is selected for filtering.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

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

    /// <summary>
    /// Represents saved filter settings.
    /// </summary>
    public class FilterSettings
    {
        public string SearchTerm { get; set; } = "";
        public bool IsCaseSensitive { get; set; } = false;
        public bool ExactMatch { get; set; } = false;
        public bool FuzzySearch { get; set; } = false;
        public bool RegexSearch { get; set; } = false;
        public bool IsAllWordsSearch { get; set; } = true;
        public bool IsAnyWordSearch { get; set; } = false;
        public bool IsPhraseSearch { get; set; } = false;
        public bool EnableColumnFilter { get; set; } = false;
        public bool EnableDateRange { get; set; } = false;
        public bool EnableTimeRange { get; set; } = false;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string StartTime { get; set; } = "00:00";
        public string EndTime { get; set; } = "23:59";
        public bool EnableResultLimit { get; set; } = false;
        public int ResultLimit { get; set; } = 100;
        public bool EnableFileFilter { get; set; } = false;
        public List<string> SelectedColumns { get; set; } = new List<string>();
        public List<ColumnValueFilterCriteria> ColumnValueFilters { get; set; } = new List<ColumnValueFilterCriteria>();
        public List<ValueRangeFilterCriteria> ValueRangeFilters { get; set; } = new List<ValueRangeFilterCriteria>();
    }
}