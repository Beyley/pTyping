using System;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Vixie.Backends.Shared;
using pTyping.Engine;
using pTyping.Online;

namespace pTyping.Graphics.Menus.Online;

public class LobbyScreen : pScreen {
    private TextDrawable     _name;
    private TextDrawable     _users;
    private TexturedDrawable _backButton;

    private OnlineLobby _lobby;

    public override void Initialize() {
        base.Initialize();

        if (pTypingGame.OnlineManager.OnlineLobby == null) {
            ScreenManager.ChangeScreen(new MenuScreen());
            return;
        }

        this._lobby = pTypingGame.OnlineManager.OnlineLobby;

        this.Manager.Add(pTypingGame.CurrentSongBackground);

        this.Manager.Add(
        this._name = new(new(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, 10), pTypingGame.JapaneseFont, "", 40) {
            ColorOverride = Color.White,
            OriginType    = OriginType.TopCenter
        }
        );

        this.Manager.Add(
        this._users = new(new(50), pTypingGame.JapaneseFont, "", 30) {
            ColorOverride = Color.White
        }
        );

        pTypingGame.LoadBackButtonTexture();

        this.Manager.Add(
        this._backButton = new TexturedDrawable(pTypingGame.BackButtonTexture, new(0, FurballGame.DEFAULT_WINDOW_HEIGHT)) {
            OriginType = OriginType.BottomLeft,
            Scale      = pTypingGame.BackButtonScale
        }
        );

        this._backButton.OnClick += delegate {
            this._lobby.Leave();

            pTypingGame.OnlineManager.OnlineLobby = null;

            ScreenManager.ChangeScreen(new MenuScreen());
        };

        this.Update();

        this._lobby.UserJoined += this.OnUserJoin;
        this._lobby.UserLeft   += this.OnUserLeft;

        this._lobby.Closed += this.OnLobbyClosed;
    }

    public override void Dispose() {
        base.Dispose();

        this._lobby.UserJoined -= this.OnUserJoin;
        this._lobby.UserLeft   -= this.OnUserLeft;

        this._lobby.Closed -= this.OnLobbyClosed;
    }

    private void OnLobbyClosed(object sender, EventArgs e) {
        pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Info, "Lobby closed.");

        ScreenManager.ChangeScreen(new MenuScreen());
    }

    private void OnUserLeft(object sender, LobbyPlayer lobbyPlayer) {
        this.Update();
    }

    private void OnUserJoin(object sender, LobbyPlayer lobbyPlayer) {
        this.Update();
    }

    private void Update() {
        this._name.Text = this._lobby.Name;

        this._users.Text = "";

        foreach (LobbyPlayer player in this._lobby.LobbySlots)
            if (player != null)
                this._users.Text += $"Username: {player.Username} id:{player.Id}\n";
            else
                this._users.Text += "\n";
    }

    public override string               Name                 => "Lobby";
    public override string               State                => "Partying to the music while waiting for the game to start...";
    public override string               Details              => "";
    public override bool                 ForceSpeedReset      => false;
    public override float                BackgroundFadeAmount => 0.7f;
    public override MusicLoopState       LoopState            => MusicLoopState.Loop;
    public override ScreenType           ScreenType           => ScreenType.Menu;
    public override ScreenUserActionType OnlineUserActionType => ScreenUserActionType.Lobbying;
}
