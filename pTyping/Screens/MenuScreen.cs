using System.IO;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using ManagedBass;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using pTyping.Drawables;
using pTyping.Online;
using pTyping.Songs;

namespace pTyping.Screens {
    public class MenuScreen : Screen {
        private TextDrawable _musicTitle;

        public override void Initialize() {
            base.Initialize();

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

            Texture2D menuButtonsTexture = ContentManager.LoadMonogameAsset<Texture2D>("menubuttons", ContentSource.User);

            float y = FurballGame.DEFAULT_WINDOW_HEIGHT * 0.35f;

            TexturedDrawable playButton = new(menuButtonsTexture, new(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, y), TexturePositions.MENU_PLAY_BUTTON) {
                OriginType = OriginType.Center,
                Scale      = new(0.75f)
            };
            TexturedDrawable editButton = new(
            menuButtonsTexture,
            new(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, y += playButton.Size.Y + 10),
            TexturePositions.MENU_EDIT_BUTTON
            ) {
                OriginType = OriginType.Center,
                Scale      = new(0.75f)
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
                pTypingGame.MenuClickSound.Play();
                ScreenManager.ChangeScreen(new SongSelectionScreen(false));
            };

            editButton.OnClick += delegate {
                pTypingGame.MenuClickSound.Play();
                ScreenManager.ChangeScreen(new SongSelectionScreen(true));
            };

            exitButton.OnClick += delegate {
                pTypingGame.MenuClickSound.Play();
                FurballGame.Instance.Exit();
            };

            optionsButton.OnClick += delegate {
                pTypingGame.MenuClickSound.Play();
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

            Texture2D editorButtonsTexture2D = ContentManager.LoadMonogameAsset<Texture2D>("editorbuttons", ContentSource.User);

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
                this.LoadSong(true);
            };

            this.Manager.Add(this._musicTitle);

            this.Manager.Add(musicPlayButton);
            this.Manager.Add(musicPauseButton);
            this.Manager.Add(musicNextButton);

            #endregion

            #region Background image

            this.Manager.Add(pTypingGame.CurrentSongBackground);
            pTypingGame.CurrentSongBackground.Tweens.Add(
            new ColorTween(
            TweenType.Color,
            pTypingGame.CurrentSongBackground.ColorOverride,
            new Color(175, 175, 175),
            pTypingGame.CurrentSongBackground.TimeSource.GetCurrentTime(),
            pTypingGame.CurrentSongBackground.TimeSource.GetCurrentTime() + 100
            )
            );

            #endregion

            if (pTypingGame.OnlineManager.State == ConnectionState.LoggedIn) {
                this.Manager.Add(pTypingGame.GetUserCard());

                pTypingGame.MenuPlayerUserCard.MoveTo(new(10f));
            }

            if (pTypingGame.CurrentSong is null || pTypingGame.CurrentSong?.Value is null)
                this.LoadSong(true);
            else
                this.LoadSong(false);
        }


        public void LoadSong(bool chooseNewOne) {
            if (SongManager.Songs.Count == 0) return;

            if (chooseNewOne) {
                int songToChoose = FurballGame.Random.Next(SongManager.Songs.Count);

                if (pTypingGame.CurrentSong == null)
                    pTypingGame.CurrentSong = new(SongManager.Songs[songToChoose]);
                else
                    pTypingGame.CurrentSong.Value = SongManager.Songs[songToChoose];
            }

            string qualifiedAudioPath = Path.Combine(pTypingGame.CurrentSong.Value.FileInfo.DirectoryName ?? string.Empty, pTypingGame.CurrentSong.Value.AudioPath);

            if (pTypingGame.MusicTrack.IsValidHandle && chooseNewOne) {
                if (pTypingGame.MusicTrack.PlaybackState == PlaybackState.Playing)
                    pTypingGame.StopMusic();

                pTypingGame.MusicTrack.Free();
            }

            if (chooseNewOne) {
                pTypingGame.LoadMusic(ContentManager.LoadRawAsset(qualifiedAudioPath, ContentSource.External));
                pTypingGame.PlayMusic();
            }

            pTypingGame.LoadBackgroundFromSong(pTypingGame.CurrentSong.Value);

            this._musicTitle.Text = $"{pTypingGame.CurrentSong.Value.Artist} - {pTypingGame.CurrentSong.Value.Name}";

            pTypingGame.UserStatusListening();
        }
    }
}
