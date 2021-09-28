using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Microsoft.Xna.Framework;

namespace pTyping.Screens {
	public class OptionsScreen : Screen {
		public override void Initialize() {
			#region Back button
			pTypingGame.LoadBackButtonTexture();
			
			TexturedDrawable backButton = new(pTypingGame.BackButtonTexture, new Vector2(0, FurballGame.DEFAULT_WINDOW_HEIGHT)) {
				OriginType = OriginType.BottomLeft,
				Scale = new (0.4f, 0.4f)
			};
			
			backButton.OnClick += delegate {
				FurballGame.Instance.ChangeScreen(new MenuScreen());
			};
			
			this.Manager.Add(backButton);
			#endregion
			
			base.Initialize();
		}
	}
}
