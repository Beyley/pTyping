using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using pTyping.LoggingLevels;
using WebSocketSharp;
using pTyping.Online.TaikoRsPackets;
using ErrorEventArgs=WebSocketSharp.ErrorEventArgs;
using Logger=Furball.Engine.Engine.Helpers.Logger.Logger;

namespace pTyping.Online {
    public class TaikoRsOnlineManager : OnlineManager {
        private WebSocket _client;

        private readonly Uri _uri;

        public TaikoRsOnlineManager(string uri) => this._uri = new(uri);
        
        public override string Username() => Config.Username;
        public override string Password() => Config.Password;

        protected override async Task Connect() {
            if (this.State == ConnectionState.Connected)
                await this.Disconnect();
            
            this.InvokeOnConnectStart(this);
            
            this._client = new(this._uri.ToString());
            await Task.Run(() => this._client.Connect());
            this._client.OnMessage += this.HandleMessage;
            this._client.OnClose += this.ClientOnClose;
            this._client.OnError += this.ClientOnError;

            this.InvokeOnConnect(this);
        }
        
        private void ClientOnError(object sender, ErrorEventArgs e) {
            this.State = ConnectionState.Disconnected;
        }

        private void ClientOnClose(object sender, CloseEventArgs e) {
            this.State = ConnectionState.Disconnected;
        }

        private void HandleMessage(object sender, MessageEventArgs message) {
            if (message.IsText) throw new InvalidDataException("The server you are connecting to does not follow the rules of the Taiko.rs server infrastructure");

            MemoryStream  stream = new(message.RawData);
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
                case TaikoRsPacketId.Server_UserJoined: {
                    PacketServerUserJoined packet = new();
                    packet.ReadPacket(reader);

                    if (this.Players.ContainsKey(packet.Player.UserId)) break;
                    
                    this.Players.Add(packet.Player.UserId, packet.Player);
                    Logger.Log($"{packet.Player.Username} joined!", new LoggerLevelOnlineInfo());
                    
                    break;
                }
                case TaikoRsPacketId.Server_UserLeft: {
                    PacketServerUserLeft packet = new();
                    packet.ReadPacket(reader);

                    if(this.Players.Remove(packet.UserId, out OnlinePlayer playerLeft))
                        Logger.Log($"{playerLeft.Username} left!", new LoggerLevelOnlineInfo());
                    
                    break;
                }
                case TaikoRsPacketId.Server_UserStatusUpdate: {
                    PacketServerUserStatusUpdate packet = new();
                    packet.ReadPacket(reader);

                    if (this.Players.TryGetValue(packet.UserId, out OnlinePlayer player)) {
                        player.Action = packet.Action;
                        Logger.Log($"{player.Username} changed status to {player.Action.Action} : {player.Action.ActionText}!", new LoggerLevelOnlineInfo());
                    }
                    
                    break;
                }
                case TaikoRsPacketId.Server_SendMessage: {
                    PacketServerSendMessage packet = new();
                    packet.ReadPacket(reader);

                    Logger.Log($"Got message: {packet.Message} in channel {packet.Channel}", new LoggerLevelOnlineInfo());
                    
                    break;
                }
            }
        }

        public override async Task SendMessage(string channel, string message) {
            await Task.Run(() => this._client.Send(new PacketClientSendMessage(channel, message).GetPacket()));
        }

        protected override async Task Disconnect() {
            await Task.Run(() => this._client.Close(CloseStatusCode.Normal, "Client Disconnecting"));
            
            this.InvokeOnDisconnect(this);
            this.State = ConnectionState.Disconnected;
        }
        
        public override async Task ChangeUserAction(UserAction action) {
            // new PacketClientStatusUpdate(action).GetPacket();

            await Task.Run(() => this._client.Send(new PacketClientStatusUpdate(action).GetPacket()));
        }

        protected override async Task ClientLogin() {
            await Task.Run(() => this._client.Send(new PacketClientUserLogin(this.Username(), this.Password()).GetPacket()));

            this.InvokeOnLoginStart(this);
            this.State = ConnectionState.Connecting;
        }

        protected override async Task ClientLogout() {
            await Task.Run(() => this._client.Send(new PacketClientUserLogout().GetPacket()));
            
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
