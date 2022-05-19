using System;
using System.IO;
using System.Reflection;
using System.Threading;
using EeveeTools.Servers.TCP;
using Kettu;
using pTyping.Web.Gopher;
using pTyping.Web.Http;

namespace pTyping.Web;

public static class Server {
    public const string BIND_IP     = "0.0.0.0";
    public const string SERVER_NAME = "pTyping Web Server";
    public const string DATA_FOLDER = "html";
    public const short  HTTP_PORT   = 8080;
    public const string GOPHER_IP   = "localhost";
    public const short  GOPHER_PORT = 7070;

    public static readonly string ExecutablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new Exception("shits fucked man");

    private static void Main() {
        Logger.StartLogging();
        Logger.AddLogger(new ConsoleLogger());
        
        Thread httpThread = new(
        () => {
            TcpServer httpServer = new(BIND_IP, HTTP_PORT, typeof(HttpClient));

            Logger.Log($"Starting HTTP server at {BIND_IP}:{HTTP_PORT}!", LoggerLevelHTTPServer.Instance);

            httpServer.Start();
        }
        );

        Thread gopherThread = new(
        () => {
            TcpServer gopherServer = new(BIND_IP, GOPHER_PORT, typeof(GopherClient));

            Logger.Log($"Starting Gopher server at {BIND_IP}:{GOPHER_PORT}!", LoggerLevelGopherServer.Instance);

            gopherServer.Start();
        }
        );

        httpThread.Start();
        gopherThread.Start();

        Logger.Log("Press enter to stop the servers!", LoggerLevelServer.Instance);
        Console.ReadLine();
        Logger.Log("Stopping servers!", LoggerLevelServer.Instance);

        Logger.Update();
        Logger.StopLogging();
        
        Environment.Exit(0);
    }
}
