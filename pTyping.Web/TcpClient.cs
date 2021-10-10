using EeveeTools.Servers.TCP;

namespace pTyping.Web {
    public class TcpClient : TcpClientHandler {
        protected override void HandleData(byte[] data) {
            HttpRequest request = HttpRequest.ParseHttpRequest(data);

            HttpResponse response = new();
            
            this.Stream.Write((byte[]) response);
            this.Stream.Flush();
            this.Client.Close();
        }
        
        protected override void HandleDisconnect() { }
    }
}
