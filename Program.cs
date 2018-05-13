using System;
using System.Diagnostics;

namespace AgentLogProcessor
{
  internal static class Program
  {
    private const string PackageName = "com.newrelic.android";
    private const string OutputFileName = "runtime.log";
    private static Process _process;
    
    public static void Main(string[] args)
    {
      ConnectLogcatBuffer();
    }

    private static void ConnectLogcatBuffer()
    {
      try
      {
        _process = new Process
        {
          StartInfo = GetStartInfo()
        };
        _process.Start();
        _process.WaitForExit();
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message + e.HelpLink);
        Console.Write(e.StackTrace);
      }
    }

    private static ProcessStartInfo GetStartInfo()
    {
      return new ProcessStartInfo
        {
          FileName = "/bin/bash",
          Arguments = $"-c \"adb logcat -d {PackageName}:* *:S > {OutputFileName} && adb logcat -c\""
        };
    }
  }
}