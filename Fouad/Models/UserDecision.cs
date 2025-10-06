using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fouad.Models
{
    /// <summary>
    /// Represents a user decision for handling a record.
    /// </summary>
    public enum UserDecision
    {
        /// <summary>
        /// Do not merge this record.
        /// </summary>
        Ignore,
        
        /// <summary>
        /// Update the record in the master table with the new data.
        /// </summary>
        Update,
        
        /// <summary>
        /// Add the record as a completely new row in the master table.
        /// </summary>
        AddAsNew
    }
}