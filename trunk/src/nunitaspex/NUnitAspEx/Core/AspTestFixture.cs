using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using NUnit.Core;

namespace NUnitAspEx.Core
{
	public class AspTestFixture : NUnitTestFixture
	{
		public AspTestFixture(Type fixtureType, int assemblyKey) : base(fixtureType, assemblyKey)
		{}

        /// <summary>
        /// This method replace all occurrences of remote ITest references within the 
        /// TestSuiteResult with local ones from the testSuite.
        /// </summary>
		private void DereferenceRemoteObjects( TestSuite testSuite, TestSuiteResult testSuiteResult )
		{
			Hashtable mapLocal = new Hashtable();
			for(int i=0;i<testSuite.Tests.Count;i++)
			{
				ITest test = (ITest) testSuite.Tests[i];
				mapLocal[test.UniqueName] = test;
			}

			FieldInfo fiTest = typeof(TestResult).GetField("test", BindingFlags.Instance|BindingFlags.NonPublic);

			foreach(TestResult childTestResult in testSuiteResult.Results)
			{
				ITest testData = (ITest) mapLocal[childTestResult.Test.UniqueName];
				fiTest.SetValue(childTestResult, testData);
			}

			fiTest.SetValue(testSuiteResult, testSuite);
		}

        /// <summary>
        /// Create the ASP.NET AppDomain and executes the fixture there.
        /// </summary>
		public override TestResult Run(EventListener listener, IFilter filter)
		{
			// create the Host instance based on given AspTestFixtureAttribute properties
			AspTestFixtureAttribute att =
				(AspTestFixtureAttribute) this.FixtureType.GetCustomAttributes(typeof(AspTestFixtureAttribute), false)[0];

			// create & execute TestSuite within Host's AppDomain
			AspFixtureHost host = null;
			try
			{
				host = AspFixtureHost.CreateInstance(att, this.FixtureType.Assembly.CodeBase);

				// RemotableEventListenerProxy below suppresses calls to SuiteStarted/SuiteFinished events
				// -> call it manually here
				listener.SuiteStarted(this);

				// wrap listener and filter with remotable references
				RemotableEventListenerProxy proxyListener = new RemotableEventListenerProxy(listener);
				filter = new RemotableFilterProxy(filter);

				TestSuiteResult result =
					(TestSuiteResult) host.CreateAndExecuteTestSuite(this.FixtureType, this.AssemblyKey, proxyListener, filter);

				// first dereference remote objects!
				this.DereferenceRemoteObjects(this, result);

				// finally inform listener that we are done
				listener.SuiteFinished(result);

				return result;
			}
			catch(Exception ex)
			{
				Trace.WriteLine("Failed executing TestSuite on asp.net host:" + ex);
				return null;
			}
			finally
			{
				if(host != null)
				{
					Trace.WriteLine("unloading ASP.NET runtime host");
					AspFixtureHost.ShutDown(host);
				}
			}
		}
	}
}