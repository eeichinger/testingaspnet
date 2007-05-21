using NUnitAspEx.Client;

namespace NUnitAspEx
{
	/// <summary>
	/// Summary description for AspTestClient.
	/// </summary>
	public class AspTestClient : HttpWebClient
	{
		public AspTestClient() : base("asptest://aspfixturehost")
		{
		}
	}
}
