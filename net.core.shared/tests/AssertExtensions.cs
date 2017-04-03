using System;
using Mighty;
using NUnit.Framework;

namespace NUnit.Framework
{
	static public partial class AssertExtensions
	{
		static public void StrictlyEqual(object o1, object o2)
		{	
			Assert.AreEqual(o1, o2);
			Assert.AreEqual(o1.GetType(), o2.GetType());
		}
	}
}