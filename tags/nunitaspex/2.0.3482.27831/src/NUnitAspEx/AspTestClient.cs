using System;
using NUnitAspEx.Client;

namespace NUnitAspEx
{
	/// <summary>
	/// Summary description for AspTestClient.
	/// </summary>
	internal class AspTestClient : HttpWebClient
	{
		public AspTestClient() : base("asptest://aspfixturehost")
		{}
	}
}
