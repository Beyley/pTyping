using System.Globalization;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Microsoft.Xna.Framework;
using pTyping.Engine;

namespace pTyping.Graphics.Menus.Options {
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
                // pTypingGame.MenuClickSound.Play();
                ScreenManager.ChangeScreen(new MenuScreen());
            };

            this.Manager.Add(backButton);

            #endregion

            #region Background image

            this.Manager.Add(pTypingGame.CurrentSongBackground);

            pTypingGame.CurrentSongBackground.Tweens.Add(
            new ColorTween(
            TweenType.Color,
            pTypingGame.CurrentSongBackground.ColorOverride,
            new(100, 100, 100),
            pTypingGame.CurrentSongBackground.TimeSource.GetCurrentTime(),
            pTypingGame.CurrentSongBackground.TimeSource.GetCurrentTime() + 1000
            )
            );

            #endregion

            #region Background Dim

            TextDrawable backgroundDimInputLabel = new(new Vector2(100, 150), FurballGame.DEFAULT_FONT, "Background Dim:", 30);
            UiTextBoxDrawable backgroundDimInput = new(
            new Vector2(110 + backgroundDimInputLabel.Size.X, 150),
            FurballGame.DEFAULT_FONT,
            ConVars.BackgroundDim.Value.ToString(CultureInfo.InvariantCulture),
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
            ConVars.Username.Value.ToString(CultureInfo.InvariantCulture),
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

            pTypingGame.UserStatusListening();
        }

        private void BackgroundDimInputOnCommit(object sender, string e) {
            ConVars.BackgroundDim.Value = float.Parse(e);
        }

        // private void TargetFpsInputOnCommit(object sender, string e) {
        //     Config.TargetFPS.Value = int.Parse(e);
        // }

        private void UsernameInputOnCommit(object sender, string e) {
            ConVars.Username.Value = e;
        }
        public override string Name    => "Options";
        public override string State   => "Tweaking the settings!";
        public override string Details => "";
    }
}
