using System;
using System.Diagnostics;
using NUnit.Framework;

namespace NAnt.NUnit2OutProc.Tests
{
	/// <summary>
	/// Summary description for SampleTests.
	/// </summary>
	[TestFixture]
	public class SampleTests
	{
	    [Test]
		public void SomeTestFunc()
		{	
			Console.WriteLine( Environment.Version.ToString(3));
#if NET_1_0
			Assert.AreEqual( Environment.Version.ToString(2), "1.0" );
#endif
#if NET_1_1
			Assert.AreEqual( Environment.Version.ToString(2), "1.1" );
#endif
#if NET_2_0
			Assert.AreEqual( Environment.Version.ToString(2), "2.0" );
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
