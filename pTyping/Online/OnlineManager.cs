using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Furball.Engine;
using Hellosam.Net.Collections;
using JetBrains.Annotations;
using pTyping.Engine;
using pTyping.Graphics.Player;
using pTyping.Shared.Scores;
using SixLabors.ImageSharp;
using Console = Furball.Engine.Engine.DevConsole.DevConsole;

namespace pTyping.Online;

public enum SpectatorState {
	Playing,
	Paused,
	Stopped,
	ChangingMap
}

public abstract class OnlineManager {
	public ObservableCollection<ChatMessage>        ChatLog       = new();
	public ObservableDictionary<uint, OnlinePlayer> OnlinePlayers = new();
	/// <summary>
	///     If you are host, this is your spectators, otherwise it is the other spectators
	/// </summary>
	public Dictionary<uint, OnlinePlayer> Spectators = new();
	/// <summary>
	///     The host you are spectating
	/// </summary>
	public OnlinePlayer Host;
	public OnlinePlayer Player = new();

	public SpectatorState SpectatorState = SpectatorState.Stopped;

	public float LastSpectatorTime = 0f;

	public PlayerScreen GameScene;

	public ObservableCollection<string> KnownChannels = new();

	public ConnectionState State {
		get;
		protected set;
	} = ConnectionState.Disconnected;

	public async Task SubmitScore(Score score) {
		if (this.State == ConnectionState.LoggedIn)
			await this.ClientSubmitScore(score);
	}

	[Pure]
	public async Task<List<Score>> GetMapScores(string hash) {
		if (this.State == ConnectionState.LoggedIn)
			return await this.ClientGetScores(hash);

		return new List<Score>();
	}

	public abstract string Username();
	public abstract string Password();

	protected abstract void ClientLogin();
	protected abstract void ClientLogout();
	protected abstract void Connect();
	protected abstract void Disconnect();
	public abstract    void SendMessage(string channel, string message);

	public abstract void SpectatorPause(double       time);
	public abstract void SpectatorResume(double      time);
	public abstract void SpectatorBuffer(double      time);
	public abstract void SpectatorScoreSync(double   time, Score       score);
	public abstract void SpectatorReplayFrame(double time, ReplayFrame frame);

	public abstract void SpectatePlayer(OnlinePlayer player);

	protected abstract Task ClientSubmitScore(Score score);
	[Pure]
	protected abstract Task<List<Score>> ClientGetScores(string hash);

	public abstract void ChangeUserAction(UserAction action);

	protected void InvokeOnLoginStart(object sender) {
		this.OnLoginStart?.Invoke(sender, null);
	}
	public event EventHandler OnLoginStart;
	protected void InvokeOnLoginComplete(object sender) {
		this.OnLoginComplete?.Invoke(sender, null);
	}
	public event EventHandler OnLoginComplete;
	protected void InvokeOnLogout(object sender) {
		this.OnLogout?.Invoke(sender, null);
	}
	public event EventHandler OnLogout;

	protected void InvokeOnConnect(object sender) {
		this.OnConnect?.Invoke(sender, null);
	}
	public event EventHandler OnConnect;
	protected void InvokeOnConnectStart(object sender) {
		this.OnConnectStart?.Invoke(sender, null);
	}
	public event EventHandler OnConnectStart;
	protected void InvokeOnDisconnect(object sender) {
		this.OnDisconnect?.Invoke(sender, null);
	}
	public event EventHandler OnDisconnect;

	public OnlineLobby OnlineLobby = null;

	public void Login() {
		lock (this.OnlinePlayers) {
			this.OnlinePlayers.Clear();
		}

		pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Info, "Logging in...");
		if (this.State == ConnectionState.Disconnected)
			this.Connect();
	}

	public void ScheduleAutomaticReconnect() {
		pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Error, "Reconnecting to the server in 15 seconds!");
		FurballGame.GameTimeScheduler.ScheduleMethod(
			delegate {
				this.Login();
			},
			FurballGame.Time + 15000
		);
	}

	public abstract string SendScreenshot(Image image);

	public abstract Image RetrieveScreenshot(string id);

	public void Logout() {
		lock (this.OnlinePlayers) {
			this.OnlinePlayers.Clear();
		}

		this.ClientLogout();

		this.Disconnect();
	}

	public virtual void Initialize() {}

	public virtual void Update(double time) {}
}

public enum ConnectionState {
	LoggedIn,
	LoggingIn,
	Connected,
	Disconnected
}
