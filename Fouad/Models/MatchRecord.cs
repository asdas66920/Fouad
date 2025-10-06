using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fouad.Models
{
    /// <summary>
    /// Represents a record that has the same unique key in both sources.
    /// Shows a comparison between the new and old data.
    /// </summary>
    public class MatchRecord
    {
        /// <summary>
        /// Gets or sets the unique identifier for the match record.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Gets or sets the archive ID linking to the ArchiveLog table.
        /// </summary>
        public int ArchiveId { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the file containing this record.
        /// </summary>
        public string? FileName { get; set; }
        
        /// <summary>
        /// Gets or sets the existing data from the master table.
        /// </summary>
        public List<string> ExistingData { get; set; } = new();

        public List<string> NewData { get; set; } = new();

        public List<string> DynamicColumnValues { get; set; } = new();

        public List<string> MasterValues { get; set; } = new();
        
        /// <summary>
        /// Gets or sets a value indicating whether this record has been reviewed.
        /// </summary>
        public bool IsReviewed { get; set; } = false;
    }
}