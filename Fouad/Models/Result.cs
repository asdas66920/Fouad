using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;

namespace Fouad.Models
{
    /// <summary>
    /// Represents a search result from a data file.
    /// Contains information about a row of data that matches a search query.
    /// </summary>
    [MessagePackObject]
    public class Result
    {
        /// <summary>
        /// Gets or sets the unique identifier for the result.
        /// </summary>
        [Key(0)]
        public int Id { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the file containing this result.
        /// </summary>
        [Key(1)]
        public string? FileName { get; set; }
        
        /// <summary>
        /// Gets or sets the content of the result.
        /// </summary>
        [Key(2)]
        public string? Content { get; set; }
        
        /// <summary>
        /// Gets or sets the date and time when the search was performed.
        /// </summary>
        [Key(3)]
        public DateTime SearchDate { get; set; }
        
        /// <summary>
        /// Gets or sets the number of matches found for this result.
        /// </summary>
        [Key(4)]
        public int MatchCount { get; set; }
        
        /// <summary>
        /// Gets or sets the dynamic column values for this result.
        /// </summary>
        [Key(5)]
        public List<string> DynamicColumnValues { get; set; } = new();
        
        /// <summary>
        /// Gets or sets a value indicating whether this result has been added to history.
        /// </summary>
        [Key(6)]
        public bool IsAddedToHistory { get; set; } = false;
    }
}