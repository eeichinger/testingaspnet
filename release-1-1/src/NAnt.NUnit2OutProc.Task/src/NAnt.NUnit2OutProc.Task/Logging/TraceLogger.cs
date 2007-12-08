using System;
using System.Diagnostics;

namespace NAnt.NUnit2OutProc.Logging
{
  /// <summary>
  /// Implements the internal ILog interface for debugging purposes that 
  /// outputs log messages to <see cref="System.Diagnostics.Trace"/>
  /// </summary>
  internal class TraceLogger : ILog
  {
    private string _name;

    public TraceLogger(string name)
    {
      this._name = name;
    }

    public void Debug(string msg, params object[] args)
    {
      LogInternal(_name, "DEBUG", msg, null, args);
    }


    public void Error(string msg, Exception ex, params object[] args)
    {
      LogInternal(_name, "ERROR", msg, ex, args);
    }

    private static void LogInternal(string loggerName, string level, string msg, Exception ex, params object[] args)
    {
      msg = (ex == null)
              ? string.Format("{0}: {1}", level, msg)
              : string.Format("{0}: {1}{2}{3}", level, msg, Environment.NewLine, "" + ex);

      msg = string.Format(msg, args);

      Trace.WriteLine(msg, loggerName);
    }
  }
}