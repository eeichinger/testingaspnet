using System;
using System.Diagnostics;
using NUnit.Framework;

namespace NUnitAspExTests
{
	/// <summary>
	/// Summary description for SampleTests.
	/// </summary>
	[TestFixture]
	public class EnsureNUnitVersion
	{
	    [Test]
		public void TestVersion()
		{
			Console.WriteLine( "EnsureNUnitVersion executing on CLR version: " + Environment.Version.ToString(3));
#if NET_1_0
			Assert.AreEqual( "1.0", Environment.Version.ToString(2) );
#endif
#if NET_1_1
			Assert.AreEqual( "1.1", Environment.Version.ToString(2) );
#endif
#if NET_2_0
			Assert.AreEqual( "2.0", Environment.Version.ToString(2) );
#endif
#if MONO_2_0
			Assert.AreEqual( Environment.Version.ToString(2), "2.0" );
#endif
			try
			{
				//Console.WriteLine( new StackTrace(1) );
				//throw new Exception("testerror");
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
	}
}
