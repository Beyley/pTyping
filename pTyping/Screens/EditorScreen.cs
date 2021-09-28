using System;
using System.IO;
using System.Collections.Generic;
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
using Furball.Engine.Engine.Input;
using ManagedBass;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using pTyping.Drawables;
using pTyping.Songs;

namespace pTyping.Screens {
	public enum EditorTool {
		None,
		Select,
		CreateNote
	}
	
	public class EditorScreen : Screen {
		public Song Song;

		private TextDrawable               _currentTimeDrawable;
		private BaseDrawable               _recepticle;
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

		public AudioStream MusicTrack;
		public EditorTool  CurrentTool = EditorTool.None;

		private readonly Vector2 _recepticlePos = new(FurballGame.DEFAULT_WINDOW_WIDTH * 0.15f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.5f);
		
		public EditorScreen(Song song) {
			this.Song = song;
			
			this.MusicTrack = new();
			this.LoadAudio();

			this._currentTimeDrawable = new TextDrawable(new Vector2(5, 5), FurballGame.DEFAULT_FONT, "", 60);
			
			this.Manager.Add(this._currentTimeDrawable);

			this._recepticle = new CirclePrimitiveDrawable(this._recepticlePos, 40f, 1f, Color.White, Color.Transparent);
			
			this.Manager.Add(this._recepticle);

			Vector2 noteStartPos = new(FurballGame.DEFAULT_WINDOW_WIDTH + 100, this._recepticlePos.Y);
			foreach (Note note in this.Song.Notes) {
				NoteDrawable noteDrawable = new(FurballGame.DEFAULT_FONT, 30) {
					Position      = new Vector2(noteStartPos.X, noteStartPos.Y + note.YOffset),
					TimeSource    = this.MusicTrack,
					ColorOverride = note.Color,
					Note          = note,
					LabelTextDrawable = {
						Text = $"{note.TextToShow}\n({note.TextToType})"
					}
				};

				noteDrawable.OnClick += delegate {
					this.OnNoteClick(noteDrawable);
				};

				noteDrawable.Tweens.Add(new VectorTween(TweenType.Movement, new(noteStartPos.X, noteStartPos.Y + note.YOffset), this._recepticlePos, (int)(note.Time - Config.ApproachTime), (int)note.Time));
				
				this.Manager.Add(noteDrawable);
				this._notes.Add(noteDrawable);
			}
			
			this._selectionRect = new() {
				RectSize      = new(100, 100),
				Filled        = false,
				Thickness     = 2f,
				ColorOverride = Color.Gray,
				Visible       = false,
				OriginType = OriginType.Center
			};

			this.Manager.Add(this._selectionRect);

			this._createLine = new LinePrimitiveDrawable(new Vector2(0, 0), new Vector2(0, 40), 1f) {
				Visible = false
			};
			
			this.Manager.Add(this._createLine);
			
			this._progressBar = new UiProgressBarDrawable(new Vector2(0, FurballGame.DEFAULT_WINDOW_HEIGHT), FurballGame.DEFAULT_FONT, new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 200, 40), Color.Gray, Color.DarkGray, Color.White) {
				OriginType = OriginType.BottomLeft
			};

			this.Manager.Add(this._progressBar);

			Texture2D editorButtonsTexture2D = ContentReader.LoadMonogameAsset<Texture2D>("editorbuttons", ContentSource.User);

			TexturedDrawable playButton = new(editorButtonsTexture2D, new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 200 + 50, FurballGame.DEFAULT_WINDOW_HEIGHT), new Rectangle(0, 0, 80, 80)) {
				Scale      = new (0.5f, 0.5f),
				OriginType = OriginType.BottomRight
			};
			TexturedDrawable pauseButton = new(editorButtonsTexture2D, new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 200+100, FurballGame.DEFAULT_WINDOW_HEIGHT), new Rectangle(80, 0, 80, 80)) {
				Scale      = new (0.5f, 0.5f),
				OriginType = OriginType.BottomRight
			};
			TexturedDrawable leftButton = new(editorButtonsTexture2D, new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH - 200+150, FurballGame.DEFAULT_WINDOW_HEIGHT), new Rectangle(160, 0, 80, 80)) {
				Scale      = new (0.5f, 0.5f),
				OriginType = OriginType.BottomRight
			};
			TexturedDrawable rightButton = new (editorButtonsTexture2D, new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH, FurballGame.DEFAULT_WINDOW_HEIGHT), new Rectangle(240, 0, 80, 80)) {
				Scale      = new (0.5f, 0.5f),
				OriginType = OriginType.BottomRight
			};

			playButton.OnClick += delegate {
				this.MusicTrack.Play();
			};
			
			pauseButton.OnClick += delegate {
				this.Pause();
			};
			
			this.Manager.Add(playButton);
			this.Manager.Add(pauseButton);
			this.Manager.Add(leftButton);
			this.Manager.Add(rightButton);
			
			this._editorToolSelect = new(new Vector2(0, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.1f), "Select", FurballGame.DEFAULT_FONT, 30, Color.Blue, Color.White, Color.White);
			this._editorToolSelect.OnClick += delegate {
				this.ChangeTool(EditorTool.Select);
			};
			
			this._editorToolCreateNote = new(new Vector2(0, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.2f), "Add Note", FurballGame.DEFAULT_FONT, 30, Color.Blue, Color.White, Color.White);
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
				
			this._selectedNote.Note.TextToType = this._textToTypeInput.Text;
			this._selectedNote.Note.TextToShow = this._textToShowInput.Text;

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
			if(this.CurrentTool == EditorTool.CreateNote) {
				(int x, int y) = e.Item1;
				if (y < this._recepticlePos.Y + 40f && y > this._recepticlePos.Y - 40f) {
					this._createLine.Visible    = true;
					this._createLine.OriginType = OriginType.Center;
					
					double currentTime  = this.MusicTrack.CurrentTime      * 1000;
					double reticulePos  = FurballGame.DEFAULT_WINDOW_WIDTH * 0.15f;
					double noteStartPos = FurballGame.DEFAULT_WINDOW_WIDTH + 100;
					
					double distanceToReticule = x - reticulePos;

					double timeToReticule = distanceToReticule / (noteStartPos - reticulePos) * Config.ApproachTime;

					double timeAtCursor = currentTime + timeToReticule;

					double noteLength = this.Song.DividedNoteLength(timeAtCursor);
					
					double snappedTimeAtCursor = Math.Round((timeAtCursor - this.Song.CurrentTimingPoint(timeAtCursor).Time) / noteLength) * noteLength + this.Song.CurrentTimingPoint(timeAtCursor).Time;
					this._mouseTime = snappedTimeAtCursor;

					double distanceInTime = snappedTimeAtCursor - currentTime;

					double scaleTime = distanceInTime     / Config.ApproachTime;
					double scaleRaw  = distanceToReticule / (noteStartPos - reticulePos);
					
					double newX = scaleTime * (noteStartPos - reticulePos) + reticulePos;
					
					this._createLine.Position = new Vector2((float)newX, this._recepticlePos.Y - 40);
					this._createLine.EndPoint = new Vector2((float)newX, this._recepticlePos.Y + 40);
				}
				else {
					this._createLine.Visible = false;
				}
			}
		}

		private void OnClick(object sender, (MouseButton, string) e) {
			if (this.CurrentTool == EditorTool.CreateNote) {
				Vector2 noteStartPos = new(FurballGame.DEFAULT_WINDOW_WIDTH + 100, this._recepticlePos.Y);
				Note    note     = new() {
					TextToShow = "changeme", 
					TextToType =  "changeme",
					Time = this._mouseTime
				};

				(int x, int y) = FurballGame.InputManager.CursorStates.Where(state => state.Name == e.Item2).ToList()[0].Position;
				if (y < this._recepticlePos.Y + 40f && y > this._recepticlePos.Y - 40f) {
					NoteDrawable noteDrawable = new(FurballGame.DEFAULT_FONT, 30) {
						Position      = noteStartPos,
						TimeSource    = this.MusicTrack,
						ColorOverride = Color.Red,
						Note          = note,
						LabelTextDrawable = {
							Text = $"{note.TextToShow}\n({note.TextToType})"
						}
					};

					noteDrawable.OnClick += delegate {
						this.OnNoteClick(noteDrawable);
					};

					noteDrawable.Tweens.Add(new VectorTween(TweenType.Movement, noteStartPos, this._recepticlePos, (int)(note.Time - Config.ApproachTime), (int)note.Time));
				
					this.Manager.Add(noteDrawable);
					this._notes.Add(noteDrawable);
					this.Song.Notes.Add(noteDrawable.Note);
				}
			}
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
				this.Song.Notes.Remove(this._selectedNote.Note);
				this._notes.Remove(this._selectedNote);
				this._selectedNote.Visible = false;
			}
						
			this._selectedNote = null;
		}

		protected override void Dispose(bool disposing) {
			this.MusicTrack.Stop();
			this.MusicTrack.Free();
			
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
			double currentTime = this.MusicTrack.CurrentTime * 1000d;
					
			double noteLength   = this.Song.DividedNoteLength(currentTime);
			double timeToSeekTo = Math.Round((this.MusicTrack.CurrentTime * 1000d - this.Song.CurrentTimingPoint(currentTime).Time) / noteLength) * noteLength;
					
			timeToSeekTo += this.Song.CurrentTimingPoint(currentTime).Time;

			if (right)
				timeToSeekTo += noteLength;
			else
				timeToSeekTo -= noteLength;

			this.MusicTrack.SeekTo(Math.Max(timeToSeekTo, 0));
		}

		public void OnKeyPress(object sender, Keys key) {
			switch (key) {
				case Keys.Space:
					this.Pause();
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
					// Exit the editor
					((FurballGame)FurballGame.Instance).ChangeScreen(new SongSelectionScreen(true));
					break;
				case Keys.Delete: {
					// Delete the current note
					this.DeselectNote(true);
					break;
				}
				case Keys.S: {
					// Save the song if ctrl+s is pressed
					if (FurballGame.InputManager.HeldKeys.Contains(Keys.LeftControl)) this.Song.Save();
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
				if (this._notes[i].Note.Hit != HitResult.Unknown) continue;

				NoteDrawable noteDrawable = this._notes[i];

				if (this.MusicTrack.CurrentTime * 1000 > noteDrawable.Note.Time+10) {
					noteDrawable.Visible = false;
				} else {
					noteDrawable.Visible = true;
				}
			}
			
			this._currentTimeDrawable.Text = $"{this.MusicTrack.CurrentTime * 1000}";
			this._progressBar.Progress     = (float)this.MusicTrack.CurrentTime * 1000f / (float)this.MusicTrack.Length;
			
			base.Update(gameTime);
		}

		public void LoadAudio() {
			string qualifiedAudioPath = Path.Combine(this.Song.FileInfo.DirectoryName ?? string.Empty, this.Song.AudioPath);
			
			this.MusicTrack.Load(File.ReadAllBytes(qualifiedAudioPath));
			this.MusicTrack.Volume = Config.Volume;
		}

		public void Pause() {
			switch (this.MusicTrack.PlaybackState) {
				case PlaybackState.Playing:
					this.MusicTrack.Pause();
					break;
				case PlaybackState.Stopped:
					this.MusicTrack.Play();
					break;
				case PlaybackState.Paused:
					this.MusicTrack.Resume();
					break;
			}
		}
	}
}
