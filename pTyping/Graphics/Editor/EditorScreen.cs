using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using Eto.Forms;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Engine.Engine.Helpers;
using Furball.Vixie;
using Furball.Volpe.Evaluation;
using JetBrains.Annotations;
using ManagedBass;
using Newtonsoft.Json;
using pTyping.Engine;
using pTyping.Graphics.Drawables;
using pTyping.Graphics.Drawables.Events;
using pTyping.Graphics.Editor.Tools;
using pTyping.Graphics.Menus.SongSelect;
using pTyping.Graphics.Player;
using pTyping.Songs;
using pTyping.Songs.Events;
using Silk.NET.Input;
using sowelipisona;
using Color=Furball.Vixie.Backends.Shared.Color;
using Drawable=Furball.Engine.Engine.Graphics.Drawables.Drawable;

namespace pTyping.Graphics.Editor;

public class EditorScreen : pScreen {
    private TextDrawable _currentTimeDrawable;

    public  Texture             NoteTexture;
    private DrawableProgressBar _progressBar;
    private TexturedDrawable    _recepticle;

    private EditorDrawable EditorDrawable;

    public EditorTool       CurrentTool;
    public List<EditorTool> EditorTools;

    public EditorState EditorState;

    private readonly List<Drawable> _selectionRects = new();

    public bool SaveNeeded = false;

    public long LastEscapeTime = 0;

    public SoundEffectPlayer HitSoundNormal = null;

    public override void Initialize() {
        base.Initialize();

        this.EditorState = new EditorState(SongManager.LoadFullSong(pTypingGame.CurrentSong.Value));

        pTypingGame.MusicTrack.Stop();

        #region Gameplay preview

        this.Manager.Add(
        this.EditorDrawable = new EditorDrawable {
            OriginType = OriginType.LeftCenter,
            Position   = new(0, FurballGame.DEFAULT_WINDOW_HEIGHT / 2f)
        }
        );
        
        FileInfo[] noteFiles = new DirectoryInfo(this.EditorState.Song.QualifiedFolderPath).GetFiles("note.png");

        this.NoteTexture = noteFiles == null || noteFiles.Length == 0 ? ContentManager.LoadTextureFromFileCached("note.png", ContentSource.User)
                               : ContentManager.LoadTextureFromFileCached(noteFiles[0].FullName,                             ContentSource.External);

        this._recepticle = new TexturedDrawable(this.NoteTexture, Player.Player.RECEPTICLE_POS) {
            Scale       = new Vector2(0.55f),
            OriginType  = OriginType.Center,
            Clickable   = false,
            CoverClicks = false,
            Hoverable   = false,
            CoverHovers = false
        };

        this.EditorDrawable.Drawables.Add(this._recepticle);

        foreach (Note note in this.EditorState.Song.Notes)
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

        this.EditorDrawable.Drawables.Add(this._playfieldBackgroundCover);

        #region background image

        this.Manager.Add(pTypingGame.CurrentSongBackground);
        
        #endregion

        #region Video background

        if (pTypingConfig.Instance.VideoBackgrounds && this.EditorState.Song.VideoPath != null)
            try {
                this._video = new VideoDrawable(
                this.EditorState.Song.QualifiedVideoPath,
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
        FurballGame.DEFAULT_FONT,
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

        #region Update timing points

        this.UpdateTimingPointDisplay();

        #endregion

        #endregion

        #region Current time

        this._currentTimeDrawable = new TextDrawable(new Vector2(10, FurballGame.DEFAULT_WINDOW_HEIGHT - 50), FurballGame.DEFAULT_FONT, "", 30) {
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
            if (this.EditorState.Song.Notes.Count > 0) {
                pTypingGame.MusicTrack.CurrentPosition = this.EditorState.Song.Notes.First().Time;
                this._video?.Seek(this.EditorState.Song.Notes.First().Time);
                foreach (NoteDrawable note in this.EditorState.Notes.Where(note => note.Note.Time > this.EditorState.CurrentTime))
                    note.EditorHitSoundQueued = true;
            }
        };

        this._rightButton.OnClick += delegate {
            if (this.EditorState.Song.Notes.Count > 0) {
                pTypingGame.MusicTrack.CurrentPosition = this.EditorState.Song.Notes.Last().Time;
                this._video?.Seek(this.EditorState.Song.Notes.Last().Time);

                foreach (NoteDrawable note in this.EditorState.Notes.Where(note => note.Note.Time > this.EditorState.CurrentTime))
                    note.EditorHitSoundQueued = true;
            }
        };

        this.Manager.Add(this._playButton);
        this.Manager.Add(this._pauseButton);
        this.Manager.Add(this._rightButton);
        this.Manager.Add(this._leftButton);

        #endregion

        #region Tool selection

        this.EditorTools = EditorTool.GetAllTools();

        float y = 10;
        foreach (EditorTool tool in this.EditorTools) {
            DrawableTickbox tickboxDrawable = new(new Vector2(10, y), pTypingGame.JapaneseFontStroked, 35, tool.Name, false, true) {
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
        new Dictionary<object, string>
        {
            {0.25d, "0.25x"},
            {0.5d, "0.50x"},
            {0.75d, "0.75x"},
            {1d, "1.00x"},
        }
        );
        this._speedDropdown.SelectedItem.Value = this._speedDropdown.Items.First(x => (double)x.Key == 1d);

        this._speedDropdown.SelectedItem.OnChange += ChangeSpeed;

        this.Manager.Add(this._speedDropdown);

        #endregion

        #endregion

        this.ChangeTool(typeof(SelectTool));

        FurballGame.InputManager.OnKeyDown     += this.OnKeyPress;
        FurballGame.InputManager.OnMouseScroll += this.OnMouseScroll;
        FurballGame.InputManager.OnMouseDown   += this.OnClick;
        FurballGame.InputManager.OnMouseMove   += this.OnMouseMove;
        FurballGame.InputManager.OnMouseDrag   += this.OnMouseDrag;

        this.HitSoundNormal = FurballGame.AudioEngine.CreateSoundEffectPlayer(ContentManager.LoadRawAsset("hitsound.wav", ContentSource.User));

        ConVars.Volume.OnChange    += this.OnVolumeChange;
        this.HitSoundNormal.Volume =  ConVars.Volume.Value.Value;
    }

    public override void Relayout(float newWidth, float newHeight) {
        base.Relayout(newWidth, newHeight);

        this._progressBar.BarSize = new Vector2(newWidth - 200, 40);

        if (this._video != null)
            this._video.Position = new Vector2(newWidth / 2f, newHeight / 2f);

        this.EditorDrawable.Scale = new(newWidth / FurballGame.DEFAULT_WINDOW_WIDTH);

        this.EditorDrawable.OverrideSize = this._playfieldBackgroundCover.Size;
    }

    private void ProgressBarOnInteractUp(object sender, (MouseButton button, Point pos) e) {
        this._video?.Seek(pTypingGame.MusicTrack.CurrentPosition);
    }

    public override ScreenUserActionType OnlineUserActionType => ScreenUserActionType.Editing;

    private void ProgressBarOnInteract(object sender, Point e) {
        this.ProgressBarOnInteract(sender, (MouseButton.Left, e));
    }

    // private void ProgressBarOnInteract(object sender, Point e) {
    //     this.ProgressBarOnInteract(sender, (e, MouseButton.LeftButton));
    // }

    private const string x025 = "x0.25";
    private const string x050 = "x0.50";
    private const string x075 = "x0.75";
    private const string x100 = "x1.00";
    private static void ChangeSpeed(object _, KeyValuePair<object, string> keyValuePair) {
        pTypingGame.MusicTrack.SetSpeed((double)keyValuePair.Key);
    }

    private void OnVolumeChange(object sender, Value.Number f) {
        this.HitSoundNormal.Volume = f.Value;
    }

    private void ProgressBarOnInteract(object sender, (MouseButton button, Point pos) e) {
        Vector2 adjustedPoint = e.pos.ToVector2() - this._progressBar.Position - this._progressBar.LastCalculatedOrigin;

        double value = (double)adjustedPoint.X / this._progressBar.Size.X;

        double time = value * pTypingGame.MusicTrack.Length;

        pTypingGame.MusicTrack.CurrentPosition = time;

        if (pTypingGame.MusicTrack.PlaybackState == PlaybackState.Playing)
            foreach (NoteDrawable note in this.EditorState.Notes.Where(note => note.Note.Time > this.EditorState.CurrentTime))
                note.EditorHitSoundQueued = true;
    }

    public void UpdateSelectionRects(object _, NotifyCollectionChangedEventArgs __) {
        this._selectionRects.ForEach(x => this.EditorDrawable.Drawables.Remove(x));

        this._selectionRects.Clear();

        foreach (Drawable @object in this.EditorState.SelectedObjects) {
            RectanglePrimitiveDrawable rect = new() {
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

            this.EditorDrawable.Drawables.Add(rect);
        }
    }

    private void OnMouseDrag(object sender, ((Vector2 lastPosition, Vector2 newPosition), string cursorName) e) {
        this.CurrentTool?.OnMouseDrag(e.Item1.newPosition);
    }

    public void CreateEvent(Event @event, bool isNew = false) {
        Drawable eventDrawable = Event.CreateEventDrawable(@event, this.NoteTexture, new GameplayDrawableTweenArgs(this.CurrentApproachTime(@event.Time), true, true));

        if (eventDrawable == null) return;

        this.EditorDrawable.Drawables.Add(eventDrawable);
        this.EditorState.Events.Add(eventDrawable);
        if (isNew) {
            this.EditorState.Song.Events.Add(@event);
            this.SaveNeeded = true;
        }

        this.CurrentTool?.OnEventCreate(eventDrawable, isNew);
    }

    public void CreateNote(Note note, bool isNew = false) {
        NoteDrawable noteDrawable = new(
        new Vector2(Player.Player.NOTE_START_POS.X, Player.Player.NOTE_START_POS.Y + note.YOffset),
        this.NoteTexture,
        pTypingGame.JapaneseFont,
        50
        ) {
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

        this.EditorDrawable.Drawables.Add(noteDrawable);
        this.EditorState.Notes.Add(noteDrawable);
        if (isNew) {
            this.EditorState.Song.Notes.Add(note);
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

    private void OnMouseMove(object sender, (Vector2 position, string cursorName) e) {
        double currentTime  = this.EditorState.CurrentTime;
        double reticuleXPos = this._recepticle.RealPosition.X + this._recepticle.RealSize.X / 2f;
        double noteStartPos = (FurballGame.DEFAULT_WINDOW_WIDTH + 100) * this.EditorDrawable.Scale.X;

        double speed = (noteStartPos - reticuleXPos) / this.CurrentApproachTime(currentTime);

        double relativeMousePosition = (e.position.X - reticuleXPos) * 0.925;

        double timeAtCursor = relativeMousePosition / speed + currentTime;

        TimingPoint timingPoint = this.EditorState.Song.CurrentTimingPoint(timeAtCursor);

        double noteLength = this.EditorState.Song.DividedNoteLength(timeAtCursor);

        timeAtCursor += noteLength / 2d;

        double roundedTime = timeAtCursor - (timeAtCursor - timingPoint.Time) % noteLength;

        this.EditorState.MouseTime = roundedTime;

        this.CurrentTool?.OnMouseMove(e.position);
    }

    private void OnClick(object sender, ((MouseButton mouseButton, Vector2 position) args, string cursorName) e) {
        this.CurrentTool?.OnMouseClick(e.args);
    }

    [Pure]
    public bool InPlayfield(Vector2 pos) => this._playfieldBackgroundCover.RealContains(pos);

    public override void Dispose() {
        FurballGame.InputManager.OnKeyDown     -= this.OnKeyPress;
        FurballGame.InputManager.OnMouseScroll -= this.OnMouseScroll;
        FurballGame.InputManager.OnMouseDown   -= this.OnClick;
        FurballGame.InputManager.OnMouseMove   -= this.OnMouseMove;
        FurballGame.InputManager.OnMouseDrag   -= this.OnMouseDrag;

        this.EditorState.SelectedObjects.CollectionChanged -= this.UpdateSelectionRects;

        this._progressBar.OnDrag -= this.ProgressBarOnInteract;

        ConVars.Volume.OnChange -= this.OnVolumeChange;

        this._video?.Dispose();

        base.Dispose();
    }

    private void OnMouseScroll(object sender, ((int scrollWheelId, float scrollAmount) scroll, string cursorName) args) {
        this.TimelineMove(args.scroll.scrollAmount <= 0);
    }

    public void TimelineMove(bool right) {
        double currentTime = pTypingGame.MusicTrackTimeSource.GetCurrentTime();

        double noteLength   = this.EditorState.Song.DividedNoteLength(currentTime);
        double timeToSeekTo = Math.Round((currentTime - this.EditorState.Song.CurrentTimingPoint(currentTime).Time) / noteLength) * noteLength;

        timeToSeekTo += this.EditorState.Song.CurrentTimingPoint(currentTime).Time;

        if (right)
            timeToSeekTo += noteLength;
        else
            timeToSeekTo -= noteLength;

        pTypingGame.MusicTrack.CurrentPosition = Math.Max(timeToSeekTo, 0);
        this._video?.Seek(pTypingGame.MusicTrack.CurrentPosition);

        if (pTypingGame.MusicTrack.PlaybackState == PlaybackState.Playing)
            foreach (NoteDrawable note in this.EditorState.Notes.Where(note => note.Note.Time > this.EditorState.CurrentTime))
                note.EditorHitSoundQueued = true;
    }

    public void DeleteSelectedObjects() {
        foreach (Drawable @object in this.EditorState.SelectedObjects)
            if (@object is NoteDrawable note) {
                this.EditorDrawable.Drawables.Remove(note);
                this.EditorState.Song.Notes.Remove(note.Note);
                this.EditorState.Notes.Remove(note);

                this.CurrentTool?.OnNoteDelete(note);
            } else if (@object is BeatLineBarEventDrawable barEvent) {
                this.EditorDrawable.Drawables.Remove(barEvent);
                this.EditorState.Song.Events.Remove(barEvent.Event);
                this.EditorState.Events.Remove(barEvent);

                this.CurrentTool?.OnEventDelete(barEvent);
            } else if (@object is BeatLineBeatEventDrawable beatEvent) {
                this.EditorDrawable.Drawables.Remove(beatEvent);
                this.EditorState.Song.Events.Remove(beatEvent.Event);
                this.EditorState.Events.Remove(beatEvent);

                this.CurrentTool?.OnEventDelete(beatEvent);
            } else if (@object is TypingCutoffEventDrawable cutoffEvent) {
                this.EditorDrawable.Drawables.Remove(cutoffEvent);
                this.EditorState.Song.Events.Remove(cutoffEvent.Event);
                this.EditorState.Events.Remove(cutoffEvent);

                this.CurrentTool?.OnEventDelete(cutoffEvent);
            } else if (@object is LyricEventDrawable lyricEvent) {
                this.EditorDrawable.Drawables.Remove(lyricEvent);
                this.EditorState.Song.Events.Remove(lyricEvent.Event);
                this.EditorState.Events.Remove(lyricEvent);

                this.CurrentTool?.OnEventDelete(lyricEvent);
            }

        this.EditorState.SelectedObjects.Clear();
        this.SaveNeeded = true;
    }

    private void OnKeyPress(object sender, Key key) {
        this.CurrentTool?.OnKeyPress(key);

        switch (key) {
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

                if (this.SaveNeeded) {
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
                                case DialogResult.Yes:
                                    SongManager.PTYPING_SONG_HANDLER.SaveSong(this.EditorState.Song);
                                    pTypingGame.CurrentSong.Value = this.EditorState.Song;
                                    SongManager.UpdateSongs();

                                    pTypingGame.MenuClickSound.PlayNew();

                                    // Exit the editor
                                    ScreenManager.ChangeScreen(new SongSelectionScreen(true));
                                    break;
                            }
                        }
                        );
                    },
                    "Do you want to save before quitting?",
                    MessageBoxButtons.YesNoCancel
                    );
                }
                break;
            case Key.Delete: {
                // Delete the selected objects
                this.DeleteSelectedObjects();
                break;
            }
            case Key.S when FurballGame.InputManager.HeldKeys.Contains(Key.ControlLeft): {
                List<LyricEvent> lyrics = this.EditorState.Song.Events.Where(x => x is LyricEvent).Cast<LyricEvent>().ToList();
                lyrics.Sort((x, y) => (int)(x.Time - y.Time));
                for (int i = 1; i < lyrics.Count; i++) {
                    LyricEvent editorStateEvent     = lyrics[i];
                    LyricEvent lastEditorStateEvent = lyrics[i - 1];

                    lastEditorStateEvent.EndTime = editorStateEvent.Time;
                }

                // Save the song if ctrl+s is pressed
                SongManager.PTYPING_SONG_HANDLER.SaveSong(this.EditorState.Song);
                // pTypingGame.CurrentSong.Value = this.EditorState.Song;
                SongManager.UpdateSongs();

                this.SaveNeeded = false;
                break;
            }
            case Key.C when FurballGame.InputManager.HeldKeys.Contains(Key.ControlLeft): {
                if (this.EditorState.SelectedObjects.Count == 0) return;

                List<NoteDrawable> sortedNotes = new();
                foreach (Drawable @object in this.EditorState.SelectedObjects)
                    if (@object is NoteDrawable note)
                        sortedNotes.Add(note);
                sortedNotes = sortedNotes.OrderBy(x => x.Note.Time).ToList();

                if (sortedNotes.Count == 0) return;

                double startTime = sortedNotes.First().Note.Time;

                List<Note> notes = new();

                foreach (NoteDrawable drawable in sortedNotes) {
                    Note note = drawable.Note.Copy();
                    note.Time = drawable.Note.Time - startTime;

                    notes.Add(note);
                }

                FurballGame.InputManager.Clipboard = JsonConvert.SerializeObject(notes);

                break;
            }
            case Key.V when FurballGame.InputManager.HeldKeys.Contains(Key.ControlLeft): {
                try {
                    List<Note> notes = JsonConvert.DeserializeObject<List<Note>>(FurballGame.InputManager.Clipboard);

                    foreach (Note note in notes) {
                        note.Time += this.EditorState.CurrentTime;

                        this.CreateNote(note, true);
                    }
                }
                catch {
                    pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Error, "Pasting of notes failed!");
                }

                break;
            }
        }
    }

    private readonly List<Drawable> _timingPoints = new();

    public void UpdateTimingPointDisplay() {
        this._timingPoints.ForEach(x => this.Manager.Remove(x));
        this._timingPoints.Clear();

        float startX = this._progressBar.Position.X;
        float length = this._progressBar.BarSize.X;

        foreach (TimingPoint timingPoint in this.EditorState.Song.TimingPoints) {
            float x = (float)(timingPoint.Time / pTypingGame.MusicTrack.Length * length + startX);

            TexturedDrawable drawable = new(FurballGame.WhitePixel, new Vector2(x, FurballGame.DEFAULT_WINDOW_HEIGHT)) {
                Scale         = new Vector2(3, this._progressBar.BarSize.Y + 10),
                OriginType    = OriginType.BottomCenter,
                ColorOverride = new Color(50, 200, 50, 100),
                ToolTip = $@"BPM:{60000d / timingPoint.Tempo:#.##}
ApproachMult:{timingPoint.ApproachMultiplier}"
            };

            this._timingPoints.Add(drawable);
            this.Manager.Add(drawable);
        }
    }

    public double CurrentApproachTime(double time) => ConVars.BASE_APPROACH_TIME / this.EditorState.Song.CurrentTimingPoint(time).ApproachMultiplier;

    private double           _lastTime = 0;
    private DrawableDropdown _speedDropdown;
    [CanBeNull]
    private VideoDrawable _video;
    private TexturedDrawable _playButton;
    private TexturedDrawable _pauseButton;
    private TexturedDrawable _rightButton;
    private TexturedDrawable _leftButton;
    private TexturedDrawable _playfieldBackgroundCover;
    public override void Update(double gameTime) {
        this.EditorState.CurrentTime = pTypingGame.MusicTrackTimeSource.GetCurrentTime();

        if (!this.EditorState.CurrentTime.Equals(this._lastTime)) {
            this.CurrentTool?.OnTimeChange(this.EditorState.CurrentTime);

            foreach (NoteDrawable note in this.EditorState.Notes) {
                note.Visible = this.EditorState.CurrentTime > note.Note.Time - this.CurrentApproachTime(note.Note.Time) &&
                               this.EditorState.CurrentTime < note.Note.Time + 1000;

                if (note.EditorHitSoundQueued && note.Note.Time < this.EditorState.CurrentTime) {
                    if(!this._progressBar.IsClicked) this.HitSoundNormal.PlayNew();
                    note.EditorHitSoundQueued = false;
                }
            }
            foreach (Drawable managedDrawable in this.EditorState.Events) {
                double time = managedDrawable switch {
                    TypingCutoffEventDrawable cutoff       => cutoff.Event.Time,
                    BeatLineBarEventDrawable beatLineBar   => beatLineBar.Event.Time,
                    BeatLineBeatEventDrawable beatLineBeat => beatLineBeat.Event.Time,
                    LyricEventDrawable lyric               => lyric.Event.Time,
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

        this._lastTime = this.EditorState.CurrentTime;

        base.Update(gameTime);
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
    public override string Details => pTypingConfig.Instance.Username == pTypingGame.CurrentSong.Value.Creator
                                          ? $"Editing {pTypingGame.CurrentSong.Value.Artist} - {pTypingGame.CurrentSong.Value.Name} [{pTypingGame.CurrentSong.Value.Difficulty}] by {pTypingGame.CurrentSong.Value.Creator}"
                                          : $"Modding {pTypingGame.CurrentSong.Value.Artist} - {pTypingGame.CurrentSong.Value.Name} [{pTypingGame.CurrentSong.Value.Difficulty}] by {pTypingGame.CurrentSong.Value.Creator}";

    public override bool           ForceSpeedReset      => true;
    public override float          BackgroundFadeAmount => 0.3f;
    public override MusicLoopState LoopState            => MusicLoopState.None;
    public override ScreenType     ScreenType           => ScreenType.Menu;
}