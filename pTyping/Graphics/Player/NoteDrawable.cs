using System.Numerics;
using FontStashSharp;
using Furball.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes.BezierPathTween;
using pTyping.Engine;
using pTyping.Graphics.Editor;
using pTyping.Songs;

namespace pTyping.Graphics.Player;

public struct GameplayDrawableTweenArgs {
    public readonly double ApproachTime;
    public readonly bool   TweenKeepAlive;
    public readonly bool   IsEditor;

    public GameplayDrawableTweenArgs(double approachTime, bool tweenKeepAlive = false, bool isEditor = false) {
        this.ApproachTime   = approachTime;
        this.TweenKeepAlive = tweenKeepAlive;
        this.IsEditor       = isEditor;
    }
}

public class NoteDrawable : CompositeDrawable {
    public TextDrawable     RawTextDrawable;
    public TextDrawable     ToTypeTextDrawable;
    public TexturedDrawable NoteTexture;

    public Note Note;
    public bool Added = false;

    public Texture Texture;

    public bool EditorHitSoundQueued = false;

    public NoteDrawable(Vector2 position, Texture texture, FontSystem font, int size) {
        this.Position = position;
        this.Texture  = texture;

        this.NoteTexture = new TexturedDrawable(this.Texture, new(0)) {
            OriginType  = OriginType.TopLeft,
            Clickable   = false,
            CoverClicks = false
        };

        this.RawTextDrawable = new TextDrawable(new(this.NoteTexture.Size.X / 2f, this.NoteTexture.Size.Y + 20), font, "", size) {
            Scale      = new(1.5f),
            OriginType = OriginType.TopCenter
        };
        this.ToTypeTextDrawable = new TextDrawable(new(this.NoteTexture.Size.X / 2f, this.RawTextDrawable.Position.Y + 40), font, "", size) {
            Scale      = new(1.5f),
            OriginType = OriginType.TopCenter
        };

        this._drawables.Add(this.NoteTexture);
        this._drawables.Add(this.RawTextDrawable);
        this._drawables.Add(this.ToTypeTextDrawable);

        this.OriginType = OriginType.Center;
    }

    public override Vector2 Size => this.NoteTexture.Size * this.Scale;

    public void CreateTweens(GameplayDrawableTweenArgs tweenArgs) {
        this.Tweens.Clear();

        Vector2 noteStartPos  = tweenArgs.IsEditor ? EditorScreen.NOTE_START_POS : Player.NOTE_START_POS;
        Vector2 noteEndPos    = tweenArgs.IsEditor ? EditorScreen.NOTE_END_POS : Player.NOTE_END_POS;
        Vector2 recepticlePos = tweenArgs.IsEditor ? EditorScreen.RECEPTICLE_POS : Player.RECEPTICLE_POS;

        float travelDistance = noteStartPos.X - recepticlePos.X;
        float travelRatio    = (float)(tweenArgs.ApproachTime / travelDistance);

        float afterTravelTime = (recepticlePos.X - noteEndPos.X) * travelRatio;

        this.Tweens.Add(
        new VectorTween(
        TweenType.Movement,
        new(noteStartPos.X, noteStartPos.Y + this.Note.YOffset),
        recepticlePos,
        (int)(this.Note.Time - tweenArgs.ApproachTime),
        (int)this.Note.Time
        ) {
            KeepAlive = tweenArgs.TweenKeepAlive
        }
        );

        this.Tweens.Add(
        new VectorTween(TweenType.Movement, recepticlePos, new(noteEndPos.X, recepticlePos.Y), (int)this.Note.Time, (int)(this.Note.Time + afterTravelTime)) {
            KeepAlive = tweenArgs.TweenKeepAlive
        }
        );
    }

    public void Reset() {
        this.RawTextDrawable.Text    = this.Note.Text;
        this.ToTypeTextDrawable.Text = "";

        this.NoteTexture.ColorOverride = this.Note.Color;
    }

    /// <summary>
    ///     Updates the positions of the text
    /// </summary>
    public void UpdateTextPositions() {
        this.RawTextDrawable.Position = new(this.NoteTexture.Size.X / 2f, this.NoteTexture.Size.Y + 20);

        this.ToTypeTextDrawable.Position = new(this.NoteTexture.Size.X / 2f, this.RawTextDrawable.Position.Y + 80);
    }

    /// <summary>
    ///     Types a character
    /// </summary>
    /// <param name="hiragana">The hiragana being typed</param>
    /// <param name="romaji">The romaji path to take</param>
    /// <param name="timeDifference">The time difference from now to the note</param>
    /// <param name="score">The current score</param>
    /// <returns>Whether the note has been fully completed</returns>
    public bool TypeCharacter(string hiragana, string romaji, double timeDifference, Player player) {
        if (this.Note.TypedRomaji == string.Empty && this.Note.Typed == string.Empty) {
            if (timeDifference < player.TIMING_EXCELLENT)
                this.Note.HitResult = HitResult.Excellent;
            else if (timeDifference < player.TIMING_GOOD)
                this.Note.HitResult = HitResult.Good;
            else if (timeDifference < player.TIMING_FAIR)
                this.Note.HitResult = HitResult.Fair;
            else if (timeDifference < player.TIMING_POOR)
                this.Note.HitResult = HitResult.Poor;

            Color     finalColor      = Helpers.RotateColor(this.Note.Color, 150);
            const int toFinalFadeTime = 100;
            this.NoteTexture.ColorOverride = finalColor;
            this.NoteTexture.Tweens.Add(
            new ColorTween(TweenType.Color, this.NoteTexture.ColorOverride, finalColor, FurballGame.Time, FurballGame.Time + toFinalFadeTime)
            );
            // this.NoteTexture.Tweens.Add(new ColorTween(TweenType.Color, finalColor,                     new((int)finalColor.R, finalColor.G, finalColor.B, 100), FurballGame.Time + toFinalFadeTime, FurballGame.Time + toFinalFadeTime + 200));
        }

        //Types the next character
        this.Note.TypedRomaji += romaji[this.Note.TypedRomaji.Length];

        //Checks if we have finished typing the current romaji
        if (string.Equals(this.Note.TypedRomaji, romaji)) {
            this.Note.Typed += hiragana;
            player.Score.AddScore(Player.SCORE_PER_CHARACTER);
            //Clear the typed romaji
            this.Note.TypedRomaji = string.Empty;

            //Checks if we have typed the full note
            if (string.Equals(this.Note.Typed, this.Note.Text)) {
                this.Hit();
                return true;
            }
        }

        return false;
    }

    public void Hit() {
        this.ToTypeTextDrawable.Visible = false;
        this.RawTextDrawable.Visible    = false;

        Color     finalColor = Helpers.RotateColor(this.Note.Color, 150);
        const int timeToDie  = 250;

        //random bool
        bool right = FurballGame.Random.Next(-1, 2) == 1;

        this.Tweens.Clear();

        this.NoteTexture.Tweens.Add(
        new ColorTween(
        TweenType.Color,
        new((int)finalColor.R, finalColor.G, finalColor.B, 127),
        new((int)this.NoteTexture.ColorOverride.R, this.NoteTexture.ColorOverride.G, this.NoteTexture.ColorOverride.B, 0),
        FurballGame.Time,
        FurballGame.Time + timeToDie
        )
        );
        this.NoteTexture.Tweens.Add(
        new PathTween(
        new Path(
        new PathSegment(
        this.NoteTexture.Position,
        this.NoteTexture.Position + new Vector2(FurballGame.Random.Next(0, 30) * (right ? 1 : -1), -FurballGame.Random.Next(80, 120)),
        this.NoteTexture.Position + new Vector2(FurballGame.Random.Next(0, 60) * (right ? 1 : -1), 150)
        )
        ),
        FurballGame.Time,
        FurballGame.Time + timeToDie
        )
        );
        // this.NoteTexture.Tweens.Add(new VectorTween(TweenType.Scale, this.NoteTexture.Scale, this.NoteTexture.Scale + new Vector2((float)(FurballGame.Random.NextDouble() / 4d + 1d)), (int)(FurballGame.Time + timeToDie / 2d), FurballGame.Time + timeToDie));
        this.Note.Typed = this.Note.Text;

        FurballGame.GameTimeScheduler.ScheduleMethod(
        delegate {
            this.Visible = false;
        },
        FurballGame.Time + timeToDie
        );
    }

    public void Miss() {
        // this.Visible        = false;
        pTypingGame.MusicTrackScheduler.ScheduleMethod(
        delegate {
            this.Visible = false;
        },
        pTypingGame.MusicTrackTimeSource.GetCurrentTime() + 1000
        );
        this.Note.Typed     = this.Note.Text;
        this.Note.HitResult = HitResult.Poor;
    }

    // public override void Draw(double time, DrawableBatch batch, DrawableManagerArgs args) {
    //     batch.SpriteBatch.Draw(
    //     this.Texture,
    //     args.Position * FurballGame.VerticalRatio,
    //     null,
    //     args.Color,
    //     args.Rotation,
    //     Vector2.Zero,
    //     args.Scale * FurballGame.VerticalRatio,
    //     args.Effects,
    //     0f
    //     );
    //
    //     // FIXME: this is a bit of a hack, it should definitely be done differently
    //     args.Scale = new(1f);
    //     // tempArgs.Position   -= this.LabelTextDrawable.Size / 2f + this.Size / 2f;
    //     args.Position.Y += 100f;
    //     args.Position.X 
    //     // args.Position.X += this.RawTextDrawable.Size.X / 4f;
    //     args.Color = new(this.RawTextDrawable.ColorOverride.R, this.RawTextDrawable.ColorOverride.G, this.RawTextDrawable.ColorOverride.B, args.Color.A);
    //     this.RawTextDrawable.Draw(time, batch, args);
    // }
}