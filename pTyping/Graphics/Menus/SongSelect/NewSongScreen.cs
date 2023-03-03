using System;
using System.IO;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Engine.Engine.Helpers;
using Furball.Engine.Engine.Input.Events;
using Furball.Vixie.Backends.Shared;
using pTyping.Shared;
using pTyping.Shared.Beatmaps;
using pTyping.Shared.ObjectModel;
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

		string[] songs = Directory.GetFiles(Path.Combine(FurballGame.DataFolder, "songs"), "*.mp3");

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

	public override ScreenUserActionType OnlineUserActionType       => ScreenUserActionType.Listening;
	public override int                  BackgroundBlurKernelRadius => 2;
	public override int                  BackgroundBlurPasses       => 3;

	private void CreateSong(FileInfo musicFileInfo) {
		if (musicFileInfo.DirectoryName is null) throw new Exception("Song directory is null? how???? wtf are you doing??????");

		File tags = File.Create(musicFileInfo.FullName);

		string performers = tags.Tag.JoinedPerformers;
		if (performers != null)
			this._songArtistTextBox.Text = performers;
		string title = tags.Tag.Title;
		if (title != null)
			this._songNameTextBox.Text = title;

		BeatmapSet set = new BeatmapSet();

		set.Artist = new AsciiUnicodeTuple(null, this._songArtistTextBox.Text);
		set.Source = "Unknown";
		set.Title  = new AsciiUnicodeTuple(null, this._songNameTextBox.Text);

		Beatmap beatmap = new Beatmap();

		beatmap.Id = Guid.NewGuid().ToString();
		beatmap.Info.Mapper = new DatabaseUser {
			Username = this._songCreatorTextBox.Text
		};
		beatmap.Info.DifficultyName = new AsciiUnicodeTuple(null, this._songDifficultyTextBox.Text);
		beatmap.Metadata.BackingLanguages.Add((int)SongLanguage.Unknown);

		beatmap.TimingPoints.Add(new TimingPoint(0, 250));

		byte[] audio = System.IO.File.ReadAllBytes(musicFileInfo.FullName);

		beatmap.FileCollection.Audio = new PathHashTuple(musicFileInfo.Name, CryptoHelper.GetMd5(audio));

		pTypingGame.FileDatabase.AddFile(audio).Wait();

		set.Beatmaps.Add(beatmap);

		pTypingGame.BeatmapDatabase.Realm.Write(() => pTypingGame.BeatmapDatabase.Realm.Add(set));
		pTypingGame.BeatmapDatabase.Realm.Refresh();

		ScreenManager.ChangeScreen(new SongSelectionScreen(true));
	}
	public override string         Name                 => "New Song";
	public override string         State                => "Getting ready to map!";
	public override string         Details              => "";
	public override bool           ForceSpeedReset      => true;
	public override float          BackgroundFadeAmount => 0.4f;
	public override MusicLoopState LoopState            => MusicLoopState.None;
	public override ScreenType     ScreenType           => ScreenType.Menu;
}
