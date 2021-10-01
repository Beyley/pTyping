using System;
using System.IO;
using Furball.Engine;
using Furball.Engine.Engine.Audio;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Microsoft.Xna.Framework;
using pTyping.Screens;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using pTyping.Songs;

namespace pTyping {
	public class pTypingGame : FurballGame {
		public static Texture2D BackButtonTexture;
		public static Texture2D DefaultBackground;

		public static readonly AudioStream MusicTrack     = new();
		public static readonly SoundEffect MenuClickSound = new();

		public static TextDrawable     VolumeSelector;
		public static TexturedDrawable CurrentSongBackground;


		public static void LoadBackButtonTexture() {
			BackButtonTexture ??= ContentManager.LoadMonogameAsset<Texture2D>("backbutton", ContentSource.User);
		}

		public static void SetBackgroundTexture(Texture2D tex) {
			CurrentSongBackground.SetTexture(tex);
					
			CurrentSongBackground.Scale = new(1f / ((float)CurrentSongBackground.Texture.Height / FurballGame.DEFAULT_WINDOW_HEIGHT));
		}

		public static void LoadBackgroundFromSong(Song song) {
			Texture2D backgroundTex;
			if (song.BackgroundPath == null)
				backgroundTex = DefaultBackground;
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
			this.Window.AllowUserResizing = true;
		}

		protected override void LoadContent() {
			base.LoadContent();

			byte[] menuClickSoundData = ContentManager.LoadRawAsset("menuhit.wav", ContentSource.User);
			MenuClickSound.Load(menuClickSoundData);

			MenuClickSound.Volume = Config.Volume;
			Config.Volume.OnChange += delegate (object _, float volume) {
				MenuClickSound.Volume = volume;
				
				if(VolumeSelector is not null)
					VolumeSelector.Text = $"Volume: {Config.Volume.Value * 100f:00.##}";
			};
			
			DefaultBackground = ContentManager.LoadMonogameAsset<Texture2D>("background");
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
		
		protected override void Initialize() {
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
		}
	}
}
