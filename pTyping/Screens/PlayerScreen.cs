using System;
using System.Linq;
using System.Collections.Generic;
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
using pTyping.Songs;
using pTyping.Player;
using pTyping.Drawables;
using pTyping.Songs.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace pTyping.Screens {
    public class PlayerScreen : Screen {
        public PlayerScore Score = new();

        private TextDrawable     _scoreDrawable;
        private TextDrawable     _accuracyDrawable;
        private TextDrawable     _comboDrawable;
        private TextDrawable     _typingIndicator;
        private TexturedDrawable _recepticle;
        private UiButtonDrawable _skipButton;

        private UiButtonDrawable _resumeButton;
        private UiButtonDrawable _restartButton;
        private UiButtonDrawable _quitButton;

        public SoundEffect HitSound = new();

        private List<NoteDrawable> _notes = new();

        public static readonly int ScoreExcellent = 1500;
        public static readonly int ScoreGood      = 1000;
        public static readonly int ScoreFair      = 500;
        public static readonly int ScorePoor      = 0;

        public static readonly int ScorePerCharacter = 500;
        public static readonly int ScoreCombo        = 10;
        public static readonly int ScoreComboMax     = 1000;

        public static readonly int TimingExcellent = 20;
        public static readonly int TimingGood      = 50;
        public static readonly int TimingFair      = 100;
        public static readonly int TimingPoor      = 200;

        private Song _song;

        private Texture2D _noteTexture;

        public static readonly Vector2 RecepticlePos = new(FurballGame.DEFAULT_WINDOW_WIDTH * 0.15f, FurballGame.DEFAULT_WINDOW_HEIGHT / 2f);

        public static readonly Vector2 NoteStartPos = new(FurballGame.DEFAULT_WINDOW_WIDTH + 200, FurballGame.DEFAULT_WINDOW_HEIGHT / 2f);
        public static readonly Vector2 NoteEndPos   = new(-100, FurballGame.DEFAULT_WINDOW_HEIGHT                                   / 2f);

        public int NoteToType;

        public PlayerScreen() {}

        public override void Initialize() {
            base.Initialize();

            this._song = pTypingGame.CurrentSong.Value.Copy();

            if (this._song.Notes.Count == 0)//TODO notify the user the map did not load correctly, for now, we just send back to the song selection menu
                ScreenManager.ChangeScreen(new SongSelectionScreen(false));

            #region UI

            this._scoreDrawable = new TextDrawable(new Vector2(5, 5), FurballGame.DEFAULT_FONT, $"{this.Score.Score:00000000}", 60);
            this._accuracyDrawable = new TextDrawable(
            new Vector2(5, 5 + this._scoreDrawable.Size.Y),
            FurballGame.DEFAULT_FONT,
            $"{this.Score.Accuracy * 100:0.00}%",
            60
            ) {};
            this._comboDrawable = new TextDrawable(new Vector2(RecepticlePos.X, RecepticlePos.Y - 70), FurballGame.DEFAULT_FONT, $"{this.Score.Combo}x", 70) {
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
                OriginType = OriginType.Center
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
                OriginType = OriginType.Center
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
                OriginType = OriginType.Center
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

            this._recepticle = new TexturedDrawable(this._noteTexture, RecepticlePos) {
                Scale      = new(0.55f),
                OriginType = OriginType.Center
            };

            this.Manager.Add(this._recepticle);

            #endregion

            this.AddNotes();
            this.CreateCutOffIndicators();

            #region Playfield decorations

            LinePrimitiveDrawable playfieldTopLine = new(new Vector2(1, RecepticlePos.Y - 50), FurballGame.DEFAULT_WINDOW_WIDTH, 0) {
                ColorOverride = Color.Gray
            };
            LinePrimitiveDrawable playfieldBottomLine = new(new Vector2(1, RecepticlePos.Y + 50), FurballGame.DEFAULT_WINDOW_WIDTH, 0) {
                ColorOverride = Color.Gray
            };
            this.Manager.Add(playfieldTopLine);
            this.Manager.Add(playfieldBottomLine);

            RectanglePrimitiveDrawable playfieldBackgroundCover = new(new(0, RecepticlePos.Y - 50), new(FurballGame.DEFAULT_WINDOW_WIDTH, 100), 0f, true) {
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
            new(1f * (1f - Config.BackgroundDim), 1f * (1f - Config.BackgroundDim), 1f * (1f - Config.BackgroundDim)),
            pTypingGame.CurrentSongBackground.TimeSource.GetCurrentTime(),
            pTypingGame.CurrentSongBackground.TimeSource.GetCurrentTime() + 1000
            )
            );
            pTypingGame.LoadBackgroundFromSong(this._song);

            #endregion
            #region typing indicator

            this._typingIndicator = new(RecepticlePos, pTypingGame.JapaneseFont, "", 60) {
                OriginType = OriginType.Center
            };

            this.Manager.Add(this._typingIndicator);

            #endregion

            #endregion

            this.HitSound.Load(ContentManager.LoadRawAsset("hitsound.wav", ContentSource.User));

            this.HitSound.Volume = Config.Volume;

            this.Play();

            FurballGame.InputManager.OnKeyDown    += this.OnKeyPress;
            FurballGame.Instance.Window.TextInput += this.OnCharacterTyped;
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
            pTypingGame.MusicTrack.SeekTo(this._song.Notes.First().Time - 2999);
        }

        public void AddNotes() {
            foreach (Note note in this._song.Notes) {
                NoteDrawable noteDrawable = this.CreateNote(note);

                this.Manager.Add(noteDrawable);
                this._notes.Add(noteDrawable);
            }
        }

        public void CreateCutOffIndicators() {
            foreach (Event @event in this._song.Events) {
                if (@event is not TypingCutoffEvent) continue;

                TexturedDrawable cutoffIndicator = new(this._noteTexture, new(NoteStartPos.X, NoteStartPos.Y)) {
                    TimeSource    = pTypingGame.MusicTrack,
                    ColorOverride = Color.LightBlue,
                    Scale         = new(0.3f),
                    OriginType    = OriginType.Center
                };

                #region tweens

                float travelTime = Config.BaseApproachTime;

                float travelDistance = NoteStartPos.X - RecepticlePos.X;
                float travelRatio    = travelTime / travelDistance;

                float afterTravelTime = (RecepticlePos.X - NoteEndPos.X) * travelRatio;

                cutoffIndicator.Tweens.Add(
                new VectorTween(TweenType.Movement, new(NoteStartPos.X, NoteStartPos.Y), RecepticlePos, (int)(@event.Time - travelTime), (int)@event.Time)
                );

                cutoffIndicator.Tweens.Add(
                new VectorTween(TweenType.Movement, RecepticlePos, new(NoteEndPos.X, RecepticlePos.Y), (int)@event.Time, (int)(@event.Time + afterTravelTime))
                );

                #endregion

                this.Manager.Add(cutoffIndicator);
            }
        }

        public NoteDrawable CreateNote(Note note) {
            NoteDrawable noteDrawable = new(new(NoteStartPos.X, NoteStartPos.Y + note.YOffset), this._noteTexture, pTypingGame.JapaneseFont, 50) {
                TimeSource    = pTypingGame.MusicTrack,
                ColorOverride = note.Color,
                LabelTextDrawable = {
                    Text  = $"{note.Text}\n{string.Join("\n", note.ThisCharacterRomaji)}",
                    Scale = new(1f)
                },
                Scale      = new(0.55f),
                OriginType = OriginType.Center
            };

            float travelTime = Config.BaseApproachTime;

            float travelDistance = NoteStartPos.X - RecepticlePos.X;
            float travelRatio    = travelTime / travelDistance;

            float afterTravelTime = (RecepticlePos.X - NoteEndPos.X) * travelRatio;

            noteDrawable.Tweens.Add(
            new VectorTween(TweenType.Movement, new(NoteStartPos.X, NoteStartPos.Y + note.YOffset), RecepticlePos, (int)(note.Time - travelTime), (int)note.Time)
            );

            noteDrawable.Tweens.Add(
            new VectorTween(TweenType.Movement, RecepticlePos, new(NoteEndPos.X, RecepticlePos.Y), (int)note.Time, (int)(note.Time + afterTravelTime))
            );

            noteDrawable.Note = note;

            return noteDrawable;
        }

        protected override void Dispose(bool disposing) {
            FurballGame.InputManager.OnKeyDown    -= this.OnKeyPress;
            FurballGame.Instance.Window.TextInput -= this.OnCharacterTyped;

            base.Dispose(disposing);
        }

        public void OnKeyPress(object sender, Keys key) {
            if (key == Keys.Escape)
                pTypingGame.PauseResumeMusic();
        }

        public void OnCharacterTyped(object sender, TextInputEventArgs args) {
            NoteDrawable noteDrawable = this._notes[this.NoteToType];

            Note note = noteDrawable.Note;

            // Makes sure we dont hit an already hit note, which would cause a crash currently
            // this case *shouldnt* happen but it could so its good to check anyway
            if (note.IsHit)
                return;

            int currentTime = pTypingGame.MusicTrack.GetCurrentTime();

            if (currentTime > note.Time - TimingPoor) {
                List<string> romajiToType = note.ThisCharacterRomaji;

                List<string> filteredRomaji = romajiToType.Where(romaji => romaji.StartsWith(note.TypedRomaji)).ToList();

                foreach (string romaji in filteredRomaji) {
                    double timeDifference = Math.Abs(currentTime - note.Time);
                    if (romaji[note.TypedRomaji.Length] == args.Character) {
                        //If true, then we finished the note, if false, then we continue
                        if (noteDrawable.TypeCharacter(romaji, timeDifference)) {
                            this.HitSound.Play();
                            this.NoteUpdate(true, note);

                            this.NoteToType++;
                        }
                        this.ShowTypingIndicator(args.Character);
                        this.Score.Score += ScorePerCharacter;

                        break;
                    }
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
                noteDrawable.LabelTextDrawable.Text = $"{noteDrawable.Note.Text}\n{string.Join("\n", noteDrawable.Note.ThisCharacterRomaji)}";
        }

        private void NoteUpdate(bool wasHit, Note note) {
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

            if (numberHit + numberMiss == 0) this.Score.Accuracy = 1d;
            else
                this.Score.Accuracy = numberHit / ((double)numberHit + (double)numberMiss);

            if (wasHit) {
                int scoreToAdd = 0;
                switch (note.HitResult) {
                    case HitResult.Excellent:
                        scoreToAdd = ScoreExcellent;
                        break;
                    case HitResult.Fair:
                        scoreToAdd = ScoreFair;
                        break;
                    case HitResult.Good:
                        scoreToAdd = ScoreGood;
                        break;
                    case HitResult.Poor:
                        scoreToAdd = ScorePoor;
                        break;
                }
                this.Score.Score += scoreToAdd + Math.Min(this.Score.Combo - 1 * ScoreCombo, ScoreComboMax);
                this.Score.Combo++;
            } else {
                this.Score.Combo = 0;
            }

            Color hitColor = Color.Red;
            switch (note.HitResult) {
                case HitResult.Excellent: {
                    this.Score.ExcellentHits++;
                    hitColor = Color.Blue;
                    break;
                }
                case HitResult.Good: {
                    this.Score.GoodHits++;
                    hitColor = Color.Green;
                    break;
                }
                case HitResult.Fair: {
                    this.Score.FairHits++;
                    hitColor = Color.Yellow;
                    break;
                }
                case HitResult.Poor: {
                    this.Score.PoorHits++;
                    hitColor = Color.Orange;
                    break;
                }
                case HitResult.Miss: {
                    this.Score.MissHits++;
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

            this._scoreDrawable.Text    = $"{this.Score.Score:00000000}";
            this._accuracyDrawable.Text = $"{this.Score.Accuracy * 100:0.00}%";
            this._comboDrawable.Text    = $"{this.Score.Combo}x";

            bool isPaused = pTypingGame.MusicTrack.PlaybackState == PlaybackState.Paused;

            this._resumeButton.Visible  = isPaused;
            this._restartButton.Visible = isPaused;
            this._quitButton.Visible    = isPaused;

            this._resumeButton.Clickable  = isPaused;
            this._restartButton.Clickable = isPaused;
            this._quitButton.Clickable    = isPaused;

            #endregion

            #region skin button visibility

            if (this._song.Notes.First().Time - pTypingGame.MusicTrack.GetCurrentTime() > 3000)
                this._skipButton.Visible = true;
            else
                this._skipButton.Visible = false;

            #endregion

            bool checkNoteHittability = true;

            if (this.NoteToType == this._notes.Count) {
                this.EndScore();
                checkNoteHittability = false;
            }

            if (checkNoteHittability) {
                NoteDrawable noteToType = this._notes[this.NoteToType];

                //Checks if the current note is not hit
                if (!noteToType.Note.IsHit && this.NoteToType < this._notes.Count - 1) {
                    NoteDrawable nextNoteToType = this._notes[this.NoteToType + 1];

                    //If we are within the next note
                    if (currentTime > nextNoteToType.Note.Time) {
                        //Miss the note
                        noteToType.Miss();
                        //Tell the game to update all the info
                        this.NoteUpdate(false, noteToType.Note);
                        //Change us to the next note
                        this.NoteToType++;
                    }
                }

                foreach (Event cutOffEvent in this._song.Events) {
                    if (cutOffEvent is not TypingCutoffEvent) continue;

                    if (currentTime > cutOffEvent.Time && cutOffEvent.Time > noteToType.Note.Time && !noteToType.Note.IsHit) {
                        //Miss the note
                        noteToType.Miss();
                        //Tell the game to update all the info
                        this.NoteUpdate(false, noteToType.Note);
                        //Change us to the next note
                        this.NoteToType++;

                        break;
                    }
                }
            }

            base.Update(gameTime);
        }

        public void EndScore() {
            ScreenManager.ChangeScreen(new ScoreResultsScreen(this.Score));
        }

        public void Play() {
            pTypingGame.PlayMusic();

            // pTypingGame.MusicTrack.SeekTo(0);
        }
    }
}
