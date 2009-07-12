using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;

namespace NUnitAspEx.Client
{
    /// <summary>
    /// Encapsulates the ugly details of deriving from HttpWebRequest.
    /// </summary>
    /// <remarks>
    /// Since various frameworks like NUnitAsp rely on the output of WebRequest.Create() being of type HttpWebRequest
    /// but HttpWebRequest is not customizable the easy way, this class shields further derived classes from the ugly reflection details.
    /// </remarks>
    public abstract class CustomizableHttpWebRequest : HttpWebRequest
    {
        private Stream _outputStream;
        
        #region Construction
        
        /// <summary>
        /// Creates a new instance for the given uri.
        /// </summary>
        /// <param name="uri">the <see cref="Uri"/> to create this request for.</param>
        public CustomizableHttpWebRequest(Uri uri)
            : base(CreateHttpWebRequestSerializationInfo(uri), new StreamingContext())
        {
            this.Uri = uri;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private static SerializationInfo CreateHttpWebRequestSerializationInfo(Uri uri)
        {
            SerializationInfo si = new SerializationInfo(typeof(CustomizableHttpWebRequest), new FormatterConverter());
            si.AddValue("_HttpRequestHeaders", new WebHeaderCollection(), typeof(WebHeaderCollection));
            si.AddValue("_Proxy", GlobalProxySelection.Select, typeof(IWebProxy));
            si.AddValue("_KeepAlive", true, typeof(bool));
            si.AddValue("_Pipelined", true, typeof(bool));
            si.AddValue("_AllowAutoRedirect", true, typeof(bool));
            si.AddValue("_AllowWriteStreamBuffering", true, typeof(bool));
            si.AddValue("_HttpWriteMode", 0, typeof(HttpWebRequest).Assembly.GetType("System.Net.HttpWriteMode"));

            si.AddValue("_MaximumAllowedRedirections", 50, typeof(int));
            si.AddValue("_AutoRedirects", 0, typeof(int));
            si.AddValue("_Timeout", 0x186a0, typeof(int));
            si.AddValue("_ReadWriteTimeout", 0x493e0, typeof(int));
            si.AddValue("_MaximumResponseHeadersLength", HttpWebRequest.DefaultMaximumResponseHeadersLength, typeof(int));
            si.AddValue("_ContentLength", -1, typeof(int));
            si.AddValue("_MediaType", null, typeof(string));
            si.AddValue("_OriginVerb", "GET", typeof(string));
            si.AddValue("_ConnectionGroupName", null, typeof(string));
            si.AddValue("_Version", HttpVersion.Version11, typeof(HttpVersion));
            si.AddValue("_OriginUri", uri, typeof(Uri));

            return si;
        }

        #endregion Construction

        #region Reflection Helpers

        private static Type GetBaseType()
        {
            return typeof(HttpWebRequest);
        }

        private object GetProperty(string name)
        {
            return GetBaseType().GetProperty(name, BindingFlags.Instance|BindingFlags.NonPublic).GetValue(this, null);
        }

        private void SetProperty(string name, object value)
        {
            GetBaseType()
                .GetProperty(name, BindingFlags.Instance|BindingFlags.NonPublic)
                .SetValue(this, value, null);
        }

        private object GetField(string name)
        {
            return GetField(name, null);
        }

        private object GetField(string name, object defaultValue)
        {
            FieldInfo field = GetBaseType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null && defaultValue != null)
            {
                return defaultValue;
            }
            else
            {
                return field.GetValue(this);
            }
        }

        private void SetField(string name, object value)
        {
            SetField(name, value, false);
        }

        private void SetField(string name, object value, bool ignoreNotFound)
        {
            FieldInfo field = GetBaseType().GetField(name, BindingFlags.Instance|BindingFlags.NonPublic);
            
            if ( ignoreNotFound && (field == null) ) return;
            
            field.SetValue(this, value);
        }


        private object InvokeMethod( Type targetType, string name, params object[] args )
        {
            return targetType
                .GetMethod( name, BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic )
                .Invoke( null, args );
        }	   

        #endregion Reflection Helpers

        #region Reflected Fields, Properties and Methods
        
        protected byte[] MakeRequestHeaders()
        {
            MethodInfo mi = typeof(HttpWebRequest).GetMethod("MakeRequest", BindingFlags.Instance | BindingFlags.NonPublic);
            if (mi == null)
            {
                // 2.0 requires explicit call to update request cookies
                CookieModule_OnSendingHeaders();
                mi = typeof(HttpWebRequest).GetMethod("SerializeHeaders", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            else
            {
                this.SetField("_Verb", this.GetField("_OriginVerb")); // avoid confusion with different types in 1.1 & 2.0
            }
            mi.Invoke(this, new object[] {});
            return WriteBuffer;
        }
        
        protected bool CanGetRequestStream
        {
            get { return (bool)GetProperty("CanGetRequestStream"); }
        }

        protected bool MissingEntityBodyDelimiter
        {
            get
            {
                return ((this.ContentLength == -1)) && (!this.AllowWriteStreamBuffering && this.KeepAlive);
            }
        }

        protected bool TransferEncodingWithoutChunked
        {
            get
            {
                return (this.WriteMode != HttpWriteMode.Chunked) && (("" + this.TransferEncoding).Trim().Length > 0);
            }
        }

        private bool _outputStreamRetrieved;
        protected bool OutputStreamRetrieved
        {
            get { return (bool)GetField("_WriteStreamRetrieved", _outputStreamRetrieved ); }
            set { SetField("_WriteStreamRetrieved", value, true); _outputStreamRetrieved = value; }
        }

        private bool _requestSubmitted;
        protected bool RequestSubmitted
        {
            get { return (bool)GetField("_RequestSubmitted", _requestSubmitted); }
            set { SetField("_RequestSubmitted", value, true); _requestSubmitted = value; }
        }

        protected HttpWebResponse HttpResponse
        {
            get { return (HttpWebResponse)GetField("_HttpResponse"); }
            set { SetField("_HttpResponse", value); }
        }
        
        protected Exception ResponseException
        {
            get { return (Exception)GetField("_ResponseException"); }
            set { SetField("_ResponseException", value); }
        }

        protected HttpWriteMode WriteMode
        {
            get { return (HttpWriteMode)((int)GetField("_HttpWriteMode")); }
            set { SetField("_HttpWriteMode", (int)value); }
        }

        protected string Verb
        {
            get { return ""+GetField("_Verb"); }
//            set { SetField("_Verb", value); }
        }

        protected string OriginVerb
        {
            get { return ""+GetField("_OriginVerb"); }
//            set { SetField("_OriginVerb", value); }
        }

        protected Uri Uri
        {
            get { return (Uri)GetField("_Uri"); }
            set { SetField("_Uri", value); }
        }

        protected byte[] WriteBuffer
        {
            get { return (byte[])GetProperty("WriteBuffer"); }
            set { SetProperty("WriteBuffer", value); }
        }

        private bool _haveResponse;
        protected bool HaveResponseInternal
        {
            get { return (bool)GetField("_HaveResponse", _haveResponse); }
            set { SetField("_HaveResponse", value, true); _haveResponse = true; }
        }

        protected bool ChunkedUploadOnHttp10
        {
            get
            {
                return (CanGetRequestStream)
                       && (WriteMode == HttpWriteMode.None)
                       && (this.SendChunked)
                       && (!this.AllowWriteStreamBuffering);
            }
        }

        #endregion Reflected Fields & Properties
        
        protected Stream OutputStream
        {
            get { return _outputStream; }
            set { _outputStream = value; }
        }
        
        public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
        {
            throw new NotSupportedException("async operation not supported");
        }

        public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            throw new NotSupportedException("async operation not supported");
        }

        public override Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            throw new NotSupportedException("async operation not supported");
        }

        public override WebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            throw new NotSupportedException("async operation not supported");
        }

        public override Stream GetRequestStream()
        {
            if(!this.CanGetRequestStream)
            {
                throw new ProtocolViolationException(SR.GetString("net_nouploadonget"));
            }
            if(this.MissingEntityBodyDelimiter)
            {
                throw new ProtocolViolationException(SR.GetString("net_contentlengthmissing"));
            }
            if(this.TransferEncodingWithoutChunked)
            {
                throw new InvalidOperationException(SR.GetString("net_needchunked"));
            }

            EnsureOutputStream();

            if(this.WriteMode == HttpWriteMode.None)
            {
                this.WriteMode = HttpWriteMode.Write;
            }
            
            //this.Verb = this.OriginVerb;
            this.SetField("_Verb", this.GetField("_OriginVerb")); // avoid confusion with different types in 1.1 & 2.0

            return this.OutputStream;
        }

        public override WebResponse GetResponse()
        {
            return this.GetHttpWebResponse();
        }

        public HttpWebResponse GetHttpWebResponse()
        {
            if((this.WriteMode != HttpWriteMode.None)&&!this.CanGetRequestStream)
            {
                throw new ProtocolViolationException(SR.GetString("net_nocontentlengthonget"));
            }
            if(this.MissingEntityBodyDelimiter)
            {
                throw new ProtocolViolationException(SR.GetString("net_contentlengthmissing"));
            }
            if (this.TransferEncodingWithoutChunked)
            {
                throw new InvalidOperationException(SR.GetString("net_needchunked"));
            }

            if(((this.WriteMode != HttpWriteMode.None)&&!this.OutputStreamRetrieved)&&
               ((this.WriteMode == HttpWriteMode.Chunked)||(this.ContentLength > 0)))
            {
                throw new InvalidOperationException(SR.GetString("net_mustwrite"));
            }

            EnsureOutputStream();
            EnsureRequestSubmitted();
            return this.HttpResponse;
        }

        private void EnsureOutputStream()
        {
            if(this._outputStream == null)
            {
                // writes WebHeaderCollection -> WriteBuffer
                MakeRequestHeaders();
                this._outputStream = CreateOutputStream();
            }
            this.OutputStreamRetrieved = true;
        }

        private void EnsureRequestSubmitted()
        {
            if(this.ChunkedUploadOnHttp10)
            {
                throw new ProtocolViolationException(SR.GetString("net_nochunkuploadonhttp10"));
            }

            SubmitRequest( this.WriteBuffer, this.OutputStream );
            this.RequestSubmitted = true;
        }

        protected void SetResponse(HttpWebResponse response)
        {
            this.HttpResponse = response;

            if (this.CookieContainer != null)
            {
                CookieModule_OnReceivedHeaders();                
            }
            
            this.HaveResponseInternal = true;
        }

        private void CookieModule_OnReceivedHeaders()
        {
            Type tCookieModule = typeof(CookieCollection).Assembly.GetType("System.Net.CookieModule");
            InvokeMethod( tCookieModule, "OnReceivedHeaders", this );
        }

        private void CookieModule_OnSendingHeaders()
        {
            Type tCookieModule = typeof(CookieCollection).Assembly.GetType("System.Net.CookieModule");
            InvokeMethod(tCookieModule, "OnSendingHeaders", this);
        }

        /// <summary>
        /// Override this method in your derived class and call <see cref="SetResponse"/> to set the response object.
        /// </summary>
        protected abstract void SubmitRequest(byte[] headerBytes,Stream outputStream);
        /// <summary>
        /// Create the stream to be used for uploading content.
        /// </summary>
        /// <returns></returns>
        protected abstract Stream CreateOutputStream();
    }
}