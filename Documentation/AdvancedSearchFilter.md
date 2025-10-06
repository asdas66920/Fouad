# Advanced Search Filter Documentation

## Overview
The Advanced Search Filter is a comprehensive filtering system that allows users to perform complex searches with multiple criteria. It provides a wide range of filtering options to help users find exactly what they're looking for in their data files.

## Features

### 1. Search Term Options
- **Case Sensitive Search**: Perform searches that distinguish between uppercase and lowercase letters
- **Exact Match**: Find records that match the search term exactly
- **Fuzzy Search**: Find records that are similar to the search term (useful for typos)
- **Regular Expression**: Use regex patterns for advanced searching
- **Search Modes**:
  - All Words: Find records containing all search terms
  - Any Word: Find records containing any of the search terms
  - Exact Phrase: Find records containing the exact phrase

### 2. Column Filters
- **Column Selection**: Choose which columns to include in the search
- **Column Value Filters**: Apply specific filters to individual columns
  - Operators: =, !=, >, <, >=, <=, Contains, Starts With, Ends With
  - Custom values for each operator
- **Value Range Filters**: Set minimum and maximum values for numeric columns

### 3. Date and Time Filters
- **Date Range**: Filter records between specific start and end dates
- **Time Range**: Filter records between specific start and end times

### 4. Result Limiting
- **Result Limit**: Limit the number of results returned (1-5000)
- **File Selection**: Choose which files to search in

### 5. Filter Management
- **Save Filter**: Save current filter settings to a file for later use
- **Load Filter**: Load previously saved filter settings
- **Reset Filter**: Reset all filter settings to default values

## Usage

### Opening the Search Filter
1. Click the "Advanced Search" button in the Results Table view
2. The Advanced Search Filter window will open

### Configuring Search Options
1. Enter your search term in the "Search Term" field
2. Select the desired search options (case sensitive, exact match, etc.)
3. Choose your preferred search mode (all words, any word, exact phrase)

### Applying Column Filters
1. Check "Enable Column Filtering"
2. Select columns from the "Available Columns" list
3. Use the ">>" button to move columns to the "Selected Columns" list
4. Add column value filters by specifying:
   - Column name
   - Operator (=, !=, >, <, etc.)
   - Value to compare against

### Setting Date and Time Filters
1. Check "Enable Date Range Filter"
2. Select start and end dates using the date pickers
3. For time filtering, check "Enable Time Range" and enter start/end times

### Limiting Results
1. Check "Limit Results"
2. Use the slider to set the maximum number of results to return

### Saving and Loading Filters
1. Click "Save Filter" to save current settings to a file
2. Click "Load Filter" to load previously saved settings
3. Click "Reset" to clear all filter settings

## Technical Implementation

### View Model
The `SearchFilterViewModel` handles all the logic for the search filter window:
- Manages filter properties and their states
- Handles command execution for all filter operations
- Provides data binding for the UI elements
- Serializes/deserializes filter settings to/from JSON

### Models
- `FilterColumn`: Represents a column that can be filtered
- `ColumnValueFilter`: Represents a filter applied to a specific column value
- `ValueRangeFilter`: Represents a range filter for numeric values
- `FilterFile`: Represents a file that can be included in the search
- `FilterSettings`: Serializable object that stores all filter settings

### Services
- `ISearchService`: Performs the actual search operations using the filter criteria
- `IFileDataService`: Provides access to file data and column information

## Integration with Main Application
The search filter integrates with the main application through:
- The Results Table view model
- The service container for dependency injection
- The file data service for column information
- The search service for executing searches

## File Storage
Saved filter settings are stored in:
- Location: `Documents/Fouad/Filters/`
- Format: JSON files with timestamps in the filename
- Each filter file contains all settings for a specific search configuration