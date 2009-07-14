using System;
using System.Net;
using NUnitAspEx.Client;

namespace NUnitAspEx
{
    /// <summary>
	/// Summary description for IAspFixtureHost.
	/// </summary>
	public interface IAspFixtureHost: IDisposable
	{
        string RootLocation { get; }
	    AppDomain CreatorDomain { get; }
	    void Execute(TestAction testAction);
        HttpWebRequest CreateWebRequest(string virtualPath);
        HttpWebClient CreateWebClient();
	}
}
