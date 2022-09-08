using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Engine.Engine.Localization;
using Furball.Engine.Engine.Localization.Languages;
using Furball.Vixie.Backends.Shared;
using Furball.Volpe.Evaluation;
using pTyping.Engine;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace pTyping.Graphics.Menus.Options;

public class OptionsScreen : pScreen {
    public override void Initialize() {
        base.Initialize();

        #region Back button

        pTypingGame.LoadBackButtonTexture();

        TexturedDrawable backButton = new(pTypingGame.BackButtonTexture, new Vector2(0, FurballGame.DEFAULT_WINDOW_HEIGHT)) {
            OriginType = OriginType.BottomLeft,
            Scale      = pTypingGame.BackButtonScale
        };

        backButton.OnClick += delegate {
            pTypingGame.MenuClickSound.PlayNew();
            ScreenManager.ChangeScreen(new MenuScreen());
        };

        this.Manager.Add(backButton);

        #endregion

        #region Background image

        this.Manager.Add(pTypingGame.CurrentSongBackground);

        #endregion

        #region Background Dim

        TextDrawable backgroundDimInputLabel = new(new Vector2(100, 150), FurballGame.DefaultFont, "Background Dim:", 30);
        DrawableTextBox backgroundDimInput = new(
        new Vector2(110 + backgroundDimInputLabel.Size.X, 150),
        FurballGame.DefaultFont,
        30,
        200,
        ConVars.BackgroundDim.Value.Value.ToString(CultureInfo.InvariantCulture)
        );

        backgroundDimInput.OnCommit += this.BackgroundDimInputOnCommit;

        this.Manager.Add(backgroundDimInputLabel);
        this.Manager.Add(backgroundDimInput);

        #endregion

        #region Target FPS

        // TextDrawable targetFPSInputLabel = new(new Vector2(100, 200), FurballGame.DefaultFont, "Target FPS:", 30);
        // DrawableTextBox targetFPSInput = new(
        // new Vector2(110 + targetFPSInputLabel.Size.X, 200),
        // FurballGame.DefaultFont,
        // ConVars.TargetFPS.Value.Value.ToString(CultureInfo.InvariantCulture),
        // 30,
        // 200
        // );
        //
        // targetFPSInput.OnCommit += this.TargetFpsInputOnCommit;
        //
        // this.Manager.Add(targetFPSInputLabel);
        // this.Manager.Add(targetFPSInput);

        #endregion

        #region Username

        TextDrawable usernameInputLabel = new(new Vector2(350, 200), FurballGame.DefaultFont, "Username:", 30);
        DrawableTextBox usernameInput = new(
        new Vector2(360 + usernameInputLabel.Size.X, 200),
        FurballGame.DefaultFont,
        30,
        200,
        pTypingConfig.Instance.Username
        );

        usernameInput.OnCommit += this.UsernameInputOnCommit;

        this.Manager.Add(usernameInputLabel);
        this.Manager.Add(usernameInput);

        #endregion

        #region Resolution dropdown

        List<VideoMode> supportedResolutions = FurballGame.Instance.WindowManager.Monitor.GetAllVideoModes().ToList();
        supportedResolutions.Add(new VideoMode(new Vector2D<int>(1600, 900)));

        Dictionary<object, string> items = new();
        
        foreach (VideoMode supportedResolution in supportedResolutions) {
            if (Math.Abs((double)supportedResolution.Resolution!.Value.X / (double)supportedResolution.Resolution!.Value.Y - (16d / 9d)) > 0.05d)
                continue;
            
            items.Add(supportedResolution, $"{supportedResolution.Resolution.Value.X}x{supportedResolution.Resolution.Value.Y} ({supportedResolution.RefreshRate}hz)");
        }

        DrawableDropdown resolutionDropdown = new(new Vector2(100, 300), pTypingGame.JapaneseFont, 25, new(200, 50), items);

        resolutionDropdown.SelectedItem.OnChange += delegate(object _, KeyValuePair<object, string> pair) {
            VideoMode mode = (VideoMode)pair.Key;

            FurballGame.Instance.ChangeScreenSize(mode.Resolution.Value.X, mode.Resolution.Value.Y);
        };
        
        this.Manager.Add(resolutionDropdown);
        
        #endregion

        #region Language dropdown

        Dictionary<object, string> languages = new();

        foreach (ISO639_2Code code in LocalizationManager.GetSupportedLanguages()) {
            Language language = LocalizationManager.GetLanguageFromCode(code)!;

            languages.Add(language, language.ToString());
        }

        DrawableDropdown languageDropdown = new(new Vector2(800, 100), pTypingGame.JapaneseFontStroked, 20, new Vector2(175, 40), languages) {
            SelectedItem = {
                Value = languages.First(x => ((Language)x.Key).Iso6392Code() == LocalizationManager.CurrentLanguage.Iso6392Code())
            }
        };
        languageDropdown.Update();

        languageDropdown.SelectedItem.OnChange += this.OnLanguageChange;

        this.Manager.Add(languageDropdown);

        #endregion

        #region Crash Button

        DrawableButton crash = new(new(400, 400), pTypingGame.JapaneseFont, 30, "Crash the game", Color.Red, Color.White, Color.Black, Vector2.Zero);

        crash.OnClick += delegate {
            throw new Exception("Manual crash.");
        };

        this.Manager.Add(crash);

        #endregion

        #region Lobby test

        DrawableButton lobbyTest = new(new(400, 500), pTypingGame.JapaneseFont, 30, "Crate a lobby", Color.Red, Color.White, Color.Black, Vector2.Zero);

        lobbyTest.OnClick += delegate {
            // TODO
            // DiscordManager.CreateLobby();
        };

        this.Manager.Add(lobbyTest);

        #endregion
    }

    public override ScreenUserActionType OnlineUserActionType => ScreenUserActionType.Listening;

    private void OnLanguageChange(object _, KeyValuePair<object, string> keyValuePair) {
        LocalizationManager.CurrentLanguage = (Language) keyValuePair.Key;
    }

    private void BackgroundDimInputOnCommit(object sender, string e) {
        ConVars.BackgroundDim.Value = new Value.Number(float.Parse(e));
    }

    // private void TargetFpsInputOnCommit(object sender, string e) {
    //     Config.TargetFPS.Value = int.Parse(e);
    // }

    private void UsernameInputOnCommit(object sender, string e) {
        pTypingConfig.Instance.Values["username"] = new Value.String(e);
    }
    public override string         Name                 => "Options";
    public override string         State                => "Tweaking the settings!";
    public override string         Details              => "";
    public override bool           ForceSpeedReset      => true;
    public override float          BackgroundFadeAmount => 0.4f;
    public override MusicLoopState LoopState            => MusicLoopState.NewSong;
    public override ScreenType     ScreenType           => ScreenType.Menu;
}