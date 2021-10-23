using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using FontStashSharp;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Audio;
using Furball.Engine.Engine.DevConsole;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Helpers;
using ManagedBass;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using pTyping.Drawables;
using pTyping.Online;
using pTyping.Player;
using pTyping.Screens;
using pTyping.Songs;

namespace pTyping {
    public class pTypingGame : FurballGame {
        public static Texture2D BackButtonTexture;
        public static Texture2D DefaultBackground;

        public static readonly AudioStream MusicTrack = new();
        public static          Scheduler   MusicTrackScheduler;
        public static readonly SoundEffect MenuClickSound = new();

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

        public static readonly Regex Alphanumeric = new("[^a-zA-Z0-9]");

        public static Texture2D LocalLeaderboardButtonTexture;
        public static Texture2D FriendLeaderboardButtonTexture;
        public static Texture2D GlobalLeaderboardButtonTexture;

        private          double                _musicTrackSchedulerDelta = 0;
        private readonly List<ManagedDrawable> _userPanelDrawables       = new();

        private DrawableManager _userPanelManager;

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
            if (OnlineManager.State != ConnectionState.LoggedIn) return;

            if (ConVars.Username.Value == CurrentSong.Value.Creator)
                OnlineManager.ChangeUserAction(
                new(
                UserActionType.Editing,
                $"Editing {CurrentSong.Value.Artist} - {CurrentSong.Value.Name} [{CurrentSong.Value.Difficulty}] by {CurrentSong.Value.Creator}"
                )
                );
            else
                OnlineManager.ChangeUserAction(
                new(
                UserActionType.Editing,
                $"Modding {CurrentSong.Value.Artist} - {CurrentSong.Value.Name} [{CurrentSong.Value.Difficulty}] by {CurrentSong.Value.Creator}"
                )
                );
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
            if (MusicTrack.IsValidHandle)
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
            if (MusicTrack.IsValidHandle) {
                MusicTrack.Stop();
                MusicTrack.Free();
            }
            MusicTrack.Load(data);
        }

        public static void LoadBackButtonTexture() {
            BackButtonTexture ??= ContentManager.LoadMonogameAsset<Texture2D>("backbutton", ContentSource.User);
        }

        public static void SetBackgroundTexture(Texture2D tex) {
            CurrentSongBackground.SetTexture(tex);

            CurrentSongBackground.Scale = new(1f / ((float)CurrentSongBackground.Texture.Height / DEFAULT_WINDOW_HEIGHT));
        }

        public static void LoadBackgroundFromSong(Song song) {
            Texture2D backgroundTex;
            if (song.BackgroundPath == null) {
                DefaultBackground ??= ContentManager.LoadMonogameAsset<Texture2D>("background");

                backgroundTex = DefaultBackground;
            } else {
                string qualifiedBackgroundPath = Path.Combine(song.FileInfo.DirectoryName ?? string.Empty, song.BackgroundPath);
                if (File.Exists(qualifiedBackgroundPath))
                    backgroundTex = Texture2D.FromStream(
                    Instance.GraphicsDevice,
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
            MenuClickSound.Load(menuClickSoundData);

            MenuClickSound.Volume = ConVars.Volume.Value;
            if (MusicTrack.IsValidHandle)
                MusicTrack.Volume = ConVars.Volume.Value;

            ConVars.Volume.BindableValue.OnChange += delegate(object _, float volume) {
                MenuClickSound.Volume = volume;
                if (MusicTrack.IsValidHandle)
                    MusicTrack.Volume = ConVars.Volume.Value;

                if (VolumeSelector is not null)
                    VolumeSelector.Text = $"Volume: {ConVars.Volume.Value * 100f:00.##}";
            };

            DefaultBackground = ContentManager.LoadMonogameAsset<Texture2D>("background");
            JapaneseFontData  = ContentManager.LoadRawAsset("unifont.ttf", ContentSource.User);
            JapaneseFont.AddFont(JapaneseFontData);
            JapaneseFontStroked.AddFont(JapaneseFontData);

            LocalLeaderboardButtonTexture  = Texture2D.FromStream(this.GraphicsDevice, new MemoryStream(ContentManager.LoadRawAsset("local-leaderboard-button.png")));
            FriendLeaderboardButtonTexture = Texture2D.FromStream(this.GraphicsDevice, new MemoryStream(ContentManager.LoadRawAsset("friend-leaderboard-button.png")));
            GlobalLeaderboardButtonTexture = Texture2D.FromStream(this.GraphicsDevice, new MemoryStream(ContentManager.LoadRawAsset("global-leaderboard-button.png")));
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
        protected override void Update(GameTime gameTime) {
            base.Update(gameTime);

            this._musicTrackSchedulerDelta += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (this._musicTrackSchedulerDelta > 10 && MusicTrack.IsValidHandle) {
                MusicTrackScheduler.Update(MusicTrack.GetCurrentTime());
                this._musicTrackSchedulerDelta = 0;
            }

            if (this._userPanelManager.Visible)
                this._userPanelManager.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            base.Draw(gameTime);

            if (this._userPanelManager.Visible)
                this._userPanelManager.Draw(gameTime, DrawableBatch);
        }

        protected override void EndRun() {
            MusicTrackScheduler.Dispose(0);

            if (OnlineManager.State == ConnectionState.LoggedIn)
                OnlineManager.Logout().Wait();

            base.EndRun();
        }

        public static void SubmitScore(Song song, PlayerScore score) {
            ScoreManager.AddScore(score);

            if (OnlineManager.State == ConnectionState.LoggedIn)
                OnlineManager.SubmitScore(score).Wait();
        }

        protected override void Initialize() {
            DevConsole.AddConVar(ConVars.Username);
            DevConsole.AddConVar(ConVars.Password);
            DevConsole.AddConVar(ConVars.Volume);
            DevConsole.AddConVar(ConVars.BackgroundDim);
            DevConsole.AddConVar(ConVars.BaseApproachTime);

            JapaneseFontData = ContentManager.LoadRawAsset("unifont.ttf", ContentSource.User);
            JapaneseFont.AddFont(JapaneseFontData);

            CurrentSongBackground =
                new TexturedDrawable(new Texture2D(this.GraphicsDevice, 1, 1), new Vector2(DEFAULT_WINDOW_WIDTH / 2f, DEFAULT_WINDOW_HEIGHT / 2f)) {
                    Depth      = 1f,
                    OriginType = OriginType.Center
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

            InputManager.OnMouseScroll += delegate(object _, (int, string) eventArgs) {
                if (InputManager.HeldKeys.Contains(Keys.LeftAlt))
                    ChangeGlobalVolume(eventArgs.Item1);
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

            InputManager.OnKeyDown += this.OnKeyDown;
        }

        private void OnKeyDown(object sender, Keys e) {
            if (e == Keys.F9)
                this._userPanelManager.Visible = !this._userPanelManager.Visible;
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
                    this._userPanelDrawables.Add(drawable);
                    this._userPanelManager.Add(drawable);
                }
            }
        }
    }
}
