namespace NUnit.Framework
{
	static public partial class AssertExtensions
	{
		static public void StrictlyEqual(object o1, object o2, string message = null, params object[] args)
		{	
			Assert.AreEqual(o1, o2, message, args);
			Assert.AreEqual(o1.GetType(), o2.GetType(), "StrictlyEqual test: matching values {0}={1}, but type mismatch", o1, o2);
		}
	}
}