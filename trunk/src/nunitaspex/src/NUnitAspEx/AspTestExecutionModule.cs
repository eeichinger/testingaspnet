using System;
using System.Web;
using System.Web.SessionState;

namespace NUnitAspEx
{
    /// <summary>
    /// Summary description for AspTestExecutionModule.
    /// </summary>
    public class AspTestExecutionModule : IHttpModule
    {
        private static HttpContext _httpContext;
        private static HttpSessionState _httpSession;

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
        {}

        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(context_BeginRequest);
            context.PostRequestHandlerExecute += new EventHandler(context_PostRequestHandlerExecute);
        }

        public void Dispose()
        {}

        private void context_BeginRequest(object sender, EventArgs e)
        {
            Clear();
        }

        private void context_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            // store for later use in Testmethod
            _httpContext = ((HttpApplication)sender).Context;
            _httpSession = _httpContext.Session;
        }
    }
}
