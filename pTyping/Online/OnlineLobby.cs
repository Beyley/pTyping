using System;

namespace pTyping.Online;

public class LobbyPlayer {
	public long   Id;
	public string Username;

	public bool Ready;
}

public abstract class OnlineLobby {
	private uint _lobbySize;
	public uint LobbySize {
		get => this._lobbySize;
		protected set {
			if (value < this._lobbySize)
				for (int i = (int)value; i < this._lobbySize; i++)
					if (this.LobbySlots[i] != null)
						this.DisconnectUser(this.LobbySlots[i]);

			this._lobbySize = value;

			Array.Resize(ref this.LobbySlots, (int)value);
		}
	}
	public LobbyPlayer[] LobbySlots = Array.Empty<LobbyPlayer>();

	public OnlinePipe Pipe;

	public abstract void DisconnectUser(LobbyPlayer id);

	public event EventHandler<LobbyPlayer> UserJoined;
	public event EventHandler<LobbyPlayer> UserLeft;
	public event EventHandler              Closed;
	public event EventHandler              GeneralUpdate;

	public bool Full() {
		for (int i = 0; i < this.LobbySlots.Length; i++)
			if (this.LobbySlots[i] == null)
				return false;

		return true;
	}

	public int UsedSlots() {
		int count = 0;
		for (int i = 0; i < this.LobbySlots.Length; i++)
			if (this.LobbySlots[i] != null)
				count++;

		return count;
	}

	public abstract string Name {
		get;
		set;
	}

	/// <summary>
	///     Gets the first empty slot in the match, -1 means no empty space
	/// </summary>
	/// <returns></returns>
	public int FirstEmptySlot() {
		for (int i = 0; i < this.LobbySlots.Length; i++)
			if (this.LobbySlots[i] == null)
				return i;

		return -1;
	}

	protected void OnUserJoined(LobbyPlayer userid) {
		this.UserJoined?.Invoke(this, userid);
	}
	protected void OnUserLeft(LobbyPlayer userid) {
		this.UserLeft?.Invoke(this, userid);
	}
	protected void OnClosed() {
		this.Closed?.Invoke(this, EventArgs.Empty);
	}
	protected void OnGeneralUpdate() {
		this.GeneralUpdate?.Invoke(this, EventArgs.Empty);
	}

	public abstract void Leave();
}
