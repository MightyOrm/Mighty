using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using NUnit.Framework;

using Mighty;

namespace Massive.Tests.MySql
{
	public class MyNVC : NameValueCollection
	{
		public MyNVC() : base() { }
	}

	[TestFixture]
	public class ReadTests
	{
		[Test]
		public void MyTest()
		{
			//var db = MightyORM.DB("northwind");
			Assert.AreEqual(1, 1);
		}

		[Test]
		public void MyTest2()
		{
			//AssertExtensions.StrictlyEqual(1, (long)1);
		}

		[Test]
		public void MyTest3()
		{
			var nv = new MyNVC();
			nv.Add("Mike", "Beaton");
			nv.Add("Dan", "Evans");
			var o = nv.ToExpando();
			AssertExtensions.StrictlyEqual(1, (long)1);
		}
	}
}