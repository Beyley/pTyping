using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Microsoft.Xna.Framework;
using pTyping.Songs;
using SpriteFontPlus;

namespace pTyping.Screens {
	public class SongSelectionScreen : Screen {
		public override void Initialize() {
			UiButtonDrawable backButton = new("Back", FurballGame.DEFAULT_FONT, 30, Color.Blue, Color.White, Color.White) {
				Position   = new Vector2(0, FurballGame.DEFAULT_WINDOW_HEIGHT),
				OriginType = OriginType.BottomLeft
			};
			
			backButton.OnClick += delegate {
				((FurballGame)FurballGame.Instance).ChangeScreen(new MenuScreen());
			};
			
			this.Manager.Add(backButton);
			
			SongManager.UpdateSongs();

			float tempY = 50;
			foreach (Song song in SongManager.Songs) {
				UiButtonDrawable drawable = new($"{song.Artist} - {song.Name}", FurballGame.DEFAULT_FONT, 35, Color.Aqua, Color.Black, Color.Black, 5f, new []{ CharacterRange.BasicLatin, CharacterRange.CyrillicSupplement, CharacterRange.Latin1Supplement, CharacterRange.LatinExtendedA, CharacterRange.LatinExtendedB, CharacterRange.Cyrillic, CharacterRange.Hiragana, CharacterRange.Katakana, new CharacterRange('★') }) {
					Position = new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 50, tempY),
					OriginType = OriginType.TopRight
				};

				drawable.OnClick += delegate {
					((FurballGame) FurballGame.Instance).ChangeScreen(new PlayerScreen(song));
				};

				this.Manager.Add(drawable);

				tempY += 50;
			}
			
			base.Initialize();
		}
	}
}
