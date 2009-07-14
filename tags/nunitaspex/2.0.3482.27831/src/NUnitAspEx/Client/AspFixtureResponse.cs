using System;
using System.IO;
using System.Net;

namespace NUnitAspEx.Client
{
    /// <summary>
    /// Encapsulates a response from an <see cref="AspFixtureRequest" />.
    /// </summary>
    internal class AspFixtureResponse : CustomizableHttpWebResponse
    {
        private Stream _responseStream;

        public AspFixtureResponse(Uri responseUri, string verb, WebHeaderCollection httpResponseHeaders, Version version, HttpStatusCode statusCode, string statusDescription, string mediaType, long contentLength, bool usesProxySemantics, Stream responseStream) : base(responseUri, verb, httpResponseHeaders, version, statusCode, statusDescription, mediaType, contentLength, usesProxySemantics)
        {
            this._responseStream = responseStream;
        }

        public override Stream GetResponseStream()
        {
            return this._responseStream;
        }
    }
}