using System;
using System.IO;
using System.Text.RegularExpressions;
using FontStashSharp;
using ManagedBass;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Audio;
using Furball.Engine.Engine.Helpers;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using pTyping.Songs;
using pTyping.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using pTyping.Online;
using pTyping.Player;

namespace pTyping {
    public class pTypingGame : FurballGame {
        public static Texture2D BackButtonTexture;
        public static Texture2D DefaultBackground;

        public static readonly AudioStream MusicTrack          = new();
        public static          Scheduler   MusicTrackScheduler;
        public static readonly SoundEffect MenuClickSound      = new();

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

        public static readonly Regex Alphanumeric = new("[^a-zA-Z0-9]");

        public static void UserStatusEditing() {
            if (OnlineManager.State != ConnectionState.LoggedIn) return;
            
            if(Config.Username == CurrentSong.Value.Creator)
                OnlineManager.ChangeUserAction(new(UserActionType.Editing, $"Editing {CurrentSong.Value.Artist} - {CurrentSong.Value.Name} [{CurrentSong.Value.Difficulty}] by {CurrentSong.Value.Creator}"));
            else
                OnlineManager.ChangeUserAction(new(UserActionType.Editing, $"Modding {CurrentSong.Value.Artist} - {CurrentSong.Value.Name} [{CurrentSong.Value.Difficulty}] by {CurrentSong.Value.Creator}"));
        }
        
        public static void UserStatusPickingSong() {
            if (OnlineManager.State != ConnectionState.LoggedIn) return;

            OnlineManager.ChangeUserAction(new(UserActionType.Idle, $"Choosing a song!"));
        }
        
        public static void UserStatusListening() {
            if (OnlineManager.State != ConnectionState.LoggedIn) return;

            OnlineManager.ChangeUserAction(new(UserActionType.Idle, $"Listening to {CurrentSong.Value.Artist} - {CurrentSong.Value.Name}"));
        }

        public static void UserStatusPlaying() {
            if (OnlineManager.State != ConnectionState.LoggedIn) return;

            OnlineManager.ChangeUserAction(new(UserActionType.Ingame, $"Playing {CurrentSong.Value.Artist} - {CurrentSong.Value.Name} [{CurrentSong.Value.Difficulty}]"));
        }
        
        public static void PlayMusic() {
            MusicTrack.Play();
            if (MusicTrack.IsValidHandle)
                MusicTrack.Volume = Config.Volume;
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

        public pTypingGame() : base(new MenuScreen()) {
            // this.Window.AllowUserResizing = true;
        }

        protected override void LoadContent() {
            base.LoadContent();

            byte[] menuClickSoundData = ContentManager.LoadRawAsset("menuhit.wav", ContentSource.User);
            MenuClickSound.Load(menuClickSoundData);

            MenuClickSound.Volume = Config.Volume;
            if (MusicTrack.IsValidHandle)
                MusicTrack.Volume = Config.Volume;
            Config.Volume.OnChange += delegate(object _, float volume) {
                MenuClickSound.Volume = volume;
                if (MusicTrack.IsValidHandle)
                    MusicTrack.Volume = Config.Volume;

                if (VolumeSelector is not null)
                    VolumeSelector.Text = $"Volume: {Config.Volume.Value * 100f:00.##}";
            };

            DefaultBackground = ContentManager.LoadMonogameAsset<Texture2D>("background");
            JapaneseFontData  = ContentManager.LoadRawAsset("unifont.ttf", ContentSource.User);
            JapaneseFont.AddFont(JapaneseFontData);
        }

        public static void ChangeGlobalVolume(int mouseScroll) {
            VolumeSelector.Tweens.Clear();
            
            VolumeSelector.Tweens.Add(new FloatTween(TweenType.Fade, VolumeSelector.ColorOverride.A / 255f, 1f, Time, Time + 200));

            VolumeSelector.Tweens.Add(new FloatTween(TweenType.Fade, 1f, 0f, Time + 2200, Time + 3200));

            if (mouseScroll > 0)
                Config.Volume.Value = Math.Clamp(Config.Volume + 0.05f, 0f, 1f);
            else
                Config.Volume.Value = Math.Clamp(Config.Volume - 0.05f, 0f, 1f);
        }

        public static void ChangeTargetFPS(double target) {
            // if (target == 0) {
            // Instance.IsFixedTimeStep   = false;

            // return;
            // }

            // Instance.TargetElapsedTime = TimeSpan.FromMilliseconds(1000d / target);
            // Instance.IsFixedTimeStep   = true;
        }

        private double _musicTrackSchedulerDelta = 0;
        protected override void Update(GameTime gameTime) {
            base.Update(gameTime);

            this._musicTrackSchedulerDelta += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (this._musicTrackSchedulerDelta > 10 && MusicTrack.IsValidHandle) {
                MusicTrackScheduler.Update(MusicTrack.GetCurrentTime());
                this._musicTrackSchedulerDelta = 0;
            }
        }

        protected override void EndRun() {
            MusicTrackScheduler.Dispose(0);
            
            if (OnlineManager.State == ConnectionState.LoggedIn) 
                OnlineManager.Logout().Wait();
            
            base.EndRun();
        }

        public static void SubmitScore(Song song, PlayerScore score) {
            ScoreManager.AddScore(score);
        }

        protected override void Initialize() {
            JapaneseFontData = ContentManager.LoadRawAsset("unifont.ttf", ContentSource.User);
            JapaneseFont.AddFont(JapaneseFontData);

            CurrentSongBackground =
                new TexturedDrawable(new Texture2D(this.GraphicsDevice, 1, 1), new Vector2(DEFAULT_WINDOW_WIDTH / 2f, DEFAULT_WINDOW_HEIGHT / 2f)) {
                    Depth      = 1f,
                    OriginType = OriginType.Center
                };
                
            ScoreManager.Load();
            
            OnlineManager = new TaikoRsOnlineManager("ws://localhost:8080");
            OnlineManager.Initialize();
            OnlineManager.Login();

            base.Initialize();

            VolumeSelector = new(new(DEFAULT_WINDOW_WIDTH, DEFAULT_WINDOW_HEIGHT), DEFAULT_FONT, $"Volume {Config.Volume.Value}", 50) {
                OriginType = OriginType.BottomRight
            };

            //Set the opacity to 0
            VolumeSelector.Tweens.Add(new FloatTween(TweenType.Fade, 0f, 0f, 0, 0));

            InputManager.OnMouseScroll += delegate(object _, (int, string) eventArgs) {
                if (InputManager.HeldKeys.Contains(Keys.LeftAlt))
                    ChangeGlobalVolume(eventArgs.Item1);
            };

            DrawableManager.Add(VolumeSelector);

            ChangeTargetFPS(Config.TargetFPS);

            Config.TargetFPS.OnChange += delegate(object _, int newTarget) {
                ChangeTargetFPS(newTarget);
            };

            HiraganaConversion.LoadConversion();
            ScreenManager.SetBlankTransition();
            SongManager.UpdateSongs();

            MusicTrackScheduler = new();
        }
    }
}
