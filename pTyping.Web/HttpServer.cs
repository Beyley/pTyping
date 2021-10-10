using System;
using EeveeTools.Servers.TCP;

namespace pTyping.Web {
    public static class HTTPServer {
        public static string Server = "pTyping Web Server";
        
        static void Main(string[] args) {
            string location = "0.0.0.0";
            short  port     = 8080;
            
            TcpServer server = new(location, port, typeof(TcpClient));
            
            Console.WriteLine($"Starting HTTP server at {location}:{port}!");

            server.Start();
        }
    }
}
