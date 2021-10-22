using System;
using System.IO;
using System.Reflection;
using EeveeTools.Servers.TCP;

namespace pTyping.Web {
    public static class HttpServer {
        public static string Server         = "pTyping Web Server";
        public static string ExecutablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new Exception("shits fucked man");
        public static string DataFolder     = "html";

        private static void Main() {
            string location = "0.0.0.0";
            short  port     = 8080;

            TcpServer server = new(location, port, typeof(TcpClient));

            Console.WriteLine($"Starting HTTP server at {location}:{port}!");

            server.Start();
        }
    }
}
