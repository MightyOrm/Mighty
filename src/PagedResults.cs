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

        /// <summary>
        /// Construct a <see cref="PagedResults{T}"/> object.
        /// </summary>
        /// <remarks>
        /// Explicitly document that <see cref="PagedResults{T}"/> has and should keep a public parameterless constructor
        /// (i.e. other people are - and should remain - allowed to construct their own instances of it
        /// for their own reasons; for example, building a new results object with the type of <see cref="Items"/>
        /// converted to something else which Mighty doesn't need to know about).
        /// This is also effectively locks down (even more...) the fields of this objects to be public, read-write-able
        /// fields (not properties, not readonly, not protected or private write only), which is quick-n-dirty but okay
        /// (i.e. if you changed any of the above, Mighty would not be link-compatible with libraries compiled against
        /// the pre-change version).
        /// </remarks>
        public PagedResults() { }
    }
}
