using System;
using DiscordSDK;
using Furball.Engine.Engine.Helpers;
using Furball.Engine.Engine.Platform;
using pTyping.Engine;

namespace pTyping.Online.Discord;

public class DiscordOnlinePipe : OnlinePipe {
    private readonly Bindable<Lobby?> connectedLobby;

    public bool Connected;

    private const int RELIABLE_CHANNEL = 0;

    public DiscordOnlinePipe(Bindable<Lobby?> lobby) {
        if (!lobby.Value.HasValue)
            throw new Exception("No lobby has been created!");

        this.connectedLobby = lobby;
    }

    public override void Connect() {
        DiscordManager.LobbyManager.ConnectNetwork(this.connectedLobby.Value.Value.Id);
        DiscordManager.LobbyManager.OpenNetworkChannel(this.connectedLobby.Value.Value.Id, RELIABLE_CHANNEL, true);

        DiscordManager.LobbyManager.OnNetworkMessage += this.OnNetworkMessage;

        this.Connected = true;

        if (RuntimeInfo.IsDebug())
            pTypingGame.NotificationManager.CreateNotification(
            NotificationManager.NotificationImportance.Info,
            $"Pipe connected for lobby {this.connectedLobby.Value.Value.Id}"
            );
    }

    private void OnNetworkMessage(long lobbyid, long userid, byte channelid, byte[] data) {
        this.InvokeMessageRecieved(
        new OnlinePipeMessage {
            Data   = data,
            Source = userid
        }
        );
    }

    public override void Disconnect() {
        DiscordManager.LobbyManager.OnNetworkMessage -= this.OnNetworkMessage;

        DiscordManager.LobbyManager.DisconnectNetwork(this.connectedLobby.Value.Value.Id);

        this.Connected = false;

        if (RuntimeInfo.IsDebug())
            pTypingGame.NotificationManager.CreateNotification(
            NotificationManager.NotificationImportance.Info,
            $"Pipe disconnected for lobby {this.connectedLobby.Value.Value.Id}"
            );
    }

    public override void SendMessage(OnlinePipeMessage message) {
        if (message.Target == -1) {
            int memberCount = DiscordManager.LobbyManager.MemberCount(this.connectedLobby.Value.Value.Id);

            for (int i = 0; i < memberCount; i++) {
                long userid = DiscordManager.LobbyManager.GetMemberUserId(this.connectedLobby.Value.Value.Id, i);
                DiscordManager.LobbyManager.SendNetworkMessage(this.connectedLobby.Value.Value.Id, userid, RELIABLE_CHANNEL, message.Data);
            }
        } else {
            DiscordManager.LobbyManager.SendNetworkMessage(this.connectedLobby.Value.Value.Id, message.Target, RELIABLE_CHANNEL, message.Data);
        }
    }
}
