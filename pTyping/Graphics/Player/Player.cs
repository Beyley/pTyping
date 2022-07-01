using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes.BezierPathTween;
using Furball.Vixie.Backends.Shared;
using Furball.Volpe.Evaluation;
using JetBrains.Annotations;
using pTyping.Engine;
using pTyping.Graphics.Player.Mods;
using pTyping.Scores;
using pTyping.Songs;
using pTyping.Songs.Events;
using Silk.NET.Input;
using sowelipisona;
using Path=Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes.BezierPathTween.Path;


// using Furball.Engine.Engine.Audio;

namespace pTyping.Graphics.Player;

public class Player : CompositeDrawable {
    public override Vector2 Size => new(FurballGame.DEFAULT_WINDOW_WIDTH, 100);

    public const uint SCORE_EXCELLENT = 1500;
    public const uint SCORE_GOOD      = 1000;
    public const uint SCORE_FAIR      = 500;
    public const uint SCORE_POOR      = 0;

    public const uint SCORE_PER_CHARACTER = 500;
    public const uint SCORE_COMBO         = 10;
    public const uint SCORE_COMBO_MAX     = 1000;

    public float TIMING_EXCELLENT => 20  / (this.Song.Settings.Strictness / 5f);
    public float TIMING_GOOD      => 50  / (this.Song.Settings.Strictness / 5f);
    public float TIMING_FAIR      => 100 / (this.Song.Settings.Strictness / 5f);
    public float TIMING_POOR      => 200 / (this.Song.Settings.Strictness / 5f);

    public static readonly Color COLOR_EXCELLENT = new(255, 255, 0);
    public static readonly Color COLOR_GOOD      = new(0, 255, 0);
    public static readonly Color COLOR_FAIR      = new(0, 128, 255);
    public static readonly Color COLOR_POOR      = new(128, 128, 128);

    public const float NOTE_HEIGHT = 50f;

    public static readonly Vector2 NOTE_START_POS = new(FurballGame.DEFAULT_WINDOW_WIDTH + 200, NOTE_HEIGHT);
    public static readonly Vector2 NOTE_END_POS   = new(-100, NOTE_HEIGHT);

    public double BaseApproachTime = ConVars.BASE_APPROACH_TIME;
    public double CurrentApproachTime(double time) => this.BaseApproachTime / this.Song.CurrentTimingPoint(time).ApproachMultiplier;

    private readonly TexturedDrawable _recepticle;

    // private readonly LinePrimitiveDrawable _playfieldTopLine;
    // private readonly LinePrimitiveDrawable _playfieldBottomLine;
    private readonly TexturedDrawable _playfieldBackground;

    private readonly TextDrawable[] _typingIndicators = new TextDrawable[8];
    private          int            _currentTypingIndicatorIndex;
    private TextDrawable _currentTypingIndicator {
        get => this._typingIndicators[this._currentTypingIndicatorIndex];
        set => this._typingIndicators[this._currentTypingIndicatorIndex] = value;
    }

    private readonly List<NoteDrawable>          _notes  = new();
    private readonly List<Tuple<Drawable, bool>> _events = new();

    public static readonly Vector2 RECEPTICLE_POS = new(FurballGame.DEFAULT_WINDOW_WIDTH * 0.15f, NOTE_HEIGHT);

    private readonly Texture _noteTexture;

    public Song Song;

    public PlayerScore Score;

    private int _noteToType;

    public SoundEffectPlayer HitSoundNormal = null;

    public bool RecordReplay = true;

    public bool IsSpectating = false;

    // private          bool              _playingReplay;
    // private readonly PlayerScore       _playingScoreReplay = new();
    public readonly List<ReplayFrame> ReplayFrames = new();
    public event EventHandler<double> OnCorrectCharTyped;

    public event EventHandler<Color> OnComboUpdate;
    public event EventHandler        OnAllNotesComplete;

    public Player(Song song) {
        this.Song = song;

        this.BaseApproachTime /= song.Settings.GlobalApproachMultiplier;

        this.Score            = new PlayerScore(this.Song.MapHash, pTypingConfig.Instance.Username);
        this.Score.Mods       = pTypingGame.SelectedMods;
        this.Score.ModsString = string.Join(',', this.Score.Mods);

        this._playfieldBackground = new TexturedDrawable(ContentManager.LoadTextureFromFile("playfield-background.png", ContentSource.User), new Vector2(0)) {
            Depth = -0.95f
        };

        this.Drawables.Add(this._playfieldBackground);

        FileInfo[] noteFiles = new DirectoryInfo(this.Song.QualifiedFolderPath).GetFiles("note.png");

        this._noteTexture = noteFiles.Length == 0 ? ContentManager.LoadTextureFromFile("note.png", ContentSource.User)
                                : ContentManager.LoadTextureFromFile(noteFiles[0].FullName,        ContentSource.External);


        this._recepticle = new TexturedDrawable(this._noteTexture, RECEPTICLE_POS) {
            Scale      = new Vector2(0.55f),
            OriginType = OriginType.Center
        };

        this.Drawables.Add(this._recepticle);

        //Called before creating the notes
        this.Score.Mods.ForEach(mod => mod.BeforeNoteCreate(this));

        this.CreateEvents();
        this.CreateNotes();

        this.HitSoundNormal = FurballGame.AudioEngine.CreateSoundEffectPlayer(ContentManager.LoadRawAsset("hitsound.wav", ContentSource.User));

        ConVars.Volume.OnChange    += this.OnVolumeChange;
        this.HitSoundNormal.Volume =  ConVars.Volume.Value.Value;
        
        this.Play();

        this._sortDrawables = true;

        double speedMod = 1f;
        foreach (PlayerMod mod in pTypingGame.SelectedMods) {
            mod.OnMapStart(pTypingGame.MusicTrack, this._notes, this);
            speedMod *= mod.SpeedMultiplier();
        }
        this.Score.Speed = speedMod;
        pTypingGame.MusicTrack.SetSpeed(speedMod);
    }

    private void OnVolumeChange(object sender, Value.Number f) {
        this.HitSoundNormal.Volume = f.Value;
    }

    private void CreateEvents() {
        for (int i = 0; i < this.Song.Events.Count; i++) {
            Event @event = this.Song.Events[i];

            Drawable drawable = Event.CreateEventDrawable(@event, this._noteTexture, new GameplayDrawableTweenArgs(this.CurrentApproachTime(@event.Time)));

            if (drawable != null) {
                drawable.TimeSource = pTypingGame.MusicTrackTimeSource;
                drawable.Depth      = 0f;

                this._events.Add(new Tuple<Drawable, bool>(drawable, false));
            }
        }
    }

    private void CreateNotes() {
        foreach (Note note in this.Song.Notes) {
            NoteDrawable noteDrawable = this.CreateNote(note);

            this._notes.Add(noteDrawable);
        }
    }

    [Pure]
    private NoteDrawable CreateNote(Note note) {
        NoteDrawable noteDrawable = new(new Vector2(NOTE_START_POS.X, NOTE_START_POS.Y + note.YOffset), this._noteTexture, pTypingGame.JapaneseFont, 50) {
            TimeSource = pTypingGame.MusicTrackTimeSource,
            NoteTexture = {
                ColorOverride = note.Color
            },
            RawTextDrawable = {
                Text      = $"{note.Text}",
                Colors    = new System.Drawing.Color[note.Text.Length],
                ColorType = TextColorType.Repeating
            },
            ToTypeTextDrawable = {
                Text = $"{string.Join("\n", note.TypableRomaji.Romaji)}"
            },
            Scale      = new Vector2(0.55f),
            Depth      = 0.5f,
            OriginType = OriginType.Center,
            Note       = note
        };
        
        for (int i = 0; i < noteDrawable.RawTextDrawable.Colors.Length; i++) {
            noteDrawable.RawTextDrawable.Colors[i] = System.Drawing.Color.White;
        }

        noteDrawable.UpdateTextPositions();

        noteDrawable.CreateTweens(new GameplayDrawableTweenArgs(this.CurrentApproachTime(note.Time)));

        return noteDrawable;
    }

    public void TypeCharacter(object sender, char e) => this.TypeCharacter(e);
    public void TypeCharacter(char @char, bool checkingNext = false) {
        //Ignore control chars (fuck control chars all my homies hate control chars)
        if (char.IsControl(@char))
            return;

        //If we are recording a replay or we are spectating someone, record the keypress
        if (this.RecordReplay || this.IsSpectating) {
            ReplayFrame f = new() {
                Character = @char,
                Time      = pTypingGame.MusicTrackTimeSource.GetCurrentTime()
            };
            this.ReplayFrames.Add(f);
        }

        //If we already hit all the notes in the song, wtf are we doing here? stop hitting your keyboard you monkey
        if (this.Song.AllNotesHit()) return;

        //The drawable for the note we are going to check
        NoteDrawable noteDrawable = this._notes[checkingNext ? this._noteToType + 1 : this._noteToType];

        //The extracted `Note` object 
        Note note = noteDrawable.Note;

        // Makes sure we dont hit an already hit note, which would cause a crash currently
        // this case *shouldnt* happen but it could so its good to check anyway
        if (note.IsHit)
            return;

        //Get the current time of the music
        double currentTime = pTypingGame.MusicTrackTimeSource.GetCurrentTime();

        //If we are within or past the notes timing point,
        if (currentTime > note.Time - this.TIMING_POOR) {
            //Get the list of the currently typable romaji
            (string hiragana, List<string> romajiToType) = note.TypableRomaji;

            //Filter the typable romaji by the ones which start with the already typed romaji
            List<string> filteredRomaji = romajiToType.Where(romaji => romaji.StartsWith(note.TypedRomaji)).ToList();

            //Get the time difference between the current time and the notes exact time
            double timeDifference = Math.Abs(currentTime - note.Time);
            
            //For all the possible romaji options,
            foreach (string romaji in filteredRomaji) {
                //Check if the next romaji to type is the character we typed
                if (romaji[note.TypedRomaji.Length] == @char) {
                    //If we are checking the next note, and the current note is not hit,
                    if (checkingNext && !this._notes[this._noteToType].Note.IsHit) {
                        //Miss the current note
                        this._notes[this._noteToType].Miss();
                        this.NoteUpdate(false, this._notes[this._noteToType].Note);

                        //Go to the next note
                        this._noteToType++;
                        //Say that we are now checking the next note as the primary note
                        checkingNext = false;
                    }
                    
                    //If true, then we finished the note
                    if (noteDrawable.TypeCharacter(hiragana, romaji, timeDifference, currentTime - note.Time, this)) {
                        //Play the hitsound
                        this.HitSoundNormal.PlayNew();
                        //Update the note saying its been typed
                        this.NoteUpdate(true, note);
                        this.OnCorrectCharTyped?.Invoke(this, noteDrawable.TimeDifference);

                        //Update the current note to the note after the one we are checking right now
                        this._noteToType += checkingNext ? 2 : 1;
                    }
                    this.ShowTypingIndicator(@char);
                    
                    foreach (PlayerMod mod in pTypingGame.SelectedMods)
                        mod.OnCharacterTyped(note, @char.ToString(), true);

                    break;
                }

                //If we are not on the last note of the song, we are not checking the next note, and we are after the current note,
                if (this._noteToType != this.Song.Notes.Count - 1 && !checkingNext && currentTime > note.Time) {
                    //Then check the next note instead
                    this.TypeCharacter(@char, true);
                    return;
                }

                this.ShowTypingIndicator(@char, true);

                foreach (PlayerMod mod in pTypingGame.SelectedMods)
                    mod.OnCharacterTyped(note, @char.ToString(), false);
            }
        }

        //Update the text on all notes to show the new Romaji paths
        this.UpdateNoteText(noteDrawable);
    }

    private void ShowTypingIndicator(char character, bool miss = false) {
        if (this._currentTypingIndicator != null)
            this.Drawables.Remove(this._currentTypingIndicator);

        if (this._currentTypingIndicator == null) {
            this._currentTypingIndicator = new TextDrawable(RECEPTICLE_POS, pTypingGame.JapaneseFont, character.ToString(), 60) {
                OriginType = OriginType.Center
            };
        } else {
            this._currentTypingIndicator.Tweens.Clear();
            this._currentTypingIndicator.Position = RECEPTICLE_POS;
            this._currentTypingIndicator.Text     = character.ToString();
        }

        this.Drawables.Add(this._currentTypingIndicator);

        if (miss) {
            //random bool
            bool right = FurballGame.Random.Next(-1, 2) == 1;

            this._currentTypingIndicator.Tweens.Add(new ColorTween(TweenType.Color, new Color(200, 0, 0, 255), new Color(200, 0, 0, 0), FurballGame.Time, FurballGame.Time + 400));
            this._currentTypingIndicator.Tweens.Add(
            new PathTween(
            new Path(
            new PathSegment(
            this._currentTypingIndicator.Position,
            this._currentTypingIndicator.Position + new Vector2(FurballGame.Random.Next(9,  26) * (right ? 1 : -1), -FurballGame.Random.Next(9, 31)),
            this._currentTypingIndicator.Position + new Vector2(FurballGame.Random.Next(24, 46) * (right ? 1 : -1), FurballGame.Random.Next(29, 51))
            )
            ),
            FurballGame.Time,
            FurballGame.Time + 400
            )
            );
        } else {
            this._currentTypingIndicator.Tweens.Add(new ColorTween(TweenType.Color, Color.White, new Color(255, 255, 255, 0), FurballGame.Time, FurballGame.Time + 400));
            this._currentTypingIndicator.Tweens.Add(new VectorTween(TweenType.Scale, new Vector2(1f), new Vector2(1.5f), FurballGame.Time, FurballGame.Time                + 400));
        }

        this._currentTypingIndicatorIndex++;
        this._currentTypingIndicatorIndex %= this._typingIndicators.Length;
    }

    private void UpdateNoteText(NoteDrawable noteDrawable) {
        // foreach (NoteDrawable noteDrawable in this._notes) {
            // noteDrawable.RawTextDrawable.Text    = $"{noteDrawable.Note.Text}";
        noteDrawable.ToTypeTextDrawable.Text = $"{string.Join("\n", noteDrawable.Note.TypableRomaji.Romaji)}";
        
        for (int i = 0; i < noteDrawable.RawTextDrawable.Colors.Length; i++) {
            if(i < noteDrawable.Note.Typed.Length)
                noteDrawable.RawTextDrawable.Colors[i] = System.Drawing.Color.Gray;
            else
                noteDrawable.RawTextDrawable.Colors[i] = System.Drawing.Color.White;
        }
        
        noteDrawable.UpdateTextPositions();
        // }
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
            uint scoreToAdd = note.HitResult switch {
                HitResult.Excellent => SCORE_EXCELLENT,
                HitResult.Fair      => SCORE_FAIR,
                HitResult.Good      => SCORE_GOOD,
                HitResult.Poor      => SCORE_POOR,
                _                   => 0
            };

            uint scoreCombo = Math.Min(SCORE_COMBO * this.Score.Combo, SCORE_COMBO_MAX);
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

                this.Score.Combo = 0;
                break;
            }
        }

        this.OnComboUpdate?.Invoke(this, hitColor);
    }

    public override void Update(double time) {
        double currentTime = pTypingGame.MusicTrackTimeSource.GetCurrentTime();

        #region spawn notes and bars as needed

        for (int i = 0; i < this._notes.Count; i++) {
            NoteDrawable note = this._notes[i];

            if (note.Added) continue;

            if (currentTime < note.Note.Time - this.CurrentApproachTime(note.Note.Time)) continue;

            this.Drawables.Add(note);
            note.Added = true;
        }

        for (int i = 0; i < this._events.Count; i++) {
            (Drawable drawable, bool added) = this._events[i];

            if (added) continue;

            if (currentTime < drawable.Tweens[0].StartTime) continue;

            this.Drawables.Add(drawable);
            this._events[i] = new Tuple<Drawable, bool>(drawable, true);
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

    public override void Dispose() {
        ConVars.Volume.OnChange -= this.OnVolumeChange;

        base.Dispose();
    }

    public void CallMapEnd() {
        foreach (PlayerMod mod in pTypingGame.SelectedMods)
            mod.OnMapEnd(pTypingGame.MusicTrack, this._notes, this);
    }

    public void EndScore() {
        this.OnAllNotesComplete?.Invoke(this, EventArgs.Empty);
    }

    public void Play() {
        if (!this.IsSpectating)
            pTypingGame.PlayMusic();
        else
            pTypingGame.MusicTrack.Stop();
    }
    public void TypeCharacter(object sender, (IKeyboard keyboard, char character) e) {
        this.TypeCharacter(sender, e.character);
    }
}