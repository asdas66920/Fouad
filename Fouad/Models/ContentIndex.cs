using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fouad.Models
{
    /// <summary>
    /// Represents an entry in the content index table.
    /// Contains indexed data from imported files.
    /// </summary>
    public class ContentIndex
    {
        /// <summary>
        /// Gets or sets the unique identifier for the content index entry.
        /// </summary>
        public int ContentIndexId { get; set; }
        
        /// <summary>
        /// Gets or sets the archive ID linking to the ArchiveLog table.
        /// </summary>
        public int ArchiveId { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the worksheet.
        /// </summary>
        public string? SheetName { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the column.
        /// </summary>
        public string? ColumnName { get; set; }
        
        /// <summary>
        /// Gets or sets the row number.
        /// </summary>
        public int RowNumber { get; set; }
        
        /// <summary>
        /// Gets or sets the cell value.
        /// </summary>
        public string? CellValue { get; set; }
    }
}