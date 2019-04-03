using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty
{
	public class PagedResults<T>
	{
		public IEnumerable<T> Items;
		public int TotalRecords;
		public int TotalPages;
	}
}
