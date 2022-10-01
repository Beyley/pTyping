using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Engine.Engine.Input.Events;
using Furball.Vixie.Backends.Shared;
using JetBrains.Annotations;
using Kettu;
using ManagedBass;
using pTyping.Engine;
using pTyping.Graphics.Menus.SongSelect;
using pTyping.Online;
using pTyping.Online.Tataku;
using pTyping.Shared.Beatmaps;
using pTyping.Shared.Mods;
using pTyping.Shared.Scores;
using Silk.NET.Input;

namespace pTyping.Graphics.Player;

public class PlayerScreen : pScreen {
	private TextDrawable _accuracyDrawable;
	private TextDrawable _comboDrawable;
	private TextDrawable _scoreDrawable;

	private bool _endScheduled;

	private DrawableButton _quitButton;
	private DrawableButton _restartButton;
	private DrawableButton _resumeButton;
	private DrawableButton _skipButton;

	private LyricDrawable _lyricDrawable;

	private AccuracyBarDrawable _accuracyBar;

	public Beatmap Song;

	public Player Player;

	public PlayerScreen(Mod[] mods) {
		this._mods = mods;
	}

	private          bool  _playingReplay;
	private readonly Score _playingScoreReplay;

	public bool                 IsSpectating;
	public List<SpectatorFrame> SpectatorQueue = new List<SpectatorFrame>();

	[CanBeNull]
	private VideoDrawable _video;

	/// <summary>
	///     Used to play a replay
	/// </summary>
	/// <param name="replay">The score to get the replay from</param>
	public PlayerScreen(Score replay) {
		this._playingReplay      = true;
		this._playingScoreReplay = replay.Clone();
	}

	public PlayerScreen(OnlinePlayer host) {
		//host is unused, shhh
		this.IsSpectating = true;
	}

	public override void Initialize() {
		base.Initialize();

		this.Song = pTypingGame.CurrentSong.Value.Clone();

		#region song validation

		if (this.Song.HitObjects.Count == 0) {
			pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Error, "That map has 0 notes!");
			ScreenManager.ChangeScreen(new SongSelectionScreen(false));
			return;
		}

		if (this.Song.Difficulty.Strictness <= 0) {
			pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Error, "That map has an invalid Strictness!");
			ScreenManager.ChangeScreen(new SongSelectionScreen(false));
			return;
		}

		// if (this.Song.Settings.GlobalApproachMultiplier <= 0) {
		//     pTypingGame.NotificationManager.CreateNotification(
		//     NotificationManager.NotificationImportance.Error,
		//     "That map has an invalid global approach multiplier!"
		//     );
		//     ScreenManager.ChangeScreen(new SongSelectionScreen(false));
		//     return;
		// }

		#endregion

		Mod[] mods = this._playingReplay ? this._playingScoreReplay.Mods : this._mods;

		this.Player = new Player(this.Song, mods) {
			Position     = new Vector2(0, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.5f),
			OriginType   = OriginType.LeftCenter,
			RecordReplay = !this._playingReplay,
			IsSpectating = this.IsSpectating
		};

		this.Player.OnAllNotesComplete += this.EndScore;

		this.Manager.Add(this.Player);

		#region UI

		this._scoreDrawable = new TextDrawable(new Vector2(5, 5), FurballGame.DefaultFont, $"{this.Player.Score.AchievedScore:00000000}", 60);
		this._accuracyDrawable = new TextDrawable(
			new Vector2(5, 5 + this._scoreDrawable.Size.Y),
			FurballGame.DefaultFont,
			$"{this.Player.Score.Accuracy * 100:0.00}%",
			60
		);
		this._comboDrawable = new TextDrawable(
			new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH * 0.15f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.5f - 70),
			FurballGame.DefaultFont,
			$"{this.Player.Score.CurrentCombo}x",
			70
		) {
			OriginType = OriginType.BottomCenter
		};

		this.Manager.Add(this._scoreDrawable);
		this.Manager.Add(this._accuracyDrawable);
		this.Manager.Add(this._comboDrawable);

		this._skipButton = new DrawableButton(
			new Vector2(5),
			FurballGame.DefaultFont,
			50,
			"Skip Intro",
			Color.Blue,
			Color.White,
			Color.White,
			new Vector2(0),
			this.SkipButtonClick
		) {
			OriginType       = OriginType.BottomRight,
			ScreenOriginType = OriginType.BottomRight,
			Visible          = false
		};

		this.Manager.Add(this._skipButton);

		this._lyricDrawable = new LyricDrawable(new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH * 0.1f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.8f), this.Song);
		this.Manager.Add(this._lyricDrawable);

		#region Pause UI

		Vector2 pauseUiButtonSize = new Vector2(170, 50);

		this._resumeButton = new DrawableButton(
			new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.2f),
			FurballGame.DefaultFont,
			50,
			"Resume",
			Color.Green,
			Color.White,
			Color.White,
			pauseUiButtonSize
		) {
			OriginType = OriginType.Center,
			Depth      = -1f
		};

		this._restartButton = new DrawableButton(
			new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.3f),
			FurballGame.DefaultFont,
			50,
			"Restart",
			Color.Yellow,
			Color.White,
			Color.White,
			pauseUiButtonSize
		) {
			OriginType = OriginType.Center,
			Depth      = -1f
		};

		this._quitButton = new DrawableButton(
			new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.4f),
			FurballGame.DefaultFont,
			50,
			"Quit",
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

		this.Manager.Add(this._accuracyBar = new AccuracyBarDrawable(new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, FurballGame.DEFAULT_WINDOW_HEIGHT), this.Player) {
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
				new Color(
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

		#region Background video

		if (pTypingConfig.Instance.VideoBackgrounds && this.Song.FileCollection.BackgroundVideo != null)
			try {
				this._video = new VideoDrawable(
					pTypingGame.FileDatabase.GetFile(this.Song.FileCollection.BackgroundVideo.Hash),
					pTypingGame.MusicTrack.GetSpeed(),
					pTypingGame.MusicTrackTimeSource,
					new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, FurballGame.DEFAULT_WINDOW_HEIGHT / 2f)
				) {
					OriginType = OriginType.Center,
					Depth      = 1f
				};

				this._video.FadeColor(
					new Color(
						(float)(1f * (1f - ConVars.BackgroundDim.Value.Value)),
						(float)(1f * (1f - ConVars.BackgroundDim.Value.Value)),
						(float)(1f * (1f - ConVars.BackgroundDim.Value.Value))
					),
					1000
				);

				this._video.Scale = new Vector2(1f / ((float)this._video.Texture.Height / FurballGame.DEFAULT_WINDOW_HEIGHT));

				this._video.StartTime = 0;

				this.Manager.Add(this._video);
			}
			catch {
				pTypingConfig.Instance.VideoBackgrounds = false;
				this._video                             = null;
				pTypingGame.NotificationManager.CreateNotification(
					NotificationManager.NotificationImportance.Error,
					"Unable to load background video! Disabling video support..."
				);
			}

		#endregion

		#endregion

		this.Player.OnComboUpdate += this.OnComboUpdate;

		FurballGame.InputManager.OnKeyDown += this.OnKeyPress;
		if (!this._playingReplay)
			FurballGame.InputManager.OnCharInput += this.Player.TypeCharacter; //TODO: make this use the input stack system

		double offset = OffsetManager.GetOffset(this.Song);

		if (offset != 0) {
			pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Info, $"Using local offset of {offset}");
			pTypingGame.MusicTrackTimeSource.Offset = offset;
		}

		if (!this._playingReplay)
			pTypingGame.OnlineManager.GameScene = this;
	}

	public override void Relayout(float newWidth, float newHeight) {
		base.Relayout(newWidth, newHeight);

		this._accuracyBar.Position = new Vector2(newWidth / 2f, newHeight);
		if (this._video != null)
			this._video.Position.X = newWidth / 2f;

		this._resumeButton.Position.X  = newWidth / 2f;
		this._restartButton.Position.X = newWidth / 2f;
		this._quitButton.Position.X    = newWidth / 2f;

		float horiScale = newWidth / FurballGame.DEFAULT_WINDOW_WIDTH;

		this.Player.Scale = new(horiScale);
	}

	public override ScreenUserActionType OnlineUserActionType => ScreenUserActionType.Playing;

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

	private void ResumeButtonClick(object sender, MouseButtonEventArgs mouseButtonEventArgs) {
		pTypingGame.MenuClickSound.PlayNew();
		pTypingGame.PauseResumeMusic();

		pTypingGame.OnlineManager.SpectatorResume(pTypingGame.MusicTrack.CurrentPosition);
	}

	private void RestartButtonClick(object sender, MouseButtonEventArgs mouseButtonEventArgs) {
		pTypingGame.MenuClickSound.PlayNew();
		pTypingGame.MusicTrack.CurrentPosition = 0;
		ScreenManager.ChangeScreen(new PlayerScreen(this._mods));
	}

	private void QuitButtonClick(object sender, MouseButtonEventArgs mouseButtonEventArgs) {
		pTypingGame.MenuClickSound.PlayNew();
		ScreenManager.ChangeScreen(new SongSelectionScreen(false));
	}

	private void SkipButtonClick(object sender, MouseButtonEventArgs mouseButtonEventArgs) {
		pTypingGame.MenuClickSound.PlayNew();
		pTypingGame.MusicTrack.CurrentPosition = this.Song.HitObjects.First().Time - 2999;
		this._video?.Seek(this.Song.HitObjects.First().Time - 2999);
	}

	public override void Dispose() {
		FurballGame.InputManager.OnKeyDown   -= this.OnKeyPress;
		FurballGame.InputManager.OnCharInput -= this.Player.TypeCharacter;

		this.Player.OnAllNotesComplete -= this.EndScore;
		this.Player.OnComboUpdate      -= this.OnComboUpdate;

		if (pTypingGame.OnlineManager.GameScene == this)
			pTypingGame.OnlineManager.GameScene = null;

		this._video?.Dispose();

		base.Dispose();
	}

	private double lastPress;
	private void OnKeyPress(object sender, KeyEventArgs e) {
		if (e.Key != Key.Minus && e.Key != Key.Equal)
			this.lastPress = FurballGame.Time;

		switch (e.Key) {
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

	private          double timeSinceLastBuffer;
	private          double timeSinceLastScoreSync;
	private readonly Mod[]  _mods;

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
							this._video?.Seek(f.Time);
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
								this._video?.Seek(currentTime - 2000);
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
					//TODO: reimplement spectating
					// pTypingGame.OnlineManager.SpectatorBuffer(currentTime);
					// if (this.Player.Score.ReplayFrames.Count != 0) {
					// this.Player.Score.ReplayFrames.ForEach(x => pTypingGame.OnlineManager.SpectatorReplayFrame(x.Time, x));
					// this.Player.Score.ReplayFrames.Clear();
					// }
				}
				else if (this.IsSpectating) {
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
			this._scoreDrawable.Text    = $"{this.Player.Score.AchievedScore:00000000}";
			this._accuracyDrawable.Text = $"{this.Player.Score.Accuracy * 100:0.00}%";
			this._comboDrawable.Text    = $"{this.Player.Score.CurrentCombo}x";
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

		if (this.Song.HitObjects.First().Time - currentTime > 3000)
			this._skipButton.Visible = true;
		else
			this._skipButton.Visible = false;

		#endregion

		if (this._playingReplay && this._playingScoreReplay.ReplayFrames.All(x => x.Used))
			this._playingReplay = false;

		if (this._playingReplay)
			for (int i = 0; i < this._playingScoreReplay.ReplayFrames.Count; i++) {
				ReplayFrame currentFrame = this._playingScoreReplay.ReplayFrames[i];

				if (currentTime > currentFrame.Time && !currentFrame.Used) {
					this.Player.TypeCharacter(this, currentFrame.Character);

					currentFrame.Used = true;
					break;
				}

				this._playingScoreReplay.ReplayFrames[i] = currentFrame;
			}

		base.Update(gameTime);
	}

	public void EndScore(object sender, EventArgs args) {
		if (!this._endScheduled) {
			FurballGame.GameTimeScheduler.ScheduleMethod(
				delegate {
					this.Player.CallMapEnd();

					this.Player.Score.Time = DateTime.Now;
					if (this._playingScoreReplay == null)
						pTypingGame.SubmitScore(this.Song, this.Player.Score);
					else
						this.Player.Score.User = this._playingScoreReplay.User;

					ScreenManager.ChangeScreen(new ScoreResultsScreen(this.Player.Score));
				},
				FurballGame.Time + 1500
			);

			this._endScheduled = true;
		}
	}

	public override string Name  => "Gameplay";
	public override string State => "Typing away!";
	public override string Details {
		get {
			BeatmapSet set = pTypingGame.CurrentSong.Value.Parent.First();
			return $@"Playing {set.Artist} - {set.Title} [{pTypingGame.CurrentSong.Value.Info.DifficultyName}]";
		}
	}
	public override bool           ForceSpeedReset      => false;
	public override float          BackgroundFadeAmount => -1f;
	public override MusicLoopState LoopState            => MusicLoopState.None;
	public override ScreenType     ScreenType           => ScreenType.Gameplay;
}
