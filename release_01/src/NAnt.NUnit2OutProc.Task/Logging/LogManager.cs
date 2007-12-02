using System;
using System.Collections;

namespace NAnt.NUnit2OutProc.Logging
{
  /// <summary>
  /// Manages Loggers.
  /// </summary>
  public class LogManager
  {
    private static readonly ILog s_nullLogger = new NullLogger();

    private static Hashtable s_loggers = new Hashtable();

    public static ILog GetLogger(Type type)
    {
      if (type == null) throw new ArgumentNullException("type");

      return GetLogger(type.FullName);
    }

    public static ILog GetLogger(object obj)
    {
      if (obj == null) throw new ArgumentNullException("obj");

      return GetLogger(obj.GetType().FullName);
    }

    public static ILog GetLogger(string name)
    {
#if LOG
      lock (s_loggers.SyncRoot)
      {
        ILog logger = (ILog) s_loggers[name];
        if (logger != null) return logger;

        logger = new TraceLogger(name);
        s_loggers.Add(name, logger);
        return logger;
      }
#else
      return s_nullLogger;
#endif
    }
  }
}