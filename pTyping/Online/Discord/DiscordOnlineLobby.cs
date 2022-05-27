using System;
using System.Collections.Generic;
using DiscordSDK;
using Furball.Engine.Engine.Helpers;
using pTyping.Engine;

namespace pTyping.Online.Discord;

public class DiscordOnlineLobby : OnlineLobby {
    private readonly Bindable<Lobby?> lobby;

    public DiscordOnlineLobby(Bindable<Lobby?> lobby) {
        if (!lobby.Value.HasValue)
            throw new Exception("No lobby has been created!");

        this.lobby = lobby;

        this.Pipe = new DiscordOnlinePipe(lobby);
        this.Pipe.Connect();

        DiscordManager.LobbyManager.OnMemberConnect    += this.OnMemberConnect;
        DiscordManager.LobbyManager.OnMemberDisconnect += this.OnMemberDisconnect;
        DiscordManager.LobbyManager.OnLobbyUpdate      += this.OnLobbyUpdate;

        this.LobbySize = this.GetDiscordLobby().Capacity;
    }

    private Lobby GetDiscordLobby() => DiscordManager.LobbyManager.GetLobby(this.lobby.Value!.Value.Id);

    private void UpdateSlots() {
        IEnumerable<User>       memberUsers = DiscordManager.LobbyManager.GetMemberUsers(this.lobby.Value!.Value.Id);
        using IEnumerator<User> enumerator  = memberUsers.GetEnumerator();
        for (int i = 0; i < this.LobbySlots.Length; i++) {
            User current = enumerator.Current;

            if (enumerator.MoveNext())
                this.LobbySlots[i] = current.Id;
            else
                this.LobbySlots[i] = 0;
        }
    }

    private void OnLobbyUpdate(long lobbyid) {
        this.LobbySize = this.GetDiscordLobby().Capacity;

        this.lobby.Value = DiscordManager.LobbyManager.GetLobby(this.lobby.Value!.Value.Id);
    }
    private void OnMemberConnect(long lobbyid, long userid) {
        this.OnUserJoined(userid);

        pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Info, $"{this.GetUsername(userid)} has joined your lobby!");

        this.UpdateSlots();
    }
    private void OnMemberDisconnect(long lobbyid, long userid) {
        this.OnUserLeft(userid);

        DiscordManager.UserManager.GetUser(
        userid,
        (Result result, ref User user) => {
            if (result != Result.Ok) {
                pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Info, $"Unknown ({user.Id}) has left your lobby!");
                return;
            }

            pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Info, $"{user.Username} has left your lobby!");
        }
        );

        this.UpdateSlots();
    }

    public override void DisconnectUser(long id) {
        throw new NotImplementedException();
    }

    public override string GetUsername(long id) => DiscordManager.LobbyManager.GetMemberUser(this.lobby.Value!.Value.Id, id).Username;

    public override void Dispose() {
        DiscordManager.LobbyManager.OnMemberConnect -= this.OnMemberConnect;

        this.Pipe.Disconnect();
    }
}
