using System;
using System.Collections.Generic;
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
        private readonly string _getScoresUrl = "/get_scores";

        public TaikoRsOnlineManager(string wsUri, string httpUri) {
            this._wsUri   = new(wsUri);
            this._httpUri = new(httpUri);

            this._httpClient = new();
        }

        public override string Username() => ConVars.Username.Value;
        public override string Password() => ConVars.Password.Value;

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

        private void ClientOnOpen(object sender, EventArgs e) {
            this.State = ConnectionState.Connected;
        }

        private void ClientOnError(object sender, ErrorEventArgs e) {
            this.State = ConnectionState.Disconnected;
        }

        private void ClientOnClose(object sender, CloseEventArgs e) {
            this.State = ConnectionState.Disconnected;
        }

        protected override async Task ClientSubmitScore(PlayerScore score) {
            try {
                string finalUri = this._httpUri + this._scoreSubmitUrl;

                HttpContent content = new ByteArrayContent(score.TaikoRsSerialize());

                await this._httpClient.PostAsync(finalUri, content);
            }
            catch {
                //TODO tell the user the score submission failed
            }
        }
        protected override async Task<List<PlayerScore>> ClientGetScores(string hash) {
            List<PlayerScore> scores = new();
                
            try {
                string finalUri = this._httpUri + this._getScoresUrl;

                MemoryStream  stream = new();
                TaikoRsWriter writer = new(stream);
                writer.Write(hash);
                writer.Flush();

                HttpContent content = new ByteArrayContent(stream.ToArray());

                Task<HttpResponseMessage> task = this._httpClient.PostAsync(finalUri, content);
                
                await task;

                TaikoRsReader reader = new(task.Result.Content.ReadAsStream());

                ulong length = reader.ReadUInt64();

                for (ulong i = 0; i < length; i++) 
                    scores.Add(PlayerScore.TaikoRsDeserialize(reader));
            }
            catch {
                //TODO tell the user that getting the scores failed
            }

            return scores;
        }

        private static void CheckMessageIntegrity(MessageEventArgs args) {
            if (args.IsText) throw new InvalidDataException("Recieved non-binary data!");

            if(args.RawData.Length == 0) throw new InvalidDataException("Recieved empty data packet!");
        }

        private void HandleMessage(object sender, MessageEventArgs args) {
            CheckMessageIntegrity(args);
            
            MemoryStream  stream = new(args.RawData);
            TaikoRsReader reader = new(stream);

            TaikoRsPacketId pid = reader.ReadPacketId();

            bool success = pid switch {
                TaikoRsPacketId.Server_LoginResponse    => this.HandleServerLoginResponsePacket(reader),
                TaikoRsPacketId.Server_UserStatusUpdate => this.HandleServerUserStatusUpdatePacket(reader),
                TaikoRsPacketId.Server_UserJoined       => this.HandleServerUserJoinedPacket(reader),
                TaikoRsPacketId.Server_UserLeft         => this.HandleServerUserLeftPacket(reader),
                TaikoRsPacketId.Server_SendMessage      => this.HandleServerSendMessagePacket(reader),
                TaikoRsPacketId.Server_SpectatorJoined  => throw new NotImplementedException(),
                TaikoRsPacketId.Server_SpectatorFrames  => throw new NotImplementedException(),
                TaikoRsPacketId.Unknown                 => throw new Exception("Got Unknown packet id?"),
                _                                       => throw new Exception("Recieved client packet?")
            };

            if (!success) throw new Exception($"Error reading packet with PID: {pid}");
        }
        
        #region Handle packets
        private bool HandleServerSendMessagePacket(TaikoRsReader reader) {
            PacketServerSendMessage packet = new();
            packet.ReadPacket(reader);

            if (this.OnlinePlayers.TryGetValue(packet.UserId, out OnlinePlayer player)) {
                ChatMessage message = new(player, packet.Channel, packet.Message);
                this.ChatLog.Add(message);

                Logger.Log($"<{message.Time.Hour:00}:{message.Time.Minute:00}> {message.Sender}: {message.Message}", new LoggerLevelChatMessage());
            }

            return true;
        }
        
        private bool HandleServerUserStatusUpdatePacket(TaikoRsReader reader) {
            PacketServerUserStatusUpdate packet = new();
            packet.ReadPacket(reader);

            if (this.OnlinePlayers.TryGetValue(packet.UserId, out OnlinePlayer player)) {
                player.Action = packet.Action;
                Logger.Log($"{player.Username} changed status to {player.Action.Action} : {player.Action.ActionText}!", new LoggerLevelOnlineInfo());
            }

            return true;
        }
        
        private bool HandleServerUserLeftPacket(TaikoRsReader reader) {
            PacketServerUserLeft packet = new();
            packet.ReadPacket(reader);

            if (this.OnlinePlayers.Remove(packet.UserId, out OnlinePlayer playerLeft))
                Logger.Log($"{playerLeft.Username} left!", new LoggerLevelOnlineInfo());

            return true;
        }
        
        private bool HandleServerLoginResponsePacket(TaikoRsReader reader) {
            PacketServerLoginResponse packet = new();
            packet.ReadPacket(reader);

            this.UserId = packet.UserId;
            this.State  = ConnectionState.LoggedIn;
            this.InvokeOnLoginComplete(this);

            return true;
        }
        
        private bool HandleServerUserJoinedPacket(TaikoRsReader reader) {
            PacketServerUserJoined packet = new();
            packet.ReadPacket(reader);

            if (this.OnlinePlayers.ContainsKey(packet.Player.UserId)) return true;

            this.OnlinePlayers.Add(packet.Player.UserId, packet.Player);
            Logger.Log($"{packet.Player.Username} joined!", new LoggerLevelOnlineInfo());

            return true;
        }
        #endregion

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

        public TaikoRsPacketId ReadPacketId() {
            TaikoRsPacketId pid = (TaikoRsPacketId)this.ReadUInt16();

            return pid;
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