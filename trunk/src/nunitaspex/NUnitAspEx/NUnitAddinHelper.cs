using System;
using System.Diagnostics;
using NUnit.Core.Extensibility;
using NUnitAspEx.Core;

namespace NUnitAspEx
{
    /// <summary>
    /// You MUST implement 1 class per assembly containing an AspTestFixture that derives
    /// from this helper class and mark it using NUnitAddinAttribute (from nunit.core.interfaces assembly).
    /// </summary>
    /// <example>
    /// [NUnit.Core.Extensibility.NUnitAddin]
    /// public class MyNUnitAddinHelper : NUnitAddinHelper 
    /// {}
    /// </example>
    [NUnitAddin]
    public class NUnitAddinHelper : IAddin
    {
        static NUnitAddinHelper()
        {
            Debugger.Break();
        }

        public bool Install(IExtensionHost host)
        {
            return InstallExtensions(host);
        }

        internal static bool InstallExtensions(IExtensionHost host)
        {
            IExtensionPoint testDecorators = host.GetExtensionPoint("TestDecorators");
            testDecorators.Install( new AspTestFixtureDecorator() );

            IExtensionPoint suiteBuilders = host.GetExtensionPoint("SuiteBuilders");
            suiteBuilders.Install( new AspTestFixtureBuilder() );

            IExtensionPoint testCaseBuilders = host.GetExtensionPoint("TestCaseBuilders");
            testCaseBuilders.Install( new AspTestCaseBuilder() );
            return true;
        }
    }
}
