using System;

namespace NUnitAspEx
{
	/// <summary>
	/// Summary description for IAspFixtureHost.
	/// </summary>
	public interface IAspFixtureHost
	{
        string RootLocation { get; }
	    AppDomain CreatorDomain { get; }
	}
}
