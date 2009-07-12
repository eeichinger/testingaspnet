using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;

namespace NUnitAspEx.Client
{
	/// <summary>
	/// Encapsulates the ugly details of deriving from HttpWebResponse.
	/// </summary>
	public class CustomizableHttpWebResponse : HttpWebResponse
	{
		public CustomizableHttpWebResponse( Uri responseUri
		                           , string verb
	                               , WebHeaderCollection httpResponseHeaders
	                               , Version version
		                           , HttpStatusCode statusCode
		                           , string statusDescription
		                           , string mediaType
		                           , long contentLength
		                           , bool usesProxySemantics
		    )
	        : base( CreateHttpWebResponseSerializationInfo(responseUri
	                                                                           , verb
	                                                                           , httpResponseHeaders
	                                                                           , version
	                                                                           , statusCode
	                                                                           , statusDescription
	                                                                           , mediaType
	                                                                           , contentLength
	                                                                           , usesProxySemantics), new StreamingContext() )
		{	
		}
	    
        #region Serialization/Construction Support

        private static SerializationInfo CreateHttpWebResponseSerializationInfo( 
            Uri uri
            , string verb
            , WebHeaderCollection httpResponseHeaders
            , Version version
            , HttpStatusCode statusCode
            , string statusDescription
            , string mediaType
            , long contentLength
            , bool usesProxySemantics
            )
        {
	        
            string text1 = httpResponseHeaders["Content-Location"];
            if (text1 != null)
            {
                try
                {
                    uri = new Uri(uri, text1);
                }
                catch (Exception)
                {
                }
            }
	        
            SerializationInfo si = new SerializationInfo(typeof(CustomizableHttpWebResponse), new FormatterConverter());
            si.AddValue( "m_HttpResponseHeaders", httpResponseHeaders, typeof(WebHeaderCollection) );
            si.AddValue( "m_Uri", uri, typeof(Uri) );
            si.AddValue( "m_Certificate", null, typeof(X509Certificate) );
            si.AddValue( "m_Version", version, typeof(Version) );
            si.AddValue( "m_StatusCode", statusCode, typeof(int) );
            si.AddValue( "m_ContentLength", contentLength, typeof(long) );
            si.AddValue( "m_Verb", verb, typeof(string) );
            si.AddValue( "m_StatusDescription", statusDescription, typeof(string) );
            si.AddValue( "m_MediaType", mediaType, typeof(string) );
            si.AddValue( "m_ContentType", null, typeof(string) );
            si.AddValue( "m_GotContentType", false, typeof(bool) );
	        
            return si;
        }
	    	    
        #endregion
	}
}
