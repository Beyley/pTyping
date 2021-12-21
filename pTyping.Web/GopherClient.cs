using System;
using System.Text;
using EeveeTools.Servers.TCP;

namespace pTyping.Web;

public class GopherClient : TcpClientHandler {
    protected override void HandleData(byte[] data) {
        GopherRequest request = new(data);

        Console.WriteLine($"Got request for data at {request.Location}");

        GopherResponse response = new(GopherPage.GetPage(request.Location).Items);

        this.Stream.Write(Encoding.UTF8.GetBytes(response.GetResult()));
        this.Stream.Flush();
        this.Stream.Close();
    }

    protected override void HandleDisconnect() {}
}