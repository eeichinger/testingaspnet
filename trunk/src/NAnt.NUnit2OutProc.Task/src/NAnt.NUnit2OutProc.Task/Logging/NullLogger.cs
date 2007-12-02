using System;

namespace NAnt.NUnit2OutProc.Logging
{
  /// <summary>
  /// A NoOp logger implementation
  /// </summary>
  internal class NullLogger : ILog
  {
    public void Debug(string msg, params object[] args)
    {
      // do nothing
    }

    public void Error(string msg, Exception ex, params object[] args)
    {
      // do nothing
    }
  }
}