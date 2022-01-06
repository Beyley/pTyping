using FontStashSharp;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using pTyping.Graphics.Editor;
using pTyping.Songs;

namespace pTyping.Graphics.Player {
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

        public Texture2D Texture;

        public bool EditorHitSoundQueued = false;

        public NoteDrawable(Vector2 position, Texture2D texture, FontSystem font, int size) {
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
            new VectorTween(
            TweenType.Movement,
            recepticlePos,
            new(noteEndPos.X, recepticlePos.Y),
            (int)this.Note.Time,
            (int)(this.Note.Time + afterTravelTime)
            ) {
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

                // this.Note.HitResult = timeDifference switch {
                //     < player.TIMING_EXCELLENT => ,
                //     < player.TIMING_GOOD      => HitResult.Good,
                //     < player.TIMING_FAIR      => HitResult.Fair,
                //     < player.TIMING_POOR      => HitResult.Poor,
                //     _                         => this.Note.HitResult
                // };
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
            this.Visible    = false;
            this.Note.Typed = this.Note.Text;
            // this.Note.HitResult = HitResult.Excellent;
        }

        public void Miss() {
            this.Visible        = false;
            this.Note.Typed     = this.Note.Text;
            this.Note.HitResult = HitResult.Poor;
        }

        // public override void Draw(GameTime time, DrawableBatch batch, DrawableManagerArgs args) {
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
}
