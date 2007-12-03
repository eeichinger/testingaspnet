using System.Diagnostics;
using NUnit.Framework;
using NUnitAspEx;

namespace NUnitAspExTests
{
    [AspTestFixture(VirtualPath = "/Test1", RelativePhysicalPath = "/TestWebs/Test1")]    
    public class AspTestClientTests
    {
        [Test]
        public void ClientExecutesPageInHost()
        {
            Assert.IsNotNull( AspTestContext.Host );

            AspTestClient clnt = new AspTestClient();
            
            string content = clnt.GetPage( "/testpage.aspx" ).Trim();
			Trace.WriteLine(content);
            Assert.AreEqual( "Hi, I'm the testpage!", content );
            // session value is set by page
            Assert.AreEqual( "testvalue", AspTestContext.HttpContext.Session["testkey"] );

            content = clnt.GetPage( "/testpage.aspx?testparam=testvalue" ).Trim();
            // request parameters are evaluated?
            Assert.AreEqual( "testparam=testvalue", content );
            // session is maintained during requests?            
            Assert.AreEqual( "testvalue", AspTestContext.HttpContext.Session["testkey"] );            
        }
    }
}