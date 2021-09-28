using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using pTyping.Screens;
using Microsoft.Xna.Framework.Graphics;

namespace pTyping {
	public class pTypingGame : FurballGame {
		public static EditorScreen EditorInstance;

		public static Texture2D BackButtonTexture;

		public static void LoadBackButtonTexture() {
			BackButtonTexture ??= ContentReader.LoadMonogameAsset<Texture2D>("backbutton", ContentSource.User);
		}
		
		public pTypingGame() : base(new MenuScreen()) {
			this.Window.AllowUserResizing = true;
		}
	}
}
