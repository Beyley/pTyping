using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
			Texture2D menuButtonsTexture = ContentReader.LoadMonogameAsset<Texture2D>("menubuttons", ContentSource.User);

			float y = FurballGame.DEFAULT_WINDOW_HEIGHT * 0.35f;

			TexturedDrawable playButton    = new(menuButtonsTexture, new(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, y), new Rectangle(0, 0, 300, 100)) {
				OriginType = OriginType.Center,
				Scale = new(0.75f)
			};
			TexturedDrawable editButton    = new(menuButtonsTexture, new(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, y += playButton.Size.Y + 10), new Rectangle(0, 100, 300, 100)) {
				OriginType = OriginType.Center,
				Scale      = new(0.75f)
			};
			TexturedDrawable optionsButton = new(menuButtonsTexture, new(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, y += editButton.Size.Y + 10), new Rectangle(0, 200, 300, 100)) {
				OriginType = OriginType.Center,
				Scale = new(0.75f)
			};
			TexturedDrawable exitButton    = new(menuButtonsTexture, new(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, y += optionsButton.Size.Y + 10), new Rectangle(0, 300, 300, 100)) {
				OriginType = OriginType.Center,
				Scale      = new(0.75f)
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
