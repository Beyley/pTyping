using System;
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
using Furball.Engine.Engine.Helpers;
using ManagedBass;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using pTyping.Drawables;
using pTyping.Player;
using pTyping.Player.Mods;
using pTyping.Songs;
using pTyping.Songs.Events;

namespace pTyping.Screens {
    public class PlayerScreen : Screen {
        public int BaseApproachTime = ConVars.BaseApproachTime.Value;
        
        public static readonly int SCORE_EXCELLENT = 1500;
        public static readonly int SCORE_GOOD      = 1000;
        public static readonly int SCORE_FAIR      = 500;
        public static readonly int SCORE_POOR      = 0;

        public static readonly int SCORE_PER_CHARACTER = 500;
        public static readonly int SCORE_COMBO         = 10;
        public static readonly int SCORE_COMBO_MAX     = 1000;

        public static readonly int TIMING_EXCELLENT = 20;
        public static readonly int TIMING_GOOD      = 50;
        public static readonly int TIMING_FAIR      = 100;
        public static readonly int TIMING_POOR      = 200;

        public static readonly Vector2 RECEPTICLE_POS = new(FurballGame.DEFAULT_WINDOW_WIDTH * 0.15f, FurballGame.DEFAULT_WINDOW_HEIGHT / 2f);

        public static readonly Vector2      NOTE_START_POS = new(FurballGame.DEFAULT_WINDOW_WIDTH + 200, FurballGame.DEFAULT_WINDOW_HEIGHT / 2f);
        public static readonly Vector2      NOTE_END_POS   = new(-100, FurballGame.DEFAULT_WINDOW_HEIGHT                                   / 2f);
        private                TextDrawable _accuracyDrawable;
        private                TextDrawable _comboDrawable;

        private bool _endScheduled = false;

        private readonly List<NoteDrawable> _notes = new();

        private Texture2D _noteTexture;

        private int              _noteToType;
        private UiButtonDrawable _quitButton;
        private TexturedDrawable _recepticle;
        private UiButtonDrawable _restartButton;

        private UiButtonDrawable _resumeButton;
        private PlayerScore      _score;

        private TextDrawable     _scoreDrawable;
        private UiButtonDrawable _skipButton;

        private LyricDrawable _lyricDrawable;

        public  Song         Song;
        private TextDrawable _typingIndicator;

        public SoundEffect HitSoundNormal = new();

        private          bool              _playingReplay;
        private readonly Replay            _replay       = new();
        private readonly List<ReplayFrame> _replayFrames = new();

        public PlayerScreen() {}

        public PlayerScreen(Replay replay) {
            this._playingReplay = true;
            this._replay        = replay;
        }

        public override void Initialize() {
            base.Initialize();

            this.Song = pTypingGame.CurrentSong.Value.Copy();

            this._score            = new(this.Song.MapHash, ConVars.Username.Value);
            this._score.Mods       = pTypingGame.SelectedMods;
            this._score.ModsString = string.Join(',', this._score.Mods);

            if (this.Song.Notes.Count == 0)//TODO notify the user the map did not load correctly, for now, we just send back to the song selection menu
                ScreenManager.ChangeScreen(new SongSelectionScreen(false));

            #region UI

            this._scoreDrawable = new TextDrawable(new Vector2(5, 5), FurballGame.DEFAULT_FONT, $"{this._score.Score:00000000}", 60);
            this._accuracyDrawable = new TextDrawable(
            new Vector2(5, 5 + this._scoreDrawable.Size.Y),
            FurballGame.DEFAULT_FONT,
            $"{this._score.Accuracy * 100:0.00}%",
            60
            );
            this._comboDrawable = new TextDrawable(new Vector2(RECEPTICLE_POS.X, RECEPTICLE_POS.Y - 70), FurballGame.DEFAULT_FONT, $"{this._score.Combo}x", 70) {
                OriginType = OriginType.BottomCenter
            };

            this.Manager.Add(this._scoreDrawable);
            this.Manager.Add(this._accuracyDrawable);
            this.Manager.Add(this._comboDrawable);

            this._skipButton = new UiButtonDrawable(
            new(FurballGame.DEFAULT_WINDOW_WIDTH, FurballGame.DEFAULT_WINDOW_HEIGHT),
            "Skip Intro",
            FurballGame.DEFAULT_FONT,
            50,
            Color.Blue,
            Color.White,
            Color.White,
            new(0),
            this.SkipButtonClick
            );
            this._skipButton.OriginType = OriginType.BottomRight;
            this._skipButton.Visible    = false;

            this.Manager.Add(this._skipButton);

            this._lyricDrawable = new(new(RECEPTICLE_POS.X - 100, RECEPTICLE_POS.Y + 200), this.Song);
            this.Manager.Add(this._lyricDrawable);

            #region Pause UI

            Vector2 pauseUiButtonSize = new(170, 50);

            this._resumeButton = new(
            new(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.2f),
            "Resume",
            FurballGame.DEFAULT_FONT,
            50,
            Color.Green,
            Color.White,
            Color.White,
            pauseUiButtonSize
            ) {
                OriginType = OriginType.Center,
                Depth      = -1f
            };
            
            this._restartButton = new(
            new(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.3f),
            "Restart",
            FurballGame.DEFAULT_FONT,
            50,
            Color.Yellow,
            Color.White,
            Color.White,
            pauseUiButtonSize
            ) {
                OriginType = OriginType.Center,
                Depth      = -1f
            };
            
            this._quitButton = new(
            new(FurballGame.DEFAULT_WINDOW_WIDTH / 2f, FurballGame.DEFAULT_WINDOW_HEIGHT * 0.4f),
            "Quit",
            FurballGame.DEFAULT_FONT,
            50,
            Color.Red,
            Color.White,
            Color.White,
            pauseUiButtonSize
            ) {
                OriginType = OriginType.Center,
                Depth      = -1f
            };

            this._resumeButton.OnClick  += this.ResumeButtonClick;
            this._restartButton.OnClick += this.RestartButtonClick;
            this._quitButton.OnClick    += this.QuitButtonClick;

            this.Manager.Add(this._resumeButton);
            this.Manager.Add(this._restartButton);
            this.Manager.Add(this._quitButton);

            #endregion

            #endregion

            #region Recepticle

            this._noteTexture = ContentManager.LoadMonogameAsset<Texture2D>("note");

            this._recepticle = new TexturedDrawable(this._noteTexture, RECEPTICLE_POS) {
                Scale      = new(0.55f),
                OriginType = OriginType.Center
            };

            this.Manager.Add(this._recepticle);

            #endregion

            this._score.Mods.ForEach(mod => mod.BeforeNoteCreate(this));

            this.AddNotes();
            this.CreateCutOffIndicators();

            #region Playfield decorations

            LinePrimitiveDrawable playfieldTopLine = new(new Vector2(1, RECEPTICLE_POS.Y - 50), FurballGame.DEFAULT_WINDOW_WIDTH, 0) {
                ColorOverride = Color.Gray
            };
            LinePrimitiveDrawable playfieldBottomLine = new(new Vector2(1, RECEPTICLE_POS.Y + 50), FurballGame.DEFAULT_WINDOW_WIDTH, 0) {
                ColorOverride = Color.Gray
            };
            this.Manager.Add(playfieldTopLine);
            this.Manager.Add(playfieldBottomLine);

            RectanglePrimitiveDrawable playfieldBackgroundCover = new(new(0, RECEPTICLE_POS.Y - 50), new(FurballGame.DEFAULT_WINDOW_WIDTH, 100), 0f, true) {
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
            new(1f * (1f - ConVars.BackgroundDim.Value), 1f * (1f - ConVars.BackgroundDim.Value), 1f * (1f - ConVars.BackgroundDim.Value)),
            pTypingGame.CurrentSongBackground.TimeSource.GetCurrentTime(),
            pTypingGame.CurrentSongBackground.TimeSource.GetCurrentTime() + 1000
            )
            );
            pTypingGame.LoadBackgroundFromSong(this.Song);

            #endregion
            #region typing indicator

            this._typingIndicator = new(RECEPTICLE_POS, pTypingGame.JapaneseFont, "", 60) {
                OriginType = OriginType.Center
            };

            this.Manager.Add(this._typingIndicator);

            #endregion

            #endregion

            this.HitSoundNormal.Load(ContentManager.LoadRawAsset("hitsound.wav", ContentSource.User));

            this.HitSoundNormal.Volume = ConVars.Volume.Value;

            this.Play();

            foreach (PlayerMod mod in pTypingGame.SelectedMods)
                mod.OnMapStart(pTypingGame.MusicTrack, this._notes, this);

            FurballGame.InputManager.OnKeyDown    += this.OnKeyPress;
            if (!this._playingReplay)
                FurballGame.Instance.Window.TextInput += this.OnCharacterTyped;

            pTypingGame.UserStatusPlaying();

            if (!this._playingReplay) {
                this._replay.Username = pTypingGame.OnlineManager.Username();
                this._replay.SongHash = this.Song.MapHash;
            }
        }

        private void ResumeButtonClick(object sender, Point e) {
            pTypingGame.PauseResumeMusic();
        }

        private void RestartButtonClick(object sender, Point e) {
            pTypingGame.MusicTrack.SeekTo(0);
            ScreenManager.ChangeScreen(new PlayerScreen());
        }

        private void QuitButtonClick(object sender, Point e) {
            ScreenManager.ChangeScreen(new SongSelectionScreen(false));
        }

        private void SkipButtonClick(object sender, Point e) {
            pTypingGame.MusicTrack.SeekTo(this.Song.Notes.First().Time - 2999);
        }

        private void AddNotes() {
            foreach (Note note in this.Song.Notes) {
                NoteDrawable noteDrawable = this.CreateNote(note);

                this.Manager.Add(noteDrawable);
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

                this.Manager.Add(cutoffIndicator);
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
                OriginType = OriginType.Center
            };

            float travelTime = this.BaseApproachTime;

            float travelDistance = NOTE_START_POS.X - RECEPTICLE_POS.X;
            float travelRatio    = travelTime / travelDistance;

            float afterTravelTime = (RECEPTICLE_POS.X - NOTE_END_POS.X) * travelRatio;

            noteDrawable.Tweens.Add(
            new VectorTween(TweenType.Movement, new(NOTE_START_POS.X, NOTE_START_POS.Y + note.YOffset), RECEPTICLE_POS, (int)(note.Time - travelTime), (int)note.Time)
            );

            noteDrawable.Tweens.Add(
            new VectorTween(TweenType.Movement, RECEPTICLE_POS, new(NOTE_END_POS.X, RECEPTICLE_POS.Y), (int)note.Time, (int)(note.Time + afterTravelTime))
            );

            noteDrawable.Note = note;

            return noteDrawable;
        }

        protected override void Dispose(bool disposing) {
            FurballGame.InputManager.OnKeyDown    -= this.OnKeyPress;
            FurballGame.Instance.Window.TextInput -= this.OnCharacterTyped;

            base.Dispose(disposing);
        }

        private void OnKeyPress(object sender, Keys key) {
            if (key == Keys.Escape)
                pTypingGame.PauseResumeMusic();
        }

        private void OnCharacterTyped(object sender, TextInputEventArgs args) {
            if (char.IsControl(args.Character))
                return;

            if (!this._playingReplay)
                this._replayFrames.Add(
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
                        if (noteDrawable.TypeCharacter(hiragana, romaji, timeDifference, this._score)) {
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

            int numberHit  = 0;
            int numberMiss = 0;
            foreach (NoteDrawable noteDrawable in this._notes)
                switch (noteDrawable.Note.HitResult) {
                    case HitResult.Excellent:
                    case HitResult.Good:
                    case HitResult.Fair:
                    case HitResult.Poor:
                        numberHit++;
                        break;
                    case HitResult.Miss:
                        numberMiss++;
                        break;
                }

            if (numberHit + numberMiss == 0) this._score.Accuracy = 1d;
            else
                this._score.Accuracy = numberHit / (numberHit + (double)numberMiss);

            if (wasHit) {
                int scoreToAdd = 0;
                switch (note.HitResult) {
                    case HitResult.Excellent:
                        scoreToAdd = SCORE_EXCELLENT;
                        break;
                    case HitResult.Fair:
                        scoreToAdd = SCORE_FAIR;
                        break;
                    case HitResult.Good:
                        scoreToAdd = SCORE_GOOD;
                        break;
                    case HitResult.Poor:
                        scoreToAdd = SCORE_POOR;
                        break;
                }
                this._score.AddScore(scoreToAdd + Math.Min(this._score.Combo - 1 * SCORE_COMBO, SCORE_COMBO_MAX));
                this._score.Combo++;
            } else {
                if (this._score.Combo > this._score.MaxCombo)
                    this._score.MaxCombo = this._score.Combo;
                this._score.Combo = 0;
            }

            Color hitColor = Color.Red;
            switch (note.HitResult) {
                case HitResult.Excellent: {
                    this._score.ExcellentHits++;
                    hitColor = Color.Blue;
                    break;
                }
                case HitResult.Good: {
                    this._score.GoodHits++;
                    hitColor = Color.Green;
                    break;
                }
                case HitResult.Fair: {
                    this._score.FairHits++;
                    hitColor = Color.Yellow;
                    break;
                }
                case HitResult.Poor: {
                    this._score.PoorHits++;
                    hitColor = Color.Orange;
                    break;
                }
                case HitResult.Miss: {
                    this._score.MissHits++;
                    hitColor = Color.Red;
                    break;
                }
            }

            this._comboDrawable.Tweens.Add(
            new ColorTween(
            TweenType.Color,
            this._comboDrawable.ColorOverride,
            hitColor,
            this._comboDrawable.TimeSource.GetCurrentTime(),
            this._comboDrawable.TimeSource.GetCurrentTime() + 100
            )
            );
            this._comboDrawable.Tweens.Add(
            new ColorTween(
            TweenType.Color,
            hitColor,
            Color.White,
            this._comboDrawable.TimeSource.GetCurrentTime() + 100,
            this._comboDrawable.TimeSource.GetCurrentTime() + 1100
            )
            );
        }

        public override void Update(GameTime gameTime) {
            int currentTime = pTypingGame.MusicTrack.GetCurrentTime();

            #region update UI

            this._scoreDrawable.Text    = $"{this._score.Score:00000000}";
            this._accuracyDrawable.Text = $"{this._score.Accuracy * 100:0.00}%";
            this._comboDrawable.Text    = $"{this._score.Combo}x";

            bool isPaused = pTypingGame.MusicTrack.PlaybackState == PlaybackState.Paused;

            this._resumeButton.Visible  = isPaused;
            this._restartButton.Visible = isPaused;
            this._quitButton.Visible    = isPaused;

            this._resumeButton.Clickable  = isPaused;
            this._restartButton.Clickable = isPaused;
            this._quitButton.Clickable    = isPaused;

            #endregion

            this._lyricDrawable.UpdateLyric(currentTime);

            #region skin button visibility

            if (this.Song.Notes.First().Time - pTypingGame.MusicTrack.GetCurrentTime() > 3000)
                this._skipButton.Visible = true;
            else
                this._skipButton.Visible = false;

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

            base.Update(gameTime);

            if (this._score.Mods.Count != 0)
                foreach (PlayerMod mod in this._score.Mods)
                    mod.Update(gameTime);

            #region Replays

            if (this._playingReplay && Array.TrueForAll(this._replay.Frames, x => x.Used))
                this._playingReplay = false;

            if (this._playingReplay) {
                for (int i = 0; i < this._replay.Frames.Length; i++) {
                    ref ReplayFrame currentFrame = ref this._replay.Frames[i];

                    if (currentTime > currentFrame.Time && !currentFrame.Used) {
                        this.OnCharacterTyped(this, new TextInputEventArgs(currentFrame.Character));

                        currentFrame.Used = true;
                        break;
                    }
                }
            }

            #endregion
        }

        public void EndScore() {
            if (!this._endScheduled) {
                pTypingGame.MusicTrackScheduler.ScheduleMethod(
                delegate {
                    foreach (PlayerMod mod in pTypingGame.SelectedMods)
                        mod.OnMapEnd(pTypingGame.MusicTrack, this._notes, this);

                    this._replay.Time   = DateTime.Now;
                    this._replay.Frames = this._replayFrames.ToArray();
                    
                    pTypingGame.SubmitScore(this.Song, this._score);

                    ScreenManager.ChangeScreen(new ScoreResultsScreen(this._score));
                },
                pTypingGame.MusicTrack.GetCurrentTime() + 1500
                );

                this._endScheduled = true;
            }
        }

        public void Play() {
            pTypingGame.PlayMusic();

            // pTypingGame.MusicTrack.SeekTo(0);
        }
    }
}
