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
        DiscordManager.LobbyManager.OnLobbyDelete      += this.OnLobbyDelete;

        this.LobbySize = this.GetDiscordLobby().Capacity;

        this.UpdateSlots();
    }
    private void OnLobbyDelete(long lobbyid, uint reason) {
        this.Dispose();
        this.OnClosed();
    }

    private void Dispose() {
        DiscordManager.LobbyManager.OnMemberConnect    -= this.OnMemberConnect;
        DiscordManager.LobbyManager.OnMemberDisconnect -= this.OnMemberDisconnect;
        DiscordManager.LobbyManager.OnLobbyUpdate      -= this.OnLobbyUpdate;
        DiscordManager.LobbyManager.OnLobbyDelete      -= this.OnLobbyDelete;
    }

    private Lobby GetDiscordLobby() => DiscordManager.LobbyManager.GetLobby(this.lobby.Value!.Value.Id);

    private void UpdateSlots() {
        IEnumerable<User>       memberUsers = DiscordManager.LobbyManager.GetMemberUsers(this.lobby.Value!.Value.Id);
        using IEnumerator<User> enumerator  = memberUsers.GetEnumerator();
        if (enumerator.MoveNext())
            for (int i = 0; i < this.LobbySlots.Length; i++) {
                User current = enumerator.Current;

                this.LobbySlots[i] = current.Id;
                enumerator.MoveNext();
            }
    }

    private void OnLobbyUpdate(long lobbyid) {
        this.LobbySize = this.GetDiscordLobby().Capacity;

        this.lobby.Value = DiscordManager.LobbyManager.GetLobby(this.lobby.Value!.Value.Id);

        this.OnGeneralUpdate();
    }
    private void OnMemberConnect(long lobbyid, long userid) {
        pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Info, $"{this.GetUsername(userid)} has joined your lobby!");
        this.UpdateSlots();
        this.OnUserJoined(userid);
    }
    private void OnMemberDisconnect(long lobbyid, long userid) {
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
        this.OnUserLeft(userid);
    }

    public override void DisconnectUser(long id) {
        throw new NotImplementedException();
    }

    public override string Name {
        get => DiscordManager.LobbyManager.GetLobbyMetadataValue(this.lobby.Value!.Value.Id, "name");
        set {
            LobbyTransaction transaction = DiscordManager.LobbyManager.GetLobbyCreateTransaction();

            transaction.SetMetadata("name", value);

            DiscordManager.LobbyManager.UpdateLobby(
            this.lobby.Value!.Value.Id,
            transaction,
            result => {
                if (result != Result.Ok)
                    pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Error, $"Unable to change lobby name! err:{result}");
            }
            );
        }
    }
    public override string GetUsername(long id) => DiscordManager.LobbyManager.GetMemberUser(this.lobby.Value!.Value.Id, id).Username;

    public override void Leave() {
        DiscordManager.LobbyManager.OnMemberConnect -= this.OnMemberConnect;
        this.Pipe.Disconnect();

        DiscordManager.LobbyManager.DisconnectLobby(
        this.lobby.Value!.Value.Id,
        delegate(Result result) {
            if (result != Result.Ok) {
                pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Error, $"Error leaving lobby! err:{result}");
                return;
            }

            pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Info, "Left lobby!");
        }
        );

        this.Dispose();
    }
}
