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
        public List<T> Items;

        /// <summary>
        /// Current page
        /// </summary>
        public int CurrentPage;

        /// <summary>
        /// Page size
        /// </summary>
        public int PageSize;

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
