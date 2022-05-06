using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Engine.Engine.Helpers;
using Kettu;
using ManagedBass;
using pTyping.Engine;
using pTyping.Graphics.Menus.SongSelect;
using pTyping.Online;
using pTyping.Online.Taiko_rs;
using pTyping.Scores;
using pTyping.Songs;
using Silk.NET.Input;
using Color=Furball.Vixie.Backends.Shared.Color;

namespace pTyping.Graphics.Player;

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

    private AccuracyBarDrawable _accuracyBar;

    public Song Song;

    public Player Player;

    public PlayerScreen() {}

    private          bool        _playingReplay = false;
    private readonly PlayerScore _playingScoreReplay;

    public bool                 IsSpectating   = false;
    public List<SpectatorFrame> SpectatorQueue = new();

    /// <summary>
    ///     Used to play a replay
    /// </summary>
    /// <param name="replay">The score to get the replay from</param>
    public PlayerScreen(PlayerScore replay) {
        this._playingReplay      = true;
        this._playingScoreReplay = replay.Copy();
    }

    public PlayerScreen(OnlinePlayer host) =>
        //host is unused, shhh
        this.IsSpectating = true;

    public override void Initialize() {
        base.Initialize();

        this.Song = pTypingGame.CurrentSong.Value.Copy();

        #region song validation

        if (this.Song.Notes.Count == 0) {
            pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Error, "That map has 0 notes!");
            ScreenManager.ChangeScreen(new SongSelectionScreen(false));
            return;
        }

        if (this.Song.Settings.Strictness <= 0) {
            pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Error, "That map has an invalid Strictness!");
            ScreenManager.ChangeScreen(new SongSelectionScreen(false));
            return;
        }

        if (this.Song.Settings.GlobalApproachMultiplier <= 0) {
            pTypingGame.NotificationManager.CreateNotification(
            NotificationManager.NotificationImportance.Error,
            "That map has an invalid global approach multiplier!"
            );
            ScreenManager.ChangeScreen(new SongSelectionScreen(false));
            return;
        }

        #endregion

        this.Player = new(this.Song) {
            Position     = new(0, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.5f),
            OriginType   = OriginType.LeftCenter,
            RecordReplay = !this._playingReplay,
            IsSpectating = this.IsSpectating
        };

        this.Player.OnAllNotesComplete += this.EndScore;

        this.Manager.Add(this.Player);

        #region UI

        this._scoreDrawable = new TextDrawable(new Vector2(5, 5), FurballGame.DEFAULT_FONT, $"{this.Player.Score.Score:00000000}", 60);
        this._accuracyDrawable = new TextDrawable(
        new Vector2(5, 5 + this._scoreDrawable.Size.Y),
        FurballGame.DEFAULT_FONT,
        $"{this.Player.Score.Accuracy * 100:0.00}%",
        60
        );
        this._comboDrawable = new TextDrawable(
        new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH * 0.15f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.5f - 70),
        FurballGame.DEFAULT_FONT,
        $"{this.Player.Score.Combo}x",
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

        this.Manager.Add(this._accuracyBar = new AccuracyBarDrawable(new(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, FurballGame.DEFAULT_WINDOW_HEIGHT), this.Player) {
            OriginType = OriginType.BottomCenter
        });
        
        #endregion

        #region Playfield decorations

        #region background image

        this.Manager.Add(pTypingGame.CurrentSongBackground);

        pTypingGame.CurrentSongBackground.Tweens.Add(
        new ColorTween(
        TweenType.Color,
        pTypingGame.CurrentSongBackground.ColorOverride,
        new(
        (float)(1f * (1f - ConVars.BackgroundDim.Value.Value)),
        (float)(1f * (1f - ConVars.BackgroundDim.Value.Value)),
        (float)(1f * (1f - ConVars.BackgroundDim.Value.Value))
        ),
        pTypingGame.CurrentSongBackground.TimeSource.GetCurrentTime(),
        pTypingGame.CurrentSongBackground.TimeSource.GetCurrentTime() + 1000
        )
        );
        // pTypingGame.LoadBackgroundFromSong(this.Song);

        #endregion

        #endregion

        this.Player.OnComboUpdate += this.OnComboUpdate;

        FurballGame.InputManager.OnKeyDown += this.OnKeyPress;
        if (!this._playingReplay)
            FurballGame.InputManager.OnCharInput += this.Player.TypeCharacter;

        pTypingGame.UserStatusPlaying();

        double offset = OffsetManager.GetOffset(this.Song);

        if (offset != 0) {
            pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Info, $"Using local offset of {offset}");
            pTypingGame.MusicTrackTimeSource.Offset = offset;
        }

        if (!this._playingReplay)
            pTypingGame.OnlineManager.GameScene = this;
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
        new ColorTween(TweenType.Color, e, Color.White, this._comboDrawable.TimeSource.GetCurrentTime() + 100, this._comboDrawable.TimeSource.GetCurrentTime() + 1100)
        );
    }

    private void ResumeButtonClick(object sender, (MouseButton button, Point pos) tuple) {
        pTypingGame.MenuClickSound.PlayNew();
        pTypingGame.PauseResumeMusic();

        pTypingGame.OnlineManager.SpectatorResume(pTypingGame.MusicTrack.CurrentPosition);
    }

    private void RestartButtonClick(object sender, (MouseButton button, Point pos) tuple) {
        pTypingGame.MenuClickSound.PlayNew();
        pTypingGame.MusicTrack.CurrentPosition = 0;
        ScreenManager.ChangeScreen(new PlayerScreen());
    }

    private void QuitButtonClick(object sender, (MouseButton button, Point pos) tuple) {
        pTypingGame.MenuClickSound.PlayNew();
        ScreenManager.ChangeScreen(new SongSelectionScreen(false));
    }

    private void SkipButtonClick(object sender, (MouseButton, Point) tuple) {
        pTypingGame.MenuClickSound.PlayNew();
        pTypingGame.MusicTrack.CurrentPosition = this.Song.Notes.First().Time - 2999;
    }


    public override void Dispose() {
        FurballGame.InputManager.OnKeyDown   -= this.OnKeyPress;
        FurballGame.InputManager.OnCharInput -= this.Player.TypeCharacter;

        this.Player.OnAllNotesComplete -= this.EndScore;
        this.Player.OnComboUpdate      -= this.OnComboUpdate;

        if (pTypingGame.OnlineManager.GameScene == this)
            pTypingGame.OnlineManager.GameScene = null;

        base.Dispose();
    }

    private double lastPress;
    private void OnKeyPress(object sender, Key key) {
        if (key != Key.Minus && key != Key.Equal)
            this.lastPress = FurballGame.Time;

        switch (key) {
            case Key.Escape when this._endScheduled: {
                return;
            }
            case Key.Escape: {
                pTypingGame.PauseResumeMusic();

                switch (pTypingGame.MusicTrack.PlaybackState) {
                    case PlaybackState.Paused:
                        pTypingGame.OnlineManager.SpectatorPause(pTypingGame.MusicTrack.CurrentPosition);
                        break;
                    case PlaybackState.Playing:
                        pTypingGame.OnlineManager.SpectatorResume(pTypingGame.MusicTrack.CurrentPosition);
                        break;
                }
                break;
            }
            case Key.Equal: {
                if (FurballGame.Time - this.lastPress < 5000 && pTypingGame.MusicTrack.PlaybackState == PlaybackState.Playing)
                    break;

                double currentOffset = OffsetManager.GetOffset(this.Song);

                OffsetManager.SetOffset(this.Song, currentOffset + 5);
                pTypingGame.NotificationManager.CreatePopup($"Offset set to {currentOffset + 5}");
                pTypingGame.MusicTrackTimeSource.Offset = currentOffset + 5;

                break;
            }
            case Key.Minus: {
                if (FurballGame.Time - this.lastPress < 5000 && pTypingGame.MusicTrack.PlaybackState == PlaybackState.Playing)
                    break;

                double currentOffset = OffsetManager.GetOffset(this.Song);

                OffsetManager.SetOffset(this.Song, currentOffset - 5);
                pTypingGame.NotificationManager.CreatePopup($"Offset set to {currentOffset - 5}");
                pTypingGame.MusicTrackTimeSource.Offset = currentOffset - 5;

                break;
            }
        }
    }

    private double timeSinceLastBuffer    = 0;
    private double timeSinceLastScoreSync = 0;

    public override void Update(double gameTime) {
        double currentTime = pTypingGame.MusicTrackTimeSource.GetCurrentTime();

        lock (this.SpectatorQueue) {
            Span<SpectatorFrame> tFrames = CollectionsMarshal.AsSpan(this.SpectatorQueue);
            for (int i = 0; i < tFrames.Length; i++) {
                SpectatorFrame f = tFrames[i];

                if (f == null) {
                    this.SpectatorQueue.RemoveAll(x => x == null);
                    continue;
                }

                if (f.Time < currentTime) {
                    this.SpectatorQueue.Remove(f);
                    Logger.Log($"Consuming frame {f.Type} with time {f.Time}  --  currenttime:{currentTime}", LoggerLevelPlayerInfo.Instance);

                    switch (f) {
                        case SpectatorFramePause: {
                            pTypingGame.OnlineManager.SpectatorState = SpectatorState.Paused;
                            pTypingGame.MusicTrack.Pause();
                            pTypingGame.MusicTrack.CurrentPosition = f.Time;
                            break;
                        }
                        // case SpectatorFrameDataType.Resume: {
                        //     pTypingGame.OnlineManager.SpectatorState = SpectatorState.Playing;
                        //     pTypingGame.MusicTrack.Resume();
                        //     break;
                        // }
                        case SpectatorFrameScoreSync ssF: {
                            this.Player.Score = ssF.Score;
                            break;
                        }
                        case SpectatorFrameBuffer: {
                            if (currentTime - f.Time > 100) {
                                pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Warning, "We got too far ahead!");
                                pTypingGame.MusicTrack.CurrentPosition = currentTime - 2000;
                            }

                            break;
                        }
                        case SpectatorFrameReplayFrame sRf: {
                            this.Player.TypeCharacter(this, sRf.Frame.Character);

                            break;
                        }
                    }
                }
            }

            this.timeSinceLastBuffer += gameTime * 1000;
            //Send a buffer every second
            if (this.timeSinceLastBuffer > 1000) {
                if (pTypingGame.OnlineManager.Spectators.Count != 0) {
                    pTypingGame.OnlineManager.SpectatorBuffer(currentTime);
                    if (this.Player.ReplayFrames.Count != 0) {
                        this.Player.ReplayFrames.ForEach(x => pTypingGame.OnlineManager.SpectatorReplayFrame(x.Time, x));
                        this.Player.ReplayFrames.Clear();
                    }
                } else if (this.IsSpectating) {
                    // if(this.SpectatorQueue.Count != 0)
                    //     if (this.SpectatorQueue[^1].Time < currentTime + 5000) {
                    //         //TODO: make this better
                    //         if (pTypingGame.MusicTrack.PlaybackState == PlaybackState.Playing && pTypingGame.OnlineManager.SpectatorState == SpectatorState.Playing) {
                    //             pTypingGame.MusicTrack.Pause();
                    //             pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Warning, "buffering");
                    //         }
                    //     } else if(pTypingGame.MusicTrack.PlaybackState == PlaybackState.Paused && pTypingGame.OnlineManager.SpectatorState == SpectatorState.Playing) {
                    //         FurballGame.GameTimeScheduler.ScheduleMethod(_ => pTypingGame.MusicTrack.Resume(), FurballGame.Time + 1000);
                    //     }
                }
                this.timeSinceLastBuffer = 0;
            }

            if (pTypingGame.OnlineManager.Spectators.Count != 0) {
                this.timeSinceLastScoreSync += gameTime * 1000;
                if (this.timeSinceLastScoreSync > 5000) {
                    pTypingGame.OnlineManager.SpectatorScoreSync(currentTime, this.Player.Score);
                    this.timeSinceLastScoreSync = 0;
                }
            }
        }

        #region update UI

        lock (this.Player.Score) {
            this._scoreDrawable.Text    = $"{this.Player.Score.Score:00000000}";
            this._accuracyDrawable.Text = $"{this.Player.Score.Accuracy * 100:0.00}%";
            this._comboDrawable.Text    = $"{this.Player.Score.Combo}x";
        }

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
                    this.Player.TypeCharacter(this, currentFrame.Character);

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
                this.Player.CallMapEnd();

                this.Player.Score.Time = DateTime.Now;
                if (this._playingScoreReplay == null) {
                    this.Player.Score.ReplayFrames = this.Player.ReplayFrames.ToArray();
                    pTypingGame.SubmitScore(this.Song, this.Player.Score);
                } else {
                    this.Player.Score.Username = this._playingScoreReplay.Username;
                }

                ScreenManager.ChangeScreen(new ScoreResultsScreen(this.Player.Score));
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
    public override bool ForceSpeedReset => false;
    public override float BackgroundFadeAmount => -1f;
    public override MusicLoopState LoopState => MusicLoopState.None;
}