using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Mighty
{
	// Object extensions which are considered as too specific to our needs to be useful outside
	public static partial class InternalExtensions
	{
		internal static IEnumerable<dynamic> YieldResult(this DbDataReader rdr)
		{
			while(rdr.Read())
			{
				yield return rdr.RecordToExpando();
			}
		}
	}
}