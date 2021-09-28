using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Microsoft.Xna.Framework;
using pTyping.Songs;

namespace pTyping.Screens {
	public class MenuScreen : Screen {
		public override void Initialize() {
			#region Title
			TextDrawable titleText = new(new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.2f), FurballGame.DEFAULT_FONT, "pTyping", 75) {
				OriginType = OriginType.Center
			};
			
			this.Manager.Add(titleText);
			#endregion
			
			#region Buttons
			UiButtonDrawable playButton = new(new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.45f), "Play", FurballGame.DEFAULT_FONT, 50, Color.Blue, Color.White, Color.White) {
				OriginType = OriginType.Center
			};
			
			UiButtonDrawable editButton = new(new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.55f), "Edit", FurballGame.DEFAULT_FONT, 50, Color.Blue, Color.White, Color.White) {
				OriginType = OriginType.Center
			};
			
			UiButtonDrawable optionsButton = new(new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.65f), "Options", FurballGame.DEFAULT_FONT, 50, Color.Blue, Color.White, Color.White) {
				OriginType = OriginType.Center
			};
			
			UiButtonDrawable exitButton = new(new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.75f), "Exit", FurballGame.DEFAULT_FONT, 50, Color.Blue, Color.White, Color.White) {
				OriginType = OriginType.Center
			};

			playButton.OnClick += delegate {
				FurballGame.Instance.ChangeScreen(new SongSelectionScreen(false));
			};
			
			editButton.OnClick += delegate {
				FurballGame.Instance.ChangeScreen(new SongSelectionScreen(true));
			};
			
			exitButton.OnClick += delegate {
				FurballGame.Instance.Exit();
			};

			optionsButton.OnClick += delegate {
				FurballGame.Instance.ChangeScreen(new OptionsScreen());
			};
			
			this.Manager.Add(playButton);
			this.Manager.Add(editButton);
			this.Manager.Add(optionsButton);
			this.Manager.Add(exitButton);
			#endregion
			
			SongManager.UpdateSongs();

			base.Initialize();
		}
	}
}
