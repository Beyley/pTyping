using Furball.Engine;
using pTyping.Screens;

namespace pTyping {
	public class pTypingGame : FurballGame {
		public static EditorScreen EditorInstance;

		public pTypingGame() : base(new MenuScreen()) {
			this.Window.AllowUserResizing = true;
		}
	}
}
