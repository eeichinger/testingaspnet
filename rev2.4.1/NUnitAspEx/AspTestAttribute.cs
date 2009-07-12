using System;

namespace NUnitAspEx
{
    /// <summary>
    /// Summary description for AspTestAttribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=false)]
    public class AspTestAttribute : Attribute
    {
        private string _appVirtualDir;
        private string _appRelativePhysicalDir;
        private string _page;
        private string _query;
        private bool _useHttpRuntime = false;
        private bool _suppressOutput = true;

        public AspTestAttribute(string page) 
            : this( null, null, page, null )
        {
        }
        
        public AspTestAttribute(string page, string query) 
            : this( null, null, page, query )
        {
        }

        public AspTestAttribute(string appVirtualDir, string appRelativePhysicalDir, string page, string query)
        {
            this.AppVirtualDir = appVirtualDir;
            this.AppRelativePhysicalDir = appRelativePhysicalDir;
            this.Page = page;
            this.Query = query;
        }

        public string AppVirtualDir
        {
            get { return _appVirtualDir; }
            set { _appVirtualDir = value; }
        }

        public string AppRelativePhysicalDir
        {
            get { return _appRelativePhysicalDir; }
            set
            {
                _appRelativePhysicalDir = (value != null) ? value.TrimStart('\\','/','~') : "";
                _appRelativePhysicalDir = _appRelativePhysicalDir.Replace('/','\\');
            }
        }

        public string Page
        {
            get { return _page; }
            set
            {
                if (value != null)
                {
                    _page = value.TrimStart('/','\\','~');
                }
            }
        }

        public string Query
        {
            get { return _query; }
            set
            {
                _query = (value != null) ? value : string.Empty;
            }
        }

        public bool UseHttpRuntime
        {
            get { return _useHttpRuntime; }
            set { _useHttpRuntime = value; }
        }

        public bool SuppressOutput
        {
            get { return _suppressOutput; }
            set { _suppressOutput = value; }
        }
    }
}