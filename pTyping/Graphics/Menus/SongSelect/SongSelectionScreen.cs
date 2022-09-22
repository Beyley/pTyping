using System;
using System.Collections.Generic;
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
using Furball.Engine.Engine.Input.Events;
using Furball.Vixie;
using Furball.Vixie.Backends.Shared;
using JetBrains.Annotations;
using ManagedBass;
using pTyping.Engine;
using pTyping.Graphics.Drawables;
using pTyping.Graphics.Editor;
using pTyping.Graphics.Player;
using pTyping.Graphics.Player.Mods;
using pTyping.Shared.Beatmaps;
using pTyping.Shared.Scores;
using Silk.NET.Input;

namespace pTyping.Graphics.Menus.SongSelect;

public class SongSelectionScreen : pScreen {

    public static    Bindable<LeaderboardType> LeaderboardType = new(SongSelect.LeaderboardType.Local);
    private readonly bool                      _editor;
    private          TexturedDrawable          _leaderboardButton;
    private          LeaderboardDrawable       _leaderboardDrawable;

    private ModSelectionMenuDrawable _modMenu;

    private float _movingDirection;

    private TextDrawable        _songInfo;
    private SongSelectDrawable  _songSelectDrawable;
    private ScrollableContainer _songSelectScrollable;
    private SongInfoDrawable    _songInfoDrawable;

    public SongSelectionScreen(bool editor) => this._editor = editor;
    public override string Name  => "Song Select";
    public override string State => "Selecting a song!";
    public override string Details {
        get {
            BeatmapSet set = pTypingGame.CurrentSong.Value.Parent;
            return $"Deciding on playing {set.Artist} - {set.Title} [{pTypingGame.CurrentSong.Value.Info.DifficultyName}]";
        }
    }
    public override bool           ForceSpeedReset      => false;
    public override float          BackgroundFadeAmount => 0.7f;
    public override MusicLoopState LoopState            => MusicLoopState.LoopFromPreviewPoint;
    public override ScreenType     ScreenType           => ScreenType.Menu;

    public override void Initialize() {
        base.Initialize();

        List<BeatmapSet> sets = pTypingGame.BeatmapDatabase.Realm.All<BeatmapSet>().ToList();

        if (!this._editor && sets.Count == 0) {
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
            EventHandler<MouseButtonEventArgs> newSongOnClick = delegate {
                pTypingGame.MenuClickSound.PlayNew();
                ScreenManager.ChangeScreen(new NewSongScreen());
            };

            DrawableButton createNewSongButton = new(
            new Vector2(backButton.Size.X + 10f, FurballGame.DEFAULT_WINDOW_HEIGHT),
            FurballGame.DefaultFont,
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

        this._songSelectDrawable = new SongSelectDrawable(
        Vector2.Zero,
        delegate(float f) {
            this._songSelectScrollable?.SetTargetScroll(f);
        }
        );
        
        this._songSelectScrollable = new ScrollableContainer(this._songSelectDrawable.Size) {
            Position         = new Vector2(10, 10),
            OriginType       = OriginType.TopRight,
            ScreenOriginType = OriginType.TopRight,
            Depth            = 0.8f,
            InvisibleToInput = true
        };
        this._songSelectScrollable.Add(this._songSelectDrawable);

        this._songSelectDrawable.UpdateDrawables();

        this.Manager.Add(this._songSelectScrollable);

        #endregion

        #region Start button

        DrawableButton startButton = new(new Vector2(5), FurballGame.DefaultFont, 60, "Start!", Color.Red, Color.White, Color.White, new Vector2(0)) {
            OriginType       = OriginType.BottomRight,
            ScreenOriginType = OriginType.BottomRight,
            Depth            = 0f
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

        this._songInfo = new TextDrawable(new Vector2(this._leaderboardButton.Size.X + 20, 10), pTypingGame.JapaneseFontStroked, "", 35) {
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
            this._modMenu = new ModSelectionMenuDrawable(new Vector2(100, FurballGame.DEFAULT_WINDOW_HEIGHT - backButton.Size.Y - 25)) {
                OriginType  = OriginType.BottomLeft,
                Depth       = 0.5f,
                Clickable   = false,
                CoverClicks = false,
                Hoverable   = false,
                CoverHovers = false
            };
            this._modMenu.Hide(true);
            this.Manager.Add(this._modMenu);

            DrawableButton toggleMods = new(
            new Vector2(backButton.Position.X + backButton.Size.X + 10, backButton.Position.Y),
            FurballGame.DefaultFontStroked,
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
                if (this._modMenu.Shown) {
                    this._modMenu.Hide();
                    this._modMenu.Clickable   = false;
                    this._modMenu.CoverClicks = false;
                    this._modMenu.Hoverable   = false;
                    this._modMenu.CoverHovers = false;
                } else {
                    this._modMenu.Show();
                    this._modMenu.Clickable   = true;
                    this._modMenu.CoverClicks = true;
                    this._modMenu.Hoverable   = true;
                    this._modMenu.CoverHovers = true;
                }
            };

            this.Manager.Add(toggleMods);
        }

        #endregion

        #region Song info

        this.Manager.Add(this._songInfoDrawable = new SongInfoDrawable {
            Position = new Vector2(100, 100),
            Depth    = 0.75f
        });
        
        #endregion

        if (pTypingGame.CurrentSong.Value == null && sets.Count > 0)
            pTypingGame.CurrentSong.Value = sets[0].Beatmaps.First();
        else if (pTypingGame.CurrentSong?.Value != null)
            this.UpdateSelectedSong(true);

        pTypingGame.CurrentSong.OnChange += this.OnSongChange;

        FurballGame.InputManager.OnKeyDown += this.OnKeyDown;
        FurballGame.InputManager.OnKeyUp   += this.OnKeyUp;

        LeaderboardType.OnChange += this.OnLeaderboardTypeChange;

        if (!this._editor)
            this._modMenu.OnModAdd += this.ModMenuOnOnModAdd;
    }

    private void ModMenuOnOnModAdd(object sender, EventArgs e) {
        double speed = 1f;
        foreach (PlayerMod moditer in pTypingGame.SelectedMods)
            speed *= moditer.SpeedMultiplier();
        pTypingGame.MusicTrack.SetSpeed(speed);
    }

    public override ScreenUserActionType OnlineUserActionType => ScreenUserActionType.ChoosingSong;

    private void ChangeLeaderboardType(object sender, MouseButtonEventArgs mouseButtonEventArgs) {
        LeaderboardType.Value = LeaderboardType.Value switch {
            SongSelect.LeaderboardType.Local  => SongSelect.LeaderboardType.Global,
            SongSelect.LeaderboardType.Global => SongSelect.LeaderboardType.Friend,
            SongSelect.LeaderboardType.Friend => SongSelect.LeaderboardType.Local,
            _                                 => LeaderboardType.Value
        };

        this._leaderboardButton.Texture = TextureFromLeaderboardType(LeaderboardType);

        // this._leaderboardButton.Tweens.Clear();
        this._leaderboardButton.Tweens.Add(new VectorTween(TweenType.Scale, this._leaderboardButton.Scale, new Vector2(0.055f), FurballGame.Time, FurballGame.Time + 50));
        this._leaderboardButton.Tweens.Add(new VectorTween(TweenType.Scale, new Vector2(0.055f),           new Vector2(0.05f),          FurballGame.Time + 50, FurballGame.Time + 100));
    }

    private void OnLeaderboardTypeChange(object sender, LeaderboardType e) {
        this.UpdateScores();
    }

    private void OnKeyDown(object sender, KeyEventArgs keyEventArgs) {
        this._movingDirection = keyEventArgs.Key switch {
            Key.Up   => 1f,
            Key.Down => -1f,
            _        => this._movingDirection
        };

        if (keyEventArgs.Key == Key.F5) {
            throw new NotImplementedException();
            // SongManager.UpdateSongs();
            // pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Info, "Reloaded the song list!");
            // ScreenManager.ChangeScreen(new SongSelectionScreen(this._editor)); //TODO: implement dynamic song select
        }
    }

    private void OnKeyUp(object sender, KeyEventArgs keyEventArgs) {
        this._movingDirection = keyEventArgs.Key switch {
            Key.Up or Key.Down => 0f,
            _                  => this._movingDirection
        };
    }

    private void OnSongChange(object sender, Beatmap beatmap) {
        this.UpdateSelectedSong();
    }

    public void PlaySelectedMap() {
        pTypingGame.MenuClickSound.PlayNew();
        ScreenManager.ChangeScreen(this._editor ? new EditorScreen() : new PlayerScreen());
    }

    public void UpdateSelectedSong(bool fromPrevScreen = false) {
        BeatmapSet set = pTypingGame.CurrentSong.Value.Parent;

        this._songInfo.Text = $@"{set.Artist} - {set.Title} [{pTypingGame.CurrentSong.Value.Info.DifficultyName}]
Created by {pTypingGame.CurrentSong.Value.Info.Mapper}
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

        List<Score> origScores = new();

        switch (LeaderboardType.Value) {
            case SongSelect.LeaderboardType.Friend: {
                //TODO: implement friend leaderboards
                pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Warning, "Friend leaderboards are not implemented!");
                break;
            }
            case SongSelect.LeaderboardType.Global: {
                Task<List<Score>> task = pTypingGame.OnlineManager.GetMapScores(pTypingGame.CurrentSong.Value.Id);

                task.Wait();

                origScores = task.Result;
                foreach (Score playerScore in origScores)
                    playerScore.OnlineScore = true;
                break;
            }
            case SongSelect.LeaderboardType.Local: {
                origScores = pTypingGame.ScoreDatabase.Realm.All<Score>().Where(x => x.BeatmapId == pTypingGame.CurrentSong.Value.Id).ToList();
                break;
            }
        }

        List<Score> scores = origScores.OrderByDescending(x => x.AchievedScore).ToList();

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

        // FurballGame.InputManager.OnMouseScroll -= this.OnMouseScroll;

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