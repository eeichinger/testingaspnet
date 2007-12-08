using System;
using System.IO;
using System.Web;
using NUnit.Framework;
using NUnitAspEx;
using NUnitAspEx.Client;

namespace NUnitAspExTests
{
    public abstract class AspFixtureRuntimeEnvironmentTestsBase
    {
        private string _configuredVirtualPath;
        private string _configuredRelativePhysicalPath;
        
        private string _appPath;
        private string _appVirtualPath;

        protected AspFixtureRuntimeEnvironmentTestsBase(string virtualpath, string relativepath)
        {
            _configuredVirtualPath = virtualpath;
            _configuredRelativePhysicalPath = relativepath;
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Console.WriteLine("testfixturesetup called");
            _appPath = HttpRuntime.AppDomainAppPath;
            _appVirtualPath = HttpRuntime.AppDomainAppVirtualPath;
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Console.WriteLine("testfixtureteardown called");
        }

        [SetUp]
        public void SetUp()
        {
            Console.WriteLine("testsetup called");
        }

        [TearDown]
        public void TearDown()
        {
            Console.WriteLine("testteardown called");
        }

        /// <summary>
        /// TODO: Which is the right "BaseDirectory"? Assembly.CodeBase? AppDomain.BaseDirectory?
        /// </summary>
        //[NUnit.Framework.Ignore("BaseDirectory issue must be solved before")]
        [Ignore]
        public void WeAreInAspAppDomainButNoHttpContext()
        {
            // check for HttpConfigurationSystem
            Assert.IsTrue(HttpReflectionUtils.IsAspAppDomain());
            
            // no HttpContext available
            Assert.IsNull(HttpContext.Current, "HttpContext should be null since we're not within a request");
            
            // we're on the right physical path relative to CreatorDomain?
            string creatorBaseDir = AspTestContext.Host.RootLocation;
            string calculatedBaseDir = Path.Combine(creatorBaseDir, _configuredRelativePhysicalPath.TrimStart('/', '\\', '~')) + "\\";
            string curBaseDir = _appPath;
            Assert.AreEqual(calculatedBaseDir.ToLower(), curBaseDir.ToLower(), "wrong physical directory");
            // we're on the right virtual path
            Assert.AreEqual( _configuredVirtualPath, _appVirtualPath, "virtual path is wrong" );
        }
    }
}