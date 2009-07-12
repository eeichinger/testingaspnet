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
			using( new TestContext() )
			{
				TestResult suiteResult = new TestResult( new TestInfo(this) );

				listener.SuiteStarted( this.TestName );
				long startTime = DateTime.Now.Ticks;

				RunWithinHost(suiteResult, listener, filter);

				long stopTime = DateTime.Now.Ticks;
				double time = ((double)(stopTime - startTime)) / (double)TimeSpan.TicksPerSecond;
				suiteResult.Time = time;

				listener.SuiteFinished(suiteResult);
				return suiteResult;
			}			
		}

        /// <summary>
        /// Create the ASP.NET AppDomain and executes the fixture there.
        /// </summary>
        private TestResult RunWithinHost(TestResult result, EventListener listener, ITestFilter filter)
        {
            // create the Host instance based on given AspTestFixtureAttribute properties
            AspTestFixtureAttribute att = (AspTestFixtureAttribute)this.FixtureType.GetCustomAttributes(typeof(AspTestFixtureAttribute), false)[0];

            // create & execute TestSuite within Host's AppDomain
            AspFixtureHost host = null;
            try
            {
                host = AspFixtureHost.CreateInstance(att, this.FixtureType.Assembly.CodeBase);

                // wrap listener and filter with remotable references
                RemotableEventListenerProxy proxyListener = new RemotableEventListenerProxy(listener);
                
                // TODO: 
                // remote filtering is not possible, because some ITest 
                // implementations (like NUnitTestMethod) are not serializable
                //ITestFilter proxyFilter  = new RemotableFilterProxy(filter);
                ITestFilter proxyFilter = TestFilter.Empty;

                result = host.CreateAndExecuteTestSuite(this.FixtureType, result, proxyListener, proxyFilter);

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