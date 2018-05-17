using System;
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
      int requestErrorCounter, activityTraceCounter, attributeCounter, eventCounter, harvestConnects;
      var requestCounter = requestErrorCounter = activityTraceCounter = attributeCounter = eventCounter = harvestConnects = 0;
      
      string line;
      var file = new StreamReader(OutputFileName);
      while ((line = file.ReadLine()) != null)
      {
//        TODO: this calls for a hash map with each node containing 3 attributes:
//         - string of unique segment from log line (like 'HTTP transactions')
//         - int for length between the end of the log line and the number logged
//         - int counter for that data type
//        TODO: then iterate through that hashmap and if line contain unique segment => try to substring, and reassign with that int
        
        if (!line.Contains("Harvester: ")) continue;
        
        if (line.Contains("Harvester: connected"))
        {
          harvestConnects++;
        }

        if (line.Contains("HTTP transactions"))
        {
          requestCounter++;
        }
        
        if (line.Contains("HTTP errors"))
        {
          requestErrorCounter++;
        }     
        
        if (line.Contains("activity traces"))
        {
          activityTraceCounter++;
        }

        if (line.Contains("session attributes"))
        {
          if (!int.TryParse(line.Substring(line.Length - 22, 2).Trim(), out attributeCounter))
          {
            Console.WriteLine("Unable to parse attribute string:" + line.Substring(line.Length - 20, 2).Trim());
            Console.WriteLine(line);
          }
        }
        
        if (line.Contains("analytics events"))
        {
          if (!int.TryParse(line.Substring(line.Length - 19, 2).Trim(), out eventCounter))
          {
            Console.WriteLine("Unable to parse analytics events string:" + line.Substring(line.Length - 19, 2).Trim());
            Console.WriteLine(line);
          }
        }
      }
      file.Close();
      
      Console.WriteLine($"There were {harvestConnects} agent payloads sent.");
      Console.WriteLine($"{attributeCounter} total session attributes");
      Console.WriteLine($"{eventCounter} total events");
    }
  }
}