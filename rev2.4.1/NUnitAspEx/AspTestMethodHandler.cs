using System;
using System.Web;
using System.Web.SessionState;
using NUnitAspEx.Core;

namespace NUnitAspEx
{
	/// <summary>
	/// Executes current TestMethod during ProcessRequest(), if any
	/// </summary>
	public class AspTestMethodHandler : IHttpHandler, IRequiresSessionState
	{
	    private static readonly string KEY_ISACTIVE = typeof(AspTestMethodHandler).FullName + ".IsActive";

	    internal static bool IsActive
	    {
	        get
	        {
	            return (HttpContext.Current.Items[KEY_ISACTIVE] != null);
	        }
	    }
	    
	    public AspTestMethodHandler()
		{
		}

	    public void ProcessRequest(HttpContext context)
	    {
	        context.Items[KEY_ISACTIVE] = true;
	        try
	        {
	            AspTestMethod currentTest = AspTestMethod.Current;
	            if (currentTest != null)
	            {
	                currentTest.DoRunFromWithinWorkerRequest();
	            }
	        }
	        finally
	        {
                context.Items.Remove(KEY_ISACTIVE);
	        }
	        return;
	    }

	    public bool IsReusable
	    {
	        get { return true; }
	    }
	}
}
