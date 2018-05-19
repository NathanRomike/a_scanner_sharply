using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AgentLogProcessor
{
  internal static class Program
  {
    private const string PackageName = "com.newrelic.android";
    private const string OutputFileName = "runtime.log";
    private const string WsUrl = "ws://127.0.0.1:80";
    
    private static Process _process;
    private static int _harvestConnects = 0;
    private static int _sessionsCaptured = 0;
    
    private static readonly List<DataPost> DataPosts = new List<DataPost>
    {
      new DataPost("HTTP errors", 14, 0),
      new DataPost("HTTP transactions", 20, 0),
      new DataPost("activity traces", 18, 0),
      new DataPost("session attributes", 22, 0),
      new DataPost("analytics events", 19, 0)
    };

    public static void Main(string[] args)
    {
      ConnectLogcatBuffer();
      ParseLogFile();
      var reportResults = ReportResults();
      reportResults.Wait();
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
      var file = new StreamReader(OutputFileName);
      string line;

      while ((line = file.ReadLine()) != null)
      {
        if (line.Contains("Harvester: connected"))
        {
          _harvestConnects++;
          continue;;
        }

        if (line.Contains("New Relic Agent v"))
        {
          _sessionsCaptured++;
          continue;
        }

        foreach (var filter in DataPosts)
        {
          if (!line.Contains(filter.Descriptor)) continue;
          filter.AddToCount(filter.GetCountFromString(line));
        }
      }
      file.Close();
    }

    private static async Task ReportResults()
    {
      Console.WriteLine($"The agent made {_harvestConnects} data posts, over {_sessionsCaptured} sessions");
      foreach (var filter in DataPosts)
      {
        Console.WriteLine($"{filter.TotalCount} {filter.Descriptor} reported");
      }

      using (var clientWebSocket = new ClientWebSocket())
      {
        var serverUri = new Uri(WsUrl);
        await clientWebSocket.ConnectAsync(serverUri, CancellationToken.None);
        while (clientWebSocket.State == WebSocketState.Open)
        {
          Console.Write("Input message ('exit' to exit): ");
          var userMessage = Console.ReadLine();
          if (userMessage == "exit")
          {
            break;
          }

          if (userMessage != null)
          {
            var dataPacket = Encoding.UTF8.GetBytes(userMessage);
          }
        }
      }
    }
  }
}