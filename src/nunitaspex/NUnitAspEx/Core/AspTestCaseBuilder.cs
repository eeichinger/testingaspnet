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
            return method.IsDefined(typeof(AspTestAttribute), false);
        }
        
        protected override TestCase MakeTestCase( MethodInfo method )
        {
            Type expectedException = null;
            string expectedExceptionName = null;
            string expectedMessage = null;

            if( parms.HasExpectedExceptionType )
            {
                Attribute attribute = Reflect.GetAttribute( method, parms.ExpectedExceptionType, false );
                if ( attribute != null )
                {
                    expectedException = (System.Type)Reflect.GetPropertyValue( 
                                                         attribute, "ExceptionType",
                                                         BindingFlags.Public | BindingFlags.Instance );
                    expectedExceptionName = (string)Reflect.GetPropertyValue(
                                                        attribute, "ExceptionName",
                                                        BindingFlags.Public | BindingFlags.Instance );
                    expectedMessage = (string)Reflect.GetPropertyValue(
                                                  attribute, "ExpectedMessage",
                                                  BindingFlags.Public | BindingFlags.Instance );
                }
            }

            return expectedException != null
                       ? new AspTestMethod( method, expectedException, expectedMessage )
                       : new AspTestMethod( method, expectedExceptionName, expectedMessage );
        }
    }
}