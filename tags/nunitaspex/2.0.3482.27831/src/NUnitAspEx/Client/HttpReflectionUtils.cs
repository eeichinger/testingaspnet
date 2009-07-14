using System;
using System.Configuration;
using System.Reflection;

namespace NUnitAspEx.Client
{
	/// <summary>
	/// Summary description for HttpReflectionUtils.
	/// </summary>
	public sealed class HttpReflectionUtils
	{
	    private HttpReflectionUtils()
	    {}
	    
        public static bool IsAspAppDomain()
        {
            Version clrVersion = Environment.Version;
            object cs = null;
            // check for HttpConfigurationSystem
            switch(clrVersion.Major)
            {
                case 1:
                    {
                        Type tConfigurationSystem = typeof(ConfigurationSettings);
                        cs =
                            tConfigurationSystem.GetField("_configSystem", BindingFlags.Static | BindingFlags.NonPublic)
                                .GetValue(null);
                    }
                    break;
                case 2:
                    {
                        Type tConfigurationSystem =
                            Type.GetType(
                                "System.Configuration.ConfigurationManager, System.Configuration, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                        cs =
                            tConfigurationSystem.GetField("s_configSystem", BindingFlags.Static | BindingFlags.NonPublic)
                                .GetValue(null);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("unknown framework version");
            }

            return ("System.Web.Configuration.HttpConfigurationSystem" == cs.GetType().FullName);
        }
	}
}
