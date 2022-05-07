using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Engine.Engine.Localization;
using Furball.Engine.Engine.Localization.Languages;
using Furball.Vixie.Backends.Shared;
using pTyping.Engine;

namespace pTyping.Graphics.Menus.Options;

public class OptionsScreen : pScreen {
    private readonly Dictionary<string, Language> _languageDictionary = new();

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

        TextDrawable backgroundDimInputLabel = new(new Vector2(100, 150), FurballGame.DEFAULT_FONT, "Background Dim:", 30);
        UiTextBoxDrawable backgroundDimInput = new(
        new Vector2(110 + backgroundDimInputLabel.Size.X, 150),
        FurballGame.DEFAULT_FONT,
        ConVars.BackgroundDim.Value.Value.ToString(CultureInfo.InvariantCulture),
        30,
        200
        );

        backgroundDimInput.OnCommit += this.BackgroundDimInputOnCommit;

        this.Manager.Add(backgroundDimInputLabel);
        this.Manager.Add(backgroundDimInput);

        #endregion

        #region Target FPS

        // TextDrawable targetFPSInputLabel = new(new Vector2(100, 200), FurballGame.DEFAULT_FONT, "Target FPS:", 30);
        // UiTextBoxDrawable targetFPSInput = new(
        // new Vector2(110 + targetFPSInputLabel.Size.X, 200),
        // FurballGame.DEFAULT_FONT,
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

        TextDrawable usernameInputLabel = new(new Vector2(350, 200), FurballGame.DEFAULT_FONT, "Username:", 30);
        UiTextBoxDrawable usernameInput = new(
        new Vector2(360 + usernameInputLabel.Size.X, 200),
        FurballGame.DEFAULT_FONT,
        ConVars.Username.Value.Value.ToString(CultureInfo.InvariantCulture),
        30,
        200
        );

        usernameInput.OnCommit += this.UsernameInputOnCommit;

        this.Manager.Add(usernameInputLabel);
        this.Manager.Add(usernameInput);

        #endregion

        #region 1600x900 res button

        UiButtonDrawable res1600X900Button = new(new(100, 300), "1600x900", FurballGame.DEFAULT_FONT, 30, Color.Blue, Color.White, Color.White, Vector2.Zero);

        res1600X900Button.OnClick += delegate {
            FurballGame.Instance.ChangeScreenSize(1600, 900);
        };

        this.Manager.Add(res1600X900Button);

        #endregion

        #region 1920x1080 res button

        UiButtonDrawable res1920X1080Button = new(new(100, 400), "1920x1080", FurballGame.DEFAULT_FONT, 30, Color.Blue, Color.White, Color.White, Vector2.Zero);

        res1920X1080Button.OnClick += delegate {
            FurballGame.Instance.ChangeScreenSize(1920, 1080);
        };

        this.Manager.Add(res1920X1080Button);

        #endregion

        #region Language dropdown

        List<string> languages = new();

        foreach (ISO639_2Code code in LocalizationManager.GetSupportedLanguages()) {
            Language language = LocalizationManager.GetLanguageFromCode(code)!;

            string languageName = language.ToString();

            languages.Add(languageName);
            this._languageDictionary.Add(languageName, language);
        }

        UiDropdownDrawable languageDropdown = new(new(800, 100), languages, new(175, 40), pTypingGame.JapaneseFontStroked, 20);
        languageDropdown.SelectedItem.Value = LocalizationManager.CurrentLanguage.ToString();
        languageDropdown.Update();

        languageDropdown.SelectedItem.OnChange += this.OnLanguageChange;

        this.Manager.Add(languageDropdown);

        #endregion

        pTypingGame.UserStatusListening();
    }

    private void OnLanguageChange(object _, string s) {
        LocalizationManager.CurrentLanguage = this._languageDictionary[s];
    }

    private void BackgroundDimInputOnCommit(object sender, string e) {
        ConVars.BackgroundDim.Value = new(float.Parse(e));
    }

    // private void TargetFpsInputOnCommit(object sender, string e) {
    //     Config.TargetFPS.Value = int.Parse(e);
    // }

    private void UsernameInputOnCommit(object sender, string e) {
        ConVars.Username.Value = new(e);
    }
    public override string         Name                 => "Options";
    public override string         State                => "Tweaking the settings!";
    public override string         Details              => "";
    public override bool           ForceSpeedReset      => true;
    public override float          BackgroundFadeAmount => 0.4f;
    public override MusicLoopState LoopState            => MusicLoopState.NewSong;
    public override ScreenType     ScreenType           => ScreenType.Menu;
}