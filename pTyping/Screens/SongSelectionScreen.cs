using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Microsoft.Xna.Framework;
using pTyping.Songs;
using SpriteFontPlus;

namespace pTyping.Screens {
	public class SongSelectionScreen : Screen {
		private bool _editor;

		private TextDrawable _songInfo;
		
		public SongSelectionScreen(bool editor) {
			this._editor = editor;
		}
		public override void Initialize() {
			SongManager.UpdateSongs();
			
			#region Back button
			UiButtonDrawable backButton = new(new Vector2(0, FurballGame.DEFAULT_WINDOW_HEIGHT), "Back", FurballGame.DEFAULT_FONT, 30, Color.Blue, Color.White, Color.White) {
				OriginType = OriginType.BottomLeft
			};
			
			backButton.OnClick += delegate {
				FurballGame.Instance.ChangeScreen(new MenuScreen());
			};
			
			this.Manager.Add(backButton);
			#endregion

			#region Create new song button
			if(this._editor) {
				UiButtonDrawable createNewSongButton = new(new Vector2(backButton.Size.X + 10f, FurballGame.DEFAULT_WINDOW_HEIGHT), "Create Song", FurballGame.DEFAULT_FONT, 30, Color.Blue, Color.White, Color.White) {
					OriginType = OriginType.BottomLeft
				};
				
				createNewSongButton.OnClick += delegate {
					FurballGame.Instance.ChangeScreen(new NewSongScreen());
				};

				this.Manager.Add(createNewSongButton);
			}
			#endregion
			
			#region Create new buttons for each song
			float tempY = 50;
			foreach (Song song in SongManager.Songs) {
				UiButtonDrawable drawable = new(new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 50, tempY), $"{song.Artist} - {song.Name} [{song.Difficulty}]", FurballGame.DEFAULT_FONT, 35, Color.Aqua, Color.Black, Color.Black, 5f, new []{ CharacterRange.BasicLatin, CharacterRange.CyrillicSupplement, CharacterRange.Latin1Supplement, CharacterRange.LatinExtendedA, CharacterRange.LatinExtendedB, CharacterRange.Cyrillic, CharacterRange.Hiragana, CharacterRange.Katakana, new CharacterRange('★') }) {
					OriginType = OriginType.TopRight
				};

				drawable.OnClick += delegate {
					if (this._editor) {
						pTypingGame.EditorInstance = new(song);
						
						FurballGame.Instance.ChangeScreen(pTypingGame.EditorInstance);
					}
					else
						((FurballGame) FurballGame.Instance).ChangeScreen(new PlayerScreen(song));
				};

				this.Manager.Add(drawable);

				tempY += 50;
			}
			#endregion
			
			base.Initialize();
		}
	}
}
