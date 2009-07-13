using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Web;
using NUnitAspEx.Client;

namespace NUnitAspEx.Core
{
    /// <summary>
    /// An AspFixtureHost lives inside it's own AppDomain with an initialized HttpRuntime.
    /// </summary>
    public class AspFixtureHost : MarshalByRefObject, IAspFixtureHost
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

        public static IAspFixtureHost CreateInstance(string virtualPath, string relativePhysicalPath, object testFixture)
        {
            return CreateInstance(virtualPath, relativePhysicalPath, testFixture.GetType().Assembly.CodeBase);
        }

        public static IAspFixtureHost ReleaseInstance(IAspFixtureHost host)
        {
            AspFixtureRequest.Factory.Host = null;
            host.Dispose();
            return null;
        }

        /// <summary>
        /// Creates a new Host instance within a new AppDomain based on the passed in Properties.
        /// </summary>
        /// <remarks>
        /// You must not call this method from within an existing Host's AppDomain.
        /// </remarks>
        private static AspFixtureHost CreateInstance(string virtualPath, string relativePhysicalPath, string rootLocation)
        {
            if (AspFixtureHost.Current != null)
            {
                throw new InvalidOperationException("Cannot create a new Host within an existing host");
            }

            string currentDir = new FileInfo(new Uri(rootLocation).LocalPath).DirectoryName; //AppDomain.CurrentDomain.BaseDirectory;

            // setup up target directory
            string physicalHostDir = currentDir.TrimEnd('\\', '/') + "\\" + relativePhysicalPath.Trim('\\', '/') + "\\";
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
            virtualPath = "/" + virtualPath.Trim('/');
            AspFixtureHost _host = (AspFixtureHost)System.Web.Hosting.ApplicationHost.CreateApplicationHost(typeof(AspFixtureHost), virtualPath, physicalHostDir);
            string[] preloadAssemblies = new string[]
				{
//					typeof(TestAttribute).Assembly.Location
//					, typeof(ITest).Assembly.Location // nunit.core.interfaces
//					, typeof(NUnit.Core.CoreExtensions).Assembly.Location // nunit.core
				};

            _host.Initialize(currentDir, AppDomain.CurrentDomain, Console.Out, preloadAssemblies);

            AspFixtureRequest.AspFixtureRequestFactory factory = AspFixtureRequest.Factory;
            WebRequest.RegisterPrefix("asptest", factory);
            factory.Host = _host;
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

        #endregion Static Instance Lifetime Handling

        #region Instance Properties

        private readonly object _syncRoot = new object();
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
        {}

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            ShutDown();
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

            // "redirect" http protocol
            RegisterAspTestPseudoProtocol();

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
            AspFixtureSimpleWorkerRequest wr = new AspFixtureSimpleWorkerRequest(string.Empty, string.Empty, sw);
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
            if (leftName == null && rightName == null)
                return true;
            if (leftName == null || rightName == null)
                return false;

            if (ignoreVersion)
            {
                leftName = leftName.Split(',')[0];
                rightName = rightName.Split(',')[0];
            }

            return (leftName == rightName);
        }

        private void RegisterAspTestPseudoProtocol()
        {
            try
            {
                // inside host, redirect all "http"-protocol requests
                AspFixtureRequest.AspFixtureRequestFactory factory = AspFixtureRequest.Factory;
                WebRequest.RegisterPrefix("http", factory);
                WebRequest.RegisterPrefix("asptest", factory);
                factory.Host = this;
                Trace.WriteLine("Registered asptest prefix");
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Failed registering asptest prefix: " + ex);
            }
        }

        #endregion Lifecycle Methods

        #region Test Execution Methods

        internal AspFixtureRequestWorkerRequest.ResponseData ProcessAspFixtureRequest(HttpWebRequest request, byte[] requestBodyBytes)
        {
            try
            {
                AspFixtureRequestWorkerRequest wr = new AspFixtureRequestWorkerRequest(request, requestBodyBytes);
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
                Trace.WriteLine("Failed executing AspFixtureRequestWorkerRequest:" + ex);
                throw;
            }
        }

        public void Execute(TestAction action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                if (!e.GetType().IsSerializable)
                {
                    throw new Exception(e.ToString());
                }
                throw;
            }
        }

        public HttpWebClient CreateWebClient()
        {
            return new AspTestClient();
        }

        public HttpWebRequest CreateWebRequest(string virtualPath)
        {
            return new AspTestClient().CreateWebRequest(virtualPath);
        }

        #endregion Test Execution Methods
    }
}
