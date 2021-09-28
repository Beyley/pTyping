using System.Globalization;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
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
				FurballGame.Instance.ChangeScreen(new MenuScreen());
			};
			
			this.Manager.Add(backButton);
			#endregion

			#region Volume
			TextDrawable      volumeInputLabel = new(new Vector2(100, 100), FurballGame.DEFAULT_FONT, "Volume:", 30);
			UiTextBoxDrawable volumeInput      = new(new Vector2(110 + volumeInputLabel.Size.X, 100), FurballGame.DEFAULT_FONT, Config.Volume.ToString(CultureInfo.InvariantCulture), 30, 200);

			volumeInput.OnCommit += this.VolumeInputOnCommit;
			
			this.Manager.Add(volumeInputLabel);
			this.Manager.Add(volumeInput);
			#endregion
			
			#region Approach time
			TextDrawable      approachTimeInputLabel = new(new Vector2(100, 150), FurballGame.DEFAULT_FONT, "Approach Time:", 30);
			UiTextBoxDrawable approachTimeInput      = new(new Vector2(110 + approachTimeInputLabel.Size.X, 150), FurballGame.DEFAULT_FONT, Config.ApproachTime.ToString(CultureInfo.InvariantCulture), 30, 200);

			volumeInput.OnCommit += this.ApproachTimeInputOnCommit;
			
			this.Manager.Add(approachTimeInputLabel);
			this.Manager.Add(approachTimeInput);
			#endregion
			
			base.Initialize();
		}
		private void VolumeInputOnCommit(object sender, string e) {
			Config.Volume = float.Parse(e);
		}
		
		private void ApproachTimeInputOnCommit(object sender, string e) {
			Config.ApproachTime = int.Parse(e);
		}
	}
}
