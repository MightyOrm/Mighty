using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mighty.Plugins
{
    /// <summary>
    /// Class used to return the two queries necessary to do a paged select and a total count.
    /// </summary>
    public class PagingQueryPair
    {
        /// <summary>
        /// The query which will return the total count of paged items
        /// </summary>
        public string CountQuery;

        /// <summary>
        /// The query which will return the selected page of items
        /// </summary>
        public string PagingQuery;
    }
}
