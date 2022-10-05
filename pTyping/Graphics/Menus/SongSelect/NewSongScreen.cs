using System;
using System.IO;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Engine.Engine.Input.Events;
using Furball.Vixie.Backends.Shared;
using File = TagLib.File;

namespace pTyping.Graphics.Menus.SongSelect;

public class NewSongScreen : pScreen {
	private DrawableTextBox _songArtistTextBox;
	private DrawableTextBox _songCreatorTextBox;
	private DrawableTextBox _songDifficultyTextBox;
	private DrawableTextBox _songNameTextBox;

	public override void Initialize() {
		base.Initialize();

		#region Back button to song select screen

		pTypingGame.LoadBackButtonTexture();

		TexturedDrawable backButton = new TexturedDrawable(pTypingGame.BackButtonTexture, new Vector2(0, FurballGame.DEFAULT_WINDOW_HEIGHT)) {
			OriginType = OriginType.BottomLeft,
			Scale      = pTypingGame.BackButtonScale
		};

		backButton.OnClick += delegate {
			pTypingGame.MenuClickSound.PlayNew();
			ScreenManager.ChangeScreen(new SongSelectionScreen(true));
		};

		this.Manager.Add(backButton);

		#endregion

		#region Create buttons for all mp3 files in songs folder

		string[] songs = Directory.GetFiles("songs", "*.mp3");

		float y = 20f;
		foreach (string songPath in songs) {
			FileInfo songInfo = new FileInfo(songPath);

			EventHandler<MouseButtonEventArgs> songOnClick = delegate {
				this.CreateSong(songInfo);
			};

			DrawableButton buttonForSong = new DrawableButton(new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 20, y), pTypingGame.JapaneseFont, 25, songInfo.Name, Color.Blue, Color.White, Color.White, Vector2.Zero, songOnClick) {
				OriginType = OriginType.TopRight
			};

			this.Manager.Add(buttonForSong);

			y += buttonForSong.Size.Y + 20f;
		}

		#endregion

		#region Create text boxes for info about the song

		y = 20f;

		this._songNameTextBox       =  new DrawableTextBox(new Vector2(20f, y), pTypingGame.JapaneseFont, 50, 400f, "Name of Song");
		y                           += 20 + this._songNameTextBox.Size.Y;
		this._songArtistTextBox     =  new DrawableTextBox(new Vector2(20f, y), pTypingGame.JapaneseFont, 50, 400f, "Artist");
		y                           += 20 + this._songArtistTextBox.Size.Y;
		this._songCreatorTextBox    =  new DrawableTextBox(new Vector2(20f, y), pTypingGame.JapaneseFont, 50, 400f, "Map Creator Name");
		y                           += 20 + this._songCreatorTextBox.Size.Y;
		this._songDifficultyTextBox =  new DrawableTextBox(new Vector2(20f, y), pTypingGame.JapaneseFont, 50, 400f, "Difficulty Name");

		this.Manager.Add(this._songNameTextBox);
		this.Manager.Add(this._songArtistTextBox);
		this.Manager.Add(this._songCreatorTextBox);
		this.Manager.Add(this._songDifficultyTextBox);

		#endregion
	}

	public override ScreenUserActionType OnlineUserActionType => ScreenUserActionType.Listening;

	private void CreateSong(FileInfo musicFileInfo) {
		if (musicFileInfo.DirectoryName is null) throw new Exception("Song directory is null? how???? wtf are you doing??????");

		File tags = File.Create(musicFileInfo.FullName);

		string performers = tags.Tag.JoinedPerformers;
		if (performers != null)
			this._songArtistTextBox.Text = performers;
		string title = tags.Tag.Title;
		if (title != null)
			this._songNameTextBox.Text = title;

		throw new Exception();

		// Beatmap song = new() {
		//     Name       = this._songNameTextBox.Text,
		//     Artist     = this._songArtistTextBox.Text,
		//     Creator    = this._songCreatorTextBox.Text,
		//     Difficulty = this._songDifficultyTextBox.Text,
		//     AudioPath  = musicFileInfo.Name
		// };

		//Add a default timing point so we dont crash
		// song.TimingPoints.Add(
		// new TimingPoint {
		//     Tempo = 100,
		//     Time  = 0
		// }
		// );

		// Get the new directory for the song
		//         string newSongFolder = Path.Combine(
		//         musicFileInfo.DirectoryName,
		//         $"{song.Artist.Replace(" ", "")}-{song.Name.Replace(" ", "")}-by-{song.Creator.Replace(" ", "")}/"
		//         );
		//
		//         if (Directory.Exists(newSongFolder)) {
		//             pTypingGame.NotificationManager.CreateNotification(
		//             NotificationManager.NotificationImportance.Error,
		//             @"Creation of song failed!
		// (song too similar to existing one)"
		//             );
		//
		//             return;
		//         }
		//
		//         // Create the directory
		//         Directory.CreateDirectory(newSongFolder);
		//
		//         musicFileInfo.MoveTo(Path.Combine(newSongFolder, Path.GetFileName(musicFileInfo.FullName)));
		//
		//         // Open the filestream to the file
		//         FileStream stream = System.IO.File.Create(Path.Combine(newSongFolder, $"{song.Difficulty} - {song.Creator}.pts"));
		//
		//         song.FilePath   = Path.GetFileName(stream.Name);
		//         song.FolderPath = newSongFolder;
		//
		//         // Close the filestream
		//         stream.Close();
		//         // Save the song to the file
		//         SongManager.PTYPING_SONG_HANDLER.SaveSong(song);
		//
		//         //Play the menu click sound (since we clicked a button)
		//         pTypingGame.MenuClickSound.PlayNew();
		//         SongManager.UpdateSongs();
		//         //Change the screen to the song select screen
		//         ScreenManager.ChangeScreen(new SongSelectionScreen(true));
	}
	public override string         Name                 => "New Song";
	public override string         State                => "Getting ready to map!";
	public override string         Details              => "";
	public override bool           ForceSpeedReset      => true;
	public override float          BackgroundFadeAmount => 0.4f;
	public override MusicLoopState LoopState            => MusicLoopState.None;
	public override ScreenType     ScreenType           => ScreenType.Menu;
}
