using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Microsoft.Xna.Framework;
using pTyping.Songs;

namespace pTyping.Screens {
	public class MenuScreen : Screen {
		public override void Initialize() {
			TextDrawable menuScreen = new(FurballGame.DEFAULT_FONT, "pTyping", 75) {
				Position = new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.2f),
				OriginType = OriginType.Center
			};

			UiButtonDrawable playButton = new("Play", FurballGame.DEFAULT_FONT, 50, Color.Blue, Color.White, Color.White) {
				Position   = new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.45f),
				OriginType = OriginType.Center
			};
			
			UiButtonDrawable optionsButton = new("Options", FurballGame.DEFAULT_FONT, 50, Color.Blue, Color.White, Color.White) {
				Position   = new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.55f),
				OriginType = OriginType.Center
			};
			
			UiButtonDrawable exitButton = new("Exit", FurballGame.DEFAULT_FONT, 50, Color.Blue, Color.White, Color.White) {
				Position   = new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.65f),
				OriginType = OriginType.Center
			};

			playButton.OnClick += delegate {
				((FurballGame)FurballGame.Instance).ChangeScreen(new SongSelectionScreen());
			};
			
			exitButton.OnClick += delegate {
				FurballGame.Instance.Exit();
			};

			optionsButton.OnClick += delegate {
				((FurballGame)FurballGame.Instance).ChangeScreen(new OptionsScreen());
			};

			this.Manager.Add(menuScreen);

			this.Manager.Add(playButton);
			this.Manager.Add(optionsButton);
			this.Manager.Add(exitButton);
			
			SongManager.UpdateSongs();

			base.Initialize();
		}
	}
}
