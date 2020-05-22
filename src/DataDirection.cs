using System;
using System.Collections.Generic;
using System.Text;

namespace Mighty
{
    /// <summary>
    /// Specify the data direction for a <see cref="DatabaseColumnAttribute"/>.
    /// </summary>
    [Flags]
    public enum DataDirection
    {
        /// <summary>
        /// Read values from the database to this data member
        /// </summary>
        ReadFromDatabase = 1 << 0,

        /// <summary>
        /// Write values from this data member to the database
        /// </summary>
        WriteToDatabase = 1 << 1
    }
}
