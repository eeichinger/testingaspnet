using System;

namespace NUnitAspEx
{
    /// <summary>
    /// AspTestFixtureAttribute is used to identify a AspTestFixture class
    /// </summary>
    [AttributeUsage( AttributeTargets.Class,AllowMultiple=false )]
    public sealed class AspTestFixtureAttribute:Attribute
    {
        private string _relativePhysicalPath;
        private string _virtualPath;

//        static AspTestFixtureAttribute()
//        {
//            if (AspFixtureHost.Current != null)
//            {
////                AspFixtureHost.RegisterAspTestPseudoProtocol();
//            }
//        }

        public AspTestFixtureAttribute()
            : this( "/","~/" )
        {
        }

        public AspTestFixtureAttribute(string virtualPath,string relativePhysicalPath)
        {
            this.VirtualPath = virtualPath;
            this.RelativePhysicalPath = relativePhysicalPath;
        }

        public string RelativePhysicalPath
        {
            get { return _relativePhysicalPath; }
            set
            {
                _relativePhysicalPath = (value != null) ? value.TrimStart( '\\','/','~' ) : "";
                _relativePhysicalPath = _relativePhysicalPath.Replace( '/','\\' );
            }
        }

        public string VirtualPath
        {
            get { return _virtualPath; }
            set
            {
                _virtualPath = (value != null) ? value.TrimStart( '\\','/','~' ) : "";
                _virtualPath = "/" + _virtualPath.Replace( '\\','/' );
            }
        }
    }
}