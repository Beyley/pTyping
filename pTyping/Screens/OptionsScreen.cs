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
			
			#region Target FPS
			TextDrawable      targetFPSInputLabel = new(new Vector2(100, 200), FurballGame.DEFAULT_FONT, "Target FPS:", 30);
			UiTextBoxDrawable targetFPSInput      = new(new Vector2(110 + targetFPSInputLabel.Size.X, 200), FurballGame.DEFAULT_FONT, Config.TargetFPS.Value.ToString(CultureInfo.InvariantCulture), 30, 200);

			targetFPSInput.OnCommit += this.TargetFpsInputOnCommit;
			
			this.Manager.Add(targetFPSInputLabel);
			this.Manager.Add(targetFPSInput);
			#endregion

			#region 1600x900 res button
			UiButtonDrawable res1600x900button = new(new(100, 300), "1600x900", FurballGame.DEFAULT_FONT, 30, Color.Blue, Color.White, Color.White, Vector2.Zero);
			
			res1600x900button.OnClick += delegate {
				FurballGame.Instance.ChangeScreenSize(1600, 900);
			};
			
			this.Manager.Add(res1600x900button);
			#endregion
			
			#region 1920x1080 res button
			UiButtonDrawable res1920x1080button = new(new(100, 400), "1920x1080", FurballGame.DEFAULT_FONT, 30, Color.Blue, Color.White, Color.White, Vector2.Zero);
			
			res1920x1080button.OnClick += delegate {
				FurballGame.Instance.ChangeScreenSize(1920, 1080);
			};
			
			this.Manager.Add(res1920x1080button);
			#endregion
			
			base.Initialize();
		}
		
		private void BackgroundDimInputOnCommit(object sender, string e) {
			Config.BackgroundDim = float.Parse(e);
		}
		
		private void TargetFpsInputOnCommit(object sender, string e) {
			Config.TargetFPS.Value = int.Parse(e);
		}
	}
}
