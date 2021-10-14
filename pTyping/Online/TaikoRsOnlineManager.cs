using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using pTyping.LoggingLevels;
using pTyping.Online.TaikoRsPackets;
using pTyping.Player;
using WebSocketSharp;
using ErrorEventArgs=WebSocketSharp.ErrorEventArgs;
using Logger=Furball.Engine.Engine.Helpers.Logger.Logger;

namespace pTyping.Online {
    public class TaikoRsOnlineManager : OnlineManager {
        private readonly Uri _httpUri;

        private readonly Uri        _wsUri;
        private          WebSocket  _client;
        private readonly HttpClient _httpClient;

        private readonly string _scoreSubmitUrl = "/score_submit";

        public TaikoRsOnlineManager(string wsUri, string httpUri) {
            this._wsUri   = new(wsUri);
            this._httpUri = new(httpUri);

            this._httpClient = new();
        }

        public override string Username() => Config.Username;
        public override string Password() => Config.Password;

        protected override async Task Connect() {
            if (this.State != ConnectionState.Disconnected)
                await this.Disconnect();

            this.InvokeOnConnectStart(this);

            this._client = new WebSocket(this._wsUri.ToString());

            #region Disable logging

            FieldInfo field = this._client.Log.GetType().GetField("_output", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(this._client.Log, new Action<LogData, string>((_, _) => {}));

            #endregion

            this._client.OnMessage += this.HandleMessage;
            this._client.OnClose   += this.ClientOnClose;
            this._client.OnError   += this.ClientOnError;
            this._client.OnOpen    += this.ClientOnOpen;

            await Task.Run(() => this._client.Connect());

            this.InvokeOnConnect(this);
        }

        private void ClientOnOpen(object? sender, EventArgs e) {
            this.State = ConnectionState.Connected;
        }

        private void ClientOnError(object sender, ErrorEventArgs e) {
            this.State = ConnectionState.Disconnected;
        }

        private void ClientOnClose(object sender, CloseEventArgs e) {
            this.State = ConnectionState.Disconnected;
        }

        protected override async Task ClientSubmitScore(PlayerScore score) {
            string finalUri = this._httpUri + this._scoreSubmitUrl;

            HttpContent content = new ByteArrayContent(score.TaikoRsSerialize());

            await this._httpClient.PostAsync(finalUri, content);
        }

        private void HandleMessage(object sender, MessageEventArgs args) {
            if (args.IsText) throw new InvalidDataException("The server you are connecting to does not follow the rules of the Taiko.rs server infrastructure");

            MemoryStream  stream = new(args.RawData);
            TaikoRsReader reader = new(stream);

            TaikoRsPacketId pid = (TaikoRsPacketId)reader.ReadUInt16();

            switch (pid) {
                case TaikoRsPacketId.Server_LoginResponse: {
                    PacketServerLoginResponse packet = new();
                    packet.ReadPacket(reader);

                    this.UserId = packet.UserId;
                    this.State  = ConnectionState.LoggedIn;
                    this.InvokeOnLoginComplete(this);

                    break;
                }
                case TaikoRsPacketId.Server_UserJoined: {
                    PacketServerUserJoined packet = new();
                    packet.ReadPacket(reader);

                    if (this.OnlinePlayers.ContainsKey(packet.Player.UserId)) break;

                    this.OnlinePlayers.Add(packet.Player.UserId, packet.Player);
                    Logger.Log($"{packet.Player.Username} joined!", new LoggerLevelOnlineInfo());

                    break;
                }
                case TaikoRsPacketId.Server_UserLeft: {
                    PacketServerUserLeft packet = new();
                    packet.ReadPacket(reader);

                    if (this.OnlinePlayers.Remove(packet.UserId, out OnlinePlayer playerLeft))
                        Logger.Log($"{playerLeft.Username} left!", new LoggerLevelOnlineInfo());

                    break;
                }
                case TaikoRsPacketId.Server_UserStatusUpdate: {
                    PacketServerUserStatusUpdate packet = new();
                    packet.ReadPacket(reader);

                    if (this.OnlinePlayers.TryGetValue(packet.UserId, out OnlinePlayer player)) {
                        player.Action = packet.Action;
                        Logger.Log($"{player.Username} changed status to {player.Action.Action} : {player.Action.ActionText}!", new LoggerLevelOnlineInfo());
                    }

                    break;
                }
                case TaikoRsPacketId.Server_SendMessage: {
                    PacketServerSendMessage packet = new();
                    packet.ReadPacket(reader);

                    if (this.OnlinePlayers.TryGetValue(packet.UserId, out OnlinePlayer player)) {
                        ChatMessage message = new(player, packet.Channel, packet.Message);
                        this.ChatLog.Add(message);

                        Logger.Log($"<{message.Time.Hour:00}:{message.Time.Minute:00}> {message.Sender}: {message.Message}", new LoggerLevelChatMessage());
                    }

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
            await Task.Run(() => this._client.Send(new PacketClientStatusUpdate(action).GetPacket()));
        }

        protected override async Task ClientLogin() {
            await Task.Run(() => this._client.Send(new PacketClientUserLogin(this.Username(), this.Password()).GetPacket()));

            this.InvokeOnLoginStart(this);
            this.State = ConnectionState.LoggingIn;
        }

        protected override async Task ClientLogout() {
            await Task.Run(() => this._client.Send(new PacketClientUserLogout().GetPacket()));

            this.InvokeOnLogout(this);
            this.State = ConnectionState.Connected;
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
        public TaikoRsWriter(Stream input) : base(input, Encoding.UTF8) {}
        public TaikoRsWriter(Stream input, bool leaveOpen) : base(input, Encoding.UTF8, leaveOpen) {}

        public override void Write(string value) {
            byte[] bytes = Encoding.UTF8.GetBytes(value);

            ulong length = (ulong)bytes.LongLength;

            this.Write(length);
            this.Write(bytes);
        }
    }
}
