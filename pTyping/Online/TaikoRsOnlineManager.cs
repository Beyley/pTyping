using System;
using System.IO;
using System.Text;
using Websocket.Client;
using System.Net.WebSockets;
using System.Threading.Tasks;
using pTyping.Online.TaikoRsPackets;

namespace pTyping.Online {
    public class TaikoRsOnlineManager : OnlineManager {
        private WebsocketClient _client;

        private readonly Uri _uri;

        public TaikoRsOnlineManager(string uri) => this._uri = new(uri);
        
        public override string Username() => Config.Username;
        public override string Password() => Config.Password;

        protected override async Task Connect() {
            if (this.State == ConnectionState.Connected)
                await this.Disconnect();
            
            this.InvokeOnConnectStart(this);

            this._client = new(this._uri);
            this._client.MessageReceived.Subscribe(this.HandleMessage);
            await this._client.StartOrFail();

            this.InvokeOnConnect(this);
        }

        private void HandleMessage(ResponseMessage message) {
            if (message.MessageType == WebSocketMessageType.Text) throw new InvalidDataException("The server you are connecting to does not follow the rules of the Taiko.rs server infrastructure");

            MemoryStream  stream = new(message.Binary);
            TaikoRsReader reader = new(stream);

            TaikoRsPacketId pid = (TaikoRsPacketId)reader.ReadUInt16();

            switch (pid) {
                case TaikoRsPacketId.Server_LoginResponse: {
                    PacketServerLoginResponse packet = new();
                    packet.ReadPacket(reader);

                    this.UserId = packet.UserId;
                    this.State  = ConnectionState.Connected;
                    this.InvokeOnLoginComplete(this);
                    
                    break;
                }
            }
        }

        protected override async Task Disconnect() {
            await this._client.Stop(WebSocketCloseStatus.NormalClosure, "Client Disconnecting");
            
            this.InvokeOnDisconnect(this);
            this.State = ConnectionState.Disconnected;
        }
        
        public override async Task ChangeUserAction(UserAction action) {
            // new PacketClientStatusUpdate(action).GetPacket();

            this._client.Send(new PacketClientStatusUpdate(action).GetPacket());
        }

        protected override async Task ClientLogin() {
            this._client.Send(new PacketClientUserLogin(this.Username(), this.Password()).GetPacket());

            this.InvokeOnLoginStart(this);
            this.State = ConnectionState.Connecting;
        }

        protected override async Task ClientLogout() {
            this._client.Send(new PacketClientUserLogout().GetPacket());
            
            this.InvokeOnLogout(this);
            this.State = ConnectionState.Disconnected;
        }
    }

    public class TaikoRsReader : BinaryReader {
        public TaikoRsReader(Stream input) : base(input, Encoding.UTF8) {}
        public TaikoRsReader(Stream input, bool leaveOpen) : base(input, Encoding.UTF8, leaveOpen) {}

        public override string ReadString() {
            ulong length = this.ReadUInt64();

            byte[] data = this.ReadBytes((int)length);

            return Encoding.UTF8.GetString(data);
        }
    }
    
    public class TaikoRsWriter : BinaryWriter {
        public TaikoRsWriter(Stream input) : base(input, Encoding.UTF8) { } 
        public TaikoRsWriter(Stream input, bool leaveOpen) : base(input, Encoding.UTF8, leaveOpen) { }

        public override void Write(string value) {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            
            ulong length = (ulong)bytes.LongLength;
            
            this.Write(length);
            this.Write(bytes);
        }
    }
}
