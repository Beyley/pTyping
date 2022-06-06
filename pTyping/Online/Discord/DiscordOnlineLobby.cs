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

        //If we are the host, the add ourselves, if we are not the host, then we'll wait for the host to send us the user information
        if (this.lobby.Value!.Value.OwnerId == DiscordManager.User.Id)
            this.OnMemberConnect(lobby.Value!.Value.Id, DiscordManager.User.Id);
    }

    // private void DetectExistingPlayers() {
    //     IEnumerable<User> memberUsers = DiscordManager.LobbyManager.GetMemberUsers(this.lobby.Value!.Value.Id);
    //     IEnumerator<User> enumerator  = memberUsers.GetEnumerator();
    //
    //     while (enumerator.MoveNext()) {
    //         User current = enumerator.Current;
    //         
    //         
    //     }
    // }
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

    private void OnLobbyUpdate(long lobbyid) {
        this.LobbySize = this.GetDiscordLobby().Capacity;

        this.lobby.Value = DiscordManager.LobbyManager.GetLobby(lobbyid);

        this.OnGeneralUpdate();
    }

    private readonly Dictionary<long, int> UserIDToSlot = new();

    private void OnMemberConnect(long lobbyid, long userid) {
        int slot = this.FirstEmptySlot();
        if (slot != -1) {
            User user = DiscordManager.LobbyManager.GetMemberUser(lobbyid, userid);

            this.LobbySlots[slot] = new LobbyPlayer {
                Username = user.Username,
                Id       = user.Id,
                Ready    = false
            };

            this.UserIDToSlot[userid] = slot;

            pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Info, $"{user.Username} has joined your lobby!");
            this.OnUserJoined(this.LobbySlots[slot]);
        } else {
            throw new Exception("what? how did an extra user join, or did we forget to clear a slot properly, anyway report this pls");
        }
    }
    private void OnMemberDisconnect(long lobbyid, long userid) {
        if (this.UserIDToSlot.TryGetValue(userid, out int slot)) {
            LobbyPlayer player = this.LobbySlots[slot];

            this.UserIDToSlot.Remove(userid);
            this.LobbySlots[slot] = null;

            pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Info, $"{player.Username} has left your lobby!");

            this.OnUserLeft(player);
        } else {
            throw new Exception("who the fuck left?");
        }
    }

    public override void DisconnectUser(LobbyPlayer id) {
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
