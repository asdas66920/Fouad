using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fouad.Models
{
    /// <summary>
    /// Represents an item in the search history.
    /// Contains information about a previous search operation.
    /// </summary>
    public class HistoryItem
    {
        /// <summary>
        /// Gets or sets the unique identifier for the history item.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the file that was searched.
        /// </summary>
        public string? FileName { get; set; }
        
        /// <summary>
        /// Gets or sets the date and time when the search was performed.
        /// </summary>
        public DateTime SearchDate { get; set; }
        
        /// <summary>
        /// Gets or sets the search term that was used.
        /// </summary>
        public string? SearchTerm { get; set; }
        
        /// <summary>
        /// Gets or sets the number of results found for this search.
        /// </summary>
        public int ResultCount { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether this item is selected.
        /// </summary>
        public bool IsSelected { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether this item has been added to history.
        /// </summary>
        public bool IsAddedToHistory { get; set; }
        
        /// <summary>
        /// Gets or sets the dynamic column values for this history item.
        /// </summary>
        public List<string> DynamicColumnValues { get; set; } = new();
    }
}