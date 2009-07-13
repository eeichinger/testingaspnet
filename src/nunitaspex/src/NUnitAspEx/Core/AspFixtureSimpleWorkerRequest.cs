using System;
using System.IO;
using System.Web;
using System.Web.Hosting;

namespace NUnitAspEx.Core
{
    internal class AspFixtureSimpleWorkerRequest : SimpleWorkerRequest
    {
		private int _statusCode;

        public AspFixtureSimpleWorkerRequest( string page, string query, TextWriter writer ) 
            : base(page, query, writer )
        {
        }

    	public int StatusCode
    	{
    		get { return this._statusCode; }
    	}

    	public override void SendStatus(int statusCode, string statusDescription)
		{
			_statusCode = statusCode;
			base.SendStatus(statusCode, statusDescription);
		}

        public override string MapPath(string virtualPath)
        {
            string path = virtualPath;
            // asking for the site root                
            if (path == null || path.Length == 0 || path.Equals("/"))
            {
                // only, if web is site-rooted
                if (HttpRuntime.AppDomainAppVirtualPath == "/")
                    return HttpRuntime.AppDomainAppPath;
                else
                {
                    // point somewhere else (otherwise duplicate config error!)
                    return Environment.SystemDirectory;
                }
            }
            
            string res = base.MapPath(virtualPath);
            return res;
        }
    }
}