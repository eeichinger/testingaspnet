using System;
using System.IO;
using System.Net;
using NUnitAspEx.Core;

namespace NUnitAspEx.Client
{
    /// <summary>
    /// Encapsulates a request from within a testsuite using the "asptest://" pseudo-protocol.
    /// </summary>
    internal class AspFixtureRequest : CustomizableHttpWebRequest
    {
        #region IWebRequestCreate Factory Implementation

        public class AspFixtureRequestFactory : IWebRequestCreate
        {
            public AspFixtureHost Host;

            public WebRequest Create(Uri uri)
            {
                return new AspFixtureRequest(Host, uri);
            }
        }

        #endregion IWebRequestCreate Factory Implementation

        private static readonly AspFixtureRequestFactory s_factory;

        static AspFixtureRequest()
        {
            s_factory = new AspFixtureRequestFactory();
        }

        public static AspFixtureRequestFactory Factory
        {
            get { return s_factory; }
        }

        private readonly AspFixtureHost _host;

        private AspFixtureRequest(AspFixtureHost host, Uri uri) : base( uri )
        {
            _host = host;
        }
        
        protected override void SubmitRequest(byte[] headerBytes, Stream outputStream)
        {
            // no need to send headerdata for a fixture-request, since we pass ourself to the WorkerRequest

            // "outputStream" is the stream created by CreateOutputStream() (we know it's a memory stream)           
            byte[] bodyPayload = ((MemoryStream)outputStream).ToArray();

            // execute request against current fixture host
            AspFixtureRequestWorkerRequest.ResponseData rp = _host.ProcessAspFixtureRequest( this, bodyPayload );
            
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