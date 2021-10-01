using System;
using System.IO;
using System.Collections.Generic;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Audio;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using ManagedBass;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using pTyping.Songs;
using pTyping.Player;
using pTyping.Drawables;

namespace pTyping.Screens {
	public class PlayerScreen : Screen {
		public PlayerScore Score = new();
		public Song        Song;

		private TextDrawable     _scoreDrawable;
		private TextDrawable     _accuracyDrawable;
		private TextDrawable     _comboDrawable;
		private TexturedDrawable _recepticle;
		
		public AudioStream MusicTrack = new();
		public SoundEffect HitSound   = new();
		
		private List<NoteDrawable> _notes = new();

		public PlayerScreen(Song song) {
			this.Song = song;

			if (this.Song.Notes.Count == 0) {
				//TODO notify the user the map did not load correctly, for now, we just send back to the song selection menu
				FurballGame.Instance.ChangeScreen(new SongSelectionScreen(false, song));
			}

			#region UI
			this._scoreDrawable    = new TextDrawable(new Vector2(5, 5), FurballGame.DEFAULT_FONT, $"{this.Score.Score:00000000}", 60) {};
			this._accuracyDrawable = new TextDrawable(new Vector2(5, 5 + this._scoreDrawable.Size.Y), FurballGame.DEFAULT_FONT, $"{this.Score.Accuracy * 100:0.00}%", 60) {};
			this._comboDrawable = new TextDrawable(new Vector2(5, FurballGame.DEFAULT_WINDOW_HEIGHT - 5), FurballGame.DEFAULT_FONT, $"{this.Score.Combo}x", 70) {
				OriginType = OriginType.BottomLeft
			};

			this.Manager.Add(this._scoreDrawable);
			this.Manager.Add(this._accuracyDrawable);
			this.Manager.Add(this._comboDrawable);
			#endregion
			
			#region Recepticle

			Texture2D noteTexture = ContentManager.LoadMonogameAsset<Texture2D>("note");
			
			Vector2 recepticlePos = new(FurballGame.DEFAULT_WINDOW_WIDTH * 0.15f, FurballGame.DEFAULT_WINDOW_HEIGHT / 2f);
			this._recepticle = new TexturedDrawable(noteTexture, recepticlePos) {
				Scale = new(0.55f),
				OriginType = OriginType.Center
			};

			this.Manager.Add(this._recepticle);
			#endregion
			
			#region Notes
			Vector2 noteStartPos = new(FurballGame.DEFAULT_WINDOW_WIDTH + 100, FurballGame.DEFAULT_WINDOW_HEIGHT / 2f);
			foreach (Note note in this.Song.Notes) {
				NoteDrawable noteDrawable = new(new(noteStartPos.X, noteStartPos.Y + note.YOffset), noteTexture, FurballGame.DEFAULT_FONT, 30) {
					TimeSource    = this.MusicTrack,
					ColorOverride = note.Color,
					LabelTextDrawable = {
						Text = $"{note.TextToShow}\n({note.TextToType})",
						Scale = new(1f)
					},
					Scale = new(0.55f, 0.55f),
					OriginType = OriginType.Center
				};

				noteDrawable.Tweens.Add(new VectorTween(TweenType.Movement, new(noteStartPos.X, noteStartPos.Y + note.YOffset), recepticlePos, (int)(note.Time - Config.BaseApproachTime), (int)note.Time));

				this.Manager.Add(noteDrawable);

				noteDrawable.Note = note;
				this._notes.Add(noteDrawable);
			}
			#endregion

			#region Playfield decorations
			LinePrimitiveDrawable playfieldTopLine    = new(new Vector2(1, recepticlePos.Y - 50),FurballGame.DEFAULT_WINDOW_WIDTH, 0) {
				ColorOverride = Color.Gray
			};
			LinePrimitiveDrawable playfieldBottomLine = new(new Vector2(1, recepticlePos.Y + 50),FurballGame.DEFAULT_WINDOW_WIDTH, 0) {
				ColorOverride = Color.Gray
			};
			this.Manager.Add(playfieldTopLine);
			this.Manager.Add(playfieldBottomLine);

			RectanglePrimitiveDrawable playfieldBackgroundCover = new(new(0, recepticlePos.Y - 50), new(FurballGame.DEFAULT_WINDOW_WIDTH, 100), 0f, true) {
				ColorOverride = new(100, 100, 100, 100),
				Depth = 0.9f
			};
			this.Manager.Add(playfieldBackgroundCover);
			
			#region background image
			this.Manager.Add(pTypingGame.CurrentSongBackground);
			
			pTypingGame.CurrentSongBackground.Tweens.Add(new ColorTween(TweenType.Color, Color.White, new(1f * (1f - Config.BackgroundDim) , 1f * (1f - Config.BackgroundDim), 1f * (1f - Config.BackgroundDim)), pTypingGame.CurrentSongBackground.TimeSource.GetCurrentTime(), pTypingGame.CurrentSongBackground.TimeSource.GetCurrentTime() + 1000));
			pTypingGame.LoadBackgroundFromSong(this.Song);
			#endregion
			#endregion
			
			this.HitSound.Load(ContentManager.LoadRawAsset("hitsound.wav", ContentSource.User));

			this.HitSound.Volume = Config.Volume;
			Config.Volume.OnChange += this.OnVolumeChange;
			
			this.Play();

			FurballGame.InputManager.OnKeyDown    += this.OnKeyPress;
			FurballGame.Instance.Window.TextInput += this.OnCharacterTyped;
		}
		private void OnVolumeChange(object _, float newVolume) {
			this.HitSound.Volume = newVolume;
		}

		protected override void Dispose(bool disposing) {
			this.MusicTrack.Stop();
			// this.MusicTrack.Free();
			
			FurballGame.InputManager.OnKeyDown    -= this.OnKeyPress;
			FurballGame.Instance.Window.TextInput -= this.OnCharacterTyped;
			
			Config.Volume.OnChange -= this.OnVolumeChange;

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
					if (result) {
						this.HitSound.Play();
						this.NoteUpdate(true);
					}

					if (this.Song.AllNotesHit()) {
						this.EndScore();
					}
					break;
				}
			}
		}

		private void NoteUpdate(bool wasHit) {
			int numberHit  = 0;
			double amountHit  = 0;
			int numberMiss = 0;
			foreach (NoteDrawable noteDrawable in this._notes) {
				switch (noteDrawable.Note.Hit) {
					case HitResult.Hit:
						numberHit++;
						amountHit += noteDrawable.Note.HitAmount;
						break;
					case HitResult.Miss:
						numberMiss++;
						amountHit += noteDrawable.Note.HitAmount;
						break;
				}
			}

			if (numberHit + numberMiss == 0) this.Score.Accuracy = 1d;
			else {
				this.Score.Accuracy = amountHit / ((double)numberHit + (double)numberMiss);
			}

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
			
			this.MusicTrack.Load(ContentManager.LoadRawAsset(qualifiedAudioPath, ContentSource.External));
			this.MusicTrack.Volume    =  Config.Volume;
			this.MusicTrack.Play();
		}

		public void Pause() {
			if(this.MusicTrack.PlaybackState       == PlaybackState.Playing) this.MusicTrack.Pause();
			else if (this.MusicTrack.PlaybackState == PlaybackState.Paused) this.MusicTrack.Resume();
		}
	}
}
