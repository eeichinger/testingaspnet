using System.Reflection;
using NUnit.Core;
using NUnit.Core.Extensibility;

namespace NUnitAspEx.Core
{
//    [TestCaseBuilder]
    public class AspTestCaseBuilder : ITestCaseBuilder2
    {
        public bool CanBuildFrom(System.Reflection.MethodInfo method)
        {
            return CanBuildFrom(method, null);
        }

        public bool CanBuildFrom(System.Reflection.MethodInfo method, Test test)
        {
            bool isDefined = method.IsDefined(typeof(AspTestAttribute), false);
            return isDefined;
        }

        public Test BuildFrom(MethodInfo method)
        {
            return BuildFrom(method, null);
        }

        public Test BuildFrom(MethodInfo method, Test parentSuite)
        {
//            return (CoreExtensions.Host.TestCaseProviders.HasTestCasesFor(method) ? BuildParameterizedMethodSuite(method, parentSuite) : BuildSingleTestMethod(method, null));
            TestMethod test = new AspTestMethod(method);
            NUnitFramework.ApplyCommonAttributes(method, test);
            NUnitFramework.ApplyExpectedExceptionAttribute(method, test);
            return test;
        }

//        protected override TestCase MakeTestCase(MethodInfo method)
//        {
//            return new AspTestMethod(method);
//        }
    }
}