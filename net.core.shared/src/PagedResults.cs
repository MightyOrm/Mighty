using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty
{
    /// <summary>
    /// The results from a paging query
    /// </summary>
    /// <typeparam name="T">The generic type for items returned by this instance</typeparam>
    public class PagedResults<T>
    {
        /// <summary>
        /// The requested page of items
        /// </summary>
        public IEnumerable<T> Items;

        /// <summary>
        /// The total number of pages available
        /// </summary>
        public int TotalRecords;

        /// <summary>
        /// The total number of items available
        /// </summary>
        public int TotalPages;
    }
}
