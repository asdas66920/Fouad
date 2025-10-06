using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fouad.Models
{
    /// <summary>
    /// Represents an entry in the archive log table.
    /// Contains metadata about imported files.
    /// </summary>
    public class ArchiveLog
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
    }
}