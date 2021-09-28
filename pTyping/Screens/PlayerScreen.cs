using System;
using System.IO;
using System.Collections.Generic;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Audio;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using ManagedBass;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using pTyping.Drawables;
using pTyping.Player;
using pTyping.Songs;

namespace pTyping.Screens {
	public class PlayerScreen : Screen {
		public PlayerScore Score = new();
		public Song        Song;

		private TextDrawable _scoreDrawable;
		private TextDrawable _accuracyDrawable;
		private TextDrawable _comboDrawable;
		private BaseDrawable _recepticle;
		
		public AudioStream MusicTrack = new();

		private List<NoteDrawable> _notes = new();

		public PlayerScreen(Song song) {
			this.Song = song;

			this._scoreDrawable    = new TextDrawable(new Vector2(5, 5), FurballGame.DEFAULT_FONT, $"{this.Score.Score:00000000}", 60) {};
			this._accuracyDrawable = new TextDrawable(new Vector2(5, 5 + this._scoreDrawable.Size.Y), FurballGame.DEFAULT_FONT, $"{this.Score.Accuracy * 100:0.00}%", 60) {};
			this._comboDrawable = new TextDrawable(new Vector2(5, FurballGame.DEFAULT_WINDOW_HEIGHT - 5), FurballGame.DEFAULT_FONT, $"{this.Score.Combo}x", 70) {
				OriginType = OriginType.BottomLeft
			};

			this.Manager.Add(this._scoreDrawable);
			this.Manager.Add(this._accuracyDrawable);
			this.Manager.Add(this._comboDrawable);

			Vector2 recepticlePos = new(FurballGame.DEFAULT_WINDOW_WIDTH * 0.15f, FurballGame.DEFAULT_WINDOW_HEIGHT / 2f);
			this._recepticle = new CirclePrimitiveDrawable(recepticlePos, 40f, 1f, Color.White, Color.Transparent);

			this.Manager.Add(this._recepticle);

			Vector2 noteStartPos = new(FurballGame.DEFAULT_WINDOW_WIDTH + 100, FurballGame.DEFAULT_WINDOW_HEIGHT / 2f);
			foreach (Note note in this.Song.Notes) {
				NoteDrawable noteDrawable = new(FurballGame.DEFAULT_FONT, 30) {
					Position      = new(noteStartPos.X, noteStartPos.Y + note.YOffset),
					TimeSource    = this.MusicTrack,
					ColorOverride = note.Color,
					LabelTextDrawable = {
						Text = $"{note.TextToShow}\n({note.TextToType})"
					}
				};

				noteDrawable.Tweens.Add(new VectorTween(TweenType.Movement, new(noteStartPos.X, noteStartPos.Y + note.YOffset), recepticlePos, (int)(note.Time - Config.ApproachTime), (int)note.Time));

				this.Manager.Add(noteDrawable);

				noteDrawable.Note = note;
				this._notes.Add(noteDrawable);
			}

			LinePrimitiveDrawable playfieldTopLine    = new(new Vector2(1, recepticlePos.Y - 50), new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH, recepticlePos.Y - 50), 10f) {
				ColorOverride = Color.Gray
			};
			LinePrimitiveDrawable playfieldBottomLine = new(new Vector2(1, recepticlePos.Y + 50), new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH, recepticlePos.Y + 50), 10f) {
				ColorOverride = Color.Gray
			};
			
			this.Manager.Add(playfieldTopLine);
			this.Manager.Add(playfieldBottomLine);

			this.Play();

			FurballGame.InputManager.OnKeyDown    += this.OnKeyPress;
			FurballGame.Instance.Window.TextInput += this.OnCharacterTyped;
		}
		
		protected override void Dispose(bool disposing) {
			this.MusicTrack.Stop();
			// this.MusicTrack.Free();
			
			FurballGame.InputManager.OnKeyDown    -= this.OnKeyPress;
			FurballGame.Instance.Window.TextInput -= this.OnCharacterTyped;
			
			base.Dispose(disposing);
		}

		public void OnKeyPress(object sender, Keys key) {
			if (key == Keys.Escape)
				this.Pause();
		}

		public void OnCharacterTyped(object sender, TextInputEventArgs args) {
			foreach (NoteDrawable noteDrawable in this._notes) {
				Note note = noteDrawable.Note;
				if (note.Hit != HitResult.Unknown) continue;

				if (note.NextToType == args.Character && Math.Abs(this.MusicTrack.CurrentTime * 1000 - note.Time) < Config.HitWindow) {
					bool result = noteDrawable.Type();
					if (result) this.NoteUpdate(true);

					if (this.Song.AllNotesHit()) {
						this.EndScore();
					}
					break;
				}
			}
		}

		private void NoteUpdate(bool wasHit) {
			int numberHit  = 0;
			int numberMiss = 0;
			foreach (NoteDrawable noteDrawable in this._notes) {
				switch (noteDrawable.Note.Hit) {
					case HitResult.Hit:
						numberHit++;
						break;
					case HitResult.Miss:
						numberMiss++;
						break;
				}
			}

			if (numberHit + numberMiss == 0) this.Score.Accuracy = 1d;
			else
				this.Score.Accuracy = (double)numberHit / ((double)numberHit + (double)numberMiss);

			if (wasHit) {
				this.Score.Combo++;
				this.Score.Score += 100 * this.Score.Combo;
			} else {
				this.Score.Combo = 0;
			}
		}

		public override void Update(GameTime gameTime) {
			for (int i = 0; i < this._notes.Count; i++) {
				if (this._notes[i].Note.Hit != HitResult.Unknown) continue;

				NoteDrawable noteDrawable = this._notes[i];

				if (this.MusicTrack.CurrentTime * 1000 - noteDrawable.Note.Time > Config.HitWindow) {
					noteDrawable.Miss();
					this.NoteUpdate(false);
					
					if (this.Song.AllNotesHit()) {
						this.EndScore();
					}
				}
			}
			
			this._scoreDrawable.Text    = $"{this.Score.Score:00000000}";
			this._accuracyDrawable.Text = $"{this.Score.Accuracy * 100:0.00}%";
			this._comboDrawable.Text    = $"{this.Score.Combo}x";
			
			base.Update(gameTime);
		}

		public void EndScore() {
			FurballGame.Instance.ChangeScreen(new ScoreResultsScreen(this.Score, this.Song));
		}

		public void Play() {
			string qualifiedAudioPath = Path.Combine(this.Song.FileInfo.DirectoryName ?? string.Empty, this.Song.AudioPath);
			
			this.MusicTrack.Load(File.ReadAllBytes(qualifiedAudioPath));
			this.MusicTrack.Volume    =  Config.Volume;
			this.MusicTrack.Play();
		}

		public void Pause() {
			if(this.MusicTrack.PlaybackState       == PlaybackState.Playing) this.MusicTrack.Pause();
			else if (this.MusicTrack.PlaybackState == PlaybackState.Paused) this.MusicTrack.Resume();
		}
	}
}
