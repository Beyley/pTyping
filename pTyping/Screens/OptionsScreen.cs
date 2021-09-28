using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Microsoft.Xna.Framework;

namespace pTyping.Screens {
	public class OptionsScreen : Screen {
		public override void Initialize() {
			UiButtonDrawable backButton = new(new Vector2(0, FurballGame.DEFAULT_WINDOW_HEIGHT), "Back", FurballGame.DEFAULT_FONT, 30, Color.Blue, Color.White, Color.White, Vector2.Zero) {
				OriginType = OriginType.BottomLeft
			};
			
			backButton.OnClick += delegate {
				FurballGame.Instance.ChangeScreen(new MenuScreen());
			};
			
			this.Manager.Add(backButton);
			
			base.Initialize();
		}
	}
}
