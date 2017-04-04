using System;
using NUnit.Framework;

//using Mighty;

namespace Massive.Tests.MySql
{
	[TestFixture]
	public class ReadTests
	{
		[Test]
		public void MyTest()
		{
			//var db = new MightyORM.DB();
			Assert.AreEqual(1, 1);
		}

		[Test]
		public void MyTest2()
		{
			AssertExtensions.StrictlyEqual((long)1, 1);
		}
	}
}