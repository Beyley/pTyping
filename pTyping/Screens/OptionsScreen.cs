using System.Globalization;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
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
				pTypingGame.MenuClickSound.Play();
				FurballGame.Instance.ChangeScreen(new MenuScreen());
			};
			
			this.Manager.Add(backButton);
			#endregion

			#region Background image
			this.Manager.Add(pTypingGame.CurrentSongBackground);
			
			pTypingGame.CurrentSongBackground.Tweens.Add(new ColorTween(TweenType.Color, pTypingGame.CurrentSongBackground.ColorOverride, new(100, 100, 100), pTypingGame.CurrentSongBackground.TimeSource.GetCurrentTime(), pTypingGame.CurrentSongBackground.TimeSource.GetCurrentTime() + 1000));
			#endregion
			
			#region Background Dim
			TextDrawable      backgroundDimInputLabel = new(new Vector2(100, 150), FurballGame.DEFAULT_FONT, "Background Dim:", 30);
			UiTextBoxDrawable backgroundDimInput      = new(new Vector2(110 + backgroundDimInputLabel.Size.X, 150), FurballGame.DEFAULT_FONT, Config.BackgroundDim.ToString(CultureInfo.InvariantCulture), 30, 200);

			backgroundDimInput.OnCommit += this.BackgroundDimInputOnCommit;
			
			this.Manager.Add(backgroundDimInputLabel);
			this.Manager.Add(backgroundDimInput);
			#endregion
			
			base.Initialize();
		}
		
		private void BackgroundDimInputOnCommit(object sender, string e) {
			Config.BackgroundDim = float.Parse(e);
		}
	}
}
