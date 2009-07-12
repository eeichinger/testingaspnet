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
		private readonly EventListener _wrappedListener;

		public RemotableEventListenerProxy(EventListener wrappedListener)
		{
			this._wrappedListener = wrappedListener;
		}

		public void RunStarted(string name, int testCount)
		{
			this._wrappedListener.RunStarted(name, testCount);
		}

		public void RunFinished(TestResult result)
		{
			this._wrappedListener.RunFinished(result);
		}

		public void RunFinished(Exception exception)
		{
			this._wrappedListener.RunFinished(exception);
		}

		public void TestStarted(TestName testName)
		{
			this._wrappedListener.TestStarted(testName);
		}

		public void TestFinished(TestResult result)
		{
			this._wrappedListener.TestFinished(result);
		}

	    public void SuiteStarted(TestName testName)
	    {
	        //this._wrappedListener.SuiteStarted(testName);
	    }

		public void SuiteFinished(TestResult result)
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