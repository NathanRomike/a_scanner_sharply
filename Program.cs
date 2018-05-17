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

      var dataPosts = new List<DataPost>
      {
        new DataPost("HTTP errors", 14, 0),
        new DataPost("HTTP transactions", 20, 0),
        new DataPost("activity traces", 18, 0),
        new DataPost("session attributes", 22, 0),
        new DataPost("analytics events", 19, 0)
      };

      while ((line = file.ReadLine()) != null)
      {
        foreach (var filter in dataPosts)
        {
          if (!line.Contains(filter.Descriptor)) continue;
          var total = 0;
          if (int.TryParse(line.Substring(line.Length - filter.IndexFromEnd, 2).Trim(), out total))
          {
            filter.AddToCount(total);
            continue;
          }
          Console.WriteLine("Unable to parse: " + line);
        }
        
        if (line.Contains("Harvester: connected"))
        {
          harvestConnects++;
        }
      }
      file.Close();

      Console.WriteLine($"The agent made {harvestConnects} data posts.");
      foreach (var filter in dataPosts)
      {
        Console.WriteLine($"{filter.Count} {filter.Descriptor} reported");
      }
    }
  }
}