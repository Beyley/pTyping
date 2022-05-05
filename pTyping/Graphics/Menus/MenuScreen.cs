using System;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Vixie.Backends.Shared;
using pTyping.Graphics.Drawables;
using pTyping.Graphics.Menus.Options;
using pTyping.Graphics.Menus.SongSelect;
using pTyping.Songs;
using static Furball.Engine.Engine.Localization.LocalizationManager;

namespace pTyping.Graphics.Menus;

public class MenuScreen : pScreen {
    private TextDrawable _musicTitle;

    private ManagedDrawable _userCard = null;

    public override void Initialize() {
        base.Initialize();

        TextDrawable gitVersionText = new(
        new(FurballGame.DEFAULT_WINDOW_WIDTH - 10, FurballGame.DEFAULT_WINDOW_HEIGHT - 10),
        pTypingGame.JapaneseFont,
        string.Format(GetLocalizedString(Localizations.MenuRevision, CurrentLanguage), Program.BuildVersion),
        30
        ) {
            OriginType = OriginType.BottomRight
        };

        this.Manager.Add(gitVersionText);

        UiButtonDrawable changelogButton = new(
        new(10, FurballGame.DEFAULT_WINDOW_HEIGHT - 10),
        GetLocalizedString(Localizations.Changelog),
        pTypingGame.JapaneseFont,
        30,
        Color.Blue,
        Color.White,
        Color.White,
        new(0)
        ) {
            OriginType = OriginType.BottomLeft
        };

        changelogButton.OnClick += (_, _) => ScreenManager.ChangeScreen(new ChangelogScreen());

        this.Manager.Add(changelogButton);

        #region Title

        TextDrawable titleText = new(
        new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.2f),
        FurballGame.DEFAULT_FONT,
        "pTyping",
        75
        ) {
            OriginType = OriginType.Center
        };

        this.Manager.Add(titleText);

        #endregion

        #region Main buttons

        Texture menuButtonsTexture = ContentManager.LoadTextureFromFile("menubuttons.png", ContentSource.User);

        float y = FurballGame.DEFAULT_WINDOW_HEIGHT * 0.35f;

        TexturedDrawable playButton = new(menuButtonsTexture, new(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, y), TexturePositions.MENU_PLAY_BUTTON) {
            OriginType = OriginType.Center,
            Scale      = new(0.75f),
            ToolTip    = "gordon this is a tool tip"
        };
        TexturedDrawable editButton = new(
        menuButtonsTexture,
        new(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, y += playButton.Size.Y + 10),
        TexturePositions.MENU_EDIT_BUTTON
        ) {
            OriginType = OriginType.Center,
            Scale      = new(0.75f),
            ToolTip    = "you can use it to get across big pits!"
        };
        TexturedDrawable optionsButton = new(
        menuButtonsTexture,
        new(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, y += editButton.Size.Y + 10),
        TexturePositions.MENU_OPTIONS_BUTTON
        ) {
            OriginType = OriginType.Center,
            Scale      = new(0.75f)
        };
        TexturedDrawable exitButton = new(
        menuButtonsTexture,
        new(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, y += optionsButton.Size.Y + 10),
        TexturePositions.MENU_EXIT_BUTTON
        ) {
            OriginType = OriginType.Center,
            Scale      = new(0.75f)
        };

        playButton.OnClick += delegate {
            pTypingGame.MenuClickSound.PlayNew();
            ScreenManager.ChangeScreen(new SongSelectionScreen(false));
        };

        editButton.OnClick += delegate {
            pTypingGame.MenuClickSound.PlayNew();
            ScreenManager.ChangeScreen(new SongSelectionScreen(true));
        };

        exitButton.OnClick += delegate {
            pTypingGame.MenuClickSound.PlayNew();
            FurballGame.Instance.WindowManager.Close();
        };

        optionsButton.OnClick += delegate {
            pTypingGame.MenuClickSound.PlayNew();
            ScreenManager.ChangeScreen(new OptionsScreen());
        };

        this.Manager.Add(playButton);
        this.Manager.Add(editButton);
        this.Manager.Add(optionsButton);
        this.Manager.Add(exitButton);

        #endregion

        #region Menu music

        this._musicTitle = new(new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 5, 5), pTypingGame.JapaneseFont, "None", 40) {
            OriginType = OriginType.TopRight
        };

        Texture editorButtonsTexture2D = ContentManager.LoadTextureFromFile("editorbuttons.png", ContentSource.User);

        TexturedDrawable musicPlayButton = new(
        editorButtonsTexture2D,
        new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 105, this._musicTitle.Size.Y + 10),
        TexturePositions.EDITOR_PLAY
        ) {
            Scale      = new(0.5f, 0.5f),
            OriginType = OriginType.TopRight
        };
        TexturedDrawable musicPauseButton = new(
        editorButtonsTexture2D,
        new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 55, this._musicTitle.Size.Y + 10),
        TexturePositions.EDITOR_PAUSE
        ) {
            Scale      = new(0.5f, 0.5f),
            OriginType = OriginType.TopRight
        };
        TexturedDrawable musicNextButton = new(
        editorButtonsTexture2D,
        new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 5, this._musicTitle.Size.Y + 10),
        TexturePositions.EDITOR_RIGHT
        ) {
            Scale      = new(0.5f, 0.5f),
            OriginType = OriginType.TopRight
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

        this.Manager.Add(musicPlayButton);
        this.Manager.Add(musicPauseButton);
        this.Manager.Add(musicNextButton);

        #endregion

        #region Background image

        this.Manager.Add(pTypingGame.CurrentSongBackground);

        #endregion

        this.UpdateUserCard(this, null);

        pTypingGame.OnlineManager.OnLoginComplete += this.UpdateUserCard;
        pTypingGame.OnlineManager.OnLogout        += this.UpdateUserCard;
        pTypingGame.CurrentSong.OnChange          += this.OnCurrentSongOnOnChange;
        
        // if (pTypingGame.CurrentSong is null || pTypingGame.CurrentSong?.Value is null)
        //     this.LoadSong();
        // else
        pTypingGame.UserStatusListening();
        this.UpdateStats();
    }
    private void OnCurrentSongOnOnChange(object sender, Song e) {
        this.UpdateStats();
    }

    public override void Dispose() {
        pTypingGame.OnlineManager.OnLoginComplete -= this.UpdateUserCard;
        pTypingGame.OnlineManager.OnLogout        -= this.UpdateUserCard;
        pTypingGame.CurrentSong.OnChange          -= this.OnCurrentSongOnOnChange;

        base.Dispose();
    }

    public void UpdateUserCard(object sender, EventArgs e) {
        // if(pTypingGame.OnlineManager.State is ConnectionState.Disconnected or ConnectionState.Connected )
        FurballGame.GameTimeScheduler.ScheduleMethod(
        _ => {
            this.Manager.Remove(this._userCard);

            this.Manager.Add(this._userCard = pTypingGame.GetUserCard());

            this._userCard.MoveTo(new(10f));

            if (sender != this)
                this._userCard.FadeInFromZero(100);
        },
        FurballGame.Time
        );
    }

    public void UpdateStats() {
        if (pTypingGame.CurrentSong.Value == null)
            this._musicTitle.Text = "None";
        else
            this._musicTitle.Text = $"{pTypingGame.CurrentSong.Value.Artist} - {pTypingGame.CurrentSong.Value.Name}";
    }
    
    public override string Name    => "Main Menu";
    public override string State   => "Vibing on the menu!";
    public override string Details => @$"Listening to {pTypingGame.CurrentSong.Value.Artist} - {pTypingGame.CurrentSong.Value.Name}";
    public override bool ForceSpeedReset => true;
    public override float BackgroundFadeAmount => 0.7f;
    public override MusicLoopState LoopState => MusicLoopState.NewSong;
}