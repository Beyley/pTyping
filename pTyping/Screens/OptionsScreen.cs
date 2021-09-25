using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Microsoft.Xna.Framework;

namespace pTyping.Screens {
	public class OptionsScreen : Screen {
		public override void Initialize() {
			UiButtonDrawable backButton = new("Back", FurballGame.DEFAULT_FONT, 30, Color.Blue, Color.White, Color.White) {
				Position   = new Vector2(0, FurballGame.DEFAULT_WINDOW_HEIGHT),
				OriginType = OriginType.BottomLeft
			};
			
			backButton.OnClick += delegate {
				((FurballGame)FurballGame.Instance).ChangeScreen(new MenuScreen());
			};
			
			this.Manager.Add(backButton);
			
			base.Initialize();
		}
	}
}
