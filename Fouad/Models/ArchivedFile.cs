using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fouad.Models
{
    /// <summary>
    /// Represents an archived file with metadata.
    /// </summary>
    public class ArchivedFile
    {
        /// <summary>
        /// Gets or sets the unique identifier for the archive entry.
        /// </summary>
        public int ArchiveId { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the original file.
        /// </summary>
        public string? FileName { get; set; }
        
        /// <summary>
        /// Gets or sets the date of the upload.
        /// </summary>
        public DateTime UploadDate { get; set; }
        
        /// <summary>
        /// Gets or sets the username of the person who uploaded the file.
        /// </summary>
        public string? UploadedBy { get; set; }
        
        /// <summary>
        /// Gets or sets the full path to the file on the server.
        /// </summary>
        public string? FilePath { get; set; }
        
        /// <summary>
        /// Gets or sets the size of the file in bytes.
        /// </summary>
        public long FileSize { get; set; }
        
        /// <summary>
        /// Gets or sets the number of rows in the file.
        /// </summary>
        public int RowCount { get; set; }
        
        /// <summary>
        /// Gets or sets the number of columns in the file.
        /// </summary>
        public int ColumnCount { get; set; }
    }
}