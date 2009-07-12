using System;
using System.Web;
using System.Web.SessionState;
using NUnitAspEx.Core;

namespace NUnitAspEx
{
	/// <summary>
	/// Summary description for AspTestExecutionModule.
	/// </summary>
	public class AspTestExecutionModule : IHttpModule
	{
	    private static bool _isActive;
	    private static HttpContext _httpContext;
        private static HttpSessionState _httpSession;

	    internal static bool IsActive
	    {
	        get { return _isActive; }
	    }
	    
	    internal static HttpContext HttpContext
	    {
	        get
	        {
				if (_httpContext != null)
				{
					_httpContext.Items["AspSession"] = _httpSession;
				}
	            return _httpContext;
	        }
	    }

	    internal static void Clear()
	    {
	        _httpContext = null;    
	        _httpSession = null;
	    }
	    
	    public AspTestExecutionModule()
		{
		}

        public void Dispose()
        {
        }
	    
	    public void Init(HttpApplication context)
	    {
	        context.PostRequestHandlerExecute += new EventHandler(context_PostRequestHandlerExecute);
	    }

        private void context_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            // test execution has been handled by AspTestMethodHandler?
            if (!(HttpContext.Current.Handler is AspTestMethodHandler))
            {
                AspTestMethod _currentTest = AspTestMethod.Current;            
                if (_currentTest != null)
                {
                    _isActive = true;
                    try
                    {
                        _currentTest.DoRunFromWithinWorkerRequest();
                    }
                    finally
                    {
                        _isActive = false;
                    }
                }
            }

            // store for later use in Testmethod
			_httpContext = ((HttpApplication)sender).Context; 
			_httpSession = _httpContext.Session;
		}
	}
}
