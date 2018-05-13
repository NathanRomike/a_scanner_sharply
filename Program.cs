using System;
using System.Diagnostics;

namespace AgentLogProcessor
{
  internal static class Program
  {
    private const string Bash = "/bin/bash";
    private const string PackageName = "com.newrelic.android";
    private const string OutputFileName = "runtime.log";
    
    private static Process _process;
    
    public static void Main(string[] args)
    {
      ConnectLogcatBuffer();
    }

    private static void ConnectLogcatBuffer()
    {
      _process = new Process
      {
        StartInfo = GetStartInfo()
      };
      _process.Start();
      _process.WaitForExit();
      Console.WriteLine(_process.StandardOutput.ReadToEnd());
    }

    private static ProcessStartInfo GetStartInfo()
    {
      return new ProcessStartInfo
        {
          FileName = Bash,
          Arguments = $"-c \"adb logcat -d {PackageName}:* *:S\"", // > {OutputFileName}
          UseShellExecute = false,
          RedirectStandardOutput = true,
          CreateNoWindow = true
        };
    }
  }
}