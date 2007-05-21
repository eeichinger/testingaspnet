using NUnitAspEx;

namespace NUnitAspExTests
{
    [AspTestFixture(
        VirtualPath = AspFixtureNonRootedRuntimeEnvironmentTests.VIRTUALPATH
        , RelativePhysicalPath = AspFixtureNonRootedRuntimeEnvironmentTests.RELATIVEPATH
        )]
    public class AspFixtureNonRootedRuntimeEnvironmentTests : AspFixtureRuntimeEnvironmentTestsBase
    {
        public const string VIRTUALPATH = "/vpath";
        public const string RELATIVEPATH = "fixture1";

        public AspFixtureNonRootedRuntimeEnvironmentTests() : base(VIRTUALPATH, RELATIVEPATH)
        {
        }        
    }
}