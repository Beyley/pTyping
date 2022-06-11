using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Engine.Engine.Helpers;
using Furball.Vixie.Backends.Shared;
using JetBrains.Annotations;
using ManagedBass;
using pTyping.Engine;
using pTyping.Graphics.Drawables;
using pTyping.Graphics.Editor;
using pTyping.Graphics.Player;
using pTyping.Graphics.Player.Mods;
using pTyping.Scores;
using pTyping.Songs;
using Silk.NET.Input;
using Color=Furball.Vixie.Backends.Shared.Color;

namespace pTyping.Graphics.Menus.SongSelect;

public class SongSelectionScreen : pScreen {

    public static    Bindable<LeaderboardType> LeaderboardType = new(SongSelect.LeaderboardType.Local);
    private readonly bool                      _editor;
    private          TexturedDrawable          _leaderboardButton;
    private          LeaderboardDrawable       _leaderboardDrawable;

    private ModSelectionScreenDrawable _modScreen;

    private float _movingDirection;

    private TextDrawable       _songInfo;
    private SongSelectDrawable _songSelectDrawable;
    private SongInfoDrawable   _songInfoDrawable;

    public SongSelectionScreen(bool editor) => this._editor = editor;
    public override string Name  => "Song Select";
    public override string State => "Selecting a song!";
    public override string Details
        => $"Deciding on playing {pTypingGame.CurrentSong.Value.Artist} - {pTypingGame.CurrentSong.Value.Name} [{pTypingGame.CurrentSong.Value.Difficulty}]";
    public override bool           ForceSpeedReset      => false;
    public override float          BackgroundFadeAmount => 0.7f;
    public override MusicLoopState LoopState            => MusicLoopState.LoopFromPreviewPoint;
    public override ScreenType     ScreenType           => ScreenType.Menu;

    public override void Initialize() {
        base.Initialize();

        if (!this._editor && SongManager.Songs.Count == 0) {
            pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Warning, "You have no songs downloaded!");
            ScreenManager.ChangeScreen(new MenuScreen());
            return;
        }

        #region Back button

        pTypingGame.LoadBackButtonTexture();

        TexturedDrawable backButton = new(pTypingGame.BackButtonTexture, new Vector2(0, FurballGame.DEFAULT_WINDOW_HEIGHT)) {
            OriginType = OriginType.BottomLeft,
            Scale      = pTypingGame.BackButtonScale
        };

        backButton.OnClick += delegate {
            pTypingGame.MenuClickSound.PlayNew();
            ScreenManager.ChangeScreen(new MenuScreen());
        };

        this.Manager.Add(backButton);

        #endregion

        #region Create new song button

        if (this._editor) {
            EventHandler<(MouseButton, Point)> newSongOnClick = delegate {
                pTypingGame.MenuClickSound.PlayNew();
                ScreenManager.ChangeScreen(new NewSongScreen());
            };

            DrawableButton createNewSongButton = new(
            new Vector2(backButton.Size.X + 10f, FurballGame.DEFAULT_WINDOW_HEIGHT),
            FurballGame.DEFAULT_FONT,
            30,
            "Create Song",
            Color.Blue,
            Color.White,
            Color.White,
            Vector2.Zero,
            newSongOnClick
            ) {
                OriginType = OriginType.BottomLeft,
                Depth      = 0.5f
            };

            this.Manager.Add(createNewSongButton);
        }

        #endregion

        #region Create new buttons for each song

        IEnumerable<Song> songList = this._editor ? SongManager.Songs.Where(x => x.Type == SongType.pTyping) : SongManager.Songs;

        this._songSelectDrawable = new SongSelectDrawable(new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 10, 10), songList) {
            OriginType = OriginType.TopRight,
            Depth      = 0.8f
        };

        this.Manager.Add(this._songSelectDrawable);

        #endregion

        #region Start button

        DrawableButton startButton = new(
        new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH, FurballGame.DEFAULT_WINDOW_HEIGHT),
        FurballGame.DEFAULT_FONT,
        60,
        "Start!",
        Color.Red,
        Color.White,
        Color.White,
        new Vector2(0)
        ) {
            OriginType = OriginType.BottomRight,
            Depth      = 0f
        };

        startButton.OnClick += delegate {
            this.PlaySelectedMap();
        };

        this.Manager.Add(startButton);

        #endregion

        #region Leaderboard select

        this._leaderboardButton = new TexturedDrawable(TextureFromLeaderboardType(LeaderboardType), new Vector2(10, 10)) {
            Scale       = new Vector2(0.05f),
            Depth       = 0.75f,
            Visible     = !this._editor,
            CoverClicks = !this._editor,
            CoverHovers = !this._editor,
            Clickable   = !this._editor,
            Hoverable   = !this._editor
        };

        this._leaderboardButton.OnClick += this.ChangeLeaderboardType;

        this.Manager.Add(this._leaderboardButton);
        
        #endregion

        #region Song info

        this._songInfo = new TextDrawable(new Vector2(this._leaderboardButton.Size.X + 20, 10), pTypingGame.JapaneseFont, "", 35) {
            Clickable   = false,
            CoverClicks = false
        };

        this.Manager.Add(this._songInfo);

        #endregion

        #region background image

        this.Manager.Add(pTypingGame.CurrentSongBackground);

        #endregion

        #region mods

        if (!this._editor) {
            this._modScreen = new ModSelectionScreenDrawable(new Vector2(25, FurballGame.DEFAULT_WINDOW_HEIGHT - backButton.Size.Y - 25)) {
                Visible    = false,
                OriginType = OriginType.BottomLeft,
                Depth      = 0.5f
            };
            this.Manager.Add(this._modScreen);

            DrawableButton toggleMods = new(
            new Vector2(backButton.Position.X + backButton.Size.X + 10, backButton.Position.Y),
            FurballGame.DEFAULT_FONT_STROKED,
            30,
            "Mods",
            Color.Blue,
            Color.White,
            Color.White,
            new Vector2(0)
            ) {
                OriginType = OriginType.BottomLeft
            };

            toggleMods.OnClick += delegate {
                this._modScreen.Visible = !this._modScreen.Visible;
            };

            this.Manager.Add(toggleMods);
        }

        #endregion

        #region Song info

        this.Manager.Add(this._songInfoDrawable = new SongInfoDrawable {
            Position = new Vector2(100, 100)
        });
        
        #endregion
        
        if (pTypingGame.CurrentSong.Value == null && SongManager.Songs.Count > 0)
            pTypingGame.CurrentSong.Value = SongManager.Songs[0];
        else if (pTypingGame.CurrentSong?.Value != null)
            this.UpdateSelectedSong(true);

        pTypingGame.CurrentSong.OnChange += this.OnSongChange;

        FurballGame.InputManager.OnKeyDown += this.OnKeyDown;
        FurballGame.InputManager.OnKeyUp   += this.OnKeyUp;

        FurballGame.InputManager.OnMouseScroll += this.OnMouseScroll;

        LeaderboardType.OnChange += this.OnLeaderboardTypeChange;

        this._modScreen.OnModAdd += this.ModScreenOnOnModAdd;
    }

    private void ModScreenOnOnModAdd(object sender, EventArgs e) {
        double speed = 1f;
        foreach (PlayerMod moditer in pTypingGame.SelectedMods)
            speed *= moditer.SpeedMultiplier();
        pTypingGame.MusicTrack.SetSpeed(speed);
    }

    public override ScreenUserActionType OnlineUserActionType => ScreenUserActionType.ChoosingSong;

    private void ChangeLeaderboardType(object sender, (MouseButton button, Point pos) tuple) {
        LeaderboardType.Value = LeaderboardType.Value switch {
            SongSelect.LeaderboardType.Local  => SongSelect.LeaderboardType.Global,
            SongSelect.LeaderboardType.Global => SongSelect.LeaderboardType.Friend,
            SongSelect.LeaderboardType.Friend => SongSelect.LeaderboardType.Local,
            _                                 => LeaderboardType.Value
        };

        this._leaderboardButton.SetTexture(TextureFromLeaderboardType(LeaderboardType));

        // this._leaderboardButton.Tweens.Clear();
        this._leaderboardButton.Tweens.Add(new VectorTween(TweenType.Scale, this._leaderboardButton.Scale, new Vector2(0.055f), FurballGame.Time, FurballGame.Time + 50));
        this._leaderboardButton.Tweens.Add(new VectorTween(TweenType.Scale, new Vector2(0.055f),           new Vector2(0.05f),          FurballGame.Time + 50, FurballGame.Time + 100));
    }

    private void OnLeaderboardTypeChange(object sender, LeaderboardType e) {
        this.UpdateScores();
    }

    private void OnMouseScroll(object sender, ((int scrollWheelId, float scrollAmount) scroll, string cursorName) e) {
        this._songSelectDrawable.TargetScroll += e.scroll.scrollAmount * 10;
    }

    public override void Update(double gameTime) {
        if (this._movingDirection != 0f)
            this._songSelectDrawable.TargetScroll += (float)(this._movingDirection * gameTime);

        base.Update(gameTime);
    }

    private void OnKeyDown(object sender, Key e) {
        this._movingDirection = e switch {
            Key.Up   => 1f,
            Key.Down => -1f,
            _        => this._movingDirection
        };

        if (e == Key.F5) {
            SongManager.UpdateSongs();
            pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Info, "Reloaded the song list!");
            ScreenManager.ChangeScreen(new SongSelectionScreen(this._editor)); //TODO: implement dynamic song select
        }
    }

    private void OnKeyUp(object sender, Key e) {
        this._movingDirection = e switch {
            Key.Up or Key.Down => 0f,
            _                  => this._movingDirection
        };
    }

    private void OnSongChange(object sender, Song e) {
        this.UpdateSelectedSong();
    }

    public void PlaySelectedMap() {
        pTypingGame.MenuClickSound.PlayNew();
        ScreenManager.ChangeScreen(this._editor ? new EditorScreen() : new PlayerScreen());
    }

    public void UpdateSelectedSong(bool fromPrevScreen = false) {
        this._songInfo.Text = $@"{pTypingGame.CurrentSong.Value.Artist} - {pTypingGame.CurrentSong.Value.Name} [{pTypingGame.CurrentSong.Value.Difficulty}]
Created by {pTypingGame.CurrentSong.Value.Creator}
BPM:{pTypingGame.CurrentSong.Value.BeatsPerMinute:00.##}".ReplaceLineEndings();

        this._songInfoDrawable.SetSong(pTypingGame.CurrentSong);
        
        if (!fromPrevScreen) {
            pTypingGame.PlayMusic();
        } else if (pTypingGame.MusicTrack.PlaybackState is PlaybackState.Paused or PlaybackState.Stopped) {
            pTypingGame.PlayMusic();
        }

        this.UpdateScores();
    }

    public void UpdateScores() {
        if (this._editor) return;
        
        this.Manager.Remove(this._leaderboardDrawable);

        List<PlayerScore> origScores = new();

        switch (LeaderboardType.Value) {
            case SongSelect.LeaderboardType.Friend: {
                //TODO: implement friend leaderboards
                pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Warning, "Friend leaderboards are not implemented!");
                break;
            }
            case SongSelect.LeaderboardType.Global: {
                Task<List<PlayerScore>> task = pTypingGame.OnlineManager.GetMapScores(pTypingGame.CurrentSong.Value.MapHash);

                task.Wait();

                origScores = task.Result;
                foreach (PlayerScore playerScore in origScores)
                    playerScore.IsOnline = true;
                break;
            }
            case SongSelect.LeaderboardType.Local: {
                origScores = pTypingGame.ScoreManager.GetScores(pTypingGame.CurrentSong.Value.MapHash);
                break;
            }
        }

        List<PlayerScore> scores = origScores.OrderByDescending(x => x.Score).ToList();

        float y = this._songInfo.Size.Y + 25;

        this._leaderboardDrawable = new LeaderboardDrawable(scores) {
            Position = new Vector2(25, y),
            Depth    = 0.9f
        };

        this.Manager.Add(this._leaderboardDrawable);
    }

    public override void Dispose() {
        pTypingGame.CurrentSong.OnChange -= this.OnSongChange;

        FurballGame.InputManager.OnKeyDown -= this.OnKeyDown;
        FurballGame.InputManager.OnKeyUp   -= this.OnKeyUp;

        FurballGame.InputManager.OnMouseScroll -= this.OnMouseScroll;

        LeaderboardType.OnChange -= this.OnLeaderboardTypeChange;

        base.Dispose();
    }

    [Pure]
    public static Texture TextureFromLeaderboardType(LeaderboardType type) {
        return type switch {
            SongSelect.LeaderboardType.Friend => pTypingGame.FriendLeaderboardButtonTexture,
            SongSelect.LeaderboardType.Global => pTypingGame.GlobalLeaderboardButtonTexture,
            SongSelect.LeaderboardType.Local  => pTypingGame.LocalLeaderboardButtonTexture,
            _                                 => throw new ArgumentOutOfRangeException(nameof (type), type, "That leaderboard type is not supported!")
        };
    }
}

public enum LeaderboardType {
    Global,
    Friend,
    Local
}