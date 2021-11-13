using System;
using System.Collections.Generic;
using System.Linq;
using Furball.Engine;
using Furball.Engine.Engine.Audio;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using pTyping.Engine;
using pTyping.Graphics.Player.Mods;
using pTyping.Scores;
using pTyping.Songs;
using pTyping.Songs.Events;

namespace pTyping.Graphics.Player {
    public class Player : CompositeDrawable {
        public override Vector2 Size => new(FurballGame.DEFAULT_WINDOW_WIDTH, 100);

        public const int SCORE_EXCELLENT = 1500;
        public const int SCORE_GOOD      = 1000;
        public const int SCORE_FAIR      = 500;
        public const int SCORE_POOR      = 0;

        public const int SCORE_PER_CHARACTER = 500;
        public const int SCORE_COMBO         = 10;
        public const int SCORE_COMBO_MAX     = 1000;

        public const int TIMING_EXCELLENT = 20;
        public const int TIMING_GOOD      = 50;
        public const int TIMING_FAIR      = 100;
        public const int TIMING_POOR      = 200;

        public static readonly Color COLOR_EXCELLENT = new(255, 255, 0);
        public static readonly Color COLOR_GOOD      = new(0, 255, 0);
        public static readonly Color COLOR_FAIR      = new(0, 128, 255);
        public static readonly Color COLOR_POOR      = new(128, 128, 128);

        public const float NOTE_HEIGHT = 50f;

        public static readonly Vector2 NOTE_START_POS = new(FurballGame.DEFAULT_WINDOW_WIDTH + 200, NOTE_HEIGHT);
        public static readonly Vector2 NOTE_END_POS   = new(-100, NOTE_HEIGHT);

        public int BaseApproachTime = ConVars.BaseApproachTime.Value;

        private readonly TexturedDrawable _recepticle;

        private readonly LinePrimitiveDrawable      _playfieldTopLine;
        private readonly LinePrimitiveDrawable      _playfieldBottomLine;
        private readonly RectanglePrimitiveDrawable _playfieldBackground;

        private readonly TextDrawable _typingIndicator;

        private readonly List<NoteDrawable>                       _notes    = new();
        private readonly List<Tuple<LinePrimitiveDrawable, bool>> _beatBars = new();

        public static readonly Vector2 RECEPTICLE_POS = new(FurballGame.DEFAULT_WINDOW_WIDTH * 0.15f, NOTE_HEIGHT);

        private readonly Texture2D _noteTexture;

        public Song Song;

        public PlayerScore Score;

        private int _noteToType;

        public SoundEffect HitSoundNormal = new();

        public bool RecordReplay = true;

        // private          bool              _playingReplay;
        // private readonly PlayerScore       _playingScoreReplay = new();
        public readonly  List<ReplayFrame> ReplayFrames        = new();

        public event EventHandler<Color> OnComboUpdate;
        public event EventHandler        OnAllNotesComplete;

        public Player(Song song) {
            this.Song = song;

            this.Score            = new(this.Song.MapHash, ConVars.Username.Value);
            this.Score.Mods       = pTypingGame.SelectedMods;
            this.Score.ModsString = string.Join(',', this.Score.Mods);

            this._playfieldTopLine = new(new Vector2(0, 0), FurballGame.DEFAULT_WINDOW_WIDTH, 0) {
                ColorOverride = Color.Gray
            };
            this._playfieldBottomLine = new(new Vector2(0, 100), FurballGame.DEFAULT_WINDOW_WIDTH, 0) {
                ColorOverride = Color.Gray
            };
            this._drawables.Add(this._playfieldTopLine);
            this._drawables.Add(this._playfieldBottomLine);

            this._playfieldBackground = new(new(0, 0), new(FurballGame.DEFAULT_WINDOW_WIDTH, 100), 0f, true) {
                ColorOverride = new(100, 100, 100, 100),
                Depth         = 0.9f
            };

            this._drawables.Add(this._playfieldBackground);

            this._typingIndicator = new(RECEPTICLE_POS, pTypingGame.JapaneseFont, "", 60) {
                OriginType = OriginType.Center
            };

            this._drawables.Add(this._typingIndicator);

            this._noteTexture = ContentManager.LoadMonogameAsset<Texture2D>("note");

            this._recepticle = new TexturedDrawable(this._noteTexture, RECEPTICLE_POS) {
                Scale      = new(0.55f),
                OriginType = OriginType.Center
            };

            this._drawables.Add(this._recepticle);

            //Called before creating the notes
            this.Score.Mods.ForEach(mod => mod.BeforeNoteCreate(this));

            this.CreateNotes();
            this.CreateCutOffIndicators();
            this.CreateBeatLines();

            this.HitSoundNormal.Load(ContentManager.LoadRawAsset("hitsound.wav", ContentSource.User));

            this.HitSoundNormal.Volume = ConVars.Volume.Value;

            //This wont be needed soon
            this._drawables = this._drawables.OrderByDescending(o => o.Depth).ToList();

            this.Play();

            foreach (PlayerMod mod in pTypingGame.SelectedMods)
                mod.OnMapStart(pTypingGame.MusicTrack, this._notes, this);
        }

        private void CreateBeatLines() {
            foreach (Event @event in this.Song.Events) {
                LinePrimitiveDrawable drawable = @event switch {
                    BeatLineBarEvent => new LinePrimitiveDrawable(new(0), 100, (float)Math.PI / 2f) {
                        Thickness = 3f
                    },
                    BeatLineBeatEvent => new LinePrimitiveDrawable(new(0), 100, (float)Math.PI / 2f),
                    _                 => null
                };

                float travelTime = this.BaseApproachTime;

                float travelDistance = NOTE_START_POS.X - RECEPTICLE_POS.X;
                float travelRatio    = travelTime / travelDistance;

                float afterTravelTime = (RECEPTICLE_POS.X - NOTE_END_POS.X) * travelRatio;

                drawable?.Tweens.Add(
                new VectorTween(
                TweenType.Movement,
                new(NOTE_START_POS.X, NOTE_START_POS.Y - drawable.Length / 2f),
                new(RECEPTICLE_POS.X, RECEPTICLE_POS.Y - drawable.Length / 2f),
                (int)(@event.Time - travelTime),
                (int)@event.Time
                )
                );

                drawable?.Tweens.Add(
                new VectorTween(
                TweenType.Movement,
                new(RECEPTICLE_POS.X, RECEPTICLE_POS.Y - drawable.Length / 2f),
                new(NOTE_END_POS.X, RECEPTICLE_POS.Y   - drawable.Length / 2f),
                (int)@event.Time,
                (int)(@event.Time + afterTravelTime)
                )
                );

                if (drawable != null) {
                    drawable.TimeSource = pTypingGame.MusicTrack;
                    drawable.Depth      = 0.5f;
                    this._beatBars.Add(new(drawable, false));
                }
            }
        }

        private void CreateNotes() {
            foreach (Note note in this.Song.Notes) {
                NoteDrawable noteDrawable = this.CreateNote(note);

                this._notes.Add(noteDrawable);
            }
        }

        private void CreateCutOffIndicators() {
            foreach (Event @event in this.Song.Events) {
                if (@event is not TypingCutoffEvent) continue;

                TexturedDrawable cutoffIndicator = new(this._noteTexture, new(NOTE_START_POS.X, NOTE_START_POS.Y)) {
                    TimeSource    = pTypingGame.MusicTrack,
                    ColorOverride = Color.LightBlue,
                    Scale         = new(0.3f),
                    OriginType    = OriginType.Center
                };

                #region tweens

                float travelTime = this.BaseApproachTime;

                float travelDistance = NOTE_START_POS.X - RECEPTICLE_POS.X;
                float travelRatio    = travelTime / travelDistance;

                float afterTravelTime = (RECEPTICLE_POS.X - NOTE_END_POS.X) * travelRatio;

                cutoffIndicator.Tweens.Add(
                new VectorTween(TweenType.Movement, new(NOTE_START_POS.X, NOTE_START_POS.Y), RECEPTICLE_POS, (int)(@event.Time - travelTime), (int)@event.Time)
                );

                cutoffIndicator.Tweens.Add(
                new VectorTween(TweenType.Movement, RECEPTICLE_POS, new(NOTE_END_POS.X, RECEPTICLE_POS.Y), (int)@event.Time, (int)(@event.Time + afterTravelTime))
                );

                #endregion

                this._drawables.Add(cutoffIndicator);
            }
        }

        private NoteDrawable CreateNote(Note note) {
            NoteDrawable noteDrawable = new(new(NOTE_START_POS.X, NOTE_START_POS.Y + note.YOffset), this._noteTexture, pTypingGame.JapaneseFont, 50) {
                TimeSource    = pTypingGame.MusicTrack,
                ColorOverride = note.Color,
                LabelTextDrawable = {
                    Text  = $"{note.Text}\n{string.Join("\n", note.TypableRomaji.Romaji)}",
                    Scale = new(1f)
                },
                Scale      = new(0.55f),
                Depth      = 0f,
                OriginType = OriginType.Center,
                Note       = note
            };

            noteDrawable.CreateTweens(new(this.BaseApproachTime));

            return noteDrawable;
        }

        public void TypeCharacter(object sender, TextInputEventArgs args) {
            if (char.IsControl(args.Character))
                return;

            if (this.RecordReplay)
                this.ReplayFrames.Add(
                new() {
                    Character = args.Character,
                    Time      = pTypingGame.MusicTrack.GetCurrentTime()
                }
                );

            if (this.Song.AllNotesHit()) return;

            NoteDrawable noteDrawable = this._notes[this._noteToType];

            Note note = noteDrawable.Note;

            // Makes sure we dont hit an already hit note, which would cause a crash currently
            // this case *shouldnt* happen but it could so its good to check anyway
            if (note.IsHit)
                return;

            int currentTime = pTypingGame.MusicTrack.GetCurrentTime();

            if (currentTime > note.Time - TIMING_POOR) {
                (string hiragana, List<string> romajiToType) = note.TypableRomaji;

                List<string> filteredRomaji = romajiToType.Where(romaji => romaji.StartsWith(note.TypedRomaji)).ToList();

                foreach (string romaji in filteredRomaji) {
                    double timeDifference = Math.Abs(currentTime - note.Time);
                    if (romaji[note.TypedRomaji.Length] == args.Character) {
                        //If true, then we finished the note, if false, then we continue
                        if (noteDrawable.TypeCharacter(hiragana, romaji, timeDifference, this.Score)) {
                            this.HitSoundNormal.Play();
                            this.NoteUpdate(true, note);

                            this._noteToType++;
                        }
                        this.ShowTypingIndicator(args.Character);

                        foreach (PlayerMod mod in pTypingGame.SelectedMods)
                            mod.OnCharacterTyped(note, args.Character.ToString(), true);

                        break;
                    }

                    foreach (PlayerMod mod in pTypingGame.SelectedMods)
                        mod.OnCharacterTyped(note, args.Character.ToString(), false);
                }
            }

            //Update the text on all notes to show the new Romaji paths
            this.UpdateNoteText();
        }

        private void ShowTypingIndicator(char character) {
            this._typingIndicator.Text = character.ToString();

            this._typingIndicator.Tweens.Add(new ColorTween(TweenType.Color, Color.White, new(255, 255, 255, 0), FurballGame.Time, FurballGame.Time + 400));
            this._typingIndicator.Tweens.Add(new VectorTween(TweenType.Scale, new(1.5f), new(3), FurballGame.Time, FurballGame.Time                 + 400));
        }

        private void UpdateNoteText() {
            foreach (NoteDrawable noteDrawable in this._notes)
                noteDrawable.LabelTextDrawable.Text = $"{noteDrawable.Note.Text}\n{string.Join("\n", noteDrawable.Note.TypableRomaji.Romaji)}";
        }

        private void NoteUpdate(bool wasHit, Note note) {
            foreach (PlayerMod mod in pTypingGame.SelectedMods)
                mod.OnNoteHit(note);

            double numberHit = 0;
            double total     = 0;
            foreach (NoteDrawable noteDrawable in this._notes) {
                switch (noteDrawable.Note.HitResult) {
                    case HitResult.Excellent:
                        numberHit++;
                        break;
                    case HitResult.Good:
                        numberHit += (double)SCORE_GOOD / SCORE_EXCELLENT;
                        break;
                    case HitResult.Fair:
                        numberHit += (double)SCORE_FAIR / SCORE_EXCELLENT;
                        break;
                    case HitResult.Poor:
                        numberHit += (double)SCORE_POOR / SCORE_EXCELLENT;
                        break;
                }

                if (noteDrawable.Note.IsHit)
                    total++;
            }

            if (total == 0) this.Score.Accuracy = 1d;
            else
                this.Score.Accuracy = numberHit / total;

            if (wasHit) {
                int scoreToAdd = note.HitResult switch {
                    HitResult.Excellent => SCORE_EXCELLENT,
                    HitResult.Fair      => SCORE_FAIR,
                    HitResult.Good      => SCORE_GOOD,
                    HitResult.Poor      => SCORE_POOR,
                    _                   => 0
                };

                int scoreCombo = Math.Min(SCORE_COMBO * this.Score.Combo, SCORE_COMBO_MAX);
                this.Score.AddScore(scoreToAdd + scoreCombo);

                if (note.HitResult == HitResult.Poor)
                    this.Score.Combo = 0;
                
                this.Score.Combo++;

                if (this.Score.Combo > this.Score.MaxCombo)
                    this.Score.MaxCombo = this.Score.Combo;
            } else {
                if (this.Score.Combo > this.Score.MaxCombo)
                    this.Score.MaxCombo = this.Score.Combo;

                this.Score.Combo = 0;
            }

            Color hitColor;
            switch (note.HitResult) {
                case HitResult.Excellent: {
                    this.Score.ExcellentHits++;
                    hitColor = COLOR_EXCELLENT;
                    break;
                }
                case HitResult.Good: {
                    this.Score.GoodHits++;
                    hitColor = COLOR_GOOD;
                    break;
                }
                case HitResult.Fair: {
                    this.Score.FairHits++;
                    hitColor = COLOR_FAIR;
                    break;
                }
                default:
                case HitResult.Poor: {
                    this.Score.PoorHits++;
                    hitColor = COLOR_POOR;
                    break;
                }
            }

            this.OnComboUpdate?.Invoke(this, hitColor);
        }

        public override void Update(GameTime time) {
            int currentTime = pTypingGame.MusicTrack.GetCurrentTime();

            #region spawn notes and bars as needed

            for (int i = 0; i < this._notes.Count; i++) {
                NoteDrawable note = this._notes[i];

                if (note.Added) continue;

                if (currentTime < note.Note.Time - this.BaseApproachTime) continue;

                this._drawables.Add(note);
                note.Added = true;
            }

            for (int i = 0; i < this._beatBars.Count; i++) {
                Tuple<LinePrimitiveDrawable, bool> note = this._beatBars[i];

                if (note.Item2) continue;

                if (currentTime < note.Item1.Tweens[0].StartTime) continue;

                this._drawables.Add(note.Item1);
                this._beatBars[i] = new(note.Item1, true);
            }

            #endregion

            bool checkNoteHittability = true;

            if (this._noteToType == this._notes.Count) {
                this.EndScore();
                checkNoteHittability = false;
            }

            if (checkNoteHittability) {
                NoteDrawable noteToType = this._notes[this._noteToType];

                //Checks if the current note is not hit
                if (!noteToType.Note.IsHit && this._noteToType < this._notes.Count - 1) {
                    NoteDrawable nextNoteToType = this._notes[this._noteToType + 1];

                    //If we are within the next note
                    if (currentTime > nextNoteToType.Note.Time) {
                        //Miss the note
                        noteToType.Miss();
                        //Tell the game to update all the info
                        this.NoteUpdate(false, noteToType.Note);
                        //Change us to the next note
                        this._noteToType++;
                    }
                }

                foreach (Event cutOffEvent in this.Song.Events) {
                    if (cutOffEvent is not TypingCutoffEvent) continue;

                    if (currentTime > cutOffEvent.Time && cutOffEvent.Time > noteToType.Note.Time && !noteToType.Note.IsHit) {
                        //Miss the note
                        noteToType.Miss();
                        //Tell the game to update all the info
                        this.NoteUpdate(false, noteToType.Note);
                        //Change us to the next note
                        this._noteToType++;

                        break;
                    }
                }
            }

            foreach (PlayerMod mod in this.Score.Mods)
                mod.Update(time);

            base.Update(time);

        }

        public void CallMapEnd() {
            foreach (PlayerMod mod in pTypingGame.SelectedMods)
                mod.OnMapEnd(pTypingGame.MusicTrack, this._notes, this);
        }

        public void EndScore() {
            this.OnAllNotesComplete?.Invoke(this, EventArgs.Empty);
        }

        public void Play() {
            pTypingGame.PlayMusic();
        }
    }
}
