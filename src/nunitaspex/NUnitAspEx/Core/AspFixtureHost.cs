using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using NUnit.Core;
using NUnit.Core.Builders;
using NUnitAspEx.Client;

namespace NUnitAspEx.Core
{
    /// <summary>
    /// An AspFixtureHost lives inside it's own AppDomain with an initialized HttpRuntime.
    /// </summary>
    internal class AspFixtureHost : MarshalByRefObject, IAspFixtureHost
    {
        #region Static Instance Lifetime Handling
        
        private static AspFixtureHost s_current = null;

        /// <summary>
        /// Use this property to gain access to the Host from within the Host's AppDomain
        /// </summary>
        /// <remarks>
        /// This property is set only within the Host's AppDomain. 
        /// Outside the AppDomain it will always return null.
        /// </remarks>
        public static AspFixtureHost Current
        {
            get { return s_current; }
        }

        /// <summary>
        /// Creates a new Host instance within a new AppDomain based on the Properties of the passed <see cref="AspTestFixtureAttribute"/>.
        /// </summary>
        /// <remarks>
        /// You must not call this method from within an existing Host's AppDomain.
        /// </remarks>
        internal static AspFixtureHost CreateInstance( AspTestFixtureAttribute att, string rootLocation )
        {
            if (AspFixtureHost.Current != null)
            {
                throw new InvalidOperationException("Cannot create a new Host within an existing host");
            }
            
            string currentDir = new FileInfo( new Uri(rootLocation).LocalPath ).DirectoryName; //AppDomain.CurrentDomain.BaseDirectory;
            
            // setup up target directory
            string physicalHostDir = Path.Combine( currentDir.TrimEnd('\\','/') + "\\", att.RelativePhysicalPath.Trim('\\','/')) + "\\";
            physicalHostDir = new DirectoryInfo(physicalHostDir).FullName;
            string physicalHostBinDir = Path.Combine( physicalHostDir, "bin\\");

            // copy all files from current build output directory to <webroot>/bin
            Directory.CreateDirectory( physicalHostBinDir );
            foreach(string file in Directory.GetFiles(currentDir,"*.*"))
            {
                string newFile = Path.Combine(physicalHostBinDir,Path.GetFileName(file));
                if (File.Exists(newFile)){File.Delete(newFile);}
                File.Copy(file,newFile);
            }

            // finally create & initialize Web Application Host instance
            AspFixtureHost _host = (AspFixtureHost)System.Web.Hosting.ApplicationHost.CreateApplicationHost( typeof(AspFixtureHost), "/"+att.VirtualPath.Trim('/'), physicalHostDir );
            _host.Initialize( currentDir, AppDomain.CurrentDomain, Console.Out);
            return _host;
        }

        /// <summary>
        /// Unload the passed Host's AppDomain.
        /// </summary>
        internal static void ShutDown(AspFixtureHost host)
        {
            if (AspFixtureHost.Current != null)
            {
                throw new InvalidOperationException("Cannot shutdown the Host from within the Host's AppDomain");
            }

            try
            {
				s_current = null;
				host.ShutDown();
				AppDomain hostDomain = host.HostDomain;
				AppDomain.Unload(hostDomain);
			}
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }        
        
        private void ShutDown()
        {
            try
            {
                lock(SyncRoot)
                {
					Trace.WriteLine("shutting down host");
					s_current = null;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }                        
        }

        #endregion Static Instance Lifetime Handling
        
        #region Instance Properties
        
        private object _syncRoot = new object();
        private string _rootLocation;
        private AppDomain _creatorDomain;

        /// <summary>
        /// Use to synchronize access to this host instance
        /// </summary>
        public object SyncRoot
        {
            get { return this._syncRoot; }
        }

        /// <summary>
        /// Returns a reference to the AppDomain, that created this Host instance
        /// </summary>
        public AppDomain CreatorDomain
        {
            get { return _creatorDomain; }
        }

        /// <summary>
        /// Returns the physical assembly reference location this AppDomain has been created from.
        /// </summary>
        public string RootLocation
        {
            get { return this._rootLocation; }
        }

        private AppDomain HostDomain
        {
            get { return AppDomain.CurrentDomain; }
        }

        #endregion Instance Properties
        
        #region Lifecycle Methods

        public AspFixtureHost()
        {
            bool bRes = WebRequest.RegisterPrefix("asptest", AspFixtureRequest.Factory);                        
        }

        /// <summary>
        /// Return null to avoid timing out of remoting references
        /// </summary>
        /// <returns></returns>
        public override Object InitializeLifetimeService()
        {
            // make the Remoting-Proxy live forever
            return null;
        }
        
        /// <summary>
        /// Initializes the newly created host instance.
        /// </summary>
        private void Initialize( string rootLocation, AppDomain creatorDomain,  TextWriter cout )
        {
            if (AspFixtureHost.Current != null)
            {
                throw new InvalidOperationException("Cannot initialize the Host from within the Host's AppDomain");
            }
            
            // remember rootLocation
            _rootLocation = rootLocation;
            // remember creating AppDomain
            _creatorDomain = creatorDomain;
            // redirect cout to the passed TextWriter
            Console.SetOut( cout );
            // Make this Host instance available through AspFixtureHost.Current
            s_current = this;

            // force HttpRuntime initialization
            StringWriter sw = new StringWriter();
            SimpleWorkerRequest wr = new AspFixtureWorkerRequest(string.Empty, string.Empty, sw);
            HttpRuntime.ProcessRequest(wr);
            //Console.WriteLine(sw.ToString());
        }

        #endregion Lifecycle Methods
        
        #region Test Execution Methods
        
        internal AspTestClientRequest.ResponseData ProcessAspFixtureRequest( AspFixtureRequest request, byte[] requestBodyBytes )
        {
            try
            {
                AspTestClientRequest wr = new AspTestClientRequest( request, requestBodyBytes );    
//            // Impersonate current user
//            WindowsIdentity curId = WindowsIdentity.GetCurrent();
//            WindowsImpersonationContext ctx = curId.Impersonate();
                try
                {
                    HttpRuntime.ProcessRequest( wr );
                }
                finally
                {
//                ctx.Undo();
                }
                return wr.GetResponseData();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Failed executing AspTestClientRequest:" + ex);
                throw;
            }
        }
        
        /// <summary>
        /// Creates a new TestSuite using the passed <c>fixtureType</c> and executes it immediately
        /// </summary>
        /// <remarks>
        /// This method is called by <see cref="AspTestFixture.Run(EventListener,IFilter)"/>
        /// </remarks>
        internal TestResult CreateAndExecuteTestSuite( Type fixtureType, int assemblyKey, EventListener listener, IFilter filter )
        {
            lock (SyncRoot)
            {
                if (Current == null)
                {
                    throw new InvalidOperationException("Cannot execute fixture outside the Host's environment");
                }
            
                TestResult testResult;
                TestSuite testSuite = null;
                try
                {
                    NUnitTestFixtureBuilder builder = new NUnitTestFixtureBuilder();
                    testSuite = builder.BuildFrom( fixtureType, assemblyKey );
                    testResult = testSuite.Run( listener, filter );
                }
                catch(Exception ex)
                {
                    // signal suite creation failure - TODO: look for a better way!	            
                    TestResult tr = new TestSuiteResult( testSuite, "creating testsuite" );
                    tr.Failure("failed creating testsuite", ex.ToString());
                    testResult = tr;
                }
                return testResult;
            }
        }
        
        #endregion Test Execution Methods        
    }
}
