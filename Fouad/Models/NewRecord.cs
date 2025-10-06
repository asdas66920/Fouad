using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fouad.Models
{
    /// <summary>
    /// Represents a new record from an Excel file that does not have a match in the master table.
    /// </summary>
    public class NewRecord
    {
        /// <summary>
        /// Gets or sets the unique identifier for the new record.
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
        /// Gets or sets the dynamic column values for this record.
        /// </summary>
        public List<string> DynamicColumnValues { get; set; } = new();
    }
}