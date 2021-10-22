using System;
using System.Linq;
using System.Collections.Generic;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Input;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Engine.Engine.Helpers;
using pTyping.Songs;
using pTyping.Drawables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace pTyping.Screens {
    public enum EditorTool {
        None,
        Select,
        CreateNote
    }

    public class EditorScreen : Screen {
        private TextDrawable               _currentTimeDrawable;
        private TexturedDrawable           _recepticle;
        private NoteDrawable               _selectedNote;
        private RectanglePrimitiveDrawable _selectionRect;
        private UiProgressBarDrawable      _progressBar;
        private LinePrimitiveDrawable      _createLine;

        private UiTextBoxDrawable _textInput;
        private TextDrawable      _textInputLabel;
        private UiTextBoxDrawable _colorInput;
        private TextDrawable      _colorInputLabel;

        private TexturedDrawable _editorToolSelect;
        private TexturedDrawable _editorToolCreateNote;

        private List<NoteDrawable> _notes        = new();
        private List<NoteDrawable> _timelineBars = new();

        public EditorTool CurrentTool = EditorTool.None;

        private Texture2D _noteTexture;
        private bool      _isDragging = false;

        private const float EDITOR_TOOL_BASE_SIZE   = 0.4f;
        private const float EDITOR_TOOL_SCALED_SIZE = 0.55f;

        private Song _song;

        public override void Initialize() {
            base.Initialize();

            this._song = pTypingGame.CurrentSong.Value.Copy();

            pTypingGame.MusicTrack.Stop();

            #region Gameplay preview

            this._noteTexture = ContentManager.LoadMonogameAsset<Texture2D>("note");

            Vector2 recepticlePos = new(FurballGame.DEFAULT_WINDOW_WIDTH * 0.15f, FurballGame.DEFAULT_WINDOW_HEIGHT / 2f);
            this._recepticle = new TexturedDrawable(this._noteTexture, recepticlePos) {
                Scale      = new(0.55f),
                OriginType = OriginType.Center
            };

            this.Manager.Add(this._recepticle);

            foreach (Note note in this._song.Notes) {
                NoteDrawable noteDrawable = new(
                new Vector2(PlayerScreen.NOTE_START_POS.X, PlayerScreen.NOTE_START_POS.Y + note.YOffset),
                this._noteTexture,
                pTypingGame.JapaneseFont,
                50
                ) {
                    TimeSource    = pTypingGame.MusicTrack,
                    ColorOverride = note.Color,
                    LabelTextDrawable = {
                        Text  = $"{note.Text}",
                        Scale = new(1f)
                    },
                    Scale      = new(0.55f, 0.55f),
                    OriginType = OriginType.Center,
                    Note       = note
                };

                noteDrawable.OnClick += delegate {
                    if (this._isDragging) return;

                    this.OnNoteClick(noteDrawable);
                };

                noteDrawable.OnDrag += delegate(object _, Point point) {
                    this.OnNoteDrag(noteDrawable, point);
                };

                noteDrawable.OnDragBegin += delegate {
                    this._isDragging = true;
                };

                noteDrawable.OnDragEnd += delegate {
                    this._isDragging = false;
                };

                noteDrawable.Tweens.Add(
                new VectorTween(
                TweenType.Movement,
                new(PlayerScreen.NOTE_START_POS.X, PlayerScreen.NOTE_START_POS.Y + note.YOffset),
                PlayerScreen.RECEPTICLE_POS,
                (int)(note.Time - ConVars.BaseApproachTime.Value),
                (int)note.Time
                )
                );

                this.Manager.Add(noteDrawable);
                this._notes.Add(noteDrawable);
            }

            #endregion

            #region Playfield decorations

            LinePrimitiveDrawable playfieldTopLine = new(new Vector2(1, recepticlePos.Y - 50), FurballGame.DEFAULT_WINDOW_WIDTH, 0) {
                ColorOverride = Color.Gray
            };
            LinePrimitiveDrawable playfieldBottomLine = new(new Vector2(1, recepticlePos.Y + 50), FurballGame.DEFAULT_WINDOW_WIDTH, 0) {
                ColorOverride = Color.Gray
            };
            this.Manager.Add(playfieldTopLine);
            this.Manager.Add(playfieldBottomLine);

            RectanglePrimitiveDrawable playfieldBackgroundCover = new(new(0, recepticlePos.Y - 50), new(FurballGame.DEFAULT_WINDOW_WIDTH, 100), 0f, true) {
                ColorOverride = new(100, 100, 100, 100),
                Depth         = 0.9f
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
            pTypingGame.LoadBackgroundFromSong(this._song);

            #endregion

            #endregion

            #region Visualization drawables

            this._selectionRect = new() {
                RectSize      = new(100, 100),
                Filled        = false,
                Thickness     = 2f,
                ColorOverride = Color.Gray,
                Visible       = false,
                Clickable     = false,
                Hoverable     = false,
                CoverClicks   = false,
                CoverHovers   = false,
                OriginType    = OriginType.Center
            };

            this.Manager.Add(this._selectionRect);

            this._createLine = new LinePrimitiveDrawable(new Vector2(0, 0), 80f, (float)Math.PI / 2f) {
                Visible = false
            };

            this.Manager.Add(this._createLine);

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
                if (this._song.Notes.Count > 0)
                    pTypingGame.MusicTrack.SeekTo(this._song.Notes.First().Time);
            };

            rightButton.OnClick += delegate {
                if (this._song.Notes.Count > 0)
                    pTypingGame.MusicTrack.SeekTo(this._song.Notes.Last().Time);
            };

            this.Manager.Add(playButton);
            this.Manager.Add(pauseButton);
            this.Manager.Add(rightButton);
            this.Manager.Add(leftButton);

            #endregion

            #region Tool selection

            Texture2D editorToolButtonsTextures = ContentManager.LoadMonogameAsset<Texture2D>("editortools");

            this._editorToolSelect = new(editorToolButtonsTextures, new Vector2(10, 10), new Rectangle(0, 0, 240, 240)) {
                Scale = new(EDITOR_TOOL_BASE_SIZE),
                Depth = 0f
            };
            this._editorToolSelect.OnClick += delegate {
                this.ChangeTool(EditorTool.Select);
            };

            this._editorToolCreateNote = new(editorToolButtonsTextures, new Vector2(10, this._editorToolSelect.Size.Y + 20), new Rectangle(0, 240, 240, 240)) {
                Scale = new(EDITOR_TOOL_BASE_SIZE),
                Depth = 0.5f
            };
            this._editorToolCreateNote.OnClick += delegate {
                this.ChangeTool(EditorTool.CreateNote);
            };

            this.Manager.Add(this._editorToolSelect);
            this.Manager.Add(this._editorToolCreateNote);

            #endregion

            #region Edit note info

            #region text to show

            this._textInputLabel = new(
            new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH * 0.75f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.70f),
            FurballGame.DEFAULT_FONT,
            "Text",
            25
            ) {
                OriginType = OriginType.Center,
                Visible    = false
            };
            this._textInput = new(
            new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH * 0.75f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.75f),
            FurballGame.DEFAULT_FONT,
            "select note",
            25,
            150f
            ) {
                OriginType = OriginType.Center,
                Visible    = false
            };
            this._textInput.OnLetterTyped   += this.UpdateTextInputs;
            this._textInput.OnLetterRemoved += this.UpdateTextInputs;

            this.Manager.Add(this._textInput);
            this.Manager.Add(this._textInputLabel);

            #endregion

            #region color input

            this._colorInputLabel = new(
            new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH * 0.5f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.70f),
            FurballGame.DEFAULT_FONT,
            "HTML Color",
            25
            ) {
                OriginType = OriginType.Center,
                Visible    = false
            };
            this._colorInput = new(
            new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH * 0.5f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.75f),
            FurballGame.DEFAULT_FONT,
            "select note",
            25,
            150f
            ) {
                OriginType = OriginType.Center,
                Visible    = false
            };
            this._colorInput.OnLetterTyped   += this.UpdateTextInputs;
            this._colorInput.OnLetterRemoved += this.UpdateTextInputs;

            this.Manager.Add(this._colorInput);
            this.Manager.Add(this._colorInputLabel);

            #endregion

            #endregion

            #endregion

            this.ChangeTool(EditorTool.Select);

            FurballGame.InputManager.OnKeyDown     += this.OnKeyPress;
            FurballGame.InputManager.OnMouseScroll += this.OnMouseScroll;
            FurballGame.InputManager.OnMouseDown   += this.OnClick;
            FurballGame.InputManager.OnMouseMove   += this.OnMouseMove;

            pTypingGame.UserStatusEditing();
        }

        private void UpdateTextInputs(object sender, char _) {
            if (this._selectedNote == null)
                return;

            this._selectedNote.Note.Text = this._textInput.Text.Trim();
            // this._selectedNote.Note.TextToType = pTypingGame.Alphanumeric.Replace(this._textToTypeInput.Text.Trim(), string.Empty);
            if (this._colorInput.Text.Length - 1 is 3 or 4 or 6 or 8)
                this._selectedNote.Note.Color.FromHexString(this._colorInput.Text);

            this._selectedNote.LabelTextDrawable.Text = $"{this._selectedNote.Note.Text}";
            this._selectedNote.ColorOverride          = this._selectedNote.Note.Color;
        }

        public void ChangeTool(EditorTool tool) {
            if (this.CurrentTool == tool) return;

            this.DeselectNote();
            this.CurrentTool = tool;

            if (tool == EditorTool.Select) {
                this._textInput.Visible      = true;
                this._textInputLabel.Visible = true;
                // this._textToTypeInput.Visible      = true;
                // this._textToTypeInputLabel.Visible = true;
                this._colorInput.Visible      = true;
                this._colorInputLabel.Visible = true;

                this._editorToolSelect.Tweens.Add(
                new VectorTween(TweenType.Scale, this._editorToolSelect.Scale, new(EDITOR_TOOL_SCALED_SIZE), FurballGame.Time, FurballGame.Time + 150, Easing.Out)
                );
            } else {
                this._textInput.Visible      = false;
                this._textInputLabel.Visible = false;
                // this._textToTypeInput.Visible      = false;
                // this._textToTypeInputLabel.Visible = false;
                this._colorInput.Visible      = false;
                this._colorInputLabel.Visible = false;

                this._editorToolSelect.Tweens.Add(
                new VectorTween(TweenType.Scale, this._editorToolSelect.Scale, new(EDITOR_TOOL_BASE_SIZE), FurballGame.Time, FurballGame.Time + 150, Easing.Out)
                );
            }

            if (tool == EditorTool.CreateNote) {
                this._editorToolCreateNote.Tweens.Add(
                new VectorTween(TweenType.Scale, this._editorToolCreateNote.Scale, new(EDITOR_TOOL_SCALED_SIZE), FurballGame.Time, FurballGame.Time + 150, Easing.Out)
                );
            } else {
                this._editorToolCreateNote.Tweens.Add(
                new VectorTween(TweenType.Scale, this._editorToolCreateNote.Scale, new(EDITOR_TOOL_BASE_SIZE), FurballGame.Time, FurballGame.Time + 150, Easing.Out)
                );

                this._createLine.Visible = false;
            }
        }

        private double _mouseTime;
        private void OnMouseMove(object sender, (Point, string) e) {
            (int x, int y) = e.Item1;
            if (y < PlayerScreen.RECEPTICLE_POS.Y + 40f && y > PlayerScreen.RECEPTICLE_POS.Y - 40f) {
                if (this.CurrentTool == EditorTool.CreateNote)
                    this._createLine.Visible = true;
                this._createLine.OriginType = OriginType.Center;

                double currentTime  = pTypingGame.MusicTrack.CurrentTime * 1000;
                double reticulePos  = FurballGame.DEFAULT_WINDOW_WIDTH   * 0.15f;
                double noteStartPos = FurballGame.DEFAULT_WINDOW_WIDTH + 100;

                double distanceToReticule = x - reticulePos;

                double timeToReticule = distanceToReticule / (noteStartPos - reticulePos) * ConVars.BaseApproachTime.Value;

                double timeAtCursor = currentTime + timeToReticule;

                double noteLength = this._song.DividedNoteLength(timeAtCursor);

                double snappedTimeAtCursor = Math.Round((timeAtCursor - this._song.CurrentTimingPoint(timeAtCursor).Time) / noteLength) * noteLength +
                                             pTypingGame.CurrentSong.Value.CurrentTimingPoint(timeAtCursor).Time;
                this._mouseTime = snappedTimeAtCursor;

                double distanceInTime = snappedTimeAtCursor - currentTime;

                double scaleTime = distanceInTime / ConVars.BaseApproachTime.Value;

                double newX = scaleTime * (noteStartPos - reticulePos) + reticulePos;

                this._createLine.Position = new Vector2((float)newX, PlayerScreen.RECEPTICLE_POS.Y - 40);
            } else {
                this._createLine.Visible = false;
            }
        }

        public void AddNote(Note note) {}

        private void OnClick(object sender, ((MouseButton mouseButton, Point position) args, string cursorName) e) {
            if (this.CurrentTool == EditorTool.CreateNote) {
                Note note = new() {
                    Text = "a",
                    Time = this._mouseTime
                };

                (int _, int y) = e.args.position;
                if (y < PlayerScreen.RECEPTICLE_POS.Y + 40f && y > PlayerScreen.RECEPTICLE_POS.Y - 40f) {
                    NoteDrawable noteDrawable = new(PlayerScreen.NOTE_START_POS, this._noteTexture, pTypingGame.JapaneseFont, 50) {
                        TimeSource    = pTypingGame.MusicTrack,
                        ColorOverride = note.Color,
                        LabelTextDrawable = {
                            Text  = $"{note.Text}",
                            Scale = new(1f)
                        },
                        Scale      = new(0.55f, 0.55f),
                        OriginType = OriginType.Center,
                        Note       = note
                    };

                    noteDrawable.OnClick += delegate {
                        if (this._isDragging) return;

                        this.OnNoteClick(noteDrawable);
                    };

                    noteDrawable.OnDrag += delegate(object _, Point point) {
                        this.OnNoteDrag(noteDrawable, point);
                    };

                    noteDrawable.OnDragBegin += delegate {
                        this._isDragging = true;
                    };

                    noteDrawable.OnDragEnd += delegate {
                        this._isDragging = false;
                    };

                    noteDrawable.Tweens.Add(
                    new VectorTween(
                    TweenType.Movement,
                    PlayerScreen.NOTE_START_POS,
                    PlayerScreen.RECEPTICLE_POS,
                    (int)(note.Time - ConVars.BaseApproachTime.Value),
                    (int)note.Time
                    )
                    );

                    this.Manager.Add(noteDrawable);
                    this._notes.Add(noteDrawable);
                    this._song.Notes.Add(noteDrawable.Note);
                }
            }
        }

        public void OnNoteDrag(NoteDrawable noteDrawable, Point cursorPos) {
            if (this._selectedNote != noteDrawable) return;

            noteDrawable.Tweens.Clear();
            this._selectionRect.Tweens.Clear();

            noteDrawable.Note.Time = this._mouseTime;

            noteDrawable.Tweens.Add(
            new VectorTween(
            TweenType.Movement,
            new(PlayerScreen.NOTE_START_POS.X, PlayerScreen.NOTE_START_POS.Y + noteDrawable.Note.YOffset),
            PlayerScreen.RECEPTICLE_POS,
            (int)(noteDrawable.Note.Time - ConVars.BaseApproachTime.Value),
            (int)noteDrawable.Note.Time
            )
            );
            this._selectionRect.Tweens.Add(
            new VectorTween(
            TweenType.Movement,
            new(PlayerScreen.NOTE_START_POS.X, PlayerScreen.NOTE_START_POS.Y + noteDrawable.Note.YOffset),
            PlayerScreen.RECEPTICLE_POS,
            (int)(noteDrawable.Note.Time - ConVars.BaseApproachTime.Value),
            (int)noteDrawable.Note.Time
            )
            );
        }

        public void OnNoteClick(NoteDrawable noteDrawable) {
            if (this.CurrentTool != EditorTool.Select) return;

            this._selectedNote             = noteDrawable;
            this._selectionRect.Visible    = true;
            this._selectionRect.TimeSource = noteDrawable.TimeSource;
            this._selectionRect.Tweens     = noteDrawable.Tweens.Where(tween => tween.TweenType == TweenType.Movement && tween is VectorTween).ToList();

            this._textInput.Text  = this._selectedNote.Note.Text;
            this._colorInput.Text = this._selectedNote.Note.Color.ToHexString();
        }

        public void DeselectNote(bool delete = false) {
            if (this._selectedNote == null)
                return;

            this._selectionRect.Visible = false;

            if (delete) {
                this._song.Notes.Remove(this._selectedNote.Note);
                this._notes.Remove(this._selectedNote);
                this._selectedNote.Visible = false;
                this._selectedNote.ClearEvents();

                this.Manager.Remove(this._selectedNote);
            }

            this._selectedNote = null;
        }

        protected override void Dispose(bool disposing) {
            FurballGame.InputManager.OnKeyDown     -= this.OnKeyPress;
            FurballGame.InputManager.OnMouseScroll -= this.OnMouseScroll;
            FurballGame.InputManager.OnMouseDown   -= this.OnClick;
            FurballGame.InputManager.OnMouseMove   -= this.OnMouseMove;

            base.Dispose(disposing);
        }

        private void OnMouseScroll(object sender, (int, string) args) {
            if (args.Item1 > 0)
                this.TimelineMove(false);
            else
                this.TimelineMove(true);
        }

        public void TimelineMove(bool right) {
            double currentTime = pTypingGame.MusicTrack.CurrentTime * 1000d;

            double noteLength   = this._song.DividedNoteLength(currentTime);
            double timeToSeekTo = Math.Round((pTypingGame.MusicTrack.CurrentTime * 1000d - this._song.CurrentTimingPoint(currentTime).Time) / noteLength) * noteLength;

            timeToSeekTo += this._song.CurrentTimingPoint(currentTime).Time;

            if (right)
                timeToSeekTo += noteLength;
            else
                timeToSeekTo -= noteLength;

            pTypingGame.MusicTrack.SeekTo(Math.Max(timeToSeekTo, 0));
        }

        public void OnKeyPress(object sender, Keys key) {
            switch (key) {
                case Keys.Space:
                    pTypingGame.PauseResumeMusic();
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
                    if (this._selectedNote != null) {
                        this.DeselectNote();
                        return;
                    }

                    pTypingGame.MenuClickSound.Play();

                    // Exit the editor
                    ScreenManager.ChangeScreen(new SongSelectionScreen(true));
                    break;
                case Keys.Delete: {
                    // Delete the current note
                    this.DeselectNote(true);
                    break;
                }
                case Keys.S when FurballGame.InputManager.HeldKeys.Contains(Keys.LeftControl): {
                    // Save the song if ctrl+s is pressed
                    this._song.Save();
                    SongManager.UpdateSongs();
                    break;
                }
                case Keys.D1: {
                    this.ChangeTool(EditorTool.Select);
                    break;
                }
                case Keys.D2: {
                    this.ChangeTool(EditorTool.CreateNote);
                    break;
                }
            }
        }

        public override void Update(GameTime gameTime) {
            int currentTime = pTypingGame.MusicTrack.GetCurrentTime();

            for (int i = 0; i < this._notes.Count; i++) {
                NoteDrawable noteDrawable = this._notes[i];

                if (currentTime > noteDrawable.Note.Time + 10 || currentTime < noteDrawable.Note.Time - ConVars.BaseApproachTime.Value) {
                    noteDrawable.Visible = false;
                } else {
                    noteDrawable.Visible = true;
                    if (noteDrawable.Tweens.Count(x => x.TweenType == TweenType.Movement) == 0)
                        noteDrawable.Tweens.Add(
                        new VectorTween(
                        TweenType.Movement,
                        new(PlayerScreen.NOTE_START_POS.X, PlayerScreen.NOTE_START_POS.Y + noteDrawable.Note.YOffset),
                        PlayerScreen.RECEPTICLE_POS,
                        (int)(noteDrawable.Note.Time - ConVars.BaseApproachTime.Value),
                        (int)noteDrawable.Note.Time
                        )
                        );
                }
            }

            int milliseconds = (int)Math.Floor(currentTime         % 1000d);
            int seconds      = (int)Math.Floor(currentTime / 1000d % 60d);
            int minutes      = (int)Math.Floor(currentTime         / 1000d / 60d);

            this._currentTimeDrawable.Text = $"Time: {minutes:00}:{seconds:00}:{milliseconds:000}";
            this._progressBar.Progress     = (float)pTypingGame.MusicTrack.CurrentTime * 1000f / (float)pTypingGame.MusicTrack.Length;

            base.Update(gameTime);
        }
    }
}
