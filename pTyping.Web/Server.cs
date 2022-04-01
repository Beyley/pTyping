using System;
using System.IO;
using System.Reflection;
using System.Threading;
using EeveeTools.Servers.TCP;
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

        Thread httpThread = new(
        () => {
            TcpServer httpServer = new(BIND_IP, HTTP_PORT, typeof(HttpClient));

            Console.WriteLine($"Starting HTTP server at {BIND_IP}:{HTTP_PORT}!");

            httpServer.Start();
        }
        );

        Thread gopherThread = new(
        () => {
            TcpServer gopherServer = new(BIND_IP, GOPHER_PORT, typeof(GopherClient));

            Console.WriteLine($"Starting Gopher server at {BIND_IP}:{GOPHER_PORT}!");

            gopherServer.Start();
        }
        );

        httpThread.Start();
        gopherThread.Start();

        Console.WriteLine("Press enter to stop the server!");
        Console.ReadLine();
        Console.WriteLine("Stopping server!");

        Environment.Exit(0);
    }
}
