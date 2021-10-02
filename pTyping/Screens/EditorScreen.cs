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
		
		private UiTextBoxDrawable _textToTypeInput;
		private TextDrawable      _textToTypeInputLabel;
		private UiTextBoxDrawable _textToShowInput;
		private TextDrawable      _textToShowInputLabel;
		private UiTextBoxDrawable _colorRInput;
		private TextDrawable      _colorRInputLabel;
		private UiTextBoxDrawable _colorGInput;
		private TextDrawable      _colorGInputLabel;
		private UiTextBoxDrawable _colorBInput;
		private TextDrawable      _colorBInputLabel;
		private UiTextBoxDrawable _colorAInput;
		private TextDrawable      _colorAInputLabel;

		private UiButtonDrawable _editorToolSelect;
		private UiButtonDrawable _editorToolCreateNote;

		private List<NoteDrawable> _notes = new();
		private List<NoteDrawable> _timelineBars = new();

		public EditorTool  CurrentTool = EditorTool.None;
		
		private Texture2D _noteTexture;
		private bool      _isDragging = false;
		
		public EditorScreen() {
			pTypingGame.MusicTrack.Stop();
		
			this._currentTimeDrawable = new TextDrawable(new Vector2(5, 5), FurballGame.DEFAULT_FONT, "", 60);
			
			this.Manager.Add(this._currentTimeDrawable);

			this._noteTexture = ContentManager.LoadMonogameAsset<Texture2D>("note");
			
			Vector2 recepticlePos = new(FurballGame.DEFAULT_WINDOW_WIDTH * 0.15f, FurballGame.DEFAULT_WINDOW_HEIGHT / 2f);
			this._recepticle = new TexturedDrawable(this._noteTexture, recepticlePos) {
				Scale      = new(0.55f),
				OriginType = OriginType.Center
			};
			
			this.Manager.Add(this._recepticle);

			foreach (Note note in pTypingGame.CurrentSong.Value.Notes) {
				NoteDrawable noteDrawable = new(new Vector2(PlayerScreen.NoteStartPos.X, PlayerScreen.NoteStartPos.Y + note.YOffset), this._noteTexture, FurballGame.DEFAULT_FONT, 30) {
					TimeSource    = pTypingGame.MusicTrack,
					ColorOverride = note.Color,
					LabelTextDrawable = {
						Text  = $"{note.TextToShow}\n({note.TextToType})",
						Scale = new(1f)
					},
					Scale      = new(0.55f, 0.55f),
					OriginType = OriginType.Center,
					Note = note
				};

				noteDrawable.OnClick += delegate {
					if (this._isDragging) return;
					
					this.OnNoteClick(noteDrawable);
				};
				
				noteDrawable.OnDrag += delegate(object _, Point point) {
					this.OnNoteDrag(noteDrawable, point);
				};
				
				noteDrawable.OnDragBegin += delegate {
					_isDragging = true;
				};
				
				noteDrawable.OnDragEnd += delegate {
					this._isDragging = true;
				};

				noteDrawable.Tweens.Add(new VectorTween(TweenType.Movement, new(PlayerScreen.NoteStartPos.X, PlayerScreen.NoteStartPos.Y + note.YOffset), PlayerScreen.RecepticlePos, (int)(note.Time - Config.BaseApproachTime), (int)note.Time));
				
				this.Manager.Add(noteDrawable);
				this._notes.Add(noteDrawable);
			}
			
			#region Playfield decorations
			LinePrimitiveDrawable playfieldTopLine = new(new Vector2(1, recepticlePos.Y - 50),FurballGame.DEFAULT_WINDOW_WIDTH, 0) {
				ColorOverride = Color.Gray
			};
			LinePrimitiveDrawable playfieldBottomLine = new(new Vector2(1, recepticlePos.Y + 50),FurballGame.DEFAULT_WINDOW_WIDTH, 0) {
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
			
			pTypingGame.CurrentSongBackground.Tweens.Add(new ColorTween(TweenType.Color, pTypingGame.CurrentSongBackground.ColorOverride, new(0.3f , 0.3f, 0.3f), pTypingGame.CurrentSongBackground.TimeSource.GetCurrentTime(), pTypingGame.CurrentSongBackground.TimeSource.GetCurrentTime() + 1000));
			pTypingGame.LoadBackgroundFromSong(pTypingGame.CurrentSong.Value);
			#endregion
			#endregion
			
			this._selectionRect = new() {
				RectSize      = new(100, 100),
				Filled        = false,
				Thickness     = 2f,
				ColorOverride = Color.Gray,
				Visible       = false,
				Clickable     = false,
				OriginType    = OriginType.Center
			};

			this.Manager.Add(this._selectionRect);

			this._createLine = new LinePrimitiveDrawable(new Vector2(0, 0), 80f, (float)Math.PI / 2f) {
				Visible = false
			};
			
			this.Manager.Add(this._createLine);
			
			this._progressBar = new UiProgressBarDrawable(new Vector2(0, FurballGame.DEFAULT_WINDOW_HEIGHT), FurballGame.DEFAULT_FONT, new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 200, 40), Color.Gray, Color.DarkGray, Color.White) {
				OriginType = OriginType.BottomLeft
			};

			this.Manager.Add(this._progressBar);

			Texture2D editorButtonsTexture2D = ContentManager.LoadMonogameAsset<Texture2D>("editorbuttons", ContentSource.User);

			TexturedDrawable playButton = new(editorButtonsTexture2D, new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 150, FurballGame.DEFAULT_WINDOW_HEIGHT), TexturePositions.EDITOR_PLAY) {
				Scale      = new (0.5f, 0.5f),
				OriginType = OriginType.BottomRight
			};
			TexturedDrawable pauseButton = new(editorButtonsTexture2D, new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 100, FurballGame.DEFAULT_WINDOW_HEIGHT), TexturePositions.EDITOR_PAUSE) {
				Scale      = new (0.5f, 0.5f),
				OriginType = OriginType.BottomRight
			};
			TexturedDrawable rightButton = new(editorButtonsTexture2D, new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH, FurballGame.DEFAULT_WINDOW_HEIGHT), TexturePositions.EDITOR_RIGHT) {
				Scale      = new (0.5f, 0.5f),
				OriginType = OriginType.BottomRight
			};
			TexturedDrawable leftButton = new (editorButtonsTexture2D, new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 50, FurballGame.DEFAULT_WINDOW_HEIGHT), TexturePositions.EDITOR_LEFT) {
				Scale      = new (0.5f, 0.5f),
				OriginType = OriginType.BottomRight
			};

			playButton.OnClick += delegate {
				pTypingGame.PlayMusic();
			};
			
			pauseButton.OnClick += delegate {
				pTypingGame.PauseResumeMusic();
			};

			leftButton.OnClick += delegate {
				if(pTypingGame.CurrentSong.Value.Notes.Count > 0)
					pTypingGame.MusicTrack.SeekTo(pTypingGame.CurrentSong.Value.Notes.First().Time);
			};
			
			rightButton.OnClick += delegate {
				if(pTypingGame.CurrentSong.Value.Notes.Count > 0)
					pTypingGame.MusicTrack.SeekTo(pTypingGame.CurrentSong.Value.Notes.Last().Time);
			};
			
			this.Manager.Add(playButton);
			this.Manager.Add(pauseButton);
			this.Manager.Add(rightButton);
			this.Manager.Add(leftButton);
			
			this._editorToolSelect = new(new Vector2(0, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.1f), "Select", FurballGame.DEFAULT_FONT, 30, Color.Blue, Color.White, Color.White, Vector2.Zero);
			this._editorToolSelect.OnClick += delegate {
				this.ChangeTool(EditorTool.Select);
			};
			
			this._editorToolCreateNote = new(new Vector2(0, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.2f), "Add Note", FurballGame.DEFAULT_FONT, 30, Color.Blue, Color.White, Color.White, Vector2.Zero);
			this._editorToolCreateNote.OnClick += delegate {
				this.ChangeTool(EditorTool.CreateNote);
			};

			this.Manager.Add(this._editorToolSelect);
			this.Manager.Add(this._editorToolCreateNote);
			
			this._textToTypeInputLabel = new(new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH * 0.25f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.70f), FurballGame.DEFAULT_FONT, "Text To Type", 25) {
				OriginType = OriginType.Center,
				Visible    = false
			};
			this._textToTypeInput = new(new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH * 0.25f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.75f), FurballGame.DEFAULT_FONT, "select note", 25, 150f) {
				OriginType = OriginType.Center,
				Visible    = false
			};
			this._textToTypeInput.OnLetterTyped += this.UpdateTextInputs;
			this._textToTypeInput.OnLetterRemoved += this.UpdateTextInputs;
				
			this._textToShowInputLabel = new(new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH * 0.75f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.70f), FurballGame.DEFAULT_FONT, "Text To Show", 25) {
				OriginType = OriginType.Center,
				Visible    = false
			};
			this._textToShowInput = new(new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH * 0.75f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.75f), FurballGame.DEFAULT_FONT, "select note", 25, 150f) {
				OriginType = OriginType.Center,
				Visible    = false
			};
			this._textToShowInput.OnLetterTyped   += this.UpdateTextInputs;
			this._textToShowInput.OnLetterRemoved += this.UpdateTextInputs;

			this.Manager.Add(this._textToTypeInput);
			this.Manager.Add(this._textToTypeInputLabel);
			this.Manager.Add(this._textToShowInput);
			this.Manager.Add(this._textToShowInputLabel);
			
			this.ChangeTool(EditorTool.Select);
			
			FurballGame.Instance.Window.TextInput  += this.OnCharacterTyped;
			FurballGame.InputManager.OnKeyDown     += this.OnKeyPress;
			FurballGame.InputManager.OnMouseScroll += this.OnMouseScroll;
			FurballGame.InputManager.OnMouseDown   += this.OnClick;
			FurballGame.InputManager.OnMouseMove   += this.OnMouseMove;
		}

		private void UpdateTextInputs(object sender, char _) {
			if (this._selectedNote == null)
				return;
				
			this._selectedNote.Note.TextToType = pTypingGame.Alphanumeric.Replace(this._textToTypeInput.Text.Trim(), string.Empty);
			this._selectedNote.Note.TextToShow = this._textToShowInput.Text.Trim();

			this._selectedNote.LabelTextDrawable.Text = $"{this._selectedNote.Note.TextToShow}\n({this._selectedNote.Note.TextToType})";
		}

		public void ChangeTool(EditorTool tool) {
			if (this.CurrentTool == tool) return;
			
			this.DeselectNote();
			this.CurrentTool = tool;

			if (tool == EditorTool.Select) {
				this._textToShowInput.Visible      = true;
				this._textToShowInputLabel.Visible = true;
				this._textToTypeInput.Visible      = true;
				this._textToTypeInputLabel.Visible = true;
				
				this._editorToolSelect.ButtonColor = Color.Red;
				this._editorToolSelect.Tweens.Add(new ColorTween(TweenType.Color, this._editorToolSelect.ColorOverride, Color.Red, this._editorToolSelect.TimeSource.GetCurrentTime(), this._editorToolSelect.TimeSource.GetCurrentTime() + 150));
			} else {
				this._textToShowInput.Visible      = false; 
				this._textToShowInputLabel.Visible = false; 
				this._textToTypeInput.Visible      = false;
				this._textToTypeInputLabel.Visible = false;
								
				this._editorToolSelect.ButtonColor = Color.Blue;
				this._editorToolSelect.Tweens.Add(new ColorTween(TweenType.Color, this._editorToolSelect.ColorOverride, Color.Blue, this._editorToolSelect.TimeSource.GetCurrentTime(), this._editorToolSelect.TimeSource.GetCurrentTime() + 150));
			}
			
			if (tool == EditorTool.CreateNote) {
				this._editorToolCreateNote.ButtonColor = Color.Red;
				this._editorToolCreateNote.Tweens.Add(new ColorTween(TweenType.Color, this._editorToolCreateNote.ColorOverride, Color.Red, this._editorToolCreateNote.TimeSource.GetCurrentTime(), this._editorToolCreateNote.TimeSource.GetCurrentTime() + 150));
			} else {
				this._editorToolCreateNote.ButtonColor = Color.Blue;
				this._editorToolCreateNote.Tweens.Add(new ColorTween(TweenType.Color, this._editorToolCreateNote.ColorOverride, Color.Blue, this._editorToolCreateNote.TimeSource.GetCurrentTime(), this._editorToolCreateNote.TimeSource.GetCurrentTime() + 150));
				
				this._createLine.Visible = false;
			}
		}

		private double _mouseTime;
		private void OnMouseMove(object sender, (Point, string) e) {
			(int x, int y) = e.Item1;
			if (y < PlayerScreen.RecepticlePos.Y + 40f && y > PlayerScreen.RecepticlePos.Y - 40f) {
				if(this.CurrentTool == EditorTool.CreateNote)
					this._createLine.Visible    = true;
				this._createLine.OriginType = OriginType.Center;
				
				double currentTime  = pTypingGame.MusicTrack.CurrentTime      * 1000;
				double reticulePos  = FurballGame.DEFAULT_WINDOW_WIDTH * 0.15f;
				double noteStartPos = FurballGame.DEFAULT_WINDOW_WIDTH + 100;
				
				double distanceToReticule = x - reticulePos;

				double timeToReticule = distanceToReticule / (noteStartPos - reticulePos) * Config.BaseApproachTime;

				double timeAtCursor = currentTime + timeToReticule;

				double noteLength = pTypingGame.CurrentSong.Value.DividedNoteLength(timeAtCursor);
				
				double snappedTimeAtCursor = Math.Round((timeAtCursor - pTypingGame.CurrentSong.Value.CurrentTimingPoint(timeAtCursor).Time) / noteLength) * noteLength + pTypingGame.CurrentSong.Value.CurrentTimingPoint(timeAtCursor).Time;
				this._mouseTime = snappedTimeAtCursor;

				double distanceInTime = snappedTimeAtCursor - currentTime;

				double scaleTime = distanceInTime     / Config.BaseApproachTime;
				double scaleRaw  = distanceToReticule / (noteStartPos - reticulePos);
				
				double newX = scaleTime * (noteStartPos - reticulePos) + reticulePos;
				
				this._createLine.Position = new Vector2((float)newX, PlayerScreen.RecepticlePos.Y - 40);
			} else {
				this._createLine.Visible = false;
			}
		}

		private void OnClick(object sender, (MouseButton, string) e) {
			if (this.CurrentTool == EditorTool.CreateNote) {
				Note note = new() {
					TextToShow = "a",
					TextToType = "a",
					Time       = this._mouseTime
				};

				(int x, int y) = FurballGame.InputManager.CursorStates.Where(state => state.Name == e.Item2).ToList()[0].Position;
				if (y < PlayerScreen.RecepticlePos.Y + 40f && y > PlayerScreen.RecepticlePos.Y - 40f) {
					NoteDrawable noteDrawable = new(PlayerScreen.NoteStartPos, this._noteTexture, FurballGame.DEFAULT_FONT, 30) {
						TimeSource    = pTypingGame.MusicTrack,
						ColorOverride = note.Color,
						LabelTextDrawable = {
							Text  = $"{note.TextToShow}\n({note.TextToType})",
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
						_isDragging = true;
					};
				
					noteDrawable.OnDragEnd += delegate {
						this._isDragging = true;
					};

					noteDrawable.Tweens.Add(new VectorTween(TweenType.Movement, PlayerScreen.NoteStartPos, PlayerScreen.RecepticlePos, (int)(note.Time - Config.BaseApproachTime), (int)note.Time));
				
					this.Manager.Add(noteDrawable);
					this._notes.Add(noteDrawable);
					pTypingGame.CurrentSong.Value.Notes.Add(noteDrawable.Note);
				}
			}
		}

		public void OnNoteDrag(NoteDrawable noteDrawable, Point cursorPos) {
			if (this._selectedNote != noteDrawable) return;
			
			noteDrawable.Tweens.Clear();
			this._selectionRect.Tweens.Clear();

			noteDrawable.Note.Time = this._mouseTime;
			
			noteDrawable.Tweens.Add(new VectorTween(TweenType.Movement, new(PlayerScreen.NoteStartPos.X , PlayerScreen.NoteStartPos.Y + noteDrawable.Note.YOffset), PlayerScreen.RecepticlePos, (int)(noteDrawable.Note.Time        - Config.BaseApproachTime), (int)noteDrawable.Note.Time));
			this._selectionRect.Tweens.Add(new VectorTween(TweenType.Movement, new(PlayerScreen.NoteStartPos.X , PlayerScreen.NoteStartPos.Y + noteDrawable.Note.YOffset), PlayerScreen.RecepticlePos, (int)(noteDrawable.Note.Time - Config.BaseApproachTime), (int)noteDrawable.Note.Time));
		}

		public void OnNoteClick(NoteDrawable noteDrawable) {
			if (this.CurrentTool != EditorTool.Select) return;
			
			this._selectedNote             = noteDrawable;
			this._selectionRect.Visible    = true;
			this._selectionRect.TimeSource = noteDrawable.TimeSource;
			this._selectionRect.Tweens     = noteDrawable.Tweens.Where(tween => tween.TweenType == TweenType.Movement && tween is VectorTween).ToList();

			this._textToShowInput.Text = this._selectedNote.Note.TextToShow;
			this._textToTypeInput.Text = this._selectedNote.Note.TextToType;
		}

		public void DeselectNote(bool delete = false) {
			if (this._selectedNote == null)
				return;

			this._selectionRect.Visible = false;
			
			if(delete) {
				pTypingGame.CurrentSong.Value.Notes.Remove(this._selectedNote.Note);
				this._notes.Remove(this._selectedNote);
				this._selectedNote.Visible = false;
				this._selectedNote.ClearEvents();
				
				this.Manager.Remove(this._selectedNote);
			}
						
			this._selectedNote = null;
		}

		protected override void Dispose(bool disposing) {
			FurballGame.Instance.Window.TextInput  -= this.OnCharacterTyped;
			FurballGame.InputManager.OnKeyDown     -= this.OnKeyPress;
			FurballGame.InputManager.OnMouseScroll -= this.OnMouseScroll;
			FurballGame.InputManager.OnMouseDown   -= this.OnClick;
			FurballGame.InputManager.OnMouseMove   -= this.OnMouseMove;

			base.Dispose(disposing);
		}
		
		private void OnMouseScroll(object sender, (int, string) args) {
			if (args.Item1 > 0) {
				this.TimelineMove(false);
			} else {
				this.TimelineMove(true);
			}
		}

		public void TimelineMove(bool right) {
			double currentTime = pTypingGame.MusicTrack.CurrentTime * 1000d;
					
			double noteLength   = pTypingGame.CurrentSong.Value.DividedNoteLength(currentTime);
			double timeToSeekTo = Math.Round((pTypingGame.MusicTrack.CurrentTime * 1000d - pTypingGame.CurrentSong.Value.CurrentTimingPoint(currentTime).Time) / noteLength) * noteLength;
					
			timeToSeekTo += pTypingGame.CurrentSong.Value.CurrentTimingPoint(currentTime).Time;

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
					FurballGame.Instance.ChangeScreen(new SongSelectionScreen(true));
					break;
				case Keys.Delete: {
					// Delete the current note
					this.DeselectNote(true);
					break;
				}
				case Keys.S: {
					// Save the song if ctrl+s is pressed
					if (FurballGame.InputManager.HeldKeys.Contains(Keys.LeftControl)) pTypingGame.CurrentSong.Value.Save();
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

		public void OnCharacterTyped(object sender, TextInputEventArgs args) {
			
		}

		public override void Update(GameTime gameTime) {
			for (int i = 0; i < this._notes.Count; i++) {
				// if (this._notes[i].Note.Hit != HitResult.Unknown) continue;

				NoteDrawable noteDrawable = this._notes[i];

				if (pTypingGame.MusicTrack.CurrentTime * 1000 > noteDrawable.Note.Time+10) {
					noteDrawable.Visible = false;
				} else {
					noteDrawable.Visible = true;
				}
			}
			
			this._currentTimeDrawable.Text = $"{pTypingGame.MusicTrack.CurrentTime * 1000}";
			this._progressBar.Progress     = (float)pTypingGame.MusicTrack.CurrentTime * 1000f / (float)pTypingGame.MusicTrack.Length;
			
			base.Update(gameTime);
		}
	}
}
