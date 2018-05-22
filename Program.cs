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
    
    private static ClientWebSocket _clientWebSocket;
    
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
      
      Console.WriteLine("formatted results:");
      Console.WriteLine(FormatResults());
      
      var connected = ConnectToWebSocket();
      connected.Wait();
      
      var messageSent = SendString(_clientWebSocket, FormatResults(), CancellationToken.None);
      messageSent?.Wait();
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

    private static string FormatResults()
    {
      var stringBuilder = new StringBuilder();
      stringBuilder.AppendLine($"harvest connections:{_harvestConnects}, ");
      stringBuilder.AppendLine($"sessions captured:{_sessionsCaptured}, ");
      
      foreach (var filter in DataPosts)
      {
        var delimiter = "";
        if (!DataPosts.IndexOf(filter).Equals(DataPosts.Count - 1))
        {
          delimiter = ", ";
        }
        stringBuilder = stringBuilder.AppendLine($"{filter.Descriptor}:{filter.TotalCount}{delimiter}");
      }
      return stringBuilder.ToString();
    }

    private static Task ConnectToWebSocket()
    {
      using (_clientWebSocket = new ClientWebSocket())
      {
        var serverUri = new Uri(WsUrl);
        return _clientWebSocket.ConnectAsync(serverUri, CancellationToken.None);
      }
    }

    private static Task SendString(WebSocket webSocket, string dataToSend, CancellationToken cancellationToken)
    {
      if (_clientWebSocket.State != WebSocketState.Open) return null;
      var encoded = Encoding.UTF8.GetBytes(dataToSend);
      var buffer = new ArraySegment<byte>(encoded, 0 , encoded.Length);
      return webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
    }
  }
}