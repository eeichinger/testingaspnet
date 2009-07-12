using NUnitAspEx;

namespace NUnitAspExTests
{
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    [AspTestFixture(
        VirtualPath=AspFixtureRootedRuntimeEnvironmentTests.VIRTUALPATH
        , RelativePhysicalPath = AspFixtureRootedRuntimeEnvironmentTests.RELATIVEPATH
        )]
    public class AspFixtureRootedRuntimeEnvironmentTests : AspFixtureRuntimeEnvironmentTestsBase
    {
        public const string VIRTUALPATH = "/";
        public const string RELATIVEPATH = "fixture1";

        public AspFixtureRootedRuntimeEnvironmentTests() : base(VIRTUALPATH, RELATIVEPATH)
        {
        }
    }
}