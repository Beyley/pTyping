using System;

namespace pTyping.Online;

public abstract class OnlineLobby {
    private uint _lobbySize;
    public uint LobbySize {
        get => this._lobbySize;
        protected set {
            if (value < this._lobbySize)
                for (int i = (int)value; i < this._lobbySize; i++)
                    if (this.LobbySlots[i] != 0)
                        this.DisconnectUser(this.LobbySlots[i]);

            this._lobbySize = value;

            Array.Resize(ref this.LobbySlots, (int)value);
        }
    }
    public long[] LobbySlots = Array.Empty<long>();

    public OnlinePipe Pipe;

    public abstract void DisconnectUser(long id);

    public event EventHandler<long> UserJoined;
    public event EventHandler<long> UserLeft;

    public bool Full() {
        for (int i = 0; i < this.LobbySlots.Length; i++)
            if (this.LobbySlots[i] == 0)
                return false;

        return true;
    }

    public int UsedSlots() {
        int count = 0;
        for (int i = 0; i < this.LobbySlots.Length; i++)
            if (this.LobbySlots[i] != 0)
                count++;

        return count;
    }

    /// <summary>
    ///     Gets the first empty slot in the match, -1 means no empty space
    /// </summary>
    /// <returns></returns>
    public int FirstEmptySlot() {
        for (int i = 0; i < this.LobbySlots.Length; i++)
            if (this.LobbySlots[i] == 0)
                return i;

        return -1;
    }

    public abstract string GetUsername(long id);

    protected void OnUserJoined(long userid) {
        this.UserJoined?.Invoke(this, userid);
    }
    protected void OnUserLeft(long userid) {
        this.UserLeft?.Invoke(this, userid);
    }

    public abstract void Dispose();
}
