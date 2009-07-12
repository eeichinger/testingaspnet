using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Security;
using System.Text;

namespace NUnitAspEx.Client
{
	/// <summary>
	/// Summary description for HttpWebClient.
	/// </summary>
	public class HttpWebClient
	{
        // Fields
        private const int DefaultCopyBufferLength = 0x2000;
        private const int DefaultDownloadBufferLength = 0x10000;
        private const string DefaultUploadFileContentType = "application/octet-stream";
        private Uri m_baseAddress;
        private ICredentials m_credentials;
        private WebHeaderCollection m_headers = new WebHeaderCollection();
        private NameValueCollection m_requestParameters;
        private WebHeaderCollection m_responseHeaders;
	    private Uri m_responseUri;
	    private CookieContainer m_cookieContainer = new CookieContainer();
	    private const string UploadFileContentType = "multipart/form-data";
        private const string UploadValuesContentType = "application/x-www-form-urlencoded";
	    
        public HttpWebClient( string baseAddress )
        {
            m_baseAddress = new Uri(baseAddress);
        }
	    
        private void CopyHeadersTo(WebRequest request)
        {
            if ((this.m_headers != null) && (request is HttpWebRequest))
            {
                string accept = this.m_headers["Accept"];
                string connection = this.m_headers["Connection"];
                string contentType = this.m_headers["Content-Type"];
                string expect = this.m_headers["Expect"];
                string referer = this.m_headers["Referer"];
                string userAgent = this.m_headers["User-Agent"];
                this.m_headers.Remove("Accept");
                this.m_headers.Remove("Connection");
                this.m_headers.Remove("Content-Type");
                this.m_headers.Remove("Expect");
                this.m_headers.Remove("Referer");
                this.m_headers.Remove("User-Agent");
                request.Headers = this.m_headers;
                if ((accept != null) && (accept.Length > 0))
                {
                    ((HttpWebRequest) request).Accept = accept;
                }
                if ((connection != null) && (connection.Length > 0))
                {
                    ((HttpWebRequest) request).Connection = connection;
                }
                if ((contentType != null) && (contentType.Length > 0))
                {
                    ((HttpWebRequest) request).ContentType = contentType;
                }
                if ((expect != null) && (expect.Length > 0))
                {
                    ((HttpWebRequest) request).Expect = expect;
                }
                if ((referer != null) && (referer.Length > 0))
                {
                    ((HttpWebRequest) request).Referer = referer;
                }
                if ((userAgent != null) && (userAgent.Length > 0))
                {
                    ((HttpWebRequest) request).UserAgent = userAgent;
                }
            }
        }	
	    
        private Uri GetUri(string path)
        {
            try
            {
                Uri uri1;
                if (this.m_baseAddress != null)
                {
                    uri1 = new Uri(this.m_baseAddress, path);
                }
                else
                {
                    uri1 = new Uri(path);
                }
                if (this.m_requestParameters == null)
                {
                    return uri1;
                }
                StringBuilder builder1 = new StringBuilder();
                string text1 = string.Empty;
                for (int num1 = 0; num1 < this.m_requestParameters.Count; num1++)
                {
                    builder1.Append(text1 + this.m_requestParameters.AllKeys[num1] + "=" + this.m_requestParameters[num1]);
                    text1 = "&";
                }
                UriBuilder builder2 = new UriBuilder(uri1);
                builder2.Query = builder1.ToString();
                return builder2.Uri;
            }
            catch (UriFormatException)
            {
                return new Uri(Path.GetFullPath(path));
            }
        }
	    
        public string BaseAddress
        {
            get
            {
                if (this.m_baseAddress != null)
                {
                    return this.m_baseAddress.ToString();
                }
                return string.Empty;
            }
            set
            {
                if ((value == null) || (value.Length == 0))
                {
                    this.m_baseAddress = null;
                }
                else
                {
                    try
                    {
                        this.m_baseAddress = new Uri(value);
                    }
                    catch (Exception exception1)
                    {
                        throw new ArgumentException("value", exception1);
                    }
                }
            }
        }
	    
        public WebHeaderCollection Headers
        {
            get
            {
                if (this.m_headers == null)
                {
                    this.m_headers = new WebHeaderCollection();
                }
                return this.m_headers;
            }
            set
            {
                this.m_headers = value;
            }
        }
	    
        public NameValueCollection QueryString
        {
            get
            {
                if (this.m_requestParameters == null)
                {
                    this.m_requestParameters = new NameValueCollection();
                }
                return this.m_requestParameters;
            }
            set
            {
                this.m_requestParameters = value;
            }
        }
 	    
        public CookieContainer CookieContainer
        {
            get
            {
                return this.m_cookieContainer;
            }
            set
            {
                this.m_cookieContainer = value;
            }
        }
	    
        public ICredentials Credentials
        {
            get
            {
                return this.m_credentials;
            }
            set
            {
                this.m_credentials = value;
            }
        }
	    
        public WebHeaderCollection ResponseHeaders
        {
            get
            {
                return this.m_responseHeaders;
            }
        }

	    public Uri ResponseUri
	    {
	        get { return m_responseUri; }
	    }

	    public string GetPage( string address )
	    {
	        HttpWebResponse resp = GetResponse( address, "text/html" );
	        string charset = resp.CharacterSet;
	        
	        Encoding enc = null;
	        if (charset != null && charset.Length > 0)
	        {
	            try
	            {
	                enc = Encoding.GetEncoding(charset);
	            }
	            catch
	            {
	            }
	        }
	        
	        StreamReader sr;
	        if (enc != null)
	        {
	            sr = new StreamReader( resp.GetResponseStream(), enc );
	        }
	        else
	        {
	            sr = new StreamReader( resp.GetResponseStream(), true );
	        }

	        using(sr)
	        {
	            return sr.ReadToEnd();
	        }
	    }
	    
	    public HttpWebResponse GetResponse(string address, string mediaType)
        {
            try
            {
                this.m_responseHeaders = null;
                Uri requestUri = this.GetUri(address);
                HttpWebRequest request1 = (HttpWebRequest)WebRequest.Create(requestUri);
                request1.MediaType = mediaType;
                request1.Credentials = this.Credentials;
                request1.CookieContainer = this.m_cookieContainer;
                this.CopyHeadersTo(request1);
                HttpWebResponse response1 = (HttpWebResponse)request1.GetResponse();                
                if (this.m_headers != null)
                {
                    m_headers["Referer"] = requestUri.ToString();
                }
                this.m_responseHeaders = response1.Headers;                
                this.m_responseUri = response1.ResponseUri;
                return response1;
            }
            catch (Exception exception1)
            {
                if (!(exception1 is WebException) && !(exception1 is SecurityException))
                {
                    throw new WebException(SR.GetString("net_webclient"), exception1);
                }
                throw;
            }
        }	    
	}
}
