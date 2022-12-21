using System;
using System.Linq;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Engine.Engine.Input.Events;
using Furball.Engine.Engine.Platform;
using Furball.Vixie;
using Furball.Vixie.Backends.Shared;
using pTyping.Graphics.Drawables;
using pTyping.Graphics.Editor;
using pTyping.Graphics.Menus.Options;
using pTyping.Graphics.Menus.SongSelect;
using pTyping.Graphics.UiMaker;
using pTyping.Shared.Beatmaps;
using static Furball.Engine.Engine.Localization.LocalizationManager;

namespace pTyping.Graphics.Menus;

public class MenuScreen : pScreen {
	private TextDrawable _musicTitle;

	private Drawable            _userCard;
	private DrawableProgressBar _songProgressBar;

	private static string[] totdList = {
		"Mario Teaches Typing 4",
		"\"Typing Taiko\"",
		"Average QWERTY fan vs average DVORAK fan",
		"Average QWERTY fan vs average COLEMAK fan",
		"Average QWERTY fan vs average Workman fan",
		$"Scribe Simulator {DateTime.UtcNow.Year}",
		"We are not legally responsible for any hand injuries",
		"Oh, you like instrumentals? Mhm, great...",
		"Check out the original UTyping by toslunar!",
		"Ive hurt my hand twice playing this so far" //update this accordingly
	};

	public override void Initialize() {
		base.Initialize();

		#region Tip of the day

		//NOTE: we use UTC here to prevent it changing at different times for different people,
		//to keep up the surprise of what one comes next
		//although this does lead to it changing at a seemingly random time for people outside of Europe
		int dayIndex = DateTime.UtcNow.Day % totdList.Length;
		
		string totdString = totdList[dayIndex];
		
		this._totd = new TextDrawable(Vector2.Zero, pTypingGame.JapaneseFont, totdString, 30) {
			OriginType = OriginType.Center,
			Depth = 0.5f
		};

		this._totdBackground = new RectanglePrimitiveDrawable {
			ColorOverride = new Color(50, 50, 50, 175), 
			Filled        = true,
			OriginType    = OriginType.Center, 
			Thickness     = 0, 
			Depth = 0.75f
		};
			
		this.Manager.Add(this._totdBackground);
		this.Manager.Add(this._totd);
		
		#endregion
		
		TextDrawable gitVersionText = new TextDrawable(new Vector2(10, 10), pTypingGame.JapaneseFont, string.Format(GetLocalizedString(Localizations.MenuRevision, CurrentLanguage), Program.BuildVersion), 30) {
			OriginType       = OriginType.BottomRight,
			ScreenOriginType = OriginType.BottomRight
		};

		this.Manager.Add(gitVersionText);

		DrawableButton changelogButton = new DrawableButton(new Vector2(10, 10), pTypingGame.JapaneseFont, 30, GetLocalizedString(Localizations.Changelog), Color.Blue, Color.White, Color.White, new Vector2(0)) {
			OriginType       = OriginType.BottomLeft,
			ScreenOriginType = OriginType.BottomLeft
		};

		changelogButton.OnClick += (_, _) => ScreenManager.ChangeScreen(new ChangelogScreen());

		this.Manager.Add(changelogButton);

		DrawableButton uiEditorButton = new DrawableButton(new Vector2(changelogButton.Position.X + changelogButton.Size.X + 10, 10), pTypingGame.JapaneseFont, 30, "Ui Maker", Color.Blue, Color.White, Color.White, new Vector2(0)) {
			OriginType       = OriginType.BottomLeft,
			ScreenOriginType = OriginType.BottomLeft
		};

		uiEditorButton.OnClick += (_, _) => ScreenManager.ChangeScreen(new UiMakerScreen("test"));

		DrawableButton uiTestButton = new DrawableButton(new Vector2(uiEditorButton.Position.X + uiEditorButton.Size.X + 10, 10), pTypingGame.JapaneseFont, 30, "Ui Test Scene", Color.Blue, Color.White, Color.White, new Vector2(0)) {
			OriginType       = OriginType.BottomLeft,
			ScreenOriginType = OriginType.BottomLeft
		};

		uiTestButton.OnClick += (_, _) => ScreenManager.ChangeScreen(new UiElementsTestScreen());

		DrawableButton newEditorButton = new DrawableButton(new Vector2(uiTestButton.Position.X + uiTestButton.Size.X + 10, 10), pTypingGame.JapaneseFont, 30, "New Editor", Color.Blue, Color.White, Color.White, new Vector2(0)) {
			OriginType       = OriginType.BottomLeft,
			ScreenOriginType = OriginType.BottomLeft
		};

		newEditorButton.OnClick += (_, _) => ScreenManager.ChangeScreen(new EditorScreen(pTypingGame.CurrentSong.Value.ParentSet, pTypingGame.CurrentSong.Value.Id));

		if (RuntimeInfo.IsDebug()) {
			this.Manager.Add(uiEditorButton);
			this.Manager.Add(uiTestButton);
			this.Manager.Add(newEditorButton);
		}

		#region Title

		this.Manager.Add(
			this._titleText = new(
				new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.2f),
				FurballGame.DefaultFont,
				"pTyping",
				75
			) {
				OriginType = OriginType.Center
			}
		);

		#endregion

		#region Main buttons

		Texture menuButtonsTexture = ContentManager.LoadTextureFromFileCached("menubuttons.png", ContentSource.User);

		float y = FurballGame.DEFAULT_WINDOW_HEIGHT * 0.35f;

		this._playButton = new(menuButtonsTexture, new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, y), TexturePositions.MENU_PLAY_BUTTON) {
			OriginType = OriginType.Center,
			Scale      = new Vector2(0.75f)
		};
		this._editButton = new(
			menuButtonsTexture,
			new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, y += this._playButton.Size.Y + 10),
			TexturePositions.MENU_EDIT_BUTTON
		) {
			OriginType = OriginType.Center,
			Scale      = new Vector2(0.75f)
		};
		this._optionsButton = new(
			menuButtonsTexture,
			new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, y += this._editButton.Size.Y + 10),
			TexturePositions.MENU_OPTIONS_BUTTON
		) {
			OriginType = OriginType.Center,
			Scale      = new Vector2(0.75f)
		};
		this._exitButton = new(
			menuButtonsTexture,
			new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, y += this._optionsButton.Size.Y + 10),
			TexturePositions.MENU_EXIT_BUTTON
		) {
			OriginType = OriginType.Center,
			Scale      = new Vector2(0.75f)
		};

		this._playButton.OnClick += delegate {
			pTypingGame.MenuClickSound.PlayNew();
			ScreenManager.ChangeScreen(new SongSelectionScreen(false));
		};

		this._editButton.OnClick += delegate {
			pTypingGame.MenuClickSound.PlayNew();
			ScreenManager.ChangeScreen(new SongSelectionScreen(true));
		};

		this._exitButton.OnClick += delegate {
			pTypingGame.MenuClickSound.PlayNew();
			FurballGame.Instance.WindowManager.CloseWindow();
		};

		this._optionsButton.OnClick += delegate {
			pTypingGame.MenuClickSound.PlayNew();
			ScreenManager.ChangeScreen(new OptionsScreen());
		};

		this.Manager.Add(this._playButton);
		this.Manager.Add(this._editButton);
		this.Manager.Add(this._optionsButton);
		this.Manager.Add(this._exitButton);

		#endregion

		#region Menu music

		this._musicTitle = new TextDrawable(new Vector2(5, 5), pTypingGame.JapaneseFont, "None", 40) {
			OriginType       = OriginType.TopRight,
			ScreenOriginType = OriginType.TopRight
		};

		this._songProgressBar = new DrawableProgressBar(
			new Vector2(5, this._musicTitle.Position.Y + this._musicTitle.Size.Y + 5f),
			pTypingGame.JapaneseFont,
			(int)(25 * 0.9f),
			new Vector2(250, 25),
			Color.White,
			Color.LightGray,
			Color.White
		) {
			OriginType       = OriginType.TopRight,
			ScreenOriginType = OriginType.TopRight
		};
		this._songProgressBar.OnClick += this.OnProgressBarClick;

		Texture editorButtonsTexture2D = ContentManager.LoadTextureFromFileCached("editorbuttons.png", ContentSource.User);

		TexturedDrawable musicPlayButton = new TexturedDrawable(editorButtonsTexture2D, new Vector2(105, this._songProgressBar.Position.Y + this._songProgressBar.Size.Y + 5f), TexturePositions.EDITOR_PLAY) {
			Scale            = new Vector2(0.5f, 0.5f),
			OriginType       = OriginType.TopRight,
			ScreenOriginType = OriginType.TopRight
		};
		TexturedDrawable musicPauseButton = new TexturedDrawable(editorButtonsTexture2D, new Vector2(55, this._songProgressBar.Position.Y + this._songProgressBar.Size.Y + 5f), TexturePositions.EDITOR_PAUSE) {
			Scale            = new Vector2(0.5f, 0.5f),
			OriginType       = OriginType.TopRight,
			ScreenOriginType = OriginType.TopRight
		};
		TexturedDrawable musicNextButton = new TexturedDrawable(editorButtonsTexture2D, new Vector2(5, this._songProgressBar.Position.Y + this._songProgressBar.Size.Y + 5f), TexturePositions.EDITOR_RIGHT) {
			Scale            = new Vector2(0.5f, 0.5f),
			OriginType       = OriginType.TopRight,
			ScreenOriginType = OriginType.TopRight
		};

		musicPlayButton.OnClick += delegate {
			pTypingGame.PlayMusic();
		};

		musicPauseButton.OnClick += delegate {
			pTypingGame.PauseResumeMusic();
		};

		musicNextButton.OnClick += delegate {
			this.UpdateStats();

			pTypingGame.SelectNewSong();
			pTypingGame.PlayMusic();
		};

		this.Manager.Add(this._musicTitle);
		this.Manager.Add(this._songProgressBar);

		this.Manager.Add(musicPlayButton);
		this.Manager.Add(musicPauseButton);
		this.Manager.Add(musicNextButton);

		#endregion

		#region Background image

		this.Manager.Add(pTypingGame.CurrentSongBackground);

		#endregion

		this.UpdateUserCard(this, EventArgs.Empty);

		pTypingGame.OnlineManager.OnLoginComplete += this.UpdateUserCard;
		pTypingGame.OnlineManager.OnLogout        += this.UpdateUserCard;
		pTypingGame.CurrentSong.OnChange          += this.OnCurrentSongOnOnChange;

		this.UpdateStats();
	}

	public override void Relayout(float newWidth, float newHeight) {
		base.Relayout(newWidth, newHeight);

		this._titleText.Position = new Vector2(newWidth / 2f, newHeight * 0.2f);
		this._totd.Position = new Vector2(newWidth / 2f, newHeight * 0.825f);

		this._totdBackground.Position = this._totd.Position;
		this._totdBackground.RectSize = new Vector2(this._totd.Size.X + 10, this._totd.FontSize + 5);

		this._playButton.Position.X    = newWidth / 2f;
		this._editButton.Position.X    = newWidth / 2f;
		this._optionsButton.Position.X = newWidth / 2f;
		this._exitButton.Position.X    = newWidth / 2f;
	}

	public override ScreenUserActionType OnlineUserActionType       => ScreenUserActionType.Listening;
	public override int                  BackgroundBlurKernelRadius => 2;
	public override int                  BackgroundBlurPasses       => 3;

	private void OnProgressBarClick(object sender, MouseButtonEventArgs mouseButtonEventArgs) {
		float x = mouseButtonEventArgs.Mouse.Position.X - this._songProgressBar.RealPosition.X;

		float targetProgress = x / this._songProgressBar.Size.X;

		pTypingGame.MusicTrack.CurrentPosition = targetProgress * pTypingGame.MusicTrack.Length;
	}
	private void OnCurrentSongOnOnChange(object sender, Beatmap beatmap) {
		this.UpdateStats();
	}

	public override void Dispose() {
		pTypingGame.OnlineManager.OnLoginComplete -= this.UpdateUserCard;
		pTypingGame.OnlineManager.OnLogout        -= this.UpdateUserCard;
		pTypingGame.CurrentSong.OnChange          -= this.OnCurrentSongOnOnChange;

		base.Dispose();
	}

	private bool                       _usercardAdded;
	private TextDrawable               _titleText;
	private TexturedDrawable           _playButton;
	private TexturedDrawable           _editButton;
	private TexturedDrawable           _optionsButton;
	private TexturedDrawable           _exitButton;
	private TextDrawable               _totd;
	private RectanglePrimitiveDrawable _totdBackground;
	public void UpdateUserCard(object sender, EventArgs e) {
		if (this._usercardAdded)
			return;

		Drawable userCard = pTypingGame.GetUserCard();

		if (userCard == null)
			return;

		this.Manager.Add(this._userCard = userCard);

		if (!this._usercardAdded && sender != this) {
			this._userCard.ColorOverride = new(1f, 1f, 1f, 0f);
			this._userCard.FadeInFromZero(150);
		}

		this._usercardAdded = true;

		this._userCard.Position = new Vector2(10);
	}

	public void UpdateStats() {
		if (pTypingGame.CurrentSong.Value == null) {
			this._musicTitle.Text = "None";
		}
		else {
			BeatmapSet set = pTypingGame.CurrentSong.Value.Parent.First();

			this._musicTitle.Text = $"{set.Artist} - {set.Title}";
		}
	}

	public override void Update(double gameTime) {
		base.Update(gameTime);

		if (pTypingGame.MusicTrack == null) {
			this._songProgressBar.Progress = 0;
			return;
		}

		this._songProgressBar.Progress = (float)(pTypingGame.MusicTrack.CurrentPosition / pTypingGame.MusicTrack.Length);
	}

	public override string Name  => "Main Menu";
	public override string State => "Vibing on the menu!";
	public override string Details {
		get {
			BeatmapSet set = pTypingGame.CurrentSong.Value.Parent.First();

			return @$"Listening to {set.Artist} - {set.Title}";
		}
	}
	public override bool           ForceSpeedReset      => true;
	public override float          BackgroundFadeAmount => 0.7f;
	public override MusicLoopState LoopState            => MusicLoopState.NewSong;
	public override ScreenType     ScreenType           => ScreenType.Menu;
}
