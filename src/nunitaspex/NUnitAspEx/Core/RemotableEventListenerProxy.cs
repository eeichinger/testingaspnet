using System;
using NUnit.Core;

namespace NUnitAspEx.Core
{
	/// <summary>
	/// Wraps a local EventListener to be able to pass events across AppDomains.
	/// </summary>
	/// <remarks>
	/// Suppresses the call to SuiteStarted/SuiteFinished. These calls must *not* occur from within a remote AppDomain!
	/// </remarks>
	internal class RemotableEventListenerProxy : LongLivingMarshalByRefObject, EventListener
	{
		private EventListener _wrappedListener;

		public RemotableEventListenerProxy(EventListener wrappedListener)
		{
			this._wrappedListener = wrappedListener;
		}

		public void RunStarted(Test[] tests)
		{
			this._wrappedListener.RunStarted(tests);
		}

		public void RunFinished(TestResult[] results)
		{
			this._wrappedListener.RunFinished(results);
		}

		public void RunFinished(Exception exception)
		{
			this._wrappedListener.RunFinished(exception);
		}

		public void TestStarted(TestCase testCase)
		{
			this._wrappedListener.TestStarted(testCase);
		}

		public void TestFinished(TestCaseResult result)
		{
			this._wrappedListener.TestFinished(result);
		}

		public void SuiteStarted(TestSuite suite)
		{
			//this._wrappedListener.SuiteStarted(suite);
		}

		public void SuiteFinished(TestSuiteResult result)
		{
			// ignore
			//this._wrappedListener.SuiteFinished(result);
		}

		public void UnhandledException(Exception exception)
		{
			this._wrappedListener.UnhandledException(exception);
		}

		public void TestOutput(TestOutput testOutput)
		{
			this._wrappedListener.TestOutput(testOutput);
		}
	}
}