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
using Logger=Kettu.Logger;

namespace pTyping.Online {
    public class TaikoRsOnlineManager : OnlineManager {
        private readonly string     _getScoresUrl = "/get_scores";
        private readonly HttpClient _httpClient;
        private readonly Uri        _httpUri;

        private readonly string _scoreSubmitUrl = "/score_submit";

        private readonly Uri       _wsUri;
        private          WebSocket _client;

        public bool IsAlive => this._client.IsAlive;

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
            this.Disconnect().Wait();
        }

        private void ClientOnClose(object sender, CloseEventArgs e) {
            this.State = ConnectionState.Disconnected;
            this.Disconnect().Wait();
        }

        private async Task SendUpdateScoreRequest() {
            await this._client.SendRealAsync(new PacketClientNotifyScoreUpdate().GetPacket());
        }

        protected override async Task ClientSubmitScore(PlayerScore score) {
            try {
                string finalUri = this._httpUri + this._scoreSubmitUrl;

                MemoryStream  stream = new();
                TaikoRsWriter writer = new(stream);

                writer.Write(score.TaikoRsSerialize());
                writer.Write(this.Password());

                writer.Flush();

                HttpContent content = new ByteArrayContent(stream.ToArray());

                await this._httpClient.PostAsync(finalUri, content);
            }
            catch {
                //TODO tell the user the score submission failed
            }

            await this.SendUpdateScoreRequest();
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

            if (args.RawData.Length == 0) throw new InvalidDataException("Recieved empty data packet!");
        }

        private void HandleMessage(object sender, MessageEventArgs args) {
            CheckMessageIntegrity(args);

            MemoryStream  stream = new(args.RawData);
            TaikoRsReader reader = new(stream);

            TaikoRsPacketId pid = reader.ReadPacketId();

            bool success = pid switch {
                TaikoRsPacketId.ServerLoginResponse    => this.HandleServerLoginResponsePacket(reader),
                TaikoRsPacketId.ServerUserStatusUpdate => this.HandleServerUserStatusUpdatePacket(reader),
                TaikoRsPacketId.ServerUserJoined       => this.HandleServerUserJoinedPacket(reader),
                TaikoRsPacketId.ServerUserLeft         => this.HandleServerUserLeftPacket(reader),
                TaikoRsPacketId.ServerSendMessage      => this.HandleServerSendMessagePacket(reader),
                TaikoRsPacketId.ServerScoreUpdate      => this.HandleServerScoreUpdatePacket(reader),
                TaikoRsPacketId.ServerSpectatorJoined  => throw new NotImplementedException(),
                TaikoRsPacketId.ServerSpectatorFrames  => throw new NotImplementedException(),
                TaikoRsPacketId.Unknown                => throw new Exception("Got Unknown packet id?"),
                _                                      => throw new Exception("Recieved client packet?")
            };

            if (!success) throw new Exception($"Error reading packet with PID: {pid}");
        }

        public override async Task SendMessage(string channel, string message) {
            await this._client.SendRealAsync(new PacketClientSendMessage(channel, message).GetPacket());
        }

        protected override async Task Disconnect() {
            await Task.Run(
            () => {
                if (this._client.IsAlive) this._client.Close(CloseStatusCode.Normal, "Client Disconnecting");
            }
            );

            this.InvokeOnDisconnect(this);
            this.State = ConnectionState.Disconnected;

            Logger.Log("Disconnected from the server!", new LoggerLevelOnlineInfo());
        }

        public override async Task ChangeUserAction(UserAction action) {
            // this.Player.Action.Value = action;

            Logger.Log($"Sending action {action.Action} {action.ActionText}");

            await this._client.SendRealAsync(new PacketClientStatusUpdate(action).GetPacket());
            ;
        }

        protected override async Task ClientLogin() {
            await this._client.SendRealAsync(new PacketClientUserLogin(this.Username(), this.Password()).GetPacket());

            this.InvokeOnLoginStart(this);
            this.State = ConnectionState.LoggingIn;
        }

        protected override async Task ClientLogout() {
            if (this._client.ReadyState != WebSocketState.Open) return;

            await this._client.SendRealAsync(new PacketClientUserLogout().GetPacket());

            this.InvokeOnLogout(this);
            this.State = ConnectionState.Connected;
        }

        #region Handle packets

        private bool HandleServerPongPacket() => true;

        private bool HandleServerSendMessagePacket(TaikoRsReader reader) {
            PacketServerSendMessage packet = new();
            packet.ReadPacket(reader);

            if (this.OnlinePlayers.TryGetValue(packet.UserId, out OnlinePlayer player)) {
                ChatMessage message = new(player, packet.Channel, packet.Message);
                this.ChatLog.Add(message);

                Logger.Log(
                $"<{message.Time.Hour:00}:{message.Time.Minute:00}> [{message.Channel}] {message.Sender.Username}: {message.Message}",
                new LoggerLevelChatMessage()
                );
            }

            return true;
        }

        private bool HandleServerScoreUpdatePacket(TaikoRsReader reader) {
            PacketServerScoreUpdate packet = new();
            packet.ReadPacket(reader);

            if (this.OnlinePlayers.TryGetValue(packet.UserId, out OnlinePlayer player)) {
                player.TotalScore.Value  = packet.TotalScore;
                player.RankedScore.Value = packet.RankedScore;
                player.Accuracy.Value    = packet.Accuracy;
                player.PlayCount.Value   = packet.PlayCount;

                Logger.Log(
                $"Got score update packet: {player.Username}: {player.TotalScore}:{player.RankedScore}:{player.Accuracy}:{player.PlayCount}",
                new LoggerLevelOnlineInfo()
                );
            }

            return true;
        }

        private bool HandleServerUserStatusUpdatePacket(TaikoRsReader reader) {
            PacketServerUserStatusUpdate packet = new();
            packet.ReadPacket(reader);

            if (this.OnlinePlayers.TryGetValue(packet.UserId, out OnlinePlayer player)) {
                player.Action.Value = packet.Action;
                Logger.Log($"{player.Username} changed status to {player.Action.Value.Action} : {player.Action.Value.ActionText}!", new LoggerLevelOnlineInfo());
            }

            return true;
        }

        private bool HandleServerUserLeftPacket(TaikoRsReader reader) {
            PacketServerUserLeft packet = new();
            packet.ReadPacket(reader);

            if (this.OnlinePlayers.Remove(packet.UserId, out OnlinePlayer playerLeft))
                Logger.Log($"{playerLeft.Username} has gone offline!", new LoggerLevelOnlineInfo());

            return true;
        }

        private bool HandleServerLoginResponsePacket(TaikoRsReader reader) {
            PacketServerLoginResponse packet = new();
            packet.ReadPacket(reader);

            //TODO notify user when login failed properly
            if ((packet.UserId & (1 << 31)) != 0)
                switch (packet.UserId) {
                    case -1: {
                        Logger.Log("Login failed! User not found.", new LoggerLevelOnlineInfo());
                        this.Disconnect().Wait();
                        return true;
                    }
                    case -2: {
                        Logger.Log("Login failed! Password incorrect!", new LoggerLevelOnlineInfo());
                        this.Disconnect().Wait();
                        return true;
                    }
                }

            this.Player.Username.Value = this.Username();
            this.Player.UserId.Value   = packet.UserId;
            this.State                 = ConnectionState.LoggedIn;

            this.InvokeOnLoginComplete(this);

            this.OnlinePlayers.Add(this.Player.UserId, this.Player);

            return true;
        }

        private bool HandleServerUserJoinedPacket(TaikoRsReader reader) {
            PacketServerUserJoined packet = new();
            packet.ReadPacket(reader);

            if (this.OnlinePlayers.ContainsKey(packet.Player.UserId)) return true;

            this.OnlinePlayers.Add(packet.Player.UserId, packet.Player);
            Logger.Log($"{packet.Player.Username} is online!", new LoggerLevelOnlineInfo());

            return true;
        }

        #endregion
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
