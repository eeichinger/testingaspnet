using System;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Hosting;
using NUnitAspEx.Client;

namespace NUnitAspEx.Core
{
    internal class AspFixtureRequestWorkerRequest : SimpleWorkerRequest
    {
        internal class ResponseData
        {
            public bool HeadersSent;
            public HttpStatusCode StatusCode;
            public string StatusDescription;
            public WebHeaderCollection Headers = new WebHeaderCollection();
            public long ContentLength;
            public MemoryStream Stream = new MemoryStream();
            public Version Version = new Version(1,1);
        }
        
        private readonly AspFixtureRequest _clientRequest;
        private readonly byte[] _requestBodyBytes;        
        private bool _specialCaseStaticFileHeaders;

        private readonly ResponseData _responseData = new ResponseData();
        
        public AspFixtureRequestWorkerRequest(AspFixtureRequest clientRequest, byte[] requestBodyBytes) : base( clientRequest.RequestUri.AbsolutePath.TrimStart('/'), clientRequest.RequestUri.Query.TrimStart('?'), null)
        {
            _clientRequest = clientRequest;
            _requestBodyBytes = requestBodyBytes;
        }

        internal ResponseData GetResponseData()
        {
            _responseData.Stream.Position = 0;
            return _responseData;
        }
        ///////////////////////////////////////////////////////////////////////////////////////////////
        //
        // Implementation of HttpWorkerRequest
        //
        ///////////////////////////////////////////////////////////////////////////////////////////////

//        public override String GetUriPath()
//        {
//          return null;            
//        }

//        public override String GetQueryString()
//        {
//            return _queryString;
//        }
//
//        public override byte[] GetQueryStringRawBytes()
//        {
//            return _queryStringBytes;
//        }

//        public override String GetRawUrl()
//        {
//            return _url;
//        }

        public override String GetHttpVerbName()
        {
            return _clientRequest.Method;
        }

        public override String GetHttpVersion()
        {
            return _clientRequest.ProtocolVersion.ToString();
        }

//        public override String GetRemoteAddress()
//        {
//            return _conn.RemoteIP;
//        }
//
//        public override int GetRemotePort()
//        {
//            return _conn.RemotePort;
//        }

//        public override String GetLocalAddress()
//        {
//            return _conn.LocalIP;
//        }

        public override int GetLocalPort()
        {
            return _clientRequest.RequestUri.Port;
        }

        public override String GetServerName()
        {
            string hostport = _clientRequest.Headers.Get(GetKnownRequestHeaderName(HeaderHost));
            if (hostport != null)
            {
                string[] parts = hostport.Split(':');
                return parts[0];
            }
            return GetLocalAddress();
        }

//        public override String GetFilePath()
//        {
//            return _filePath;
//        }
//
//        public override String GetFilePathTranslated()
//        {
//            return _pathTranslated;
//        }
//
//        public override String GetPathInfo()
//        {
//            return _pathInfo;
//        }

//        public override String GetAppPath()
//        {
//            return _host.VirtualPath;
//        }

//        public override String GetAppPathTranslated()
//        {
//            return _host.PhysicalPath;
//        }

        public override byte[] GetPreloadedEntityBody()
        {
            return _requestBodyBytes;
        }

        public override bool IsEntireEntityBodyIsPreloaded()
        {
            return true;
        }

        public override int ReadEntityBody(byte[] buffer, int size)
        {
            throw new InvalidOperationException("Entity Body already preloaded");
        }

        public override String GetKnownRequestHeader(int index)
        {
            return _clientRequest.Headers.Get( GetKnownRequestHeaderName(index) );
        }

        public override String GetUnknownRequestHeader(String name)
        {
            return _clientRequest.Headers.Get( name );
        }

        public override String GetServerVariable(String name)
        {
            String s = String.Empty;

            switch (name)
            {
                case "ALL_RAW":
                    s = _clientRequest.Headers.ToString();
                    break;
                case "SERVER_PROTOCOL":
                    s = this.GetHttpVerbName();
                    break;
//                case "AUTH_TYPE":
//                    s = "NTLM";
//                    break;
//                case "LOGON_USER":
//                    s = WindowsIdentity.GetCurrent().Name;
//                    break;

                    // more needed?
            }

            return s;
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
        
        public override void SendStatus(int statusCode, String statusDescription)
        {
            _responseData.StatusCode = (HttpStatusCode)statusCode;
            _responseData.StatusDescription = GetStatusDescription( statusCode );
        }

        public override void SendKnownResponseHeader(int index, String value)
        {
            if (_responseData.HeadersSent)
                return;

            switch (index)
            {
                case HeaderServer:
                case HeaderDate:
                case HeaderConnection:
                    // ignore these
                    return;

                    // special case headers for static file responses
                case HeaderAcceptRanges:
                    if (value == "bytes")
                    {
                        _specialCaseStaticFileHeaders = true;
                        return;
                    }
                    break;
                case HeaderExpires:
                case HeaderLastModified:
                    if (_specialCaseStaticFileHeaders)
                        return;
                    break;
            }

            _responseData.Headers.Add( GetKnownResponseHeaderName(index), value );
        }

        public override void SendUnknownResponseHeader(String name, String value)
        {
            if (_responseData.HeadersSent)
                return;

            _responseData.Headers.Add( name, value );
        }

        public override void SendCalculatedContentLength(int contentLength)
        {
            if (!_responseData.HeadersSent)
            {
                _responseData.ContentLength = contentLength;
            }
        }

        public override bool HeadersSent()
        {
            return _responseData.HeadersSent;
        }

        public override bool IsClientConnected()
        {
            return true;
        }

        public override void CloseConnection()
        {
            // noop
        }

        public override void SendResponseFromMemory(byte[] data, int length)
        {
            if (length > 0)
            {
                _responseData.Stream.Write( data, 0, length );
            }
        }

        public override void SendResponseFromFile(String filename, long offset, long length)
        {
            if (length == 0)
                return;

            FileStream f = null;

            try
            {
                f = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                SendResponseFromFileStream(f, offset, length);
            }
            finally
            {
                if (f != null)
                    f.Close();
            }
        }

        public override void SendResponseFromFile(IntPtr handle, long offset, long length)
        {
            if (length == 0)
                return;

            FileStream f = null;

            try
            {
                f = new FileStream(handle, FileAccess.Read, false);
                SendResponseFromFileStream(f, offset, length);
            }
            finally
            {
                if (f != null)
                    f.Close();
            }
        }

        private void SendResponseFromFileStream(FileStream f, long offset, long length)
        {
            const int maxChunkLength = 64*1024;
            long fileSize = f.Length;

            if (length == -1)
                length = fileSize - offset;

            if (length == 0 || offset < 0 || length > fileSize - offset)
                return;

            if (offset > 0)
                f.Seek(offset, SeekOrigin.Begin);

            if (length <= maxChunkLength)
            {
                byte[] fileBytes = new byte[(int) length];
                int bytesRead = f.Read(fileBytes, 0, (int) length);
                SendResponseFromMemory(fileBytes, bytesRead);
            }
            else
            {
                byte[] chunk = new byte[maxChunkLength];
                int bytesRemaining = (int) length;

                while (bytesRemaining > 0)
                {
                    int bytesToRead = (bytesRemaining < maxChunkLength) ? bytesRemaining : maxChunkLength;
                    int bytesRead = f.Read(chunk, 0, bytesToRead);
                    SendResponseFromMemory(chunk, bytesRead);
                    bytesRemaining -= bytesRead;

                    // flush to release keep memory
                    if (bytesRemaining > 0 && bytesRead > 0)
                        FlushResponse(false);
                }
            }
        }

        public override void FlushResponse(bool finalFlush)
        {
            _responseData.HeadersSent = true;
        }

        public override void EndOfRequest()
        {
            // empty method
        }

        public override IntPtr GetUserToken()
        {
            return base.GetUserToken ();
            //           return WindowsIdentity.GetCurrent().Token;
        }

    }
}