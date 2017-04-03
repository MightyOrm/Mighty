using System;
using Mighty;
using NUnit.Framework;

namespace Massive.Tests.MySql
{
	[TestFixture]
	public class ReadTests
	{
		[Test]
		public void MyTest()
		{
			var db = new MightyORM.DB();
			Assert.AreEqual(1, 1);
		}

		[Test]
		public void MyTest2()
		{
			Assert.AreEqual(1, 2);
		}
	}
}