using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Audio;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Engine.Engine.Helpers;
using Furball.Engine.Engine.Input;
using Gtk;
using JetBrains.Annotations;
using ManagedBass;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using pTyping.Engine;
using pTyping.Graphics.Drawables;
using pTyping.Graphics.Drawables.Events;
using pTyping.Graphics.Editor.Tools;
using pTyping.Graphics.Menus.SongSelect;
using pTyping.Graphics.Player;
using pTyping.Songs;
using TextCopy;

namespace pTyping.Graphics.Editor {

    public class EditorScreen : pScreen {
        private TextDrawable _currentTimeDrawable;

        public  Texture2D             NoteTexture;
        private UiProgressBarDrawable _progressBar;
        private TexturedDrawable      _recepticle;

        public EditorTool       CurrentTool;
        public List<EditorTool> EditorTools;

        public EditorState EditorState;

        private readonly List<ManagedDrawable> _selectionRects = new();

        public static readonly Vector2 RECEPTICLE_POS = new(FurballGame.DEFAULT_WINDOW_WIDTH * 0.15f, FurballGame.DEFAULT_WINDOW_HEIGHT / 2f);
        public static readonly Vector2 NOTE_START_POS = new(FurballGame.DEFAULT_WINDOW_WIDTH + 200, FurballGame.DEFAULT_WINDOW_HEIGHT / 2f);
        public static readonly Vector2 NOTE_END_POS   = new(-100, FurballGame.DEFAULT_WINDOW_HEIGHT / 2f);

        public bool SaveNeeded = false;

        public long LastEscapeTime = 0;

        public SoundEffect HitSoundNormal = new();

        public override void Initialize() {
            base.Initialize();

            //Create a copy of the song so that we dont edit it globally
            this.EditorState = new(pTypingGame.CurrentSong.Value.Copy());

            pTypingGame.MusicTrack.Stop();

            #region Gameplay preview

            this.NoteTexture = ContentManager.LoadMonogameAsset<Texture2D>("note");

            Vector2 recepticlePos = new(FurballGame.DEFAULT_WINDOW_WIDTH * 0.15f, FurballGame.DEFAULT_WINDOW_HEIGHT / 2f);
            this._recepticle = new TexturedDrawable(this.NoteTexture, recepticlePos) {
                Scale       = new(0.55f),
                OriginType  = OriginType.Center,
                Clickable   = false,
                CoverClicks = false,
                Hoverable   = false,
                CoverHovers = false
            };

            this.Manager.Add(this._recepticle);

            foreach (Note note in this.EditorState.Song.Notes)
                this.CreateNote(note);
            foreach (Event @event in this.EditorState.Song.Events)
                this.CreateEvent(@event);

            #endregion

            #region Playfield decorations

            LinePrimitiveDrawable playfieldTopLine = new(new Vector2(1, recepticlePos.Y - 50), FurballGame.DEFAULT_WINDOW_WIDTH, 0) {
                ColorOverride = Color.Gray,
                Clickable     = false,
                CoverClicks   = false
            };
            LinePrimitiveDrawable playfieldBottomLine = new(new Vector2(1, recepticlePos.Y + 50), FurballGame.DEFAULT_WINDOW_WIDTH, 0) {
                ColorOverride = Color.Gray,
                Clickable     = false,
                CoverClicks   = false
            };
            this.Manager.Add(playfieldTopLine);
            this.Manager.Add(playfieldBottomLine);

            RectanglePrimitiveDrawable playfieldBackgroundCover = new(new(0, recepticlePos.Y - 50), new(FurballGame.DEFAULT_WINDOW_WIDTH, 100), 0f, true) {
                ColorOverride = new(100, 100, 100, 100),
                Depth         = 0.9f,
                Clickable     = false,
                CoverClicks   = false
            };
            this.Manager.Add(playfieldBackgroundCover);

            #region background image

            this.Manager.Add(pTypingGame.CurrentSongBackground);

            pTypingGame.CurrentSongBackground.Tweens.Add(
            new ColorTween(
            TweenType.Color,
            pTypingGame.CurrentSongBackground.ColorOverride,
            new(0.3f, 0.3f, 0.3f),
            pTypingGame.CurrentSongBackground.TimeSource.GetCurrentTime(),
            pTypingGame.CurrentSongBackground.TimeSource.GetCurrentTime() + 1000
            )
            );
            pTypingGame.LoadBackgroundFromSong(this.EditorState.Song);

            #endregion

            #endregion

            #region Visualization drawables

            this.EditorState.SelectedObjects.CollectionChanged += this.UpdateSelectionRects;

            #endregion

            #region UI

            #region Progress bar

            this._progressBar = new UiProgressBarDrawable(
            new Vector2(0, FurballGame.DEFAULT_WINDOW_HEIGHT),
            FurballGame.DEFAULT_FONT,
            new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 200, 40),
            Color.Gray,
            Color.DarkGray,
            Color.White
            ) {
                OriginType = OriginType.BottomLeft
            };

            this._progressBar.OnDrag  += this.ProgressBarOnInteract;
            this._progressBar.OnClick += this.ProgressBarOnInteract;

            this.Manager.Add(this._progressBar);

            #endregion

            #region Current time

            this._currentTimeDrawable = new TextDrawable(new Vector2(10, FurballGame.DEFAULT_WINDOW_HEIGHT - 50), FurballGame.DEFAULT_FONT, "", 30) {
                OriginType = OriginType.BottomLeft
            };

            this.Manager.Add(this._currentTimeDrawable);

            #endregion

            #region Playback buttons

            Texture2D editorButtonsTexture2D = ContentManager.LoadMonogameAsset<Texture2D>("editorbuttons", ContentSource.User);

            TexturedDrawable playButton = new(
            editorButtonsTexture2D,
            new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 150, FurballGame.DEFAULT_WINDOW_HEIGHT),
            TexturePositions.EDITOR_PLAY
            ) {
                Scale      = new(0.5f, 0.5f),
                OriginType = OriginType.BottomRight
            };
            TexturedDrawable pauseButton = new(
            editorButtonsTexture2D,
            new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 100, FurballGame.DEFAULT_WINDOW_HEIGHT),
            TexturePositions.EDITOR_PAUSE
            ) {
                Scale      = new(0.5f, 0.5f),
                OriginType = OriginType.BottomRight
            };
            TexturedDrawable rightButton = new(
            editorButtonsTexture2D,
            new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH, FurballGame.DEFAULT_WINDOW_HEIGHT),
            TexturePositions.EDITOR_RIGHT
            ) {
                Scale      = new(0.5f, 0.5f),
                OriginType = OriginType.BottomRight
            };
            TexturedDrawable leftButton = new(
            editorButtonsTexture2D,
            new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 50, FurballGame.DEFAULT_WINDOW_HEIGHT),
            TexturePositions.EDITOR_LEFT
            ) {
                Scale      = new(0.5f, 0.5f),
                OriginType = OriginType.BottomRight
            };

            playButton.OnClick += delegate {
                pTypingGame.PlayMusic();
            };

            pauseButton.OnClick += delegate {
                pTypingGame.PauseResumeMusic();
            };

            leftButton.OnClick += delegate {
                if (this.EditorState.Song.Notes.Count > 0)
                    pTypingGame.MusicTrack.SeekTo(this.EditorState.Song.Notes.First().Time);
            };

            rightButton.OnClick += delegate {
                if (this.EditorState.Song.Notes.Count > 0)
                    pTypingGame.MusicTrack.SeekTo(this.EditorState.Song.Notes.Last().Time);
            };

            this.Manager.Add(playButton);
            this.Manager.Add(pauseButton);
            this.Manager.Add(rightButton);
            this.Manager.Add(leftButton);

            #endregion

            #region Tool selection

            this.EditorTools = EditorTool.GetAllTools();

            float y = 10;
            foreach (EditorTool tool in this.EditorTools) {
                UiTickboxDrawable tickboxDrawable = new(new(10, y), tool.Name, 35, false, true) {
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

            #endregion

            this.ChangeTool(typeof(SelectTool));
            
            FurballGame.InputManager.OnKeyDown     += this.OnKeyPress;
            FurballGame.InputManager.OnMouseScroll += this.OnMouseScroll;
            FurballGame.InputManager.OnMouseDown   += this.OnClick;
            FurballGame.InputManager.OnMouseMove   += this.OnMouseMove;
            FurballGame.InputManager.OnMouseDrag   += this.OnMouseDrag;

            pTypingGame.UserStatusEditing();

            this.HitSoundNormal.Load(ContentManager.LoadRawAsset("hitsound.wav", ContentSource.User));

            ConVars.Volume.OnChange    += this.OnVolumeChange;
            this.HitSoundNormal.Volume =  ConVars.Volume.Value;
        }

        private void OnVolumeChange(object sender, EventArgs e) {
            this.HitSoundNormal.Volume = ConVars.Volume.Value;
        }

        private void ProgressBarOnInteract(object sender, Point e) {
            Vector2 adjustedPoint = e.ToVector2() - this._progressBar.Position - this._progressBar.LastCalculatedOrigin;

            double value = (double)adjustedPoint.X / this._progressBar.Size.X;

            double time = value * pTypingGame.MusicTrack.Length;

            pTypingGame.MusicTrack.SeekTo(time);

            if (pTypingGame.MusicTrack.PlaybackState == PlaybackState.Playing)
                foreach (NoteDrawable note in this.EditorState.Notes.Where(note => note.Note.Time > this.EditorState.CurrentTime))
                    note.EditorHitSoundQueued = true;
        }

        public void UpdateSelectionRects(object _, NotifyCollectionChangedEventArgs __) {
            this._selectionRects.ForEach(x => this.Manager.Remove(x));

            this._selectionRects.Clear();

            foreach (ManagedDrawable @object in this.EditorState.SelectedObjects) {
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
                    TimeSource    = pTypingGame.MusicTrack
                };

                @object.Tweens.ForEach(x => rect.Tweens.Add(x.Copy()));
                
                this._selectionRects.Add(rect);

                this.Manager.Add(rect);
            }
        }

        private void OnMouseDrag(object sender, ((Point lastPosition, Point newPosition), string cursorName) e) {
            this.CurrentTool?.OnMouseDrag(e.Item1.newPosition);
        }

        public void CreateEvent(Event @event, bool isNew = false) {
            ManagedDrawable eventDrawable = Event.CreateEventDrawable(@event, this.NoteTexture, new(ConVars.BaseApproachTime.Value, true, true));

            if (eventDrawable == null) return;

            this.Manager.Add(eventDrawable);
            this.EditorState.Events.Add(eventDrawable);
            if (isNew) {
                this.EditorState.Song.Events.Add(@event);
                this.SaveNeeded = true;
            }

            this.CurrentTool?.OnEventCreate(eventDrawable, isNew);
        }

        public void CreateNote(Note note, bool isNew = false) {
            NoteDrawable noteDrawable = new(
            new Vector2(NOTE_START_POS.X, NOTE_START_POS.Y + note.YOffset),
            this.NoteTexture,
            pTypingGame.JapaneseFont,
            50
            ) {
                TimeSource    = pTypingGame.MusicTrack,
                NoteTexture = {
                    ColorOverride = note.Color
                },
                RawTextDrawable = {
                    Text = $"{note.Text}"
                },
                Scale      = new(0.55f, 0.55f),
                OriginType = OriginType.Center,
                Note       = note
            };

            noteDrawable.CreateTweens(new(ConVars.BaseApproachTime.Value, true, true));

            this.Manager.Add(noteDrawable);
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

            newTool?.SelectTool(this, ref this.Manager);
        }

        private void OnMouseMove(object sender, (Point mousePos, string cursorName) e) {
            double currentTime  = this.EditorState.CurrentTime;
            double reticuleXPos = this._recepticle.Position.X;
            double noteStartPos = FurballGame.DEFAULT_WINDOW_WIDTH + 100;

            double speed = (noteStartPos - reticuleXPos) / ConVars.BaseApproachTime.Value;

            double relativeMousePosition = (e.mousePos.X - reticuleXPos) * 0.925;

            double timeAtCursor = relativeMousePosition / speed + currentTime;

            TimingPoint timingPoint = this.EditorState.Song.CurrentTimingPoint(timeAtCursor);

            double noteLength = this.EditorState.Song.DividedNoteLength(timeAtCursor);

            timeAtCursor += noteLength / 2d;

            double roundedTime = timeAtCursor - (timeAtCursor - timingPoint.Time) % noteLength;

            this.EditorState.MouseTime = roundedTime;

            this.CurrentTool?.OnMouseMove(e.mousePos);
        }

        private void OnClick(object sender, ((MouseButton mouseButton, Point position) args, string cursorName) e) {
            this.CurrentTool?.OnMouseClick(e.args);
        }

        [Pure]
        public static bool InPlayfield(Point pos) => pos.Y < RECEPTICLE_POS.Y + 40f && pos.Y > RECEPTICLE_POS.Y - 40f;

        protected override void Dispose(bool disposing) {
            FurballGame.InputManager.OnKeyDown     -= this.OnKeyPress;
            FurballGame.InputManager.OnMouseScroll -= this.OnMouseScroll;
            FurballGame.InputManager.OnMouseDown   -= this.OnClick;
            FurballGame.InputManager.OnMouseMove   -= this.OnMouseMove;
            FurballGame.InputManager.OnMouseDrag   -= this.OnMouseDrag;

            this.EditorState.SelectedObjects.CollectionChanged -= this.UpdateSelectionRects;

            this._progressBar.OnDrag -= this.ProgressBarOnInteract;

            ConVars.Volume.OnChange -= this.OnVolumeChange;

            // SongManager.UpdateSongs();
            
            base.Dispose(disposing);
        }

        private void OnMouseScroll(object sender, (int scrollAmount, string) args) {
            this.TimelineMove(args.scrollAmount <= 0);
        }

        public void TimelineMove(bool right) {
            double currentTime = pTypingGame.MusicTrack.CurrentTime * 1000d;

            double noteLength = this.EditorState.Song.DividedNoteLength(currentTime);
            double timeToSeekTo = Math.Round((pTypingGame.MusicTrack.CurrentTime * 1000d - this.EditorState.Song.CurrentTimingPoint(currentTime).Time) / noteLength) *
                                  noteLength;

            timeToSeekTo += this.EditorState.Song.CurrentTimingPoint(currentTime).Time;

            if (right)
                timeToSeekTo += noteLength;
            else
                timeToSeekTo -= noteLength;

            pTypingGame.MusicTrack.SeekTo(Math.Max(timeToSeekTo, 0));

            if (pTypingGame.MusicTrack.PlaybackState == PlaybackState.Playing)
                foreach (NoteDrawable note in this.EditorState.Notes.Where(note => note.Note.Time > this.EditorState.CurrentTime))
                    note.EditorHitSoundQueued = true;
        }

        public void DeleteSelectedObjects() {
            foreach (ManagedDrawable @object in this.EditorState.SelectedObjects)
                if (@object is NoteDrawable note) {
                    this.Manager.Remove(note);
                    this.EditorState.Song.Notes.Remove(note.Note);
                    this.EditorState.Notes.Remove(note);

                    this.CurrentTool?.OnNoteDelete(note);
                } else if (@object is BeatLineBarEventDrawable barEvent) {
                    this.Manager.Remove(barEvent);
                    this.EditorState.Song.Events.Remove(barEvent.Event);
                    this.EditorState.Events.Remove(barEvent);

                    this.CurrentTool?.OnEventDelete(barEvent);
                } else if (@object is BeatLineBeatEventDrawable beatEvent) {
                    this.Manager.Remove(beatEvent);
                    this.EditorState.Song.Events.Remove(beatEvent.Event);
                    this.EditorState.Events.Remove(beatEvent);

                    this.CurrentTool?.OnEventDelete(beatEvent);
                } else if (@object is TypingCutoffEventDrawable cutoffEvent) {
                    this.Manager.Remove(cutoffEvent);
                    this.EditorState.Song.Events.Remove(cutoffEvent.Event);
                    this.EditorState.Events.Remove(cutoffEvent);

                    this.CurrentTool?.OnEventDelete(cutoffEvent);
                }

            this.EditorState.SelectedObjects.Clear();
            this.SaveNeeded = true;
        }

        private void OnKeyPress(object sender, Keys key) {
            this.CurrentTool?.OnKeyPress(key);
            
            switch (key) {
                case Keys.Space:
                    this.ToggleMusicPlay();
                    break;
                case Keys.Left: {
                    this.TimelineMove(false);

                    break;
                }
                case Keys.Right: {
                    this.TimelineMove(true);

                    break;
                }
                case Keys.Escape:
                    //If the user has some notes selected, clear them
                    if (this.EditorState.SelectedObjects.Count != 0) {
                        this.EditorState.SelectedObjects.Clear();
                        return;
                    }

                    long unixTime = UnixTime.Now();
                    if (unixTime - this.LastEscapeTime > 5) {
                        this.LastEscapeTime = unixTime;
                        return;
                    }

                    if (this.SaveNeeded) {
                        ResponseType responseType = GtkHelper.MessageDialog(
                        "Are you sure?",
                        "Do you want to save before quitting?",
                        MessageType.Question,
                        ButtonsType.YesNo
                        );

                        if (responseType == ResponseType.Yes) {
                            SongManager.PTYPING_SONG_HANDLER.SaveSong(this.EditorState.Song);
                            pTypingGame.CurrentSong.Value = this.EditorState.Song;
                            SongManager.UpdateSongs();
                        }
                    }

                    pTypingGame.MenuClickSound.Play();

                    // Exit the editor
                    ScreenManager.ChangeScreen(new SongSelectionScreen(true));
                    break;
                case Keys.Delete: {
                    // Delete the selected objects
                    this.DeleteSelectedObjects();
                    break;
                }
                case Keys.S when FurballGame.InputManager.HeldKeys.Contains(Keys.LeftControl): {
                    // Save the song if ctrl+s is pressed
                    SongManager.PTYPING_SONG_HANDLER.SaveSong(this.EditorState.Song);
                    pTypingGame.CurrentSong.Value = this.EditorState.Song;
                    SongManager.UpdateSongs();

                    this.SaveNeeded = false;
                    break;
                }
                case Keys.C when FurballGame.InputManager.HeldKeys.Contains(Keys.LeftControl): {
                    if (this.EditorState.SelectedObjects.Count == 0) return;

                    List<NoteDrawable> sortedNotes = new();
                    foreach (ManagedDrawable @object in this.EditorState.SelectedObjects)
                        if (@object is NoteDrawable note)
                            sortedNotes.Add(note);
                    sortedNotes = sortedNotes.OrderBy(x => x.Note.Time).ToList();

                    double startTime = sortedNotes.First().Note.Time;

                    List<Note> notes = new();

                    foreach (NoteDrawable drawable in sortedNotes) {
                        Note note = drawable.Note.Copy();
                        note.Time = drawable.Note.Time - startTime;

                        notes.Add(note);
                    }

                    ClipboardService.SetText(JsonConvert.SerializeObject(notes));

                    break;
                }
                case Keys.V when FurballGame.InputManager.HeldKeys.Contains(Keys.LeftControl): {
                    try {
                        List<Note> notes = JsonConvert.DeserializeObject<List<Note>>(ClipboardService.GetText());

                        foreach (Note note in notes) {
                            note.Time += this.EditorState.CurrentTime;

                            this.CreateNote(note, true);
                        }
                    }
                    catch {
                        //TODO: Notify the user it failed
                    }

                    break;
                }
            }
        }

        private double _lastTime = 0;
        public override void Update(GameTime gameTime) {
            this.EditorState.CurrentTime = pTypingGame.MusicTrack.GetCurrentTime();

            if (!this.EditorState.CurrentTime.Equals(this._lastTime)) {
                this.CurrentTool?.OnTimeChange(this.EditorState.CurrentTime);

                foreach (NoteDrawable note in this.EditorState.Notes) {
                    note.Visible = this.EditorState.CurrentTime > note.Note.Time - 2000 && this.EditorState.CurrentTime < note.Note.Time + 1000;

                    if (note.EditorHitSoundQueued && note.Note.Time < this.EditorState.CurrentTime) {
                        this.HitSoundNormal.Play();
                        note.EditorHitSoundQueued = false;
                    }
                }
                foreach (ManagedDrawable managedDrawable in this.EditorState.Events) {
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
        public override string Details => ConVars.Username.Value == pTypingGame.CurrentSong.Value.Creator
                                              ? $"Editing {pTypingGame.CurrentSong.Value.Artist} - {pTypingGame.CurrentSong.Value.Name} [{pTypingGame.CurrentSong.Value.Difficulty}] by {pTypingGame.CurrentSong.Value.Creator}"
                                              : $"Modding {pTypingGame.CurrentSong.Value.Artist} - {pTypingGame.CurrentSong.Value.Name} [{pTypingGame.CurrentSong.Value.Difficulty}] by {pTypingGame.CurrentSong.Value.Creator}";
    }
}
