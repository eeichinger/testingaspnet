using System.Diagnostics;
using NUnit.Framework;
using NUnitAspEx;
using NUnitAspEx.Client;
using NUnitAspEx.Core;

namespace NUnitAspExTests
{
    [TestFixture]
    public class AspTestClientTests
    {
        private IAspFixtureHost host;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            host = AspFixtureHost.CreateInstance("/", "/TestData/Test1", this);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            host.Dispose();
        }

        [Test]
        public void ClientExecutesPageInHost()
        {
            // use static method to avoid passing a reference to ourselves to the host domain
            host.Execute(new TestAction(ClientExecutesPageInHostImpl));
        }

        // this code is executed entirely within the web application domain
        private static void ClientExecutesPageInHostImpl()
        {
            IAspFixtureHost host = AspTestContext.Host;
            Assert.IsNotNull(host);

            HttpWebClient clnt = host.CreateWebClient();

            string content = clnt.GetPage("/testpage.aspx").Trim();
            Trace.WriteLine(content);
            Assert.AreEqual("Hi, I'm the testpage!", content);
            // session value is set by page
            Assert.AreEqual("testvalue", AspTestContext.HttpContext.Session["testkey"]);

            content = clnt.GetPage("/testpage.aspx?testparam=testvalue").Trim();
            // request parameters are evaluated?
            Assert.AreEqual("testparam=testvalue", content);
            // session is maintained during requests?            
            Assert.AreEqual("testvalue", AspTestContext.HttpContext.Session["testkey"]);
        }

        [Test]
        public void ClientExecutesPageFromOutsideOfHost()
        {
            // use static method to avoid passing a reference to ourselves to the host domain
            HttpWebClient clnt = host.CreateWebClient();
            string content = clnt.GetPage("/testpage.aspx").Trim();
            Trace.WriteLine(content);
            Assert.AreEqual("Hi, I'm the testpage!", content);
        }
    }
}