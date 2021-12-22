using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading;
using DiscordRPC;
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
using Furball.Engine.Engine.Timing;
using Furball.Vixie.Graphics;
using ManagedBass;
using pTyping.Graphics;
using pTyping.Graphics.Menus;
using pTyping.Graphics.Online;
using pTyping.Graphics.Player;
using pTyping.Graphics.Player.Mods;
using pTyping.Online;
using pTyping.Online.Taiko_rs;
using pTyping.Scores;
using pTyping.Songs;
using Silk.NET.Input;
using sowelipisona;
using ConVars=pTyping.Engine.ConVars;

namespace pTyping {
    // ReSharper disable once InconsistentNaming
    public class pTypingGame : FurballGame {
        public static readonly Vector2 BackButtonScale = new(0.12f);

        public static Texture BackButtonTexture;
        public static Texture DefaultBackground;

        public static AudioStream           MusicTrack           = null;
        public static AudioStreamTimeSource MusicTrackTimeSource = null;
        public static SoundEffectPlayer     MenuClickSound       = null;
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
            Effect               = FontSystemEffect.None
        }
        );
        public static FontSystem JapaneseFontStroked = new(
        new FontSystemSettings {
            FontResolutionFactor = 2f,
            KernelWidth          = 1,
            KernelHeight         = 1,
            Effect               = FontSystemEffect.Stroked,
            EffectAmount         = 2
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

        public static DiscordRpcClient RpcClient;
        public static RichPresence     RichPresence = new();

        public pTypingGame() : base(new MenuScreen()) {
            // this.Window.AllowUserResizing = true;
        }

        public static ManagedDrawable GetUserCard() {
            if (OnlineManager.State != ConnectionState.LoggedIn)
                return new BlankDrawable();

            if (MenuPlayerUserCard is not null)
                return MenuPlayerUserCard;

            MenuPlayerUserCard = new(Vector2.Zero, OnlineManager.Player) {
                Scale = new(0.3f)
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

        public static void UserStatusEditing() {
            if (ConVars.Username.Value == CurrentSong.Value.Creator) {
                string text = $"Editing {CurrentSong.Value.Artist} - {CurrentSong.Value.Name} [{CurrentSong.Value.Difficulty}] by {CurrentSong.Value.Creator}";

                if (OnlineManager.State == ConnectionState.LoggedIn)
                    OnlineManager.ChangeUserAction(new(UserActionType.Editing, text));
            } else {
                string text = $"Modding {CurrentSong.Value.Artist} - {CurrentSong.Value.Name} [{CurrentSong.Value.Difficulty}] by {CurrentSong.Value.Creator}";

                if (OnlineManager.State == ConnectionState.LoggedIn)
                    OnlineManager.ChangeUserAction(new(UserActionType.Editing, text));
            }
        }

        public static void UserStatusPickingSong() {
            if (OnlineManager.State != ConnectionState.LoggedIn) return;
            OnlineManager.ChangeUserAction(new(UserActionType.Idle, "Choosing a song!"));
        }

        public static void UserStatusListening() {
            if (OnlineManager.State != ConnectionState.LoggedIn) return;
            OnlineManager.ChangeUserAction(new(UserActionType.Idle, $"Listening to {CurrentSong.Value.Artist} - {CurrentSong.Value.Name}"));
        }

        public static void UserStatusPlaying() {
            if (OnlineManager.State != ConnectionState.LoggedIn) return;
            OnlineManager.ChangeUserAction(
            new(UserActionType.Ingame, $"Playing {CurrentSong.Value.Artist} - {CurrentSong.Value.Name} [{CurrentSong.Value.Difficulty}]")
            );
        }

        public static void PlayMusic() {
            MusicTrack.Play();
            // if (MusicTrack.IsValidHandle)
            MusicTrack.Volume = ConVars.Volume.Value;
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
            MusicTrack           = AudioEngine.CreateStream(data);
            MusicTrackTimeSource = new(MusicTrack);

            // MusicTrack.TempoFrequencyLock = true;
        }

        public static void LoadBackButtonTexture() {
            BackButtonTexture ??= ContentManager.LoadTextureFromFile("backbutton.png", ContentSource.User);
        }

        public static void SetBackgroundTexture(Texture tex) {
            CurrentSongBackground.SetTexture(tex);

            CurrentSongBackground.Scale = new(1f / (CurrentSongBackground.Texture.Size.Y / DEFAULT_WINDOW_HEIGHT));
        }

        public static void LoadBackgroundFromSong(Song song) {
            Texture backgroundTex;
            if (song.BackgroundPath == null) {
                DefaultBackground ??= ContentManager.LoadTextureFromFile("background.png");

                backgroundTex = DefaultBackground;
            } else {
                string qualifiedBackgroundPath = Path.Combine(song.FileInfo.DirectoryName ?? string.Empty, song.BackgroundPath);
                if (File.Exists(qualifiedBackgroundPath))
                    backgroundTex = new Texture(
                    new MemoryStream(ContentManager.LoadRawAsset(qualifiedBackgroundPath, ContentSource.External))
                    );
                else
                    backgroundTex = DefaultBackground;
            }

            SetBackgroundTexture(backgroundTex);
        }

        protected override void LoadContent() {
            base.LoadContent();

            byte[] menuClickSoundData = ContentManager.LoadRawAsset("menuhit.wav", ContentSource.User);
            MenuClickSound = AudioEngine.CreateSoundEffectPlayer(menuClickSoundData);

            MenuClickSound.Volume = ConVars.Volume.Value;
            // if (MusicTrack.IsValidHandle)
            // MusicTrack.Volume = ConVars.Volume.Value;

            ConVars.Volume.BindableValue.OnChange += delegate(object _, float volume) {
                MenuClickSound.Volume = volume;
                // if (MusicTrack.IsValidHandle)
                MusicTrack.Volume = ConVars.Volume.Value;

                if (VolumeSelector is not null)
                    VolumeSelector.Text = $"Volume: {ConVars.Volume.Value * 100f:00.##}";
            };

            DefaultBackground = ContentManager.LoadTextureFromFile("background.png");

            JapaneseFontData = ContentManager.LoadRawAsset("unifont.ttf", ContentSource.User);

            try {
                JapaneseFont = ContentManager.LoadSystemFont(
                "Aller",
                new FontSystemSettings {
                    FontResolutionFactor = 2f,
                    KernelWidth          = 1,
                    KernelHeight         = 1,
                    Effect               = FontSystemEffect.None
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

            LocalLeaderboardButtonTexture  = new Texture(new MemoryStream(ContentManager.LoadRawAsset("local-leaderboard-button.png")));
            FriendLeaderboardButtonTexture = new Texture(new MemoryStream(ContentManager.LoadRawAsset("friend-leaderboard-button.png")));
            GlobalLeaderboardButtonTexture = new Texture(new MemoryStream(ContentManager.LoadRawAsset("global-leaderboard-button.png")));
        }

        public static void ChangeGlobalVolume(int mouseScroll) {
            VolumeSelector.Tweens.Clear();

            VolumeSelector.Tweens.Add(new FloatTween(TweenType.Fade, VolumeSelector.ColorOverride.A / 255f, 1f, Time, Time + 200));

            VolumeSelector.Tweens.Add(new FloatTween(TweenType.Fade, 1f, 0f, Time + 2200, Time + 3200));

            if (mouseScroll > 0)
                ConVars.Volume.Value = Math.Clamp(ConVars.Volume.Value + 0.05f, 0f, 1f);
            else
                ConVars.Volume.Value = Math.Clamp(ConVars.Volume.Value - 0.05f, 0f, 1f);
        }

        protected override void Update(double gameTime) {
            base.Update(gameTime);

            this._musicTrackSchedulerDelta += gameTime * 1000;
            if (this._musicTrackSchedulerDelta > 10) {
                MusicTrackScheduler.Update((int)MusicTrack.CurrentPosition);
                this._musicTrackSchedulerDelta = 0;
            }

            OnlineManager.Update(gameTime);
            
            if (this._userPanelManager.Visible)
                this._userPanelManager.Update(gameTime);
        }

        protected override void Draw(double gameTime) {
            base.Draw(gameTime);

            if (this._userPanelManager.Visible)
                this._userPanelManager.Draw(gameTime, DrawableBatch, new());
        }

        protected override void OnClosing() {
            MusicTrackScheduler.Dispose(0);

            if (OnlineManager.State == ConnectionState.LoggedIn)
                OnlineManager.Logout().Wait();

            RpcClient.Dispose();

            base.OnClosing();
        }

        public static void SubmitScore(Song song, PlayerScore score) {
            ScoreManager.AddScore(score);

            if (OnlineManager.State == ConnectionState.LoggedIn && SelectedMods.Count == 0 && score.Username == OnlineManager.Username())
                OnlineManager.SubmitScore(score).Wait();
        }

        protected override void Initialize() {
            RpcClient = new("908631391934222366");

            RpcClient.Initialize();

            RpcClient.Invoke();

            this.AfterScreenChange += (_, screen)
                => this.WindowManager.SetWindowTitle(screen is not pScreen actualScreen ? "pTyping" : $"pTyping - {actualScreen.Name}");

            Thread thread = new(
            () => {
                Thread.Sleep(1000);
            
                while (true) {
                    if (RpcClient.IsDisposed) return;
                    
                    if (RpcClient.CurrentUser == null) {
                        Thread.Sleep(1000);
                        continue;
                    }

                    pScreen screen = Instance.RunningScreen as pScreen;

                    RichPresence.State   = screen?.State;
                    RichPresence.Details = screen?.Details;

                    RpcClient.SetPresence(RichPresence);
                    RpcClient.Invoke();

                    Thread.Sleep(100);
                }
            }
            );
            thread.Start();

            DevConsole.AddConVarStore(typeof(ConVars));

            CurrentSongBackground = new TexturedDrawable(new Texture(1, 1), new Vector2(DEFAULT_WINDOW_WIDTH / 2f, DEFAULT_WINDOW_HEIGHT / 2f)) {
                    Depth       = 1f,
                    OriginType  = OriginType.Center,
                    Hoverable   = false,
                    Clickable   = false,
                    CoverClicks = false,
                    CoverHovers = false
                };

            ScoreManager.Load();

            OnlineManager = new TaikoRsOnlineManager("ws://localhost:8080", "http://127.0.0.1:8000");
            OnlineManager.Initialize();
            OnlineManager.Login().Wait();

            base.Initialize();

            VolumeSelector = new(new(DEFAULT_WINDOW_WIDTH, DEFAULT_WINDOW_HEIGHT), DEFAULT_FONT, $"Volume {ConVars.Volume.Value}", 50) {
                OriginType  = OriginType.BottomRight,
                Clickable   = false,
                CoverClicks = false
            };

            //Set the opacity to 0
            VolumeSelector.Tweens.Add(new FloatTween(TweenType.Fade, 0f, 0f, 0, 0));

            InputManager.OnMouseScroll += delegate(object? sender, ((int scrollWheelId, float scrollAmount) scroll, string cursorName) tuple) {
                if (InputManager.HeldKeys.Contains(Key.AltLeft))
                    ChangeGlobalVolume((int)tuple.scroll.scrollAmount);
            };

            DrawableManager.Add(VolumeSelector);

            HiraganaConversion.LoadConversion();
            ScreenManager.SetBlankTransition();
            SongManager.UpdateSongs();

            MusicTrackScheduler = new();

            OnlineManager.OnlinePlayers.CollectionChanged += this.UpdateUserPanel;

            this._userPanelManager         = new();
            this._userPanelManager.Visible = false;
            this.UpdateUserPanel(null, null);

            this._chatDrawable = new(new(10, DEFAULT_WINDOW_HEIGHT - 10), new(DEFAULT_WINDOW_WIDTH - 20, DEFAULT_WINDOW_HEIGHT * 0.4f)) {
                OriginType = OriginType.BottomLeft
            };
            this._userPanelManager.Add(this._chatDrawable);

            InputManager.OnKeyDown += this.OnKeyDown;
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
            }
        }

        private void UpdateUserPanel(object sender, object e) {
            lock (this._userPanelDrawables) {
                this._userPanelDrawables.ForEach(x => this._userPanelManager.Remove(x));
                this._userPanelDrawables.Clear();

                Vector2 pos = new(10);
                foreach (KeyValuePair<int, OnlinePlayer> player in OnlineManager.OnlinePlayers) {
                    UserCardDrawable drawable = player.Value.GetUserCard();
                    drawable.MoveTo(pos);
                    pos.X += drawable.Size.X + 10;

                    if (pos.X + drawable.Size.X > DEFAULT_WINDOW_WIDTH) {
                        pos.X =  10;
                        pos.Y += drawable.Size.Y + 10;
                    }
                    
                    this._userPanelDrawables.Add(drawable);
                    this._userPanelManager.Add(drawable);
                }
            }
        }
    }
}
