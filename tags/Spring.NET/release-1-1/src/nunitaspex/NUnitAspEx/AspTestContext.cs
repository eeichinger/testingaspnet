using System;
using System.Web;
using NUnitAspEx.Core;

namespace NUnitAspEx
{
	/// <summary>
	/// Summary description for AspTestContext.
	/// </summary>
	public sealed class AspTestContext
	{
		private AspTestContext()
		{
		}
	    
	    public static IAspFixtureHost Host
	    {
    	    get
    	    {
    	        return AspFixtureHost.Current;
    	    }    
	    }
	    
	    public static HttpContext HttpContext
	    {
	        get
	        {
	            return AspTestExecutionModule.HttpContext;
	        }
	    }
	    
	    public static bool IsInTestMethodHandler
	    {
	        get
	        {
	            return AspTestMethodHandler.IsActive;
	        }
	    }
	    
	    public static bool IsInTestModule
	    {
	        get
	        {
	            return AspTestExecutionModule.IsActive;
	        }
	    }
	}
}
