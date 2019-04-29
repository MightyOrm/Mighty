using System;
using System.Collections.Generic;
using System.Text;

namespace Mighty
{
    /// <summary>
    /// This data member should be ignored when reading from and writing to the database.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DatabaseIgnoreAttribute : Attribute
    {
    }
}
