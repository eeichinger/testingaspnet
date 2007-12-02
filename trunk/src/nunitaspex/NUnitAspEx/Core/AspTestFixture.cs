using System;
using System.Diagnostics;
using NUnit.Core;

namespace NUnitAspEx.Core
{
    public class AspTestFixture : NUnitTestFixture
    {
        public AspTestFixture(Type fixtureType)
            : base(fixtureType)
        { }

        /// <summary>
        /// Create the ASP.NET AppDomain and executes the fixture there.
        /// </summary>
        public override TestResult Run(EventListener listener, ITestFilter filter)
        {
            // create the Host instance based on given AspTestFixtureAttribute properties
            AspTestFixtureAttribute att = (AspTestFixtureAttribute)this.FixtureType.GetCustomAttributes(typeof(AspTestFixtureAttribute), false)[0];

            // create & execute TestSuite within Host's AppDomain
            AspFixtureHost host = null;
            try
            {
                host = AspFixtureHost.CreateInstance(att, this.FixtureType.Assembly.CodeBase);

                // RemotableEventListenerProxy below suppresses calls to SuiteStarted/SuiteFinished events
                // -> call it manually here
                listener.SuiteStarted(this.TestName);

                // wrap listener and filter with remotable references
                RemotableEventListenerProxy proxyListener = new RemotableEventListenerProxy(listener);
                
                // TODO: 
                // remote filtering is not possible, because some ITest 
                // implementations (like NUnitTestMethod) are not serializable
                //ITestFilter proxyFilter  = new RemotableFilterProxy(filter);
                ITestFilter proxyFilter = TestFilter.Empty;

                TestSuiteResult result =
                    (TestSuiteResult)host.CreateAndExecuteTestSuite(this.FixtureType, proxyListener, proxyFilter);

                // finally inform listener that we are done
                listener.SuiteFinished(result);

                return result;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Failed executing TestSuite on asp.net host:" + ex);
                throw;
            }
            finally
            {
                if (host != null)
                {
                    Trace.WriteLine("unloading ASP.NET runtime host");
                    AspFixtureHost.ShutDown(host);
                }
            }
        }
    }
}