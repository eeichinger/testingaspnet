using System;
using System.Reflection;

namespace NUnitAspEx.Client
{
    internal class SR
    {
        private static readonly Type realType = typeof(System.Uri).Assembly.GetType("System.SR");
        private static readonly MethodInfo pfnGetString1 = realType.GetMethod( "GetString", new Type[] { typeof(string) }  );
        
        public static string GetString(string name)
        {
            return (string)pfnGetString1.Invoke( null, new object[] { name } );
        }
    }
}