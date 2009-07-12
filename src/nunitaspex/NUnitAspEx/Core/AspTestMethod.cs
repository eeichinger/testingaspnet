using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using NUnit.Core;

namespace NUnitAspEx.Core
{
    /// <summary>
    /// Summary description for AspTestMethod.
    /// </summary>
    internal class AspTestMethod : NUnitTestMethod
    {
        private static AspTestMethod _currentTest = null;

        public static AspTestMethod Current
        {
            get { return _currentTest; }
        }

        private TestResult _currentTestCaseResult;
        private readonly MethodInfo _testMethod;

        public AspTestMethod(MethodInfo method)
            : base(method)
        {
            _testMethod = method;
        }

        public override void RunTestMethod(TestResult testResult)
        {
            AspTestAttribute att = (AspTestAttribute)_testMethod.GetCustomAttributes(typeof(AspTestAttribute), false)[0];

            if (att.Page == null)
            {
                throw new ArgumentNullException("Page property must be set");
            }

            HttpWorkerRequest wr;

            // check, if we're executed within host
            AspFixtureHost host = AspFixtureHost.Current;
            if (host != null)
            {
                TextWriter tw = (att.SuppressOutput) ? null : Console.Out;
                wr = new AspFixtureWorkerRequest(att.Page, att.Query, tw);
                if (att.UseHttpRuntime)
                {
                    // delegate execution to AspTestExecutionModule
                    _currentTest = this;
                    _currentTestCaseResult = testResult;
                    try
                    {
                        HttpRuntime.ProcessRequest(wr);
                        return; // don't continue!
                    }
                    finally
                    {
                        _currentTestCaseResult = null;
                        _currentTest = null;
                    }
                }
            }
            else
            {
                string physicalDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, att.AppRelativePhysicalDir);
                wr = new SimpleWorkerRequest(att.AppVirtualDir, physicalDir, att.Page, att.Query, Console.Out);
                // TODO: get extended
            }

            HttpContext ctx = new HttpContext(wr);
            HttpContext.Current = ctx;
            try
            {
                base.RunTestMethod(testResult);
            }
            finally
            {
                HttpContext.Current = null;
            }
        }

        /// <summary>
        /// "Callback" from within an executing HttpWorkerRequest
        /// </summary>
        internal void DoRunFromWithinWorkerRequest()
        {
            try
            {
                base.RunTestMethod(_currentTestCaseResult);
            }
            finally
            {
                AspTestExecutionModule.Clear();
                _currentTestCaseResult = null;
            }
        }
    }
}
