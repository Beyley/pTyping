using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using FontStashSharp;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.DevConsole;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Helpers;
using Furball.Engine.Engine.Localization;
using Furball.Engine.Engine.Timing;
using Furball.Vixie;
using Furball.Vixie.Backends.Shared;
using Furball.Volpe.Evaluation;
using ManagedBass;
using pTyping.Engine;
using pTyping.Graphics;
using pTyping.Graphics.Menus;
using pTyping.Graphics.Online;
using pTyping.Graphics.Player;
using pTyping.Graphics.Player.Mods;
using pTyping.Online;
using pTyping.Online.Tataku;
using pTyping.Scores;
using pTyping.Songs;
using Silk.NET.Input;
using SixLabors.ImageSharp;
using sowelipisona;
using Color=Furball.Vixie.Backends.Shared.Color;
using ConVars=pTyping.Engine.ConVars;
using Point=System.Drawing.Point;

namespace pTyping;

public enum Localizations {
    MenuRevision,
    Changelog
}

// ReSharper disable once InconsistentNaming
public class pTypingGame : FurballGame {
    public static readonly Vector2 BackButtonScale = new(0.12f);

    public static Texture BackButtonTexture;
    public static Texture DefaultBackground;

    public static AudioStream           MusicTrack                   = null;
    public static OffsetTimeSource      MusicTrackTimeSource         = null;
    public static AudioStreamTimeSource MusicTrackTimeSourceNoOffset = null;
    public static SoundEffectPlayer     MenuClickSound               = null;
    public static Scheduler             MusicTrackScheduler;

    public static Bindable<Song> CurrentSong = new(null);

    public static TextDrawable     VolumeSelector;
    public static TexturedDrawable CurrentSongBackground;

    public static ScoreManager ScoreManager = new();

    public static OnlineManager OnlineManager;

    public static byte[] JapaneseFontData;
    public static FontSystem JapaneseFont = new(
    new FontSystemSettings {
        FontResolutionFactor = 2f,
        KernelWidth          = 1,
        KernelHeight         = 1,
        Effect               = FontSystemEffect.None,
        TextureWidth         = 2048,
        TextureHeight        = 2048
    }
    );
    public static FontSystem JapaneseFontStroked = new(
    new FontSystemSettings {
        FontResolutionFactor = 2f,
        KernelWidth          = 1,
        KernelHeight         = 1,
        Effect               = FontSystemEffect.Stroked,
        EffectAmount         = 2,
        TextureWidth         = 2048,
        TextureHeight        = 2048
    }
    );
    public static FontSystem FurballFontRegular = new(
    new FontSystemSettings {
        FontResolutionFactor = 2f,
        KernelWidth          = 1,
        KernelHeight         = 1,
        TextureWidth         = 2048,
        TextureHeight        = 2048
    }
    );
    public static FontSystem FurballFontRegularStroked = new(
    new FontSystemSettings {
        FontResolutionFactor = 2f,
        KernelWidth          = 1,
        KernelHeight         = 1,
        Effect               = FontSystemEffect.Stroked,
        EffectAmount         = 2,
        TextureWidth         = 2048,
        TextureHeight        = 2048
    }
    );

    public static UserCardDrawable MenuPlayerUserCard;

    public static Texture LocalLeaderboardButtonTexture;
    public static Texture FriendLeaderboardButtonTexture;
    public static Texture GlobalLeaderboardButtonTexture;

    private          double                _musicTrackSchedulerDelta = 0;
    private readonly List<ManagedDrawable> _userPanelDrawables       = new();

    private DrawableManager _userPanelManager;
    private ChatDrawable    _chatDrawable;

    public static List<PlayerMod> SelectedMods = new();

    public static  NotificationManager NotificationManager;
    private static TextDrawable        _OnlineUsersText;
    private static MusicLoopState      CurrentLoopState;
    private        pScreen             CurrentRealScreen;

    public pTypingGame() : base(new MenuScreen()) {
        // this.Window.AllowUserResizing = true;

        pTypingConfig.Instance.Load();

        //We set this flag so that we can fully control the FPS without the engine mucking it up
        BypassFurballFPSLimit = pTypingConfig.Instance.FpsBasedOnMonitorHz;
    }

    public static ManagedDrawable GetUserCard() {
        if (OnlineManager.State != ConnectionState.LoggedIn)
            return new BlankDrawable();

        if (MenuPlayerUserCard is not null)
            return MenuPlayerUserCard;

        MenuPlayerUserCard = new UserCardDrawable(Vector2.Zero, OnlineManager.Player) {
            Scale = new Vector2(0.3f)
        };

        MenuPlayerUserCard.Player.OnChange                               += (_, _) => MenuPlayerUserCard.UpdateDrawable();
        MenuPlayerUserCard.Player.Value.TotalScore.OnChange              += (_, _) => MenuPlayerUserCard.UpdateDrawable();
        MenuPlayerUserCard.Player.Value.RankedScore.OnChange             += (_, _) => MenuPlayerUserCard.UpdateDrawable();
        MenuPlayerUserCard.Player.Value.Accuracy.OnChange                += (_, _) => MenuPlayerUserCard.UpdateDrawable();
        MenuPlayerUserCard.Player.Value.PlayCount.OnChange               += (_, _) => MenuPlayerUserCard.UpdateDrawable();
        MenuPlayerUserCard.Player.Value.Action.OnChange                  += (_, _) => MenuPlayerUserCard.UpdateDrawable();
        MenuPlayerUserCard.Player.Value.Action.Value.Action.OnChange     += (_, _) => MenuPlayerUserCard.UpdateDrawable();
        MenuPlayerUserCard.Player.Value.Action.Value.Mode.OnChange       += (_, _) => MenuPlayerUserCard.UpdateDrawable();
        MenuPlayerUserCard.Player.Value.Action.Value.ActionText.OnChange += (_, _) => MenuPlayerUserCard.UpdateDrawable();

        return MenuPlayerUserCard;
    }

    public static void PlayMusic() {
        MusicTrack.Play();
        // if (MusicTrack.IsValidHandle)
        MusicTrack.Volume = ConVars.Volume.Value.Value;
    }

    public static void PauseResumeMusic() {
        if (MusicTrack.PlaybackState == PlaybackState.Playing)
            MusicTrack.Pause();
        else
            MusicTrack.Resume();
    }

    public static void StopMusic() {
        MusicTrack.Stop();
    }

    public static void LoadMusic(byte[] data) {
        // if (MusicTrack.IsValidHandle) {
        if (MusicTrack != null) {
            MusicTrack.Stop();
            AudioEngine.DisposeStream(MusicTrack);
        }
        // MusicTrack.Free();
        // }
        MusicTrack                   = AudioEngine.CreateStream(data);
        MusicTrackTimeSourceNoOffset = new AudioStreamTimeSource(MusicTrack);
        MusicTrackTimeSource         = new OffsetTimeSource(MusicTrackTimeSourceNoOffset, 0);

        SetSongLoopState(CurrentLoopState);
        
        // MusicTrack.TempoFrequencyLock = true;
    }

    public static void LoadBackButtonTexture() {
        BackButtonTexture ??= ContentManager.LoadTextureFromFile("backbutton.png", ContentSource.User);
    }

    public static void SetBackgroundTexture(Texture tex) {
        CurrentSongBackground.SetTexture(tex);

        CurrentSongBackground.Scale = new Vector2(1f / ((float) CurrentSongBackground.Texture.Height / DEFAULT_WINDOW_HEIGHT));
    }

    public static void LoadBackgroundFromSong(Song song) {
        Texture backgroundTex;
        if (song.BackgroundPath == null) {
            DefaultBackground ??= ContentManager.LoadTextureFromFile("background.png", ContentSource.User);

            backgroundTex = DefaultBackground;
        } else {
            string qualifiedBackgroundPath = Path.Combine(song.FileInfo.DirectoryName ?? string.Empty, song.BackgroundPath);
            backgroundTex = File.Exists(qualifiedBackgroundPath)
                                ? ContentManager.LoadTextureFromFile(qualifiedBackgroundPath, ContentSource.External)
                                : DefaultBackground;
        }

        SetBackgroundTexture(backgroundTex);
    }

    protected override void LoadContent() {
        byte[] menuClickSoundData = ContentManager.LoadRawAsset("menuhit.wav", ContentSource.User);
        MenuClickSound = AudioEngine.CreateSoundEffectPlayer(menuClickSoundData);

        MenuClickSound.Volume = ConVars.Volume.Value.Value;
        // if (MusicTrack.IsValidHandle)
        // MusicTrack.Volume = ConVars.Volume.Value;

        ConVars.Volume.OnChange += delegate(object _, Value.Number volume) {
            MenuClickSound.Volume = volume.Value;
            // if (MusicTrack.IsValidHandle)
            MusicTrack.Volume = ConVars.Volume.Value.Value;

            if (VolumeSelector is not null)
                VolumeSelector.Text = $"Volume: {ConVars.Volume.Value.Value * 100d:00.##}";
        };

        DefaultBackground = ContentManager.LoadTextureFromFile("background.png", ContentSource.User);

        JapaneseFontData = ContentManager.LoadRawAsset("unifont.ttf", ContentSource.User);

        try {
            JapaneseFont = ContentManager.LoadSystemFont(
            "Aller",
            new FontSystemSettings {
                FontResolutionFactor = 2f, KernelWidth = 1, KernelHeight = 1, Effect = FontSystemEffect.None
            }
            );
            JapaneseFont.AddFont(JapaneseFontData);

            JapaneseFontStroked = ContentManager.LoadSystemFont(
            "Aller",
            new FontSystemSettings {
                FontResolutionFactor = 2f,
                KernelWidth          = 1,
                KernelHeight         = 1,
                Effect               = FontSystemEffect.Stroked,
                EffectAmount         = 2
            }
            );
            JapaneseFontStroked.AddFont(JapaneseFontData);
        }
        catch {
            JapaneseFont.AddFont(JapaneseFontData);
            JapaneseFontStroked.AddFont(JapaneseFontData);
        }

        FurballFontRegular.AddFont(ContentManager.LoadRawAsset("furball-regular.ttf",        ContentSource.User, true));
        FurballFontRegularStroked.AddFont(ContentManager.LoadRawAsset("furball-regular.ttf", ContentSource.User, true));

        LocalLeaderboardButtonTexture  = ContentManager.LoadTextureFromFile("local-leaderboard-button.png");
        FriendLeaderboardButtonTexture = ContentManager.LoadTextureFromFile("friend-leaderboard-button.png");
        GlobalLeaderboardButtonTexture = ContentManager.LoadTextureFromFile("global-leaderboard-button.png");
    }

    public static void ChangeGlobalVolume(float mouseScroll) {
        VolumeSelector.Tweens.Clear();

        VolumeSelector.Tweens.Add(new FloatTween(TweenType.Fade, VolumeSelector.ColorOverride.A / 255f, 1f, Time, Time + 200));

        VolumeSelector.Tweens.Add(new FloatTween(TweenType.Fade, 1f, 0f, Time + 2200, Time + 3200));

        if (mouseScroll > 0)
            ConVars.Volume.Value = new Value.Number(Math.Clamp(ConVars.Volume.Value.Value + 0.05d, 0d, 1d));
        else
            ConVars.Volume.Value = new Value.Number(Math.Clamp(ConVars.Volume.Value.Value - 0.05d, 0d, 1d));
    }

    public static void SelectNewSong() {
        CurrentSong.Value = SongManager.Songs[Random.Next(SongManager.Songs.Count)];
    }

    protected override void Update(double deltaTime) {
        base.Update(deltaTime);
        DiscordManager.Update(deltaTime);

        if (MusicTrack != null) {
            if (CurrentLoopState == MusicLoopState.Loop && MusicTrack.PlaybackState == PlaybackState.Stopped)
                PlayMusic();

            if (CurrentLoopState         == MusicLoopState.LoopFromPreviewPoint &&
                MusicTrack.PlaybackState == PlaybackState.Stopped)//TODO: Set playback position to non-existant preview point i have yet to add.
                PlayMusic();

            if (CurrentLoopState == MusicLoopState.NewSong && MusicTrack.CurrentPosition > MusicTrack.Length - 0.1d) {
                SelectNewSong();
                PlayMusic();
            }
        }


        this._musicTrackSchedulerDelta += deltaTime * 1000;
        if (this._musicTrackSchedulerDelta > 10) {
            if (MusicTrack != null)
                MusicTrackScheduler.Update((int) MusicTrack.CurrentPosition);
            this._musicTrackSchedulerDelta = 0;
        }

        OnlineManager.Update(deltaTime);

        if (this._userPanelManager.Visible)
            this._userPanelManager.Update(deltaTime);

        NotificationManager.Update(deltaTime);
    }

    protected override void Draw(double gameTime) {
        base.Draw(gameTime);

        if (this._userPanelManager.Visible)
            this._userPanelManager.Draw(gameTime, DrawableBatch, new DrawableManagerArgs());

        NotificationManager.Draw(gameTime, DrawableBatch, new DrawableManagerArgs());
    }

    public static void SubmitScore(Song song, PlayerScore score) {
        ScoreManager.AddScore(score);

        if (OnlineManager.State == ConnectionState.LoggedIn && SelectedMods.Count == 0 && score.Username == OnlineManager.Username())
            OnlineManager.SubmitScore(score).Wait();
    }

    protected override void OnClosing() {
        MusicTrackScheduler.Dispose(0);

        // if (OnlineManager.State == ConnectionState.LoggedIn)
        OnlineManager.Logout();

        DiscordManager.Dispose();

        OffsetManager.Save();

        pTypingConfig.Instance.Save();

        base.OnClosing();
    }

    protected override void Initialize() {
        DiscordManager.Initialize();
        
        this.BeforeScreenChange += this.BeforeOnScreenChange;
        this.AfterScreenChange  += this.OnScreenChange;

        OffsetManager.Initialize();

        CurrentSongBackground = new TexturedDrawable(Resources.CreateTexture(), new Vector2(DEFAULT_WINDOW_WIDTH / 2f, DEFAULT_WINDOW_HEIGHT / 2f)) {
            Depth       = 1f,
            OriginType  = OriginType.Center,
            Hoverable   = false,
            Clickable   = false,
            CoverClicks = false,
            CoverHovers = false
        };

        ScoreManager.Load();

        NotificationManager = new NotificationManager();

        ScreenManager.SetBlankTransition();

        // this.LoadContent();
        base.Initialize();

        ScreenManager.SetBlankTransition();

        DevConsole.VolpeEnvironment.SetVariable(ConVars.Volume);
        DevConsole.VolpeEnvironment.SetVariable(ConVars.BackgroundDim);

        DevConsole.VolpeEnvironment.AddBuiltin(ConVars.Login);
        DevConsole.VolpeEnvironment.AddBuiltin(ConVars.SendMessage);
        DevConsole.VolpeEnvironment.AddBuiltin(ConVars.Logout);
        DevConsole.VolpeEnvironment.AddBuiltin(ConVars.LoadUTypingReplay);
        DevConsole.VolpeEnvironment.AddBuiltin(ConVars.LoadAutoReplay);

        OnlineManager = new TatakuOnlineManager(pTypingConfig.Instance.ServerWebsocketUrl, pTypingConfig.Instance.ServerWebUrl);
        OnlineManager.Initialize();

        OnlineManager.OnLogout += delegate {
            if (this._userPanelManager != null)
                this._userPanelManager.Visible = false;
        };
        OnlineManager.OnDisconnect += delegate {
            if (this._userPanelManager != null)
                this._userPanelManager.Visible = false;
        }; 

        if (pTypingConfig.Instance.Username != string.Empty)
            OnlineManager.Login();

        VolumeSelector = new TextDrawable(new Vector2(DEFAULT_WINDOW_WIDTH, DEFAULT_WINDOW_HEIGHT), DEFAULT_FONT, $"Volume {ConVars.Volume.Value.Value}", 50) {
            OriginType = OriginType.BottomRight, Clickable = false, CoverClicks = false
        };

        //Set the opacity to 0
        VolumeSelector.Tweens.Add(new FloatTween(TweenType.Fade, 0f, 0f, 0, 0));

        InputManager.OnMouseScroll += delegate(object _, ((int scrollWheelId, float scrollAmount) scroll, string cursorName) eventArgs) {
            if (InputManager.HeldKeys.Contains(Key.AltLeft))
                ChangeGlobalVolume(eventArgs.scroll.scrollAmount);
        };

        DebugOverlayDrawableManager.Add(VolumeSelector);

        HiraganaConversion.LoadConversion();
        SongManager.UpdateSongs();

        MusicTrackScheduler = new Scheduler();

        OnlineManager.OnlinePlayers.CollectionChanged += this.UpdateUserPanel;

        this._userPanelManager = new DrawableManager();
        this._userPanelManager.Add(
        new TexturedDrawable(WhitePixel, new Vector2(0)) {
            Scale = new Vector2(DEFAULT_WINDOW_WIDTH, DEFAULT_WINDOW_HEIGHT), Depth = 1.5f, ColorOverride = new Color(0, 0, 0, 100)
        }
        );
        this._userPanelManager.Add(
        _OnlineUsersText = new TextDrawable(new Vector2(10), JapaneseFontStroked, "Online Users: 0", 50) {
            Depth = 0f
        }
        );
        this._userPanelManager.Visible = false;
        this.UpdateUserPanel(null, null);

        this._chatDrawable = new ChatDrawable(new Vector2(10, DEFAULT_WINDOW_HEIGHT - 10), new Vector2(DEFAULT_WINDOW_WIDTH - 20, DEFAULT_WINDOW_HEIGHT * 0.4f)) {
            OriginType = OriginType.BottomLeft
        };
        this._userPanelManager.Add(this._chatDrawable);

        InputManager.OnKeyDown                  += this.OnKeyDown;
        GraphicsBackend.Current.ScreenshotTaken += this.OnScreenshotTaken;
        CurrentSong.OnChange                    += this.OnSongChange;

        SelectNewSong();
        PlayMusic();
    }
    
    private void BeforeOnScreenChange(object sender, Screen e) {
        if (e is pScreen s)
            this.CurrentRealScreen = s;
    }
    private void OnSongChange(object sender, Song song) {
        string qualifiedAudioPath = Path.Combine(song.FileInfo.DirectoryName ?? string.Empty, song.AudioPath);

        LoadMusic(ContentManager.LoadRawAsset(qualifiedAudioPath, ContentSource.External));
        
        LoadBackgroundFromSong(song);

        UpdateCurrentOnlineStatus(this.CurrentRealScreen);
    }

    public static void UpdateCurrentOnlineStatus(pScreen screen) {
        if (OnlineManager.State != ConnectionState.LoggedIn || screen == null) return;

        UserActionType actionType = UserActionType.Unknown;
        string         final      = "Unknown";

        switch (screen.OnlineUserActionType) {
            case ScreenUserActionType.Listening:
                actionType = UserActionType.Idle;
                final      = $"Listening to {CurrentSong.Value.Artist} - {CurrentSong.Value.Name}";
                break;
            case ScreenUserActionType.Editing:
                final = pTypingConfig.Instance.Username == CurrentSong.Value.Creator
                            ? $"Editing {CurrentSong.Value.Artist} - {CurrentSong.Value.Name} [{CurrentSong.Value.Difficulty}] by {CurrentSong.Value.Creator}"
                            : $"Modding {CurrentSong.Value.Artist} - {CurrentSong.Value.Name} [{CurrentSong.Value.Difficulty}] by {CurrentSong.Value.Creator}";
                actionType = UserActionType.Editing;
                break;
            case ScreenUserActionType.ChoosingSong:
                final      = "Choosing a song!";
                actionType = UserActionType.Idle;
                break;
            case ScreenUserActionType.Playing:
                final      = $"Playing {CurrentSong.Value.Artist} - {CurrentSong.Value.Name} [{CurrentSong.Value.Difficulty}]";
                actionType = UserActionType.Ingame;
                break;
            case ScreenUserActionType.Lobbying:
                final      = $"Partying to {CurrentSong.Value.Artist} - {CurrentSong.Value.Name} [{CurrentSong.Value.Difficulty}]";
                actionType = UserActionType.Idle;
                break;
            case ScreenUserActionType.Multiplaying:
                final      = $"Multiplaying {CurrentSong.Value.Artist} - {CurrentSong.Value.Name} [{CurrentSong.Value.Difficulty}]";
                actionType = UserActionType.Ingame;
                break;
        }

        OnlineManager.ChangeUserAction(new UserAction(actionType, final));
    }
    
    private void OnScreenChange(object _, Screen screen) {
        pScreen actualScreen = screen as pScreen;

        this.WindowManager.SetWindowTitle(screen is not pScreen ? "pTyping" : $"pTyping - {actualScreen.Name}");

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (actualScreen != null) {
            this.CurrentRealScreen = actualScreen;
            
            if (actualScreen.BackgroundFadeAmount != -1f)
                CurrentSongBackground.Tweens.Add(
                new ColorTween(
                TweenType.Color,
                CurrentSongBackground.ColorOverride,
                new Color(actualScreen.BackgroundFadeAmount, actualScreen.BackgroundFadeAmount, actualScreen.BackgroundFadeAmount),
                CurrentSongBackground.TimeSource.GetCurrentTime(),
                CurrentSongBackground.TimeSource.GetCurrentTime() + 1000
                )
                );

            if (actualScreen.ForceSpeedReset)
                MusicTrack.SetSpeed(1f);
            
            SetSongLoopState(actualScreen.LoopState);

            if (pTypingConfig.Instance.FpsBasedOnMonitorHz) {
                int? videoModeRefreshRate = this.WindowManager.Monitor.VideoMode.RefreshRate ?? 60;

                switch (actualScreen.ScreenType) {
                    case ScreenType.Gameplay:
                        if (pTypingConfig.Instance.UnlimitedFpsGameplay)
                            this.SetTargetFps(-1);
                        else
                            this.SetTargetFps((int)(videoModeRefreshRate.Value * pTypingConfig.Instance.GameplayFpsMult));
                        break;
                    case ScreenType.Menu:
                        if (pTypingConfig.Instance.UnlimitedFpsMenu)
                            this.SetTargetFps(-1);
                        else
                            this.SetTargetFps((int)(videoModeRefreshRate.Value * pTypingConfig.Instance.MenuFpsMult));
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }

            UpdateCurrentOnlineStatus(actualScreen);
        }
    }
    private static void SetSongLoopState(MusicLoopState loopState) {
        CurrentLoopState = loopState;
        switch (loopState) {
            case MusicLoopState.Loop:
                MusicTrack.Loop = true;
                break;
            case MusicLoopState.LoopFromPreviewPoint:
                MusicTrack.Loop = false;
                break;
            case MusicLoopState.NewSong:
                MusicTrack.Loop = false;
                break;
            case MusicLoopState.None:
                MusicTrack.Loop = false;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnScreenshotTaken(object sender, Image e) {
        string filename = "screenshot.png";//TODO: Dont just overwrite the image again and again, and maybe add a `screenshots` folder?

        e.SaveAsPng(filename);

        NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Info, $"Screenshot taken as {filename}!");
    }

    public override void InitializeLocalizations() {
        //default language is already english, and im an english speaker, so no need to set it here

        LocalizationManager.AddDefaultTranslation(Localizations.MenuRevision, "Revision {0}");
        LocalizationManager.AddDefaultTranslation(Localizations.Changelog,    "Changelog");

        base.InitializeLocalizations();
    }

    private void OnKeyDown(object sender, Key e) {
        switch (e) {
            case Key.Escape:
                if (!this._userPanelManager.Visible) break;
                goto case Key.F8;
            case Key.F8:
            case Key.F9:
                if (OnlineManager.State != ConnectionState.LoggedIn) return;

                this._userPanelManager.Visible = !this._userPanelManager.Visible;

                this._chatDrawable.MessageInputDrawable.Selected = false;

                foreach (BaseDrawable drawable in this._userPanelManager.Drawables)
                    drawable.Visible = this._userPanelManager.Visible;

                break;
            case Key.F11: {
                this.ChangeScreenSize((int) this.WindowManager.WindowSize.X, (int) this.WindowManager.WindowSize.Y, !this.WindowManager.Fullscreen);
                break;
            }
            case Key.F2: {
                GraphicsBackend.Current.TakeScreenshot();
                break;
            }
        }
    }

    private void UpdateUserPanel(object sender, object e) {
        GameTimeScheduler.ScheduleMethod(
        _ => {
            _OnlineUsersText.Text = $"Online Users: {OnlineManager.OnlinePlayers.Count(x => !x.Bot)} ({OnlineManager.OnlinePlayers.Count})";

            this._userPanelDrawables.ForEach(x => { this._userPanelManager.Remove(x); });
            this._userPanelDrawables.Clear();

            Vector2 pos = new(10, 10 + _OnlineUsersText.Size.Y + 10);
            foreach (OnlinePlayer player in OnlineManager.OnlinePlayers) {
                UserCardDrawable drawable = player.GetUserCard();
                drawable.MoveTo(pos);
                pos.X += drawable.Size.X + 10;

                if (pos.X + drawable.Size.X > DEFAULT_WINDOW_WIDTH) {
                    pos.X =  10;
                    pos.Y += drawable.Size.Y + 10;
                }

                drawable.ClearEvents();
                drawable.OnClick += delegate(object _, (MouseButton button, Point pos) a) {
                    switch (a.button) {
                        case MouseButton.Left: {
                            lock (OnlineManager.KnownChannels) {
                                if (!OnlineManager.KnownChannels.Contains(player.Username))
                                    OnlineManager.KnownChannels.Add(player.Username);
                            }
                            break;
                        }
                        case MouseButton.Right:
                            if (OnlineManager.Host != null) return;

                            OnlineManager.SpectatePlayer(player);
                            break;
                    }

                };

                this._userPanelDrawables.Add(drawable);
                this._userPanelManager.Add(drawable);
            }
        },
        Time
        );
    }
}
