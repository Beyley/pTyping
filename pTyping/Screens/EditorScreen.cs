using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Audio;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Engine.Engine.Input;
using ManagedBass;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using pTyping.Drawables;
using pTyping.Songs;

namespace pTyping.Screens {
	public enum EditorTool {
		Select,
		CreateNote,
	}
	
	public class EditorScreen : Screen {
		public Song Song;

		private TextDrawable               _currentTimeDrawable;
		private BaseDrawable               _recepticle;
		private NoteDrawable               _selectedNote;
		private RectanglePrimitiveDrawable _selectionRect;
		private UiProgressBarDrawable      _progressBar;
		private LinePrimitiveDrawable      _createLine;
		
		private List<NoteDrawable> _notes = new();
		private List<NoteDrawable> _timelineBars = new();

		public AudioStream MusicTrack;
		public EditorTool  CurrentTool = EditorTool.Select;

		private readonly Vector2 _recepticlePos = new(FurballGame.DEFAULT_WINDOW_WIDTH * 0.15f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.75f);
		
		public EditorScreen(Song song) {
			this.Song = song;
			
			this.MusicTrack = new();
			this.LoadAudio();

			this._currentTimeDrawable    = new TextDrawable(FurballGame.DEFAULT_FONT, "", 60) {
				Position = new Vector2(5, 5)
			};
			
			this.Manager.Add(this._currentTimeDrawable);

			this._recepticle = new CirclePrimitiveDrawable(this._recepticlePos, 40f, 1f, Color.White);
			
			this.Manager.Add(this._recepticle);

			Vector2 noteStartPos = new(FurballGame.DEFAULT_WINDOW_WIDTH + 100, this._recepticlePos.Y);
			foreach (Note note in this.Song.Notes) {
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

			this._createLine = new LinePrimitiveDrawable(80, (float)Math.PI / 2f) {
				Visible = false,
				Thickness = 2f
			};
			
			this.Manager.Add(this._createLine);
			
			this._progressBar = new UiProgressBarDrawable(FurballGame.DEFAULT_FONT, new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH, 40), Color.Gray, Color.DarkGray, Color.White) {
				Position = new Vector2(0, FurballGame.DEFAULT_WINDOW_HEIGHT),
				OriginType = OriginType.BottomLeft,
			};

			this.Manager.Add(this._progressBar);

			UiButtonDrawable selectTool = new("Select", FurballGame.DEFAULT_FONT, 30, Color.Blue, Color.White, Color.White) {
				Position = new Vector2(0, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.4f)
			};

			selectTool.OnClick += delegate {
				this.CurrentTool = EditorTool.Select;
			};
			
			UiButtonDrawable createNoteTool = new("Add Note", FurballGame.DEFAULT_FONT, 30, Color.Blue, Color.White, Color.White) {
				Position = new Vector2(0, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.5f)
			};
			
			createNoteTool.OnClick += delegate {
				this.CurrentTool = EditorTool.CreateNote;
				this.DeselectNote();
			};

			this.Manager.Add(selectTool);
			this.Manager.Add(createNoteTool);
			
			FurballGame.Instance.Window.TextInput  += this.OnCharacterTyped;
			FurballGame.InputManager.OnKeyDown     += this.OnKeyPress;
			FurballGame.InputManager.OnMouseScroll += this.OnMouseScroll;
			FurballGame.InputManager.OnMouseDown   += this.OnClick;
			FurballGame.InputManager.OnMouseMove   += this.OnMouseMove;
		}

		private double _mouseTime;
		private void OnMouseMove(object? sender, (Point, string) e) {
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
					
					this._createLine.Position = new Vector2((float)newX, this._recepticlePos.Y);
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
					TextToShow = "suc", 
					TextToType =  "suc",
					Time = this._mouseTime,
				};

				(int x, int y) = FurballGame.InputManager.CursorStates.Where(state => state.Name == e.Item2).ToList()[0].State.Position;
				if (y < this._recepticlePos.Y + 40f || y > this._recepticlePos.Y - 40f) {
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
				}
			}
		}

		public void OnNoteClick(NoteDrawable noteDrawable) {
			if (this.CurrentTool != EditorTool.Select) return;
			
			this._selectedNote             = noteDrawable;
			this._selectionRect.Visible    = true;
			this._selectionRect.TimeSource = noteDrawable.TimeSource;
			this._selectionRect.Tweens     = noteDrawable.Tweens.Where(tween => tween.TweenType == TweenType.Movement && tween is VectorTween).ToList();
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
		
		private void OnMouseScroll(object? sender, (int, string) args) {
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
					((FurballGame)FurballGame.Instance).ChangeScreen(new SongSelectionScreen(true));
					break;
				case Keys.Delete: {
					this.DeselectNote(true);
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
