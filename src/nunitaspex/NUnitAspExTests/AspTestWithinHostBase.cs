using System;
using System.Web;
using NUnit.Framework;
using NUnitAspEx;

namespace NUnitAspExTests
{
    public abstract class AspTestWithinHostBase
	{
        [AspTest(null)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PagePropertyRequired()
        {
        }
     
        [AspTest("/dummy.test", UseHttpRuntime=true)]
        public void AllContextPropertiesAreAvailableOnUseHttRuntimeDuringTestRequest()
        {
            AllContextPropertiesAreAvailableOnUseHttpRuntime();
        }

        [AspTest("/testpage.aspx", UseHttpRuntime=true)]
        public void AllContextPropertiesAreAvailableOnUseHttRuntimeDuringPageRequest()
        {
            AllContextPropertiesAreAvailableOnUseHttpRuntime();
        }
        
        private void AllContextPropertiesAreAvailableOnUseHttpRuntime()
        {
            HttpContext ctx = HttpContext.Current;
            Assert.IsNotNull( ctx );
            Assert.IsNotNull( ctx.ApplicationInstance );
            Assert.IsNotNull( ctx.Session );
            Assert.IsNotNull( ctx.Request );
            Assert.IsNotNull( ctx.Response );
        }
        
	    [AspTest("/testpage.aspx", UseHttpRuntime=true)]
        public void IsExecutedByModuleAfterExistingPage()
	    {
	        IHttpHandler handler = HttpContext.Current.Handler;
	        Assert.IsTrue( AspTestContext.IsInTestModule );
	        Assert.AreEqual( "ASP.testpage_aspx", handler.GetType().FullName );
	        Assert.AreEqual( "testvalue", HttpContext.Current.Session["testkey"] );
	    }

        [AspTest("/dummy.test", UseHttpRuntime=true)]
        public void IsExecutedByTestHandlerIfPageExtensionMapsToTestHandler()
        {
            HttpContext ctx = HttpContext.Current;
            IHttpHandler handler = HttpContext.Current.Handler;
            Assert.AreEqual( typeof(AspTestMethodHandler), handler.GetType() );
            Assert.IsTrue(AspTestContext.IsInTestMethodHandler);
        }	    
        
        [AspTest("/somepage.aspx", UseHttpRuntime=false)]
        public void FilePathIsSetByAttribute()
        {
            HttpContext ctx = HttpContext.Current;
            Assert.IsNotNull(ctx);

            // defaults to GET
            Assert.AreEqual( "GET", ctx.Request.HttpMethod );
            
            string filePath = ctx.Request.FilePath;
            string calculatedFilePath = HttpRuntime.AppDomainAppVirtualPath.TrimEnd('/') + "/somepage.aspx";
            Assert.AreEqual( calculatedFilePath, filePath );
            
            Console.WriteLine("dummytest2 called");
        }
	}
}
