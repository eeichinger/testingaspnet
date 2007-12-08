using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using NUnit.Core;
using NUnit.Framework;
using NUnitAspEx.Client;
using TestCase = NUnit.Core.TestCase;

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
        internal static AspFixtureHost CreateInstance(AspTestFixtureAttribute att, string rootLocation)
        {
            if (AspFixtureHost.Current != null)
            {
                throw new InvalidOperationException("Cannot create a new Host within an existing host");
            }

            string currentDir = new FileInfo(new Uri(rootLocation).LocalPath).DirectoryName; //AppDomain.CurrentDomain.BaseDirectory;

            // setup up target directory
            string physicalHostDir = currentDir.TrimEnd('\\', '/') + "\\" + att.RelativePhysicalPath.Trim('\\', '/') + "\\";
            physicalHostDir = new DirectoryInfo(physicalHostDir).FullName.TrimEnd('\\') + "\\";
            string physicalHostBinDir = physicalHostDir + "bin\\";

            // copy all files from current build output directory to <webroot>/bin
            Directory.CreateDirectory(physicalHostBinDir);
            foreach (string file in Directory.GetFiles(currentDir, "*.*"))
            {
                if (
                    (file.ToLower().IndexOf("nunit.core.") > -1)
                    //|| file.ToLower().IndexOf("nunit.framework.") > -1
                    )
                {
                    continue;
                }
                string newFile = Path.Combine(physicalHostBinDir, Path.GetFileName(file));
                if (File.Exists(newFile)) { File.Delete(newFile); }
                File.Copy(file, newFile);
            }

            // copy framework specific web.config file
            string webConfigFile = string.Format("{0}\\web.config.net-{1}", physicalHostDir, Environment.Version.ToString(2));
            if (File.Exists(webConfigFile))
            {
                string defaultConfigFile = string.Format("{0}\\web.config", physicalHostDir);
                if (File.Exists(defaultConfigFile))
                {
                    File.Delete(defaultConfigFile);
                }
                File.Copy(webConfigFile, defaultConfigFile);
            }

            // finally create & initialize Web Application Host instance
            string virtualPath = "/" + att.VirtualPath.Trim('/');
            AspFixtureHost _host = (AspFixtureHost)System.Web.Hosting.ApplicationHost.CreateApplicationHost(typeof(AspFixtureHost), virtualPath, physicalHostDir);
            string[] preloadAssemblies = new string[]
				{
					typeof(ITest).Assembly.Location // nunit.core.interfaces
					, typeof(TestCase).Assembly.Location // nunit.core
					, typeof(TestAttribute).Assembly.Location
				};

            _host.Initialize(currentDir, AppDomain.CurrentDomain, Console.Out, preloadAssemblies);
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
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception during fixture host shutdown:" + ex);
            }
        }

        private void ShutDown()
        {
            try
            {
                lock (SyncRoot)
                {
                    Trace.WriteLine("inside shutting down host");
                    s_current = null;
                    HttpRuntime.Close();
                    HttpRuntime.UnloadAppDomain();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception inside unloading HttpRuntime host:" + ex);
            }
        }

        #endregion Static Instance Lifetime Handling

        #region Instance Properties

        private object _syncRoot = new object();
        private string _rootLocation;
        private AppDomain _creatorDomain;
        private Assembly[] _preloadedAssemblies;

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
            //            bool bRes = WebRequest.RegisterPrefix("asptest", AspFixtureRequest.Factory);                        
            //            RegisterAspTestPseudoProtocol();
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
        private void Initialize(string rootLocation, AppDomain creatorDomain, TextWriter cout, string[] preloadAssemblies)
        {
            if (AspFixtureHost.Current != null)
            {
                throw new InvalidOperationException("Cannot initialize the Host from within the Host's AppDomain");
            }

            _preloadedAssemblies = new Assembly[preloadAssemblies.Length];
            for (int i = 0; i < _preloadedAssemblies.Length; i++)
            {
                _preloadedAssemblies[i] = Assembly.LoadFrom(preloadAssemblies[i]);
            }

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            //AppDomain.CurrentDomain.UnhandledException+=new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            // "redirect" http protocol
            RegisterAspTestPseudoProtocol();
            // register our nunit extentsions in this AppDomain as well
            RegisterNUnitAddin();

            // remember rootLocation
            _rootLocation = rootLocation;
            // remember creating AppDomain
            _creatorDomain = creatorDomain;
            // redirect cout to the passed TextWriter
            Console.SetOut(cout);
            // Make this Host instance available through AspFixtureHost.Current
            s_current = this;

            // force HttpRuntime initialization
            StringWriter sw = new StringWriter();
            AspFixtureWorkerRequest wr = new AspFixtureWorkerRequest(string.Empty, string.Empty, sw);
            HttpRuntime.ProcessRequest(wr);
            if (wr.StatusCode != 200 && wr.StatusCode != 404)
            {
                throw new Exception("HttpRuntime Setup Failure:" + sw.ToString());
            }
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            foreach (Assembly ass in _preloadedAssemblies)
            {
                if (args.Name == ass.GetName().FullName)
                {
                    return ass;
                }
            }
            return null;
        }

        private bool AssemblyNamesEqual(string leftName, string rightName, bool ignoreVersion)
        {
            if (leftName == null && rightName == null) return true;
            if (leftName == null || rightName == null) return false;

            if (ignoreVersion)
            {
                leftName = leftName.Split(',')[0];
                rightName = rightName.Split(',')[0];
            }

            return (leftName == rightName);
        }

        //		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        //		{
        //			Trace.WriteLine("Unhandled exception occured:" + e.ExceptionObject);
        //		}

        private static void RegisterNUnitAddin()
        {
            NUnitAddinHelper.InstallExtensions(CoreExtensions.Host);
        }

        private void RegisterAspTestPseudoProtocol()
        {
            try
            {
                // inside host, redirect all "http"-protocol requests
                IWebRequestCreate factory = AspFixtureRequest.Factory;
                WebRequest.RegisterPrefix("http", factory);
                WebRequest.RegisterPrefix("asptest", factory);
                Trace.WriteLine("Registered asptest prefix");
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Failed registering asptest prefix: " + ex);
            }
        }

        #endregion Lifecycle Methods

        #region Test Execution Methods

        internal AspTestClientRequest.ResponseData ProcessAspFixtureRequest(AspFixtureRequest request, byte[] requestBodyBytes)
        {
            try
            {
                AspTestClientRequest wr = new AspTestClientRequest(request, requestBodyBytes);
                //            // Impersonate current user
                //            WindowsIdentity curId = WindowsIdentity.GetCurrent();
                //            WindowsImpersonationContext ctx = curId.Impersonate();
                try
                {
                    HttpRuntime.ProcessRequest(wr);
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
        /// This method is called by <see cref="AspTestFixture.Run(EventListener,ITestFilter)"/>
        /// </remarks>
        internal TestSuiteResult CreateAndExecuteTestSuite(Type fixtureType, TestSuiteResult suiteResult, EventListener listener, ITestFilter filter)
        {
            lock (SyncRoot)
            {
                if (Current == null)
                {
                    throw new InvalidOperationException("Cannot execute fixture outside the Host's environment");
                }

                if (filter == null)
                {
                    filter = TestFilter.Empty;
                }

                TestSuite testSuite = null;
                //                try
                //                {
                AspTestFixtureBuilder builder = new AspTestFixtureBuilder();
                testSuite = (TestSuite)builder.BuildFrom(fixtureType);
                suiteResult = (TestSuiteResult)testSuite.Run(listener, filter);
                //                }
                //                catch(Exception ex)
                //                {
                //                    Trace.WriteLine( "Error executing TestSuite:" + ex );
                //                    // signal suite creation failure - TODO: look for a better way!	            
                //                    TestInfo testInfo = new TestInfo(testSuite);
                //
                //                    TestResult tr = new TestSuiteResult( testInfo, testSuite.TestName.FullName );
                //                    tr.Failure("failed creating testsuite", ex.ToString());
                //                    testResult = tr;
                //                    testResult = new TestSuiteResult(null, ex.ToString());
                //                }
                return suiteResult;
            }
        }


        #endregion Test Execution Methods
    }
}
