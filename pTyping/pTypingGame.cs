using System;
using System.IO;
using System.Text.RegularExpressions;
using ManagedBass;
using Furball.Engine;
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

namespace pTyping {
	public class pTypingGame : FurballGame {
		public static Texture2D BackButtonTexture;
		public static Texture2D DefaultBackground;

		public static readonly AudioStream MusicTrack     = new();
		public static readonly SoundEffect MenuClickSound = new();

		public static Bindable<Song> CurrentSong;
			
		public static TextDrawable     VolumeSelector;
		public static TexturedDrawable CurrentSongBackground;

		public static byte[] UniFont;
		
		public static readonly Regex Alphanumeric = new("[^a-zA-Z0-9]");

		public static void PlayMusic() {
			MusicTrack.Play();
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
					
			CurrentSongBackground.Scale = new(1f / ((float)CurrentSongBackground.Texture.Height / FurballGame.DEFAULT_WINDOW_HEIGHT));
		}

		public static void LoadBackgroundFromSong(Song song) {
			Texture2D backgroundTex;
			if (song.BackgroundPath == null) {
				DefaultBackground ??= ContentManager.LoadMonogameAsset<Texture2D>("background");

				backgroundTex = DefaultBackground;
			}
			else {
				string qualifiedBackgroundPath = Path.Combine(song.FileInfo.DirectoryName ?? string.Empty, song.BackgroundPath);
				if(File.Exists(qualifiedBackgroundPath))
					backgroundTex = Texture2D.FromStream(Instance.GraphicsDevice, new MemoryStream(ContentManager.LoadRawAsset(qualifiedBackgroundPath, ContentSource.External)));
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
			MusicTrack.Volume     = Config.Volume;
			Config.Volume.OnChange += delegate (object _, float volume) {
				MenuClickSound.Volume = volume;
				MusicTrack.Volume = volume;
				
				if(VolumeSelector is not null)
					VolumeSelector.Text = $"Volume: {Config.Volume.Value * 100f:00.##}";
			};
			
			DefaultBackground = ContentManager.LoadMonogameAsset<Texture2D>("background");
			UniFont           = ContentManager.LoadRawAsset("unifont.ttf", ContentSource.User);
		}

		public static void ChangeGlobalVolume(int mouseScroll) {
			VolumeSelector.Tweens.Add(new FloatTween(TweenType.Fade, VolumeSelector.ColorOverride.A / 255f, 1f, Time, Time + 200));
			
			VolumeSelector.Tweens.Add(new FloatTween(TweenType.Fade, 1f, 0f, Time + 2200, Time + 3200));

			if (mouseScroll > 0) {
				Config.Volume.Value = Math.Clamp(Config.Volume + 0.05f, 0f, 1f);
			} else {
				Config.Volume.Value = Math.Clamp(Config.Volume - 0.05f, 0f, 1f);
			}
		}

		public static void ChangeTargetFPS(double target) {
			// if (target == 0) {
				// Instance.IsFixedTimeStep   = false;
				
				// return;
			// }
			
			// Instance.TargetElapsedTime = TimeSpan.FromMilliseconds(1000d / target);
			// Instance.IsFixedTimeStep   = true;
		}
		
		protected override void Initialize() {
			UniFont ??= ContentManager.LoadRawAsset("unifont.ttf", ContentSource.User);

			CurrentSongBackground = new TexturedDrawable(new Texture2D(this.GraphicsDevice, 1, 1), new Vector2(DEFAULT_WINDOW_WIDTH / 2f, DEFAULT_WINDOW_HEIGHT / 2f)) {
				Depth      = 1f,
				OriginType = OriginType.Center
			};
			
			base.Initialize();
			
			VolumeSelector = new(new(DEFAULT_WINDOW_WIDTH, DEFAULT_WINDOW_HEIGHT), DEFAULT_FONT, $"Volume {Config.Volume.Value}", 50) {
				OriginType = OriginType.BottomRight
			};
			
			//Set the opacity to 0
			VolumeSelector.Tweens.Add(new FloatTween(TweenType.Fade, 0f, 0f, 0, 0));
			
			InputManager.OnMouseScroll += delegate(object _, (int, string) eventArgs) {
				if(InputManager.HeldKeys.Contains(Keys.LeftAlt))
					ChangeGlobalVolume(eventArgs.Item1);
			};
			
			DrawableManager.Add(VolumeSelector);

			ChangeTargetFPS(Config.TargetFPS);
			
			Config.TargetFPS.OnChange += delegate(object _, int newTarget) {
				ChangeTargetFPS(newTarget);
			};

			HiraganaConversion.LoadConversion();
			
			// ScreenManager.SetBlankTransition();
		}
	}
}
