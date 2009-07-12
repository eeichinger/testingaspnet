using System;
using System.Reflection;
using NUnit.Core;
using NUnit.Core.Builders;

namespace NUnitAspEx.Core
{
    [TestCaseBuilder]
    public class AspTestCaseBuilder : NUnitTestCaseBuilder
    {
        public override bool CanBuildFrom(System.Reflection.MethodInfo method)
        {
            bool isDefined = method.IsDefined(typeof(AspTestAttribute), false);
            return isDefined;
        }

        protected override TestCase MakeTestCase(MethodInfo method)
        {
            return new AspTestMethod(method);
        }
    }
}