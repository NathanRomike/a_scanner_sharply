using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

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
      ParseLogFile();
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

    private static void ParseLogFile()
    {
      var harvestConnects = 0;

      var file = new StreamReader(OutputFileName);
      string line;
      var filterDictionary = new Dictionary<string, int[]>
      {
//      Key = unique segment, Value = [0]Char count from end of log line to number in log [1]Instance counter
        {"HTTP errors", new[] {14, 0}},
        {"HTTP transactions", new[] {20, 0}},
        {"activity traces", new[] {18, 0}},
        {"session attributes", new[] {22, 0}},
        {"analytics events", new[] {19, 0}}
      };

      while ((line = file.ReadLine()) != null)
      {
        foreach (var filter in filterDictionary)
        {
          if (!line.Contains(filter.Key)) continue;
          if (int.TryParse(line.Substring(line.Length - filter.Value[0], 2).Trim(), out filter.Value[1])) continue;
          Console.WriteLine("Unable to parse: " + line);
        }
        
        if (line.Contains("Harvester: connected"))
        {
          harvestConnects++;
        }
      }
      file.Close();

      Console.WriteLine($"The agent posted data {harvestConnects} times.");
      foreach (var filter in filterDictionary)
      {
        Console.WriteLine($"{filter.Value[1]} {filter.Key} reported");
      }
    }
  }
}