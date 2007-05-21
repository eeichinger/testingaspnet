using System;
using NUnit.Core;
using NUnit.Core.Builders;

namespace NUnitAspEx.Core
{
	/// <summary>
    /// AspTestFixtureBuilder knows how to build an [AspTestFixture]
    /// </summary>
    [SuiteBuilder]
    public class AspTestFixtureBuilder : NUnitTestFixtureBuilder
    {
		#region GenericTestFixtureBuilder Overrides

		protected override TestSuite MakeSuite(Type type, int assemblyKey)
		{
			return new AspTestFixture(type, assemblyKey);
		}

		#endregion

        #region ISuiteBuilder Members

        // The builder recognizes the types that it can use by the presense
        // of SampleFixtureExtensionAttribute. Note that an attribute does not
        // have to be used. You can use any arbitrary set of rules that can be 
        // implemented using reflection on the type.
        public override bool CanBuildFrom(Type type)
        {
            return type.IsDefined(typeof(AspTestFixtureAttribute), false);
        }

        #endregion        
    }
}