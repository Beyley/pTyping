using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Helpers;
using Furball.Engine.Engine.Platform;
using pTyping.Engine;
using pTyping.Graphics.Menus;
using pTyping.Graphics.Menus.SongSelect;
using pTyping.Graphics.Player;
using pTyping.Online.Taiko_rs.Packets;
using pTyping.Scores;
using pTyping.Songs;
using WebSocketSharp;
using ErrorEventArgs=WebSocketSharp.ErrorEventArgs;
using Logger=Kettu.Logger;

namespace pTyping.Online.Taiko_rs;

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

    public override string Username() => ConVars.Username.Value.Value;
    public override string Password() => CryptoHelper.GetSha512(Encoding.UTF8.GetBytes(ConVars.Password.Value.Value));

    private Thread _sendThread;
    private Thread _replayFrameThread;

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

        this._client.Compression = CompressionMethod.Deflate;

        this._client.Connect();
        Logger.Log("Connection established", LoggerLevelOnlineInfo.Instance);

        this.InvokeOnConnect(this);

        this._sendPackets = true;
        this._sendThread  = new Thread(this.SpectatorFrameQueueRun);
        this._sendThread.Start();

        this._spectatorFrameSendThreadRun = true;
        this._replayFrameThread           = new Thread(this.PacketSendMain);
        this._replayFrameThread.Start();

        this._lastFrameSendTime = UnixTime.Now();

        FurballGame.Instance.AfterScreenChange  += this.OnScreenChangeAfter;
        FurballGame.Instance.BeforeScreenChange += this.OnScreenChangeBefore;
    }

    private long _lastFrameSendTime;
    private void SpectatorFrameQueueRun() {
        while (this._spectatorFrameSendThreadRun) {
            if ((double)UnixTime.Now() - this._lastFrameSendTime > 1 || this.SpectatorFrameQueue.Count > 10) {
                this.SendSpectatorFrames();
                this._lastFrameSendTime = UnixTime.Now();
            }

            Thread.Sleep(50);
        }
    }

    private void SendSpectatorFrames() {
        if (this.SpectatorFrameQueue.IsEmpty) return;

        Logger.Log($"Sending spectator frames c:{this.SpectatorFrameQueue.Count}", LoggerLevelOnlineInfo.Instance);

        this.PacketQueue.Enqueue(
        new ClientSpectatorFramesPacket {
            Frames = this.SpectatorFrameQueue.ToArray()
        }
        );
        this.SpectatorFrameQueue.Clear();
    }

    private void OnScreenChangeAfter(object sender, Screen e) {
        lock (this.Spectators) {
            if (this.Spectators.Count == 0) return;
        }

        switch (e) {
            case PlayerScreen: {
                SpectatorFramePlay frame = new() {
                    Mode        = PlayMode.pTyping,
                    BeatmapHash = pTypingGame.CurrentSong.Value.MapHash,
                    Modinfo = new() {
                        Autoplay = false,
                        Speed    = 1f
                    },
                    Time = 0
                };

                Logger.Log("Sending spectate frame play", LoggerLevelOnlineInfo.Instance);
                this.PacketQueue.Enqueue(
                new ClientSpectatorFramesPacket {
                    Frames = new SpectatorFrame[] {
                        frame
                    }
                }
                );
                break;
            }
            case SongSelectionScreen or MenuScreen:
                Logger.Log("Sending spectate frame change song", LoggerLevelOnlineInfo.Instance);
                this.SpectatorFrameQueue.Enqueue(
                new SpectatorFrameChangingMap {
                    Time = 0
                }
                );
                break;
        }
    }

    public override void SpectatorPause(double time) {
        lock (this.Spectators) {
            if (this.Spectators.Count == 0) return;
        }

        Logger.Log("Sending spectator pause", LoggerLevelOnlineInfo.Instance);
        this.SpectatorFrameQueue.Enqueue(
        new SpectatorFramePause {
            Time = (float)time
        }
        );
    }

    public override void SpectatorResume(double time) {
        lock (this.Spectators) {
            if (this.Spectators.Count == 0) return;
        }

        Logger.Log("Sending spectator resume", LoggerLevelOnlineInfo.Instance);
        this.SpectatorFrameQueue.Enqueue(
        new SpectatorFrameUnpause {
            Time = (float)time
        }
        );
    }
    public override void SpectatorBuffer(double time) {
        lock (this.Spectators) {
            if (this.Spectators.Count == 0) return;
        }

        Logger.Log("Sending spectator buffer", LoggerLevelOnlineInfo.Instance);
        this.SpectatorFrameQueue.Enqueue(
        new SpectatorFrameBuffer {
            Time = (float)time
        }
        );
    }
    public override void SpectatorScoreSync(double time, PlayerScore score) {
        lock (this.Spectators) {
            if (this.Spectators.Count == 0) return;
        }

        Logger.Log("Sending score frame", LoggerLevelOnlineInfo.Instance);
        this.SpectatorFrameQueue.Enqueue(
        new SpectatorFrameScoreSync {
            Time  = (float)time,
            Score = score
        }
        );
    }
    public override void SpectatorReplayFrame(double time, ReplayFrame frame) {
        lock (this.Spectators) {
            if (this.Spectators.Count == 0) return;
        }

        Logger.Log("Sending replay frame", LoggerLevelOnlineInfo.Instance);
        this.SpectatorFrameQueue.Enqueue(
        new SpectatorFrameReplayFrame {
            Time  = (float)time,
            Frame = frame
        }
        );
    }

    public override void SpectatePlayer(OnlinePlayer player) {
        if (this.Host != null)
            return;

        // this.Host = player;

        this.PacketQueue.Enqueue(
        new ClientSpectatePacket {
            HostId = player.UserId
        }
        );

        Logger.Log($"Trying to spectate {player.Username}", LoggerLevelOnlineInfo.Instance);

        pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Info, $"Attepmting to spectate {player.Username}");
    }

    private void OnScreenChangeBefore(object sender, Screen e) {
        if (e is PlayerScreen) {
            // lock(this.Spectators)
            //     if (this.Spectators.Count == 0) return;
            //
            // SpectatorFrameStop frame = new();
            //
            // this.SpectatorFrameQueue.Enqueue(frame);
        }
    }

    public ConcurrentQueue<Packet>         PacketQueue         = new();
    public ConcurrentQueue<SpectatorFrame> SpectatorFrameQueue = new();

    private bool _sendPackets;
    private bool _spectatorFrameSendThreadRun;

    private long _lastPing;

    private void PacketSendMain() {
        while (this._sendPackets) {
            if (this.PacketQueue.TryDequeue(out Packet packet) && this._client.IsAlive) {
                MemoryStream  s = new();
                TaikoRsWriter w = new(s);
                packet.WriteDataToStream(w);
                this._client.Send(s.ToArray());
            }

            if (UnixTime.Now() - this._lastPing > 5) {
                this._lastPing = UnixTime.Now();
                this._client.Ping();
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
        this.ScheduleAutomaticReconnect();
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
            PacketId.ServerSpectatorJoined         => this.HandleServerSpectatorJoinedPacket(reader),
            PacketId.ServerSpectatorFrames         => this.HandleServerSpectatorFrames(reader),
            PacketId.ServerSpectatorLeft           => this.HandleServerSpectatorLeftPacket(reader),
            PacketId.ServerNotification            => this.HandleServerNotificationPacket(reader),
            PacketId.ServerDropConnection          => this.HandleServerDropConnectionPacket(reader),
            PacketId.ServerError                   => this.HandleServerErrorPacket(reader),
            PacketId.ServerSpectatorPlayingRequest => this.HandleServerSpectatorPlayingRequest(reader),
            PacketId.ServerSpectateResult          => this.HandleServerSpectateResult(reader),
            PacketId.ServerPermissions             => throw new NotImplementedException(),
            #region we ignore these

            PacketId.Ping => true,
            PacketId.Pong => true,

            #endregion
            PacketId.Unknown => throw new Exception("Got Unknown packet id?"),
            _                => throw new Exception("Recieved client packet?")
        };

        if (RuntimeInfo.IsDebug() && reader.BaseStream.Position != reader.BaseStream.Length)
            throw new Exception($"Packet {pid} was not fully read!");

        if (!success) throw new Exception($"Error reading packet with PID: {pid}");
    }

    private bool HandleServerSpectateResult(TaikoRsReader reader) {
        ServerSpectateResultPacket p = new();

        p.ReadDataFromStream(reader);

        switch (p.Result) {
            case SpectateResult.Ok:
                lock (this.OnlinePlayers) {
                    if (this.OnlinePlayers.TryGetValue(p.HostId, out OnlinePlayer player)) {
                        this.Host = player;
                        pTypingGame.NotificationManager.CreateNotification(
                        NotificationManager.NotificationImportance.Info,
                        $"You have started spectating {player.Username}!"
                        );
                    } else {
                        goto case SpectateResult.ErrorUnknown;
                    }
                }
                break;
            case SpectateResult.ErrorSpectatingBot:
                pTypingGame.NotificationManager.CreateNotification(
                NotificationManager.NotificationImportance.Error,
                "You cannot spectate a bot! (yo wait thats bars)"
                );
                this.Host = null;
                break;
            case SpectateResult.ErrorHostOffline:
                pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Error, "The host is offline!");
                this.Host = null;
                break;
            case SpectateResult.ErrorSpectatingYourself:
                pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Error, "You are unable to spectate yourself!");
                this.Host = null;
                break;
            case SpectateResult.ErrorUnknown:
                pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Error, "Unable to spectate player!");
                this.Host = null;
                break;
            default: {
                this.Host = null;
                throw new ArgumentOutOfRangeException();
            }
        }

        return true;
    }

    private bool HandleServerSpectatorPlayingRequest(TaikoRsReader reader) {
        ServerSpectatorPlayingRequestPacket p = new();

        p.ReadDataFromStream(reader);

        //TODO: Actually implement this

        return true;
    }

    private bool HandleServerErrorPacket(TaikoRsReader reader) {
        ServerErrorPacket p = new();
        p.ReadDataFromStream(reader);

        pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Error, $"Server Error {p.ErrorCode}");

        return true;
    }

    private bool HandleServerDropConnectionPacket(TaikoRsReader reader) {
        ServerDropConnectionPacket p = new();
        p.ReadDataFromStream(reader);

        string final = "Dropped from server!\nReason: ";

        switch (p.Reason) {
            case ServerDropReason.Other: {
                final += $"Unknown, Details: {p.Message}";
                break;
            }
            case ServerDropReason.BadPacket: {
                final += "Packet Reading Error";

                if (RuntimeInfo.IsDebug())
                    final += $"\n{p.Message}";
                break;
            }
            case ServerDropReason.OtherLogin: {
                final += "Someone else logged in with your account!";
                break;
            }
            case ServerDropReason.ServerClosing: {
                final += "Server Closing!";
                break;
            }
        }

        pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Error, final);

        this.Disconnect();

        return true;
    }


    private bool HandleServerNotificationPacket(TaikoRsReader reader) {
        ServerNotificationPacket p = new();
        p.ReadDataFromStream(reader);

        pTypingGame.NotificationManager.CreateNotification(p.Importance, p.Message);

        return true;
    }

    private bool HandleServerSpectatorFrames(TaikoRsReader reader) {
        ulong length = reader.ReadUInt64();

        for (ulong i = 0; i < length; i++) {
            SpectatorFrame frame = SpectatorFrame.ReadFrame(reader);

            if (this.Host == null && RuntimeInfo.IsDebug()) {
                Logger.Log("Recieved spectator frames while we were not spectating!", LoggerLevelOnlineInfo.Instance);
                throw new Exception("Recieved spectator frames while we were not spectating!");
            }

            this.LastSpectatorTime = frame.Time;

            Logger.Log($"Got spectator frame {frame.Type} at {frame.Time}", LoggerLevelOnlineInfo.Instance);

            switch (frame.Type) {
                case SpectatorFrameDataType.Play: {
                    if (this.Host == this.Player) break;

                    SpectatorFramePlay pFrame = (SpectatorFramePlay)frame;

                    Logger.Log($"host playing map: {pFrame.BeatmapHash} starting to switch", LoggerLevelOnlineInfo.Instance);

                    FurballGame.GameTimeScheduler.ScheduleMethod(
                    _ => {
                        if (FurballGame.Instance.RunningScreen is not SongSelectionScreen)
                            ScreenManager.ChangeScreen(new SongSelectionScreen(false));

                        pTypingGame.CurrentSong.Value = SongManager.Songs.FirstOrDefault(x => x.MapHash == pFrame.BeatmapHash);
                        // pTypingGame.LoadBackgroundFromSong(pTypingGame.CurrentSong);

                        if (pTypingGame.CurrentSong.Value == default) {
                            pTypingGame.NotificationManager.CreateNotification(
                            NotificationManager.NotificationImportance.Warning,
                            $"You do not have that map!\nHash: {pFrame.BeatmapHash}"
                            );
                            return;
                        }

                        ScreenManager.ChangeScreen(new PlayerScreen(this.Host));

                        //Wait 5s for now
                        FurballGame.GameTimeScheduler.ScheduleMethod(
                        _ => {
                            pTypingGame.MusicTrack.Play();
                        },
                        FurballGame.Time + 5000
                        );

                        this.SpectatorState = SpectatorState.Playing;
                    },
                    0
                    );

                    break;
                }
                case SpectatorFrameDataType.Pause: {
                    if (this.Host == this.Player) break;

                    lock (this.GameScene.SpectatorQueue) {
                        this.GameScene.SpectatorQueue.Add(frame);
                    }

                    break;
                }
                case SpectatorFrameDataType.Resume: {
                    if (this.Host == this.Player) break;

                    // lock (this.GameScene.SpectatorQueue) {
                    //     this.GameScene.SpectatorQueue.Add(frame);
                    // }

                    FurballGame.GameTimeScheduler.ScheduleMethod(
                    _ => {
                        pTypingGame.OnlineManager.SpectatorState = SpectatorState.Playing;
                        pTypingGame.MusicTrack.Resume();
                    },
                    FurballGame.Time + 5000
                    );
                    

                    break;
                }
                case SpectatorFrameDataType.Buffer: {
                    if (this.GameScene != null)
                        lock (this.GameScene.SpectatorQueue) {
                            this.GameScene.SpectatorQueue.Add(frame);
                        }

                    break;
                }
                case SpectatorFrameDataType.SpectatingOther: { throw new NotImplementedException(); }
                case SpectatorFrameDataType.ReplayFrame: {
                    if (this.Host == this.Player) break;

                    if (this.GameScene != null)
                        lock (this.GameScene.SpectatorQueue) {
                            this.GameScene.SpectatorQueue.Add(frame);
                        }

                    break;
                }
                case SpectatorFrameDataType.ScoreSync: {
                    if (this.Host == this.Player) break;

                    if (this.GameScene != null)
                        lock (this.GameScene.SpectatorQueue) {
                            this.GameScene.SpectatorQueue.Add(frame);
                        }

                    break;
                }
                case SpectatorFrameDataType.ChangingMap: {
                    if (this.Host == this.Player) break;

                    this.SpectatorState = SpectatorState.ChangingMap;
                    pTypingGame.MusicTrack.Stop();

                    break;
                }
                case SpectatorFrameDataType.PlayingResponse: {
                    //TODO: this
                    break;
                }
            }
        }

        return true;
    }

    private bool HandleServerSpectatorLeftPacket(TaikoRsReader reader) {
        ServerSpectatorLeftPacket p = new();
        p.ReadDataFromStream(reader);

        if (p.UserId == this.Player.UserId) {
            this.Host = null;
            lock (this.Spectators) {
                this.Spectators.Clear();
            }

            FurballGame.GameTimeScheduler.ScheduleMethod(
            _ => {
                ScreenManager.ChangeScreen(new MenuScreen());
            },
            FurballGame.Time
            );

            return true;
        }

        lock (this.Spectators) {
            this.Spectators.Remove(p.UserId);
        }

        Logger.Log($"User {p.UserId} started speccing us (or joined us in speccing someone else)!", LoggerLevelOnlineInfo.Instance);

        return true;
    }

    private bool HandleServerSpectatorJoinedPacket(TaikoRsReader reader) {
        ServerSpectatorJoinedPacket p = new();
        p.ReadDataFromStream(reader);

        // if (this.Host == null) {
        //     
        // }

        lock (this.OnlinePlayers) {
            if (!this.OnlinePlayers.TryGetValue(p.UserId, out OnlinePlayer player))
                return false;

            lock (this.Spectators) {
                this.Spectators.TryAdd(p.UserId, player);
            }
        }

        Logger.Log($"User {p.Username} started speccing us (or joined us in speccing someone else)!", LoggerLevelOnlineInfo.Instance);

        return true;
    }

    public override void SendMessage(string channel, string message) {
        message = message.Trim();

        if (string.IsNullOrWhiteSpace(message)) return;

        this.PacketQueue.Enqueue(new ClientSendMessagePacket(channel, message));
    }

    protected override void Disconnect() {
        this._sendPackets                 = false;
        this._spectatorFrameSendThreadRun = false;

        this.PacketQueue.Clear();

        if (this._client?.IsAlive ?? false)
            this._client.Close(CloseStatusCode.Normal, "Client Disconnecting");

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
        if (this._client?.ReadyState != WebSocketState.Open) return;

        FurballGame.Instance.AfterScreenChange += this.OnScreenChangeAfter;

        this.PacketQueue.Enqueue(new ClientLogOutPacket());

        this.InvokeOnLogout(this);
        //Note we set to connected because we didnt actually *disconnect* yet, just log out
        this.State = ConnectionState.Connected;
    }

    #region Handle packets

    private bool HandleServerSendMessagePacket(TaikoRsReader reader) {
        ServerSendMessagePacket packet = new();
        packet.ReadDataFromStream(reader);

        lock (this.OnlinePlayers) {
            if (this.OnlinePlayers.TryGetValue(packet.SenderId, out OnlinePlayer player)) {
                ChatMessage message = new(player, packet.Channel, packet.Message);

                if (message.Message.Length > 128) message.Message = message.Message[..Math.Min(128, message.Message.Length)];

                this._chatQueue.Enqueue(message);

                Logger.Log(
                $"<{message.Time.Hour:00}:{message.Time.Minute:00}> [{message.Channel}] {message.Sender.Username}: {message.Message}",
                LoggerLevelChatMessage.Instance
                );

                lock (this.KnownChannels) {
                    if (!this.KnownChannels.Contains(packet.Channel))
                        this.KnownChannels.Add(packet.Channel);
                }
            }
        }

        return true;
    }

    private bool HandleServerScoreUpdatePacket(TaikoRsReader reader) {
        ServerScoreUpdatePacket packet = new();
        packet.ReadDataFromStream(reader);

        lock (this.OnlinePlayers) {
            if (this.OnlinePlayers.TryGetValue(packet.UserId, out OnlinePlayer player)) {
                FurballGame.GameTimeScheduler.ScheduleMethod(
                _ => {
                    player.TotalScore.Value  = packet.TotalScore;
                    player.RankedScore.Value = packet.RankedScore;
                    player.Accuracy.Value    = packet.Accuracy;
                    player.PlayCount.Value   = packet.Playcount;
                    player.Rank.Value        = packet.Rank;

                    Logger.Log(
                    $"Got score update packet: {player.Username}: {player.TotalScore}:{player.RankedScore}:{player.Accuracy}:{player.PlayCount}",
                    LoggerLevelOnlineInfo.Instance
                    );
                },
                0
                );
            }
        }

        return true;
    }

    private bool HandleServerUserStatusUpdatePacket(TaikoRsReader reader) {
        ServerUserStatusUpdatePacket packet = new();
        packet.ReadDataFromStream(reader);

        lock (this.OnlinePlayers) {
            if (this.OnlinePlayers.TryGetValue(packet.UserId, out OnlinePlayer player)) {
                FurballGame.GameTimeScheduler.ScheduleMethod(
                _ => {
                    player.Action.Value = packet.Action;
                    Logger.Log(
                    $"{player.Username} changed status to {player.Action.Value.Action} : {player.Action.Value.ActionText}! Mode: {packet.Action.Mode.Value}",
                    LoggerLevelOnlineInfo.Instance
                    );
                },
                0
                );
            }
        }

        return true;
    }

    private bool HandleServerUserLeftPacket(TaikoRsReader reader) {
        ServerUserLeftPacket packet = new();
        packet.ReadDataFromStream(reader);

        lock (this.OnlinePlayers) {
            if (this.OnlinePlayers.TryGetValue(packet.UserId, out OnlinePlayer player)) {
                Logger.Log($"{player.Username} has gone offline!", LoggerLevelOnlineInfo.Instance);
                this.OnlinePlayers.Remove(packet.UserId);
            }
        }

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
                pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Error, "Login failed! (unknown reason, contact eve)");

                this.Disconnect();
                return true;
            }
        }

        this.Player.Username.Value          = this.Username();
        this.Player.Action.Value.Mode.Value = PlayMode.pTyping;
        this.Player.UserId.Value            = packet.UserId;
        this.State                          = ConnectionState.LoggedIn;

        this.InvokeOnLoginComplete(this);

        lock (this.OnlinePlayers) {
            this.OnlinePlayers.Add(this.Player.UserId, this.Player);
        }

        pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Info, $"Login complete! Welcome {this.Player.Username}!");

        this.PacketQueue.Enqueue(new ClientStatusUpdatePacket(new UserAction(UserActionType.Idle, "")));
        this.PacketQueue.Enqueue(new ClientNotifyScoreUpdatePacket());

        lock (this.KnownChannels) {
            if (!this.KnownChannels.Contains("#general"))
                this.KnownChannels.Add("#general");

            if (!this.KnownChannels.Contains("#pTyping"))
                this.KnownChannels.Add("#pTyping");
        }

        return true;
    }

    public override void Update(double time) {
        while (this._chatQueue.TryDequeue(out ChatMessage message))
            lock (this.ChatLog) {
                this.ChatLog.Add(message);
            }

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

        lock (this.OnlinePlayers) {
            if (this.OnlinePlayers.ContainsKey(packet.UserId)) return true;

            this.OnlinePlayers.Add(packet.UserId, player);
        }
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

        if (length != 0) {
            byte[] data = this.ReadBytes((int)length);

            return Encoding.UTF8.GetString(data);
        }

        return "";
    }

    public PacketId ReadPacketId() => (PacketId)this.ReadUInt16();

    public SpectatorFrameDataType ReadSpectatorFrameType() => (SpectatorFrameDataType)this.ReadByte();

    public PlayMode ReadPlayMode() => PlayModeMethods.FromString(this.ReadString());
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

public enum ServerErrorCode {
    Unknown      = 0,
    CantSpectate = 1
}