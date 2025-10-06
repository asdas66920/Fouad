# Fouad - Advanced Data Processing Application
Fouad is a comprehensive data processing and analysis application built with .NET 9.0 and WPF. It provides powerful features for handling Excel and CSV files with database integration, advanced search capabilities, and a user-friendly interface.

## Features
- **File Processing**: Load and process Excel (.xlsx, .xls) and CSV files
- **Data Search**: Advanced search with pagination and fuzzy matching
- **Data Export**: Export data to Excel and PDF formats
- **Database Integration**: SQLite database for persistent storage and file archiving
- **Data Review**: Manual review process for imported data with reconciliation
- **Caching**: Binary serialization caching for faster file loading
- **Dependency Injection**: Service container for better testability and modularity
- **MVVM Pattern**: Clean separation of UI, business logic, and data layers
- **Service-Oriented Design**: Modular services with defined interfaces
- **Advanced Search Filter**: Comprehensive filtering system with multiple criteria and save/load capabilities

## Core Functionality
- **Data Search**: Advanced search with pagination and fuzzy matching
- **Dependency Injection**: Service container for better testability and modularity
- **Performance Optimizations**: Improved search algorithms and memory management

## Architecture Improvements
- Added interfaces for all services to improve testability
- Implemented dependency injection container
- Created configuration service for application settings
- Added advanced search service with complex criteria

## Testing Improvements
- Added comprehensive unit tests for new services
- Created integration tests for service interactions
- Implemented performance tests for critical operations
- Added edge case testing for error conditions

## New Services
- **ExportService**: Complete implementation for Excel and PDF export
- **ConfigurationService**: Management of application settings
- **SearchService**: Advanced search with multiple criteria
- **ServiceContainer**: Dependency injection container

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- Windows 10 or later

### Installation
1. Clone the repository
2. Restore NuGet packages
3. Build the solution
4. Run the application

### Usage
1. Load Excel or CSV files using the file import functionality
2. Use the search features to find specific data
3. Review and reconcile data as needed
4. Export results to Excel or PDF formats

## Advanced Search Filter
The application includes a powerful Advanced Search Filter that allows users to:
- Perform complex searches with multiple criteria
- Filter by column values with various operators
- Set date and time ranges
- Limit results with customizable thresholds
- Save and load filter configurations
- Use fuzzy matching and regular expressions

See [Advanced Search Filter Documentation](Documentation/AdvancedSearchFilter.md) for detailed information.

## Testing

The application includes comprehensive tests:
- Unit tests for all services and view models
- Integration tests for service interactions
- Performance tests for critical operations
- Edge case testing for error conditions