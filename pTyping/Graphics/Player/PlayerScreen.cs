using System;
using System.Linq;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Engine.Engine.Helpers;
using ManagedBass;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using pTyping.Engine;
using pTyping.Graphics.Menus.SongSelect;
using pTyping.Scores;
using pTyping.Songs;

namespace pTyping.Graphics.Player {
    public class PlayerScreen : pScreen {
        private TextDrawable _accuracyDrawable;
        private TextDrawable _comboDrawable;
        private TextDrawable _scoreDrawable;

        private bool _endScheduled = false;
        
        private UiButtonDrawable _quitButton;
        private UiButtonDrawable _restartButton;
        private UiButtonDrawable _resumeButton;
        private UiButtonDrawable _skipButton;
        
        private LyricDrawable _lyricDrawable;

        public Song Song;

        private Player _player;

        public PlayerScreen() {}

        private          bool        _playingReplay = false;
        private readonly PlayerScore _playingScoreReplay;
        
        /// <summary>
        ///     Used to play a replay
        /// </summary>
        /// <param name="replay">The score to get the replay from</param>
        public PlayerScreen(PlayerScore replay) {
            this._playingReplay      = true;
            this._playingScoreReplay = replay;
        }

        public override void Initialize() {
            base.Initialize();

            this.Song = pTypingGame.CurrentSong.Value.Copy();

            if (this.Song.Notes.Count == 0) {
                //TODO notify the user the map did not load correctly, for now, we just send back to the song selection menu
                ScreenManager.ChangeScreen(new SongSelectionScreen(false));
                return;
            }

            this._player = new(this.Song) {
                Position     = new(0, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.5f),
                OriginType   = OriginType.LeftCenter,
                RecordReplay = !this._playingReplay
            };

            this._player.OnAllNotesComplete += this.EndScore;

            this.Manager.Add(this._player);

            #region UI

            this._scoreDrawable = new TextDrawable(new Vector2(5, 5), FurballGame.DEFAULT_FONT, $"{this._player.Score.Score:00000000}", 60);
            this._accuracyDrawable = new TextDrawable(
            new Vector2(5, 5 + this._scoreDrawable.Size.Y),
            FurballGame.DEFAULT_FONT,
            $"{this._player.Score.Accuracy * 100:0.00}%",
            60
            );
            this._comboDrawable = new TextDrawable(
            new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH * 0.15f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.5f - 70),
            FurballGame.DEFAULT_FONT,
            $"{this._player.Score.Combo}x",
            70
            ) {
                OriginType = OriginType.BottomCenter
            };

            this.Manager.Add(this._scoreDrawable);
            this.Manager.Add(this._accuracyDrawable);
            this.Manager.Add(this._comboDrawable);

            this._skipButton = new UiButtonDrawable(
            new(FurballGame.DEFAULT_WINDOW_WIDTH, FurballGame.DEFAULT_WINDOW_HEIGHT),
            "Skip Intro",
            FurballGame.DEFAULT_FONT,
            50,
            Color.Blue,
            Color.White,
            Color.White,
            new(0),
            this.SkipButtonClick
            );
            this._skipButton.OriginType = OriginType.BottomRight;
            this._skipButton.Visible    = false;

            this.Manager.Add(this._skipButton);

            this._lyricDrawable = new(new(FurballGame.DEFAULT_WINDOW_WIDTH * 0.1f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.8f), this.Song);
            this.Manager.Add(this._lyricDrawable);

            #region Pause UI

            Vector2 pauseUiButtonSize = new(170, 50);

            this._resumeButton = new(
            new(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.2f),
            "Resume",
            FurballGame.DEFAULT_FONT,
            50,
            Color.Green,
            Color.White,
            Color.White,
            pauseUiButtonSize
            ) {
                OriginType = OriginType.Center,
                Depth      = -1f
            };
            
            this._restartButton = new(
            new(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.3f),
            "Restart",
            FurballGame.DEFAULT_FONT,
            50,
            Color.Yellow,
            Color.White,
            Color.White,
            pauseUiButtonSize
            ) {
                OriginType = OriginType.Center,
                Depth      = -1f
            };
            
            this._quitButton = new(
            new(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.4f),
            "Quit",
            FurballGame.DEFAULT_FONT,
            50,
            Color.Red,
            Color.White,
            Color.White,
            pauseUiButtonSize
            ) {
                OriginType = OriginType.Center,
                Depth      = -1f
            };

            this._resumeButton.OnClick  += this.ResumeButtonClick;
            this._restartButton.OnClick += this.RestartButtonClick;
            this._quitButton.OnClick    += this.QuitButtonClick;

            this.Manager.Add(this._resumeButton);
            this.Manager.Add(this._restartButton);
            this.Manager.Add(this._quitButton);

            #endregion

            #endregion

            #region Playfield decorations

            #region background image

            this.Manager.Add(pTypingGame.CurrentSongBackground);

            pTypingGame.CurrentSongBackground.Tweens.Add(
            new ColorTween(
            TweenType.Color,
            pTypingGame.CurrentSongBackground.ColorOverride,
            new(1f * (1f - ConVars.BackgroundDim.Value), 1f * (1f - ConVars.BackgroundDim.Value), 1f * (1f - ConVars.BackgroundDim.Value)),
            pTypingGame.CurrentSongBackground.TimeSource.GetCurrentTime(),
            pTypingGame.CurrentSongBackground.TimeSource.GetCurrentTime() + 1000
            )
            );
            pTypingGame.LoadBackgroundFromSong(this.Song);

            #endregion

            #endregion

            this._player.OnComboUpdate += this.OnComboUpdate;

            FurballGame.InputManager.OnKeyDown    += this.OnKeyPress;
            if (!this._playingReplay)
                FurballGame.Instance.Window.TextInput += this._player.TypeCharacter;

            pTypingGame.UserStatusPlaying();
        }
        private void OnComboUpdate(object sender, Color e) {
            this._comboDrawable.Tweens.Add(
            new ColorTween(
            TweenType.Color,
            this._comboDrawable.ColorOverride,
            e,
            this._comboDrawable.TimeSource.GetCurrentTime(),
            this._comboDrawable.TimeSource.GetCurrentTime() + 100
            )
            );
            this._comboDrawable.Tweens.Add(
            new ColorTween(
            TweenType.Color,
            e,
            Color.White,
            this._comboDrawable.TimeSource.GetCurrentTime() + 100,
            this._comboDrawable.TimeSource.GetCurrentTime() + 1100
            )
            );
        }

        private void ResumeButtonClick(object sender, Point e) {
            pTypingGame.MenuClickSound.PlayNew();
            pTypingGame.PauseResumeMusic();
        }

        private void RestartButtonClick(object sender, Point e) {
            pTypingGame.MenuClickSound.PlayNew();
            pTypingGame.MusicTrack.CurrentPosition = 0;
            ScreenManager.ChangeScreen(new PlayerScreen());
        }

        private void QuitButtonClick(object sender, Point e) {
            pTypingGame.MenuClickSound.PlayNew();
            ScreenManager.ChangeScreen(new SongSelectionScreen(false));
        }

        private void SkipButtonClick(object sender, Point e) {
            pTypingGame.MenuClickSound.PlayNew();
            pTypingGame.MusicTrack.CurrentPosition = this.Song.Notes.First().Time - 2999;
        }

        
        protected override void Dispose(bool disposing) {
            FurballGame.InputManager.OnKeyDown    -= this.OnKeyPress;
            FurballGame.Instance.Window.TextInput -= this._player.TypeCharacter;

            this._player.OnAllNotesComplete -= this.EndScore;
            this._player.OnComboUpdate      -= this.OnComboUpdate;

            base.Dispose(disposing);
        }

        private void OnKeyPress(object sender, Keys key) {
            if (key == Keys.Escape) {
                if (this._endScheduled) return;
                
                pTypingGame.PauseResumeMusic();
            }
        }

        public override void Update(GameTime gameTime) {
            int currentTime = pTypingGame.MusicTrackTimeSource.GetCurrentTime();

            #region update UI

            this._scoreDrawable.Text    = $"{this._player.Score.Score:00000000}";
            this._accuracyDrawable.Text = $"{this._player.Score.Accuracy * 100:0.00}%";
            this._comboDrawable.Text    = $"{this._player.Score.Combo}x";

            bool isPaused = pTypingGame.MusicTrack.PlaybackState == PlaybackState.Paused;

            this._resumeButton.Visible  = isPaused;
            this._restartButton.Visible = isPaused;
            this._quitButton.Visible    = isPaused;

            this._resumeButton.Clickable  = isPaused;
            this._restartButton.Clickable = isPaused;
            this._quitButton.Clickable    = isPaused;

            #endregion

            this._lyricDrawable.UpdateLyric(currentTime);

            #region skip button visibility

            if (this.Song.Notes.First().Time - currentTime > 3000)
                this._skipButton.Visible = true;
            else
                this._skipButton.Visible = false;

            #endregion

            if (this._playingReplay && Array.TrueForAll(this._playingScoreReplay.ReplayFrames, x => x.Used))
                this._playingReplay = false;

            if (this._playingReplay)
                for (int i = 0; i < this._playingScoreReplay.ReplayFrames.Length; i++) {
                    ref ReplayFrame currentFrame = ref this._playingScoreReplay.ReplayFrames[i];

                    if (currentTime > currentFrame.Time && !currentFrame.Used) {
                        this._player.TypeCharacter(this, new TextInputEventArgs(currentFrame.Character));

                        currentFrame.Used = true;
                        break;
                    }
                }

            base.Update(gameTime);
        }

        public void EndScore(object sender, EventArgs args) {
            if (!this._endScheduled) {
                FurballGame.GameTimeScheduler.ScheduleMethod(
                delegate {
                    this._player.CallMapEnd();

                    this._player.Score.Time         = DateTime.Now;
                    if (this._playingScoreReplay == null) {
                        this._player.Score.ReplayFrames = this._player.ReplayFrames.ToArray();
                        pTypingGame.SubmitScore(this.Song, this._player.Score);
                    } else {
                        this._player.Score.Username = this._playingScoreReplay.Username;
                    }

                    ScreenManager.ChangeScreen(new ScoreResultsScreen(this._player.Score));
                },
                FurballGame.Time + 1500
                );

                this._endScheduled = true;
            }
        }

        public override string Name  => "Gameplay";
        public override string State => "Typing away!";
        public override string Details
            => $@"Playing {pTypingGame.CurrentSong.Value.Artist} - {pTypingGame.CurrentSong.Value.Name} [{pTypingGame.CurrentSong.Value.Difficulty}]";
    }
}
