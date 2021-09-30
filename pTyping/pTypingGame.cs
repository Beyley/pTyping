using Furball.Engine;
using Furball.Engine.Engine.Audio;
using Furball.Engine.Engine.Graphics;
using pTyping.Screens;
using Microsoft.Xna.Framework.Graphics;

namespace pTyping {
	public class pTypingGame : FurballGame {
		public static EditorScreen EditorInstance;

		public static Texture2D BackButtonTexture;

		public static readonly SoundEffect MenuClickSound = new();

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
		}
	}
}
