using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Eto.Forms;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Engine.Engine.Helpers;
using Furball.Engine.Engine.Input.Events;
using Furball.Vixie;
using Furball.Vixie.Backends.Shared;
using Furball.Volpe.Evaluation;
using JetBrains.Annotations;
using ManagedBass;
using Newtonsoft.Json;
using pTyping.Engine;
using pTyping.Graphics.Drawables;
using pTyping.Graphics.Drawables.Events;
using pTyping.Graphics.Menus.SongSelect;
using pTyping.Graphics.OldEditor.Tools;
using pTyping.Graphics.Player;
using pTyping.Shared;
using pTyping.Shared.Beatmaps;
using pTyping.Shared.Beatmaps.Exporters;
using pTyping.Shared.Beatmaps.HitObjects;
using pTyping.Shared.Events;
using Silk.NET.Input;
using sowelipisona;
using Drawable = Furball.Engine.Engine.Graphics.Drawables.Drawable;
using KeyEventArgs = Furball.Engine.Engine.Input.Events.KeyEventArgs;

namespace pTyping.Graphics.OldEditor;

public class OldEditorScreen : pScreen {
	private TextDrawable _currentTimeDrawable;

	public  Texture             NoteTexture;
	private DrawableProgressBar _progressBar;
	private TexturedDrawable    _recepticle;

	private EditorDrawable EditorDrawable;

	public EditorTool       CurrentTool;
	public List<EditorTool> EditorTools;

	public EditorState EditorState;

	private readonly List<Drawable> _selectionRects = new List<Drawable>();

	public bool SaveNeeded;

	public long LastEscapeTime;

	public SoundEffectPlayer HitSoundNormal;
	public SoundEffectPlayer HitSoundMetronome1;
	public SoundEffectPlayer HitSoundMetronome2;

	public override void Initialize() {
		base.Initialize();

		this.RealmMap = pTypingGame.CurrentSong.Value;

		BeatmapSet clonedSet = this.RealmMap.ParentSet.Clone();

		this.EditorState = new EditorState(clonedSet.Beatmaps.First(x => x.Id == this.RealmMap.Id), clonedSet);

		pTypingGame.MusicTrack.Stop();

		#region Gameplay preview

		this.Manager.Add(
			this.EditorDrawable = new EditorDrawable {
				OriginType = OriginType.LeftCenter,
				Position   = new(0, FurballGame.DEFAULT_WINDOW_HEIGHT / 2f)
			}
		);

		this.NoteTexture = ContentManager.LoadTextureFromFileCached("note.png", ContentSource.User);

		this._recepticle = new TexturedDrawable(this.NoteTexture, Player.Player.RECEPTICLE_POS) {
			Scale       = new Vector2(0.55f),
			OriginType  = OriginType.Center,
			Clickable   = false,
			CoverClicks = false,
			Hoverable   = false,
			CoverHovers = false
		};

		this.EditorDrawable.Children.Add(this._recepticle);

		foreach (HitObject note in this.EditorState.Song.HitObjects)
			this.CreateNote(note);
		foreach (Event @event in this.EditorState.Song.Events)
			this.CreateEvent(@event);

		#endregion

		#region Playfield decorations

		this._playfieldBackgroundCover = new(ContentManager.LoadTextureFromFileCached("playfield-background.png", ContentSource.User), Vector2.Zero) {
			Depth       = -0.95f,
			Clickable   = false,
			CoverClicks = false
		};

		this.EditorDrawable.Children.Add(this._playfieldBackgroundCover);

		#region Create waveform

		Stream stream = pTypingGame.FileDatabase.GetFileStream(this.EditorState.Song.FileCollection.Audio!.Hash);

		this._audioWaveform = FurballGame.AudioEngine.GetWaveform(stream);

		stream.Close();

		this._waveformDrawable = new WaveformDrawable(new Vector2(Player.Player.NOTE_END_POS.X, this._playfieldBackgroundCover.Position.Y), this._audioWaveform, this._playfieldBackgroundCover.Size.Y) {
			Depth = -1.1f
		};

		this._waveformDrawable.StartCrop = 0;
		this._waveformDrawable.EndCrop   = 100;

		this.EditorDrawable.Children.Add(this._waveformDrawable);

		#endregion

		#region background image

		this.Manager.Add(pTypingGame.CurrentSongBackground);

		#endregion

		#region Video background

		if (pTypingConfig.Instance.VideoBackgrounds && this.EditorState.Song.FileCollection.BackgroundVideo != null)
			try {
				this._video = new VideoDrawable(
					pTypingGame.FileDatabase.GetFile(this.EditorState.Song.FileCollection.BackgroundVideo.Hash),
					1f,
					pTypingGame.MusicTrackTimeSource,
					new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, FurballGame.DEFAULT_WINDOW_HEIGHT / 2f)
				) {
					OriginType = OriginType.Center,
					Depth      = 1f
				};

				this._video.Scale = new Vector2(1f / ((float)this._video.Texture.Height / FurballGame.DEFAULT_WINDOW_HEIGHT));

				this._video.StartTime = 0;

				this.Manager.Add(this._video);
			}
			catch {
				pTypingConfig.Instance.VideoBackgrounds = false;

				this._video = null;

				pTypingGame.NotificationManager.CreateNotification(
					NotificationManager.NotificationImportance.Error,
					"Unable to load background video! Disabling video support..."
				);
			}

		#endregion

		#endregion

		#region Visualization drawables

		this.EditorState.SelectedObjects.CollectionChanged += this.UpdateSelectionRects;

		#endregion

		#region UI

		#region Progress bar

		this._progressBar = new DrawableProgressBar(
			new Vector2(0, 0),
			FurballGame.DefaultFont,
			(int)(40 * 0.9f),
			new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 200, 40),
			Color.Gray,
			Color.DarkGray,
			Color.White
		) {
			OriginType       = OriginType.BottomLeft,
			ScreenOriginType = OriginType.BottomLeft
		};

		this._progressBar.OnDrag    += this.ProgressBarOnInteract;
		this._progressBar.OnClick   += this.ProgressBarOnInteract;
		this._progressBar.OnClickUp += this.ProgressBarOnInteractUp;

		this.Manager.Add(this._progressBar);
		this._progressBar.RegisterForInput();

		#region Update timing points

		this.UpdateTimingPointDisplay();

		#endregion

		#endregion

		#region Current time

		this._currentTimeDrawable = new TextDrawable(new Vector2(10, FurballGame.DEFAULT_WINDOW_HEIGHT - 50), FurballGame.DefaultFont, "", 30) {
			OriginType = OriginType.BottomLeft
		};

		this.Manager.Add(this._currentTimeDrawable);

		#endregion

		#region Playback buttons

		Texture editorButtonsTexture2D = ContentManager.LoadTextureFromFileCached("editorbuttons.png", ContentSource.User);

		this._playButton = new(
			editorButtonsTexture2D,
			new Vector2(150, 0),
			TexturePositions.EDITOR_PLAY
		) {
			Scale            = new Vector2(0.5f, 0.5f),
			ScreenOriginType = OriginType.BottomRight,
			OriginType       = OriginType.BottomRight
		};
		this._pauseButton = new(
			editorButtonsTexture2D,
			new Vector2(100, 0),
			TexturePositions.EDITOR_PAUSE
		) {
			Scale            = new Vector2(0.5f, 0.5f),
			ScreenOriginType = OriginType.BottomRight,
			OriginType       = OriginType.BottomRight
		};
		this._rightButton = new(
			editorButtonsTexture2D,
			new Vector2(0, 0),
			TexturePositions.EDITOR_RIGHT
		) {
			Scale            = new Vector2(0.5f, 0.5f),
			ScreenOriginType = OriginType.BottomRight,
			OriginType       = OriginType.BottomRight
		};
		this._leftButton = new(
			editorButtonsTexture2D,
			new Vector2(50, 0),
			TexturePositions.EDITOR_LEFT
		) {
			Scale            = new Vector2(0.5f, 0.5f),
			OriginType       = OriginType.BottomRight,
			ScreenOriginType = OriginType.BottomRight
		};

		this._playButton.OnClick += delegate {
			pTypingGame.PlayMusic();
			ChangeSpeed(null, this._speedDropdown.SelectedItem);
		};

		this._pauseButton.OnClick += delegate {
			pTypingGame.PauseResumeMusic();
		};

		this._leftButton.OnClick += delegate {
			if (this.EditorState.Song.HitObjects.Count > 0) {
				pTypingGame.MusicTrack.CurrentPosition = this.EditorState.Song.HitObjects.First().Time;
				this._video?.Seek(this.EditorState.Song.HitObjects.First().Time);
				foreach (NoteDrawable note in this.EditorState.Notes.Where(note => note.Note.Time > this.EditorState.CurrentTime))
					note.EditorHitSoundQueued = true;
			}
		};

		this._rightButton.OnClick += delegate {
			if (this.EditorState.Song.HitObjects.Count > 0) {
				pTypingGame.MusicTrack.CurrentPosition = this.EditorState.Song.HitObjects.Last().Time;
				this._video?.Seek(this.EditorState.Song.HitObjects.Last().Time);

				foreach (NoteDrawable note in this.EditorState.Notes.Where(note => note.Note.Time > this.EditorState.CurrentTime))
					note.EditorHitSoundQueued = true;
			}
		};

		this.Manager.Add(this._playButton);
		this._playButton.RegisterForInput();
		this.Manager.Add(this._pauseButton);
		this._pauseButton.RegisterForInput();
		this.Manager.Add(this._rightButton);
		this._rightButton.RegisterForInput();
		this.Manager.Add(this._leftButton);
		this._leftButton.RegisterForInput();

		#endregion

		#region Tool selection

		this.EditorTools = EditorTool.GetAllTools();

		float y = 10;
		foreach (EditorTool tool in this.EditorTools) {
			DrawableTickbox tickboxDrawable = new DrawableTickbox(new Vector2(10, y), pTypingGame.JapaneseFont, 35, tool.Name, false, true) {
				ToolTip = tool.Tooltip
			};

			tickboxDrawable.OnClick += delegate {
				this.ChangeTool(tool.GetType());
			};

			tool.TickBoxDrawable = tickboxDrawable;

			this.Manager.Add(tickboxDrawable);

			y += tickboxDrawable.Size.Y + 10;
		}

		#endregion

		//Add the ui container that tools use 
		this.Manager.Add(this.EditorState.EditorToolUiContainer);

		#region Speed dropdown

		this._speedDropdown = new DrawableDropdown(
			new Vector2(10, 200),
			pTypingGame.JapaneseFont,
			20,
			new Vector2(100, 20),
			new Dictionary<object, string> {
				{ 0.25d, "0.25x" },
				{ 0.5d, "0.50x" },
				{ 0.75d, "0.75x" },
				{ 1d, "1.00x" }
			}
		);
		this._speedDropdown.SelectedItem.Value = this._speedDropdown.Items.First(x => (double)x.Key == 1d);

		this._speedDropdown.SelectedItem.OnChange += ChangeSpeed;

		this.Manager.Add(this._speedDropdown);

		#endregion

		ScrollableContainer scrollable = new ScrollableContainer(new Vector2(310, 500)) {
			InvisibleToInput = true
		};
		this._songForm = new DrawableForm($"Beatmap settings for {this.RealmMap.ParentSet.Artist} - {this.RealmMap.ParentSet.Title} [{this.RealmMap.Info.DifficultyName}]", scrollable);

		scrollable.Add(new EditorSongFormContents(this.EditorState.Song, this) {
			InvisibleToInput = true
		});

		this._songForm.OnTryClose += (_, _) => {
			this.Manager.Remove(this._songForm);
		};

		#endregion

		this.ChangeTool(typeof(SelectTool));

		FurballGame.InputManager.OnKeyDown     += this.OnKeyPress;
		FurballGame.InputManager.OnMouseScroll += this.OnMouseScroll;
		FurballGame.InputManager.OnMouseDown   += this.OnClick;
		FurballGame.InputManager.OnMouseMove   += this.OnMouseMove;
		FurballGame.InputManager.OnMouseDrag   += this.OnMouseDrag;

		this.HitSoundNormal     = FurballGame.AudioEngine.CreateSoundEffectPlayer(ContentManager.LoadRawAsset("hitsound.wav", ContentSource.User));
		this.HitSoundMetronome1 = FurballGame.AudioEngine.CreateSoundEffectPlayer(ContentManager.LoadRawAsset("metronome-1.wav", ContentSource.User));
		this.HitSoundMetronome2 = FurballGame.AudioEngine.CreateSoundEffectPlayer(ContentManager.LoadRawAsset("metronome-2.wav", ContentSource.User));

		pTypingConfig.Instance.MasterVolumeChanged += this.OnVolumeChange;
		// this.HitSoundNormal.Volume =  ConVars.Volume.Value.Value;

		FurballGame.Instance.FileDrop += this.OnFileDrop;
	}

	private void OnFileDrop(object sender, string[] paths) {
		//Make sure the user only dragged in one path
		if (paths.Length != 1)
			return;

		//Create an array of video extensions 
		string[] videoExtensions = { ".mp4", ".avi", ".mkv", ".webm" };

		//Create an array of image extensions
		string[] imageExtensions = { ".png", ".bmp", ".jpg", ".jpeg" };

		//Check if the first path is a video
		if (videoExtensions.Any(x => paths[0].EndsWith(x))) {
			byte[] videoBytes = File.ReadAllBytes(paths[0]);

			string md5 = CryptoHelper.GetMd5(videoBytes);

			pTypingGame.FileDatabase.AddFile(videoBytes).Wait();

			this.EditorState.Song.FileCollection.BackgroundVideo = new PathHashTuple(Path.GetFileName(paths[0]), md5);

			this.SaveNeeded = true;

			pTypingGame.NotificationManager.CreatePopup("Reload the map to see the video!");
		}

		if (imageExtensions.Any(x => paths[0].EndsWith(x))) {
			byte[] imageBytes = File.ReadAllBytes(paths[0]);

			string md5 = CryptoHelper.GetMd5(imageBytes);

			pTypingGame.FileDatabase.AddFile(imageBytes).Wait();

			this.EditorState.Song.FileCollection.Background = new PathHashTuple(Path.GetFileName(paths[0]), md5);

			this.SaveNeeded = true;

			pTypingGame.NotificationManager.CreatePopup("Reload the map to see the image!");
		}
	}

	private void ProgressBarOnInteract(object sender, MouseButtonEventArgs e) {
		this.ProgressBarOnInteract(sender, new MouseDragEventArgs(Vector2.Zero, Vector2.Zero, Vector2.Zero, e.Button, e.Mouse));
	}

	public override void Relayout(float newWidth, float newHeight) {
		base.Relayout(newWidth, newHeight);

		this._progressBar.BarSize = new Vector2(newWidth - 200, 40);

		if (this._video != null)
			this._video.Position = new Vector2(newWidth / 2f, newHeight / 2f);

		this.EditorDrawable.Scale = new(newWidth / FurballGame.DEFAULT_WINDOW_WIDTH);

		this.EditorDrawable.OverrideSize = this._playfieldBackgroundCover.Size;
	}

	private void ProgressBarOnInteractUp(object sender, MouseButtonEventArgs mouseButtonEventArgs) {
		this._video?.Seek(pTypingGame.MusicTrack.CurrentPosition);
	}

	public override ScreenUserActionType OnlineUserActionType       => ScreenUserActionType.Editing;
	public override int                  BackgroundBlurKernelRadius => 0;
	public override int                  BackgroundBlurPasses       => 0;

	private static void ChangeSpeed(object _, KeyValuePair<object, string> keyValuePair) {
		pTypingGame.MusicTrack.SetSpeed((double)keyValuePair.Key);
	}

	private void OnVolumeChange(object sender, double d) {
		// this.HitSoundNormal.Volume = f.Value;
	}

	private void ProgressBarOnInteract(object sender, MouseDragEventArgs e) {
		Vector2 adjustedPoint = e.Position - this._progressBar.Position - this._progressBar.LastCalculatedOrigin;

		double value = (double)adjustedPoint.X / this._progressBar.Size.X;

		double time = value * pTypingGame.MusicTrack.Length;

		pTypingGame.MusicTrack.CurrentPosition = time;

		if (pTypingGame.MusicTrack.PlaybackState == PlaybackState.Playing)
			foreach (NoteDrawable note in this.EditorState.Notes.Where(note => note.Note.Time > this.EditorState.CurrentTime))
				note.EditorHitSoundQueued = true;
	}

	public void UpdateSelectionRects(object _, NotifyCollectionChangedEventArgs __) {
		//Dont recalc selection rects if we shouldnt be, this is used to stop stuttering when selecting all notes
		if (this.CancelSelectionEvents)
			return;

		this._selectionRects.ForEach(x => this.EditorDrawable.Children.Remove(x));

		this._selectionRects.Clear();

		foreach (Drawable @object in this.EditorState.SelectedObjects) {
			RectanglePrimitiveDrawable rect = new RectanglePrimitiveDrawable {
				RectSize      = @object.Size + new Vector2(20f),
				Filled        = false,
				Thickness     = 2f,
				ColorOverride = Color.Gray,
				Clickable     = false,
				Hoverable     = false,
				CoverClicks   = false,
				CoverHovers   = false,
				OriginType    = OriginType.Center,
				TimeSource    = pTypingGame.MusicTrackTimeSource
			};

			@object.Tweens.ForEach(x => rect.Tweens.Add(x.Copy()));

			this._selectionRects.Add(rect);

			this.EditorDrawable.Children.Add(rect);
		}
	}

	private void OnMouseDrag(object sender, MouseDragEventArgs e) {
		this.CurrentTool?.OnMouseDrag(e.Position);
	}

	public void CreateEvent(Event @event, bool isNew = false) {
		Drawable                  eventDrawable = null;
		GameplayDrawableTweenArgs args          = new GameplayDrawableTweenArgs(this.CurrentApproachTime(@event.Start), true, true);
		switch (@event.Type) {
			case EventType.TypingCutoff:
				TypingCutoffEventDrawable drawable = new TypingCutoffEventDrawable(this.NoteTexture, @event);

				drawable.CreateTweens(args);

				eventDrawable = drawable;
				break;
			case EventType.Lyric:
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}

		if (eventDrawable == null) return;

		eventDrawable.RegisterForInput();
		this.EditorDrawable.Children.Add(eventDrawable);
		this.EditorState.Events.Add(eventDrawable);
		if (isNew) {
			this.EditorState.Song.Events.Add(@event);
			this.SaveNeeded = true;
		}

		this.CurrentTool?.OnEventCreate(eventDrawable, isNew);
	}

	public void CreateNote(HitObject note, bool isNew = false) {
		NoteDrawable noteDrawable = new NoteDrawable(new Vector2(Player.Player.NOTE_START_POS.X, Player.Player.NOTE_START_POS.Y), this.NoteTexture, pTypingGame.JapaneseFont, 50, null, PlayerStateArguments.DefaultPlayer) {
			TimeSource = pTypingGame.MusicTrackTimeSource,
			NoteTexture = {
				ColorOverride = note.Color
			},
			RawTextDrawable = {
				Text = $"{note.Text}"
			},
			Scale      = new Vector2(0.55f, 0.55f),
			OriginType = OriginType.Center,
			Note       = note,
			Depth      = -1f
		};

		noteDrawable.CreateTweens(new GameplayDrawableTweenArgs(this.CurrentApproachTime(note.Time), true, true));
		noteDrawable.RegisterForInput();

		this.EditorDrawable.Children.Add(noteDrawable);
		this.EditorState.Notes.Add(noteDrawable);
		if (isNew) {
			this.EditorState.Song.HitObjects.Add(note);
			this.SaveNeeded = true;
		}

		this.CurrentTool?.OnNoteCreate(noteDrawable, isNew);
	}

	public void ChangeTool(Type type) {
		EditorTool newTool = this.EditorTools.First(x => x.GetType() == type);

		if (newTool == this.CurrentTool) return;

		foreach (EditorTool tool in this.EditorTools)
			tool.TickBoxDrawable.Selected.Value = tool == newTool;

		this.CurrentTool?.DeselectTool(this);

		this.CurrentTool = newTool;

		newTool?.SelectTool(this, ref this.EditorDrawable);
	}

	private void OnMouseMove(object sender, MouseMoveEventArgs e) {
		double currentTime  = this.EditorState.CurrentTime;
		double reticuleXPos = this._recepticle.RealPosition.X + this._recepticle.RealSize.X / 2f;
		double noteStartPos = (FurballGame.DEFAULT_WINDOW_WIDTH + 100) * this.EditorDrawable.Scale.X;

		double speed = (noteStartPos - reticuleXPos) / this.CurrentApproachTime(currentTime);

		double relativeMousePosition = (e.Position.X - reticuleXPos) * 0.925;

		double timeAtCursor = relativeMousePosition / speed + currentTime;

		TimingPoint timingPoint = this.EditorState.Song.CurrentTimingPoint(timeAtCursor);

		double noteLength = this.EditorState.Song.DividedNoteLength(timeAtCursor);

		timeAtCursor += noteLength / 2d;

		double roundedTime = timeAtCursor - (timeAtCursor - timingPoint.Time) % noteLength;

		this.EditorState.MouseTime = roundedTime;

		this.CurrentTool?.OnMouseMove(e.Position);
	}

	private void OnClick(object sender, MouseButtonEventArgs e) {
		this.CurrentTool?.OnMouseClick((e.Button, e.Mouse.Position));
	}

	[Pure]
	public bool InPlayfield(Vector2 pos) {
		return this._playfieldBackgroundCover.RealContains(pos);
	}

	public override void Dispose() {
		FurballGame.InputManager.OnKeyDown     -= this.OnKeyPress;
		FurballGame.InputManager.OnMouseScroll -= this.OnMouseScroll;
		FurballGame.InputManager.OnMouseDown   -= this.OnClick;
		FurballGame.InputManager.OnMouseMove   -= this.OnMouseMove;
		FurballGame.InputManager.OnMouseDrag   -= this.OnMouseDrag;

		this.EditorState.SelectedObjects.CollectionChanged -= this.UpdateSelectionRects;

		this._progressBar.OnDrag -= this.ProgressBarOnInteract;

		pTypingConfig.Instance.MasterVolumeChanged -= this.OnVolumeChange;

		FurballGame.Instance.FileDrop -= this.OnFileDrop;

		this._video?.Dispose();

		base.Dispose();
	}

	private void OnMouseScroll(object sender, MouseScrollEventArgs e) {
		this.TimelineMove(e.ScrollAmount.Y <= 0);
	}

	public void TimelineMove(bool right) {
		double currentTime = pTypingGame.MusicTrackTimeSource.GetCurrentTime();

		TimingPoint currentTimingPoint = this.EditorState.Song.CurrentTimingPoint(currentTime);

		double noteLength   = this.EditorState.Song.DividedNoteLength(currentTime);
		double timeToSeekTo = Math.Round((currentTime - currentTimingPoint.Time) / noteLength) * noteLength;

		timeToSeekTo += currentTimingPoint.Time;

		timeToSeekTo += right ? noteLength : -noteLength;

		pTypingGame.MusicTrack.CurrentPosition = Math.Max(timeToSeekTo, 0);
		this._video?.Seek(pTypingGame.MusicTrack.CurrentPosition);

		if (pTypingGame.MusicTrack.PlaybackState == PlaybackState.Playing)
			foreach (NoteDrawable note in this.EditorState.Notes.Where(note => note.Note.Time > this.EditorState.CurrentTime))
				note.EditorHitSoundQueued = true;
	}

	public void DeleteSelectedObjects() {
		foreach (Drawable @object in this.EditorState.SelectedObjects) {
			if (@object is NoteDrawable note) {
				this.EditorDrawable.Children.Remove(note);
				this.EditorState.Song.HitObjects.Remove(note.Note);
				this.EditorState.Notes.Remove(note);

				this.CurrentTool?.OnNoteDelete(note);
			}
			else if (@object is BeatLineBarEventDrawable barEvent) {
				this.EditorDrawable.Children!.Remove(barEvent);
				this.EditorState.Song.Events.Remove(barEvent.Event);
				this.EditorState.Events.Remove(barEvent);

				this.CurrentTool?.OnEventDelete(barEvent);
			}
			else if (@object is BeatLineBeatEventDrawable beatEvent) {
				this.EditorDrawable.Children!.Remove(beatEvent);
				this.EditorState.Song.Events.Remove(beatEvent.Event);
				this.EditorState.Events.Remove(beatEvent);

				this.CurrentTool?.OnEventDelete(beatEvent);
			}
			else if (@object is TypingCutoffEventDrawable cutoffEvent) {
				this.EditorDrawable.Children!.Remove(cutoffEvent);
				this.EditorState.Song.Events.Remove(cutoffEvent.Event);
				this.EditorState.Events.Remove(cutoffEvent);

				this.CurrentTool?.OnEventDelete(cutoffEvent);
			}
			else if (@object is LyricEventDrawable lyricEvent) {
				this.EditorDrawable.Children!.Remove(lyricEvent);
				this.EditorState.Song.Events.Remove(lyricEvent.Event);
				this.EditorState.Events.Remove(lyricEvent);

				this.CurrentTool?.OnEventDelete(lyricEvent);
			}
			@object.Dispose();
		}

		this.EditorState.SelectedObjects.Clear();
		this.SaveNeeded = true;
	}

	private void OnKeyPress(object sender, KeyEventArgs e) {
		this.CurrentTool?.OnKeyPress(e.Key);

		switch (e.Key) {
			case Key.Space:
				this.ToggleMusicPlay();
				break;
			case Key.Left: {
				this.TimelineMove(false);

				break;
			}
			case Key.Right: {
				this.TimelineMove(true);

				break;
			}
			case Key.M: {
				this.ToggleMetronome();

				break;
			}
			case Key.F5: {
				if (this.Manager.Drawables.Contains(this._songForm))
					break;

				this.Manager.Add(this._songForm);

				break;
			}
			case Key.Escape:
				//If the user has some notes selected, clear them
				if (this.EditorState.SelectedObjects.Count != 0) {
					this.EditorState.SelectedObjects.Clear();
					return;
				}

				//The current time
				long unixTime = UnixTime.Now();
				//Have 5 seconds elapsed between now and the last press of the button?
				if (unixTime - this.LastEscapeTime > 5) {
					this.LastEscapeTime = unixTime;
					return;
				}

				if (this.SaveNeeded)
					EtoHelper.MessageDialog(
						(o, responseType) => {
							FurballGame.GameTimeScheduler.ScheduleMethod(
								_ => {
									switch (responseType) {
										case DialogResult.Cancel:
											return;
										case DialogResult.No:
											pTypingGame.MenuClickSound.PlayNew();

											// Exit the editor
											ScreenManager.ChangeScreen(new SongSelectionScreen(true));
											break;
										case DialogResult.Yes: {
											this.Save();
											ScreenManager.ChangeScreen(new SongSelectionScreen(true));
											break;
										}
									}
								}
							);
						},
						"Do you want to save before quitting?",
						MessageBoxButtons.YesNoCancel
					);
				else //If we dont have anything to save, then just exit back to the song select
					ScreenManager.ChangeScreen(new SongSelectionScreen(true));
				break;
			case Key.Delete: {
				// Delete the selected objects
				this.DeleteSelectedObjects();
				break;
			}
			case Key.E when FurballGame.InputManager.ControlHeld: {
				if (this.SaveNeeded) {
					pTypingGame.NotificationManager.CreatePopup("You have to save first!");
					return;
				}

				(double StartTime, string BeatmapId) taskState = (StartTime: FurballGame.Time, BeatmapId: this.EditorState.Song.Id);

				pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Info, "Exporting map!");

				//Start a new task to export the beatmap
				Task.Factory.StartNew(stateObj => {
					(double StartTime, string BeatmapId) state = ((double StartTime, string BeatmapId))stateObj;

					//Create the exporter
					pTypingBeatmapExporter exporter = new pTypingBeatmapExporter();

					//Create an instance of the realm for this thread
					BeatmapDatabase beatmapDatabase = new BeatmapDatabase(FurballGame.DataFolder);

					//Get the path of the export dir
					string exportPath = Path.Combine(FurballGame.DataFolder, "exports");

					//Create the export dir if it does not exist
					if (!Directory.Exists(exportPath))
						Directory.CreateDirectory(exportPath);

					//Get the beatmap set from the database
					BeatmapSet set = beatmapDatabase.Realm.Find<Beatmap>(state.BeatmapId).Parent.First().Clone();

					//Open the write stream to the file we will export the zip to
					using FileStream fileStream = File.OpenWrite(Path.Combine(exportPath, $"{set.Artist} - {set.Title}.ptm"));

					//Export to the zip file
					exporter.ExportBeatmapSet(set, fileStream, pTypingGame.FileDatabase);

					FurballGame.GameTimeScheduler.ScheduleMethod(_ => {
						pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Info, $"Finished exporting map! Took {FurballGame.Time - state.StartTime:N2}ms");
					});
				}, taskState);

				break;
			}
			case Key.S when FurballGame.InputManager.ControlHeld: {
				// List<Event> lyrics = this.EditorState.Song.Events.Where(x => x is Event).Cast<Event>().ToList();
				// lyrics.Sort((x, y) => (int)(x.Time - y.Time));
				// for (int i = 1; i < lyrics.Count; i++) {
				//     Event editorStateEvent     = lyrics[i];
				//     Event lastEditorStateEvent = lyrics[i - 1];
				//
				//     lastEditorStateEvent.EndTime = editorStateEvent.Time;
				// }
				//
				// // Save the song if ctrl+s is pressed
				// // SongManager.PTYPING_SONG_HANDLER.SaveSong(this.EditorState.Song);
				// // pTypingGame.CurrentSong.Value = this.EditorState.Song;
				// // SongManager.UpdateSongs();
				//
				// this.SaveNeeded = false;
				// break;

				this.Save();
				break;
			}
			case Key.C when FurballGame.InputManager.ControlHeld: {
				if (this.EditorState.SelectedObjects.Count == 0) return;

				List<NoteDrawable> sortedNotes = new List<NoteDrawable>();
				foreach (Drawable @object in this.EditorState.SelectedObjects)
					if (@object is NoteDrawable note)
						sortedNotes.Add(note);
				sortedNotes = sortedNotes.OrderBy(x => x.Note.Time).ToList();

				if (sortedNotes.Count == 0) return;

				double startTime = sortedNotes.First().Note.Time;

				List<HitObject> notes = new List<HitObject>();

				foreach (NoteDrawable drawable in sortedNotes) {
					HitObject note = drawable.Note.Copy();
					note.Time = drawable.Note.Time - startTime;

					notes.Add(note);
				}

				e.Keyboard.SetClipboard(JsonConvert.SerializeObject(notes));
				
				break;
			}
			case Key.V when FurballGame.InputManager.ControlHeld: {
				try {
					List<HitObject> notes = JsonConvert.DeserializeObject<List<HitObject>>(e.Keyboard.GetClipboard());

					foreach (HitObject note in notes) {
						note.Time += this.EditorState.CurrentTime;

						this.CreateNote(note, true);
					}
				}
				catch {
					pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Error, "Pasting of notes failed!");
				}

				break;
			}
			case Key.A when FurballGame.InputManager.ControlHeld: {
				this.CancelSelectionEvents = true;
				this.EditorState.SelectedObjects.Clear();
				foreach (NoteDrawable note in this.EditorState.Notes)
					this.EditorState.SelectedObjects.Add(note);
				this.CancelSelectionEvents = false;
				this.UpdateSelectionRects(null, null);

				break;
			}
		}
	}
	private void ToggleMetronome() {
		this._metronome = !this._metronome;
	}

	private readonly List<Drawable> _timingPoints = new List<Drawable>();

	public void UpdateTimingPointDisplay() {
		this._timingPoints.ForEach(x => this.Manager.Remove(x));
		this._timingPoints.Clear();

		float startX = this._progressBar.Position.X;
		float length = this._progressBar.BarSize.X;

		foreach (TimingPoint timingPoint in this.EditorState.Song.TimingPoints) {
			float x = (float)(timingPoint.Time / pTypingGame.MusicTrack.Length * length + startX);

			TexturedDrawable drawable = new TexturedDrawable(FurballGame.WhitePixel, new Vector2(x, FurballGame.DEFAULT_WINDOW_HEIGHT)) {
				Scale         = new Vector2(3, this._progressBar.BarSize.Y + 10),
				OriginType    = OriginType.BottomCenter,
				ColorOverride = new Color(50, 200, 50, 100),
				ToolTip       = $@"BPM:{60000d / timingPoint.Tempo:#.##}"
			};
			drawable.RegisterForInput();

			this._timingPoints.Add(drawable);
			this.Manager.Add(drawable);
		}
	}

	public void Save() {
		Guid       id     = this.EditorState.Set.Id;
		BeatmapSet toSave = this.EditorState.Set.Clone();

		new Thread(_ => {
			BeatmapDatabase database = new BeatmapDatabase(FurballGame.DataFolder);

			BeatmapSet beatmap = database.Realm.Find<BeatmapSet>(id);

			database.Realm.Write(() => {
				toSave.CopyInto(beatmap);
			});

			database.Realm.Refresh();

			FurballGame.GameTimeScheduler.ScheduleMethod(_ => {
				// pTypingGame.BeatmapDatabase.Realm.Refresh();
				this.SaveNeeded = false;
			});
		}).Start();
	}

	// public double CurrentApproachTime(double time) => ConVars.BASE_APPROACH_TIME / this.EditorState.Song.CurrentTimingPoint(time).ApproachMultiplier;
	public double CurrentApproachTime(double time) {
		return ConVars.BASE_APPROACH_TIME;
		//TODO: support per note approach times
	}

	private double           _timeAtLastUpdate;
	private DrawableDropdown _speedDropdown;
	[CanBeNull]
	private VideoDrawable _video;
	private TexturedDrawable _playButton;
	private TexturedDrawable _pauseButton;
	private TexturedDrawable _rightButton;
	private TexturedDrawable _leftButton;
	private TexturedDrawable _playfieldBackgroundCover;
	private DrawableForm     _songForm;
	private bool             CancelSelectionEvents;
	public  Beatmap          RealmMap;
	private bool             _metronome;
	private bool             _metronomeFlipFlop;
	private Waveform         _audioWaveform;
	private WaveformDrawable _waveformDrawable;
	public override void Update(double _) {
		this.EditorState.CurrentTime = pTypingGame.MusicTrackTimeSource.GetCurrentTime();

		//Get the current timing point
		TimingPoint timingPoint = this.EditorState.Song.CurrentTimingPoint(this.EditorState.CurrentTime);

		//Get the time since the timing point started
		double timeSinceTimingPoint = this.EditorState.CurrentTime - timingPoint.Time;

		//Get the time since the timing point started from the point of view of the last frame
		double timeSinceTimingPointLastFrame = this._timeAtLastUpdate - timingPoint.Time;

		//Check how many beats have passed since the last timing point
		double beatsSinceLastTimingPoint = Math.Floor(timeSinceTimingPoint / timingPoint.Tempo);

		//Check how many beats have passed since the last timing point from the point of view of the last frame
		double beatsSinceLastTimingPointLast = Math.Floor(timeSinceTimingPointLastFrame / timingPoint.Tempo);

		//If we have passed a beat since the last frame, play a metronome sound
		if (this._metronome && beatsSinceLastTimingPoint > beatsSinceLastTimingPointLast) {
			//Flip between metronome 1 and 2 every beat
			this._metronomeFlipFlop = !this._metronomeFlipFlop;

			//Play the metronome sound
			if (this._metronomeFlipFlop)
				this.HitSoundMetronome1.PlayNew();
			else
				this.HitSoundMetronome2.PlayNew();
		}

		#region update waveform

		float travelDistance = Player.Player.NOTE_START_POS.X - Player.Player.RECEPTICLE_POS.X;
		float travelRatio    = (float)(this.CurrentApproachTime(this.EditorState.CurrentTime) / travelDistance);

		float afterTravelTime = (Player.Player.RECEPTICLE_POS.X - Player.Player.NOTE_END_POS.X) * travelRatio;

		this._waveformDrawable.StartCrop = this.EditorState.CurrentTime - afterTravelTime;
		this._waveformDrawable.EndCrop   = this.EditorState.CurrentTime + ConVars.BASE_APPROACH_TIME;

		//How far the notes have to travel in gamefield pixels
		float distanceFromEndToStart = Player.Player.NOTE_START_POS.X - Player.Player.NOTE_END_POS.X;

		//Scale the waveform to fit the distance
		this._waveformDrawable.Scale = new Vector2((float)(distanceFromEndToStart / (this._waveformDrawable.EndCrop - this._waveformDrawable.StartCrop)), 1);

		#endregion

		if (!this.EditorState.CurrentTime.Equals(this._timeAtLastUpdate)) {
			this.CurrentTool?.OnTimeChange(this.EditorState.CurrentTime);

			foreach (NoteDrawable note in this.EditorState.Notes) {
				note.Visible = this.EditorState.CurrentTime > note.Note.Time - this.CurrentApproachTime(note.Note.Time) &&
							   this.EditorState.CurrentTime < note.Note.Time + 1000;

				if (note.EditorHitSoundQueued && note.Note.Time < this.EditorState.CurrentTime) {
					if (!this._progressBar.IsClicked) this.HitSoundNormal.PlayNew();
					note.EditorHitSoundQueued = false;
				}
			}
			foreach (Drawable managedDrawable in this.EditorState.Events) {
				double time = managedDrawable switch {
					TypingCutoffEventDrawable cutoff       => cutoff.Event.Start,
					BeatLineBarEventDrawable beatLineBar   => beatLineBar.Event.Start,
					BeatLineBeatEventDrawable beatLineBeat => beatLineBeat.Event.Start,
					LyricEventDrawable lyric               => lyric.Event.Start,
					_                                      => 0
				};

				managedDrawable.Visible = this.EditorState.CurrentTime > time - 2000 && this.EditorState.CurrentTime < time + 1000;
			}
		}

		int milliseconds = (int)Math.Floor(this.EditorState.CurrentTime         % 1000d);
		int seconds      = (int)Math.Floor(this.EditorState.CurrentTime / 1000d % 60d);
		int minutes      = (int)Math.Floor(this.EditorState.CurrentTime         / 1000d / 60d);

		this._currentTimeDrawable.Text = $"Time: {minutes:00}:{seconds:00}:{milliseconds:000}";
		this._progressBar.Progress     = (float)this.EditorState.CurrentTime / (float)pTypingGame.MusicTrack.Length;

		this._timeAtLastUpdate = this.EditorState.CurrentTime;

		base.Update(_);
	}

	public void ToggleMusicPlay() {
		pTypingGame.PauseResumeMusic();

		switch (pTypingGame.MusicTrack.PlaybackState) {
			case PlaybackState.Playing: {
				this.EditorState.Notes.ForEach(x => x.EditorHitSoundQueued = false);

				foreach (NoteDrawable note in this.EditorState.Notes.Where(note => note.Note.Time > this.EditorState.CurrentTime))
					note.EditorHitSoundQueued = true;

				break;
			}
			case PlaybackState.Paused: {
				this.EditorState.Notes.ForEach(x => x.EditorHitSoundQueued = false);

				break;
			}
		}
	}

	public override string Name  => "Editor";
	public override string State => "Editing a map!";
	public override string Details {
		get {
			BeatmapSet set = pTypingGame.CurrentSong.Value.Parent.First();
			return pTypingConfig.Instance.Username == pTypingGame.CurrentSong.Value.Info.Mapper.Username
				? $"Editing {set.Artist} - {set.Title} [{pTypingGame.CurrentSong.Value.Difficulty}] by {pTypingGame.CurrentSong.Value.Info.Mapper}"
				: $"Modding {set.Artist} - {set.Title} [{pTypingGame.CurrentSong.Value.Difficulty}] by {pTypingGame.CurrentSong.Value.Info.Mapper}";
		}
	}

	public override bool           ForceSpeedReset      => true;
	public override float          BackgroundFadeAmount => 0.3f;
	public override MusicLoopState LoopState            => MusicLoopState.None;
	public override ScreenType     ScreenType           => ScreenType.Menu;
}
