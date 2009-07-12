using System;
using System.IO;
using System.Net;
using NUnitAspEx.Core;

namespace NUnitAspEx.Client
{
    /// <summary>
    /// Encapsulates a request from within a testsuite marked as <see cref="AspTestFixtureAttribute" /> using the "asptest://" pseudo-protocol.
    /// </summary>
    internal class AspFixtureRequest : CustomizableHttpWebRequest
    {
        #region IWebRequestCreate Factory Implementation

        private class AspFixtureRequestFactory : IWebRequestCreate
        {
            public WebRequest Create(Uri uri)
            {
                return new AspFixtureRequest(uri);
            }
        }

        private static readonly IWebRequestCreate s_factory = new AspFixtureRequestFactory();
        public static IWebRequestCreate Factory
        {
            get { return s_factory; }
        }

        #endregion IWebRequestCreate Factory Implementation

        public AspFixtureRequest(Uri uri) : base( uri )
        {}
        
        protected override void SubmitRequest(byte[] headerBytes, Stream outputStream)
        {
            // no need to send headerdata for a fixture-request, since we pass ourself to the WorkerRequest

            // "outputStream" is the stream created by CreateOutputStream() (we know it's a memory stream)           
            byte[] bodyPayload = ((MemoryStream)outputStream).ToArray();

            // execute request
            AspFixtureHost host = AspFixtureHost.Current;
            AspTestClientRequest.ResponseData rp = host.ProcessAspFixtureRequest( this, bodyPayload );
            
            HttpWebResponse response = new AspFixtureResponse( this.Uri, this.Verb, rp.Headers, rp.Version, rp.StatusCode, rp.StatusDescription, this.MediaType, rp.ContentLength, false, rp.Stream );
            
            // set response object here
            SetResponse( response );
        }

        protected override Stream CreateOutputStream()
        {
            Stream stm = new MemoryStream();
            return stm;
        }        
    }
}