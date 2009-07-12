using System.Reflection;
using NUnit.Core;
using NUnit.Core.Extensibility;

namespace NUnitAspEx
{
    public class AspTestFixtureDecorator : ITestDecorator
    {
        public Test Decorate(Test test, MemberInfo member)
        {
            return test;
        }
    }
}