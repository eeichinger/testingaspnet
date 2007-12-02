using System;

namespace NAnt.NUnit2OutProc.Logging
{
  public interface ILog
  {
    void Debug(string msg, params object[] args);
    void Error(string msg, Exception ex, params object[] args);
  }
}