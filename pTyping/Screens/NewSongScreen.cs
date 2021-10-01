using System.IO;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Microsoft.Xna.Framework;
using pTyping.Songs;

namespace pTyping.Screens {
	public class NewSongScreen : Screen {
		private UiTextBoxDrawable _songNameTextBox;
		private UiTextBoxDrawable _songArtistTextBox;
		private UiTextBoxDrawable _songCreatorTextBox;
		private UiTextBoxDrawable _songDifficultyTextBox;
		
		public override void Initialize() {
			#region Back button to song select screen
			pTypingGame.LoadBackButtonTexture();
			
			TexturedDrawable backButton = new(pTypingGame.BackButtonTexture, new Vector2(0, FurballGame.DEFAULT_WINDOW_HEIGHT)) {
				OriginType = OriginType.BottomLeft,
				Scale      = new (0.4f, 0.4f)
			};
			
			backButton.OnClick += delegate {
				pTypingGame.MenuClickSound.Play();
				FurballGame.Instance.ChangeScreen(new SongSelectionScreen(true, null));
			};
			
			this.Manager.Add(backButton);
			#endregion

			#region Create buttons for all mp3 files in songs folder
			string[] songs = Directory.GetFiles("songs", "*.mp3");

			float y = 20f;
			foreach (string songPath in songs) {
				FileInfo songInfo = new(songPath);

				UiButtonDrawable buttonForSong = new(new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 20, y), songInfo.Name, FurballGame.DEFAULT_FONT, 25, Color.Blue, Color.White, Color.White, Vector2.Zero) {
					OriginType = OriginType.TopRight
				};

				buttonForSong.OnClick += delegate {
					this.CreateSong(songInfo);
				};
				
				this.Manager.Add(buttonForSong);
				
				y += buttonForSong.Size.Y + 20f;
			}
			#endregion

			#region Create text boxes for info about the song
			y = 20f;
			
			this._songNameTextBox       =  new(new Vector2(20f, y), FurballGame.DEFAULT_FONT, "Name of Song", 50, 400f);
			y                           += 20 + this._songNameTextBox.Size.Y;
			this._songArtistTextBox     =  new(new Vector2(20f, y), FurballGame.DEFAULT_FONT, "Artist", 50, 400f);
			y                           += 20 + this._songArtistTextBox.Size.Y;
			this._songCreatorTextBox    =  new(new Vector2(20f, y), FurballGame.DEFAULT_FONT, "Map Creator Name", 50, 400f);
			y                           += 20 + this._songCreatorTextBox.Size.Y;
			this._songDifficultyTextBox =  new(new Vector2(20f, y), FurballGame.DEFAULT_FONT, "Difficulty Name", 50, 400f);

			this.Manager.Add(this._songNameTextBox);
			this.Manager.Add(this._songArtistTextBox);
			this.Manager.Add(this._songCreatorTextBox);
			this.Manager.Add(this._songDifficultyTextBox);
			#endregion
		}

		private void CreateSong(FileInfo songInfo) {
			Song song = new() {
				Name       = this._songNameTextBox.Text,
				Artist     = this._songArtistTextBox.Text,
				Creator    = this._songCreatorTextBox.Text,
				Difficulty = this._songDifficultyTextBox.Text,
				AudioPath  = songInfo.Name
			};
			
			song.TimingPoints.Add(new() {Tempo = 100, Time = 0});
			FileStream stream = File.Create(Path.ChangeExtension(songInfo.FullName, "pts"));
			song.FileInfo = new FileInfo(Path.ChangeExtension(songInfo.FullName, "pts"));
			song.Save(stream);
			stream.Close();
			
			SongManager.UpdateSongs();
			
			pTypingGame.MenuClickSound.Play();
			FurballGame.Instance.ChangeScreen(new SongSelectionScreen(true, song));
		}
	}
}
