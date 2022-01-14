using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Furball.Engine.Engine.Helpers;
using Microsoft.Xna.Framework;
using pTyping.Engine;
using pTyping.Online.Taiko_rs.Packets;
using pTyping.Scores;
using WebSocketSharp;
using ErrorEventArgs=WebSocketSharp.ErrorEventArgs;
using Logger=Kettu.Logger;

namespace pTyping.Online.Taiko_rs {
    public class TaikoRsOnlineManager : OnlineManager {
        public const ushort PROTOCOL_VERSION = 1;
        
        private readonly string     _getScoresUrl = "/get_scores";
        private readonly HttpClient _httpClient;
        private readonly Uri        _httpUri;

        private readonly string _scoreSubmitUrl = "/score_submit";

        private readonly Uri       _wsUri;
        private          WebSocket _client;

        public bool IsAlive => this._client.IsAlive;

        private readonly Queue<ChatMessage> _chatQueue = new();

        public TaikoRsOnlineManager(string wsUri, string httpUri) {
            this._wsUri   = new(wsUri);
            this._httpUri = new(httpUri);

            this._httpClient = new();
        }

        public override string Username() => ConVars.Username.Value;
        public override string Password() => CryptoHelper.GetSha512(Encoding.UTF8.GetBytes(ConVars.Password.Value));

        private Thread _sendThread;

        protected override void Connect() {
            if (this.State != ConnectionState.Disconnected)
                this.Disconnect();

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

            this._client.SslConfiguration.EnabledSslProtocols = SslProtocols.Tls13 | SslProtocols.Tls12;

            this._client.Connect();
            Logger.Log("Connection established", LoggerLevelOnlineInfo.Instance);

            this.InvokeOnConnect(this);

            this._sendPackets = true;
            this._sendThread  = new Thread(this.PacketSendMain);
            this._sendThread.Start();
        }

        public ConcurrentQueue<Packet> PacketQueue = new();

        private bool _sendPackets;

        private void PacketSendMain() {
            while (this._sendPackets) {
                if (this.PacketQueue.TryDequeue(out Packet packet) && this._client.IsAlive) {
                    MemoryStream  s = new();
                    TaikoRsWriter w = new(s);
                    packet.WriteDataToStream(w);
                    this._client.Send(s.ToArray());
                }

                Thread.Sleep(50);
            }
        }

        private void ClientOnOpen(object sender, EventArgs e) {
            this.State = ConnectionState.Connected;
        }

        private void ClientOnError(object sender, ErrorEventArgs e) {
            this.State = ConnectionState.Disconnected;
            this.Disconnect();
        }

        private void ClientOnClose(object sender, CloseEventArgs e) {
            this.State = ConnectionState.Disconnected;
            this.Disconnect();
        }

        private void SendUpdateScoreRequest() {
            this.PacketQueue.Enqueue(new ClientNotifyScoreUpdatePacket());
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
                pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Error, "Score submission failed!");
            }

            this.SendUpdateScoreRequest();
        }
        protected override async Task<List<PlayerScore>> ClientGetScores(string hash) {
            List<PlayerScore> scores = new();

            try {
                string finalUri = this._httpUri + this._getScoresUrl;

                MemoryStream  stream = new();
                TaikoRsWriter writer = new(stream);
                writer.Write(hash);
                writer.Write((byte)PlayMode.pTyping);
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
                pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Error, "Loading leaderboards failed!");
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

            PacketId pid = reader.ReadPacketId();

            bool success = pid switch {
                PacketId.ServerLoginResponse           => this.HandleServerLoginResponsePacket(reader),
                PacketId.ServerUserStatusUpdate        => this.HandleServerUserStatusUpdatePacket(reader),
                PacketId.ServerUserJoined              => this.HandleServerUserJoinedPacket(reader),
                PacketId.ServerUserLeft                => this.HandleServerUserLeftPacket(reader),
                PacketId.ServerSendMessage             => this.HandleServerSendMessagePacket(reader),
                PacketId.ServerScoreUpdate             => this.HandleServerScoreUpdatePacket(reader),
                PacketId.ServerSpectatorJoined         => throw new NotImplementedException(),
                PacketId.ServerSpectatorFrames         => throw new NotImplementedException(),
                PacketId.ServerSpectatorLeft           => throw new NotImplementedException(),
                PacketId.ServerSpectatorPlayingRequest => throw new NotImplementedException(),
                PacketId.Ping                          => true,
                PacketId.Pong                          => true,
                PacketId.Unknown                       => throw new Exception("Got Unknown packet id?"),
                _                                      => throw new Exception("Recieved client packet?")
            };

            if (reader.BaseStream.Position != reader.BaseStream.Length)
                throw new Exception("you didnt handle the packet all the way smh");

            if (!success) throw new Exception($"Error reading packet with PID: {pid}");
        }

        public override void SendMessage(string channel, string message) {
            message = message.Trim();

            if (string.IsNullOrWhiteSpace(message)) return;

            this.PacketQueue.Enqueue(new ClientSendMessagePacket(channel, message));
        }

        protected override void Disconnect() {
            this._sendPackets = false;

            this.PacketQueue.Clear();

            if (this._client.IsAlive) this._client.Close(CloseStatusCode.Normal, "Client Disconnecting");

            this.InvokeOnDisconnect(this);
            this.State = ConnectionState.Disconnected;

            Logger.Log("Disconnected from the server!", LoggerLevelOnlineInfo.Instance);
        }

        public override void ChangeUserAction(UserAction action) {
            this.PacketQueue.Enqueue(new ClientStatusUpdatePacket(action));
        }

        protected override void ClientLogin() {
            this.PacketQueue.Enqueue(new ClientUserLoginPacket(this.Username(), this.Password()));

            this.InvokeOnLoginStart(this);
            this.State = ConnectionState.LoggingIn;
        }

        protected override void ClientLogout() {
            if (this._client.ReadyState != WebSocketState.Open) return;

            this.PacketQueue.Enqueue(new ClientLogOutPacket());

            this.InvokeOnLogout(this);
            //Note we set to connected because we didnt actually *disconnect* yet, just log out
            this.State = ConnectionState.Connected;
        }

        #region Handle packets

        private bool HandleServerSendMessagePacket(TaikoRsReader reader) {
            ServerSendMessagePacket packet = new();
            packet.ReadDataFromStream(reader);

            if (this.OnlinePlayers.TryGetValue(packet.SenderId, out OnlinePlayer player)) {
                ChatMessage message = new(player, packet.Channel, packet.Message);
                this._chatQueue.Enqueue(message);

                Logger.Log(
                $"<{message.Time.Hour:00}:{message.Time.Minute:00}> [{message.Channel}] {message.Sender.Username}: {message.Message}",
                LoggerLevelChatMessage.Instance
                );
            }

            return true;
        }

        private bool HandleServerScoreUpdatePacket(TaikoRsReader reader) {
            ServerScoreUpdatePacket packet = new();
            packet.ReadDataFromStream(reader);

            if (this.OnlinePlayers.TryGetValue(packet.UserId, out OnlinePlayer player)) {
                player.TotalScore.Value  = packet.TotalScore;
                player.RankedScore.Value = packet.RankedScore;
                player.Accuracy.Value    = packet.Accuracy;
                player.PlayCount.Value   = packet.Playcount;
                player.Rank.Value        = packet.Rank;

                Logger.Log(
                $"Got score update packet: {player.Username}: {player.TotalScore}:{player.RankedScore}:{player.Accuracy}:{player.PlayCount}",
                LoggerLevelOnlineInfo.Instance
                );
            }

            return true;
        }

        private bool HandleServerUserStatusUpdatePacket(TaikoRsReader reader) {
            ServerUserStatusUpdatePacket packet = new();
            packet.ReadDataFromStream(reader);

            if (this.OnlinePlayers.TryGetValue(packet.UserId, out OnlinePlayer player)) {
                player.Action.Value = packet.Action;
                Logger.Log($"{player.Username} changed status to {player.Action.Value.Action} : {player.Action.Value.ActionText}!", LoggerLevelOnlineInfo.Instance);
            }

            return true;
        }

        private bool HandleServerUserLeftPacket(TaikoRsReader reader) {
            ServerUserLeftPacket packet = new();
            packet.ReadDataFromStream(reader);

            if (this.OnlinePlayers.Remove(packet.UserId, out OnlinePlayer playerLeft))
                Logger.Log($"{playerLeft.Username} has gone offline!", LoggerLevelOnlineInfo.Instance);

            return true;
        }

        private bool HandleServerLoginResponsePacket(TaikoRsReader reader) {
            ServerLoginResponsePacket packet = new();
            packet.ReadDataFromStream(reader);

            switch (packet.LoginStatus) {
                case LoginStatus.NoUser: {
                    Logger.Log("Login failed! User not found.", LoggerLevelOnlineInfo.Instance);
                    pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Error, "Login failed! (user not found)");

                    this.Disconnect();
                    return true;
                }
                case LoginStatus.BadPassword: {
                    Logger.Log("Login failed! Password incorrect!", LoggerLevelOnlineInfo.Instance);
                    pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Error, "Login failed! (incorrect password)");

                    this.Disconnect();
                    return true;
                }
                case LoginStatus.UnknownError: {
                    Logger.Log("Login failed! Unknown reason", LoggerLevelOnlineInfo.Instance);
                    pTypingGame.NotificationManager.CreateNotification(
                    NotificationManager.NotificationImportance.Error,
                    "Login failed! (unknown reason, contact eve)"
                    );

                    this.Disconnect();
                    return true;
                }
            }

            this.Player.Username.Value = this.Username();
            this.Player.UserId.Value   = packet.UserId;
            this.State                 = ConnectionState.LoggedIn;
            
            this.InvokeOnLoginComplete(this);

            this.OnlinePlayers.Add(this.Player.UserId, this.Player);

            pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Info, $"Login complete! Welcome {this.Player.Username}!");

            this.PacketQueue.Enqueue(new ClientStatusUpdatePacket(new UserAction(UserActionType.Idle, "")));
            this.PacketQueue.Enqueue(new ClientNotifyScoreUpdatePacket());
            
            return true;
        }

        public override void Update(GameTime time) {
            while (this._chatQueue.TryDequeue(out ChatMessage message))
                this.ChatLog.Add(message);

            base.Update(time);
        }

        private bool HandleServerUserJoinedPacket(TaikoRsReader reader) {
            ServerUserJoinedPacket packet = new();
            packet.ReadDataFromStream(reader);

            OnlinePlayer player = new() {
                Username = {
                    Value = packet.Username
                },
                UserId = {
                    Value = packet.UserId
                }
            };

            if (this.OnlinePlayers.ContainsKey(packet.UserId)) return true;

            this.OnlinePlayers.Add(packet.UserId, player);
            Logger.Log($"{player.Username} is online!", LoggerLevelOnlineInfo.Instance);

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

        public PacketId ReadPacketId() {
            PacketId pid = (PacketId)this.ReadUInt16();

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
