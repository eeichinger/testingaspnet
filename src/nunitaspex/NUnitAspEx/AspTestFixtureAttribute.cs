using System;
using System.Diagnostics;
using System.Net;
using NUnit.Core;
using NUnitAspEx.Client;
using NUnitAspEx.Core;

namespace NUnitAspEx
{
    /// <summary>
    /// AspTestFixtureAttribute is used to identify a AspTestFixture class
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public sealed class AspTestFixtureAttribute : Attribute
    {
        private string _relativePhysicalPath;
        private string _virtualPath;
        
        static AspTestFixtureAttribute()
        {
            try
            {
                Addins.Register( new AspTestFixtureBuilder() );
				Addins.Register( new AspTestCaseBuilder() );

                if (AspFixtureHost.Current != null)
                {
                    // inside host, redirect all "http"-protocol requests
					IWebRequestCreate factory = AspFixtureRequest.Factory;
					WebRequest.RegisterPrefix("http", factory);
                    WebRequest.RegisterPrefix("asptest", factory);
                    Trace.WriteLine("Registered asptest prefix");
                }            
            }
            catch(Exception ex)
            {
                Trace.WriteLine("Failed registering asptest prefix: "+ex);
            }
        }

        public AspTestFixtureAttribute() : this("/", "~/")
        {
        }

        public AspTestFixtureAttribute(string virtualPath, string relativePhysicalPath)
        {
            this.VirtualPath = virtualPath;
            this.RelativePhysicalPath = relativePhysicalPath;
        }

        public string RelativePhysicalPath
        {
            get { return _relativePhysicalPath; }
            set
            {
                _relativePhysicalPath = (value != null) ? value.TrimStart('\\','/','~') : "";
                _relativePhysicalPath = _relativePhysicalPath.Replace('/','\\');
            }
        }

        public string VirtualPath
        {
            get { return _virtualPath; }
            set
            {
                _virtualPath = (value != null) ? value.TrimStart('\\','/','~') : "";
                _virtualPath = "/" + _virtualPath.Replace('\\','/');
            }
        }
    }
}