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
using pTyping.Songs;

namespace pTyping.Screens {
	public class PlayerScreen : Screen {
		public long   Score;
		public double Accuracy = 1d;
		public int    Combo;
		public Song   Song;

		private TextDrawable _scoreDrawable;
		private TextDrawable _accuracyDrawable;
		private TextDrawable _comboDrawable;
		private BaseDrawable _recepticle;
		
		public AudioStream MusicTrack = new();

		private List<NoteDrawable> _notes = new();

		public PlayerScreen(Song song) {
			this.Song = song;

			this._scoreDrawable    = new TextDrawable(FurballGame.DEFAULT_FONT, $"{this.Score:00000000}", 30) {
				Position = new Vector2(5, 5)
			};
			this._accuracyDrawable = new TextDrawable(FurballGame.DEFAULT_FONT, $"{this.Accuracy * 100:0.00}%", 30) {
				Position = new Vector2(5, 5 + this._scoreDrawable.Size.Y)
			};
			this._comboDrawable = new TextDrawable(FurballGame.DEFAULT_FONT, $"{this.Combo}x", 30) {
				Position = new Vector2(5, FurballGame.DEFAULT_WINDOW_HEIGHT - 5),
				OriginType = OriginType.BottomLeft
			};
			
			this.Manager.Add(this._scoreDrawable);
			this.Manager.Add(this._accuracyDrawable);
			this.Manager.Add(this._comboDrawable);

			Vector2 recepticlePos = new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH * 0.15f, FurballGame.DEFAULT_WINDOW_HEIGHT / 2f);
			this._recepticle = new CirclePrimitiveDrawable(40f, 1f, Color.White) {
				Position = recepticlePos,
				Sides    = 20
			};
			
			this.Manager.Add(this._recepticle);

			Vector2 noteStartPos = new Vector2(FurballGame.DEFAULT_WINDOW_WIDTH + 100, FurballGame.DEFAULT_WINDOW_HEIGHT / 2f);
			foreach (Note note in this.Song.Notes) {
				NoteDrawable noteDrawable = new(FurballGame.DEFAULT_FONT, note.TextToShow, 30) {
					Position   = noteStartPos,
					TimeSource = this.MusicTrack,
					ColorOverride = Color.Red
				};

				noteDrawable.Tweens.Add(new VectorTween(TweenType.Movement, noteStartPos, recepticlePos, note.Time - Config.ApproachTime, note.Time));
				
				this.Manager.Add(noteDrawable);

				noteDrawable.Note = note;
				this._notes.Add(noteDrawable);
			}

			this.Play();

			FurballGame.InputManager.OnKeyDown    += this.OnKeyPress;
			FurballGame.Instance.Window.TextInput += this.OnCharacterTyped;
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

			if (numberHit + numberMiss == 0) this.Accuracy = 1d;
			else
				this.Accuracy = (double)numberHit / ((double)numberHit + (double)numberMiss);

			if (wasHit) {
				this.Combo++;
				this.Score += 100 * this.Combo;
			} else {
				this.Combo = 0;
			}
		}

		public override void Update(GameTime gameTime) {
			for (int i = 0; i < this._notes.Count; i++) {
				if (this._notes[i].Note.Hit != HitResult.Unknown) continue;

				NoteDrawable noteDrawable = this._notes[i];

				if (this.MusicTrack.CurrentTime * 1000 - noteDrawable.Note.Time > Config.HitWindow) {
					noteDrawable.Miss();
					this.NoteUpdate(false);
				}
			}
			
			this._scoreDrawable.Text    = $"{this.Score:00000000}";
			this._accuracyDrawable.Text = $"{this.Accuracy * 100:0.00}%";
			this._comboDrawable.Text    = $"{this.Combo}x";
			
			base.Update(gameTime);
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
