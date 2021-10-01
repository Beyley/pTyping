using System;
using Furball.Engine;
using Furball.Engine.Engine.Audio;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using pTyping.Screens;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace pTyping {
	public class pTypingGame : FurballGame {
		public static EditorScreen EditorInstance;

		public static Texture2D BackButtonTexture;

		public static readonly SoundEffect MenuClickSound = new();

		public static TextDrawable VolumeSelector;

		public static void LoadBackButtonTexture() {
			BackButtonTexture ??= ContentManager.LoadMonogameAsset<Texture2D>("backbutton", ContentSource.User);
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
			
			FurballGame.DrawableManager.Add(VolumeSelector);
		}
	}
}
