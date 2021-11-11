using FontStashSharp;
using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using pTyping.Scores;
using pTyping.Songs;

namespace pTyping.Graphics.Player {
    public class NoteDrawable : TexturedDrawable {
        public TextDrawable LabelTextDrawable;

        public Note Note;
        public bool Added = false;

        public NoteDrawable(Vector2 position, Texture2D texture, FontSystem font, int size) : base(texture, position) {
            this.LabelTextDrawable       = new TextDrawable(Vector2.Zero, font, "", size);
            this.LabelTextDrawable.Scale = new(1);
        }

        public override Vector2 Size => new Vector2(this.Texture.Width, this.Texture.Height) * this.Scale;

        /// <summary>
        ///     Types a character
        /// </summary>
        /// <param name="hiragana">The hiragana being typed</param>
        /// <param name="romaji">The romaji path to take</param>
        /// <param name="timeDifference">The time difference from now to the note</param>
        /// <returns>Whether the note has been fully completed</returns>
        public bool TypeCharacter(string hiragana, string romaji, double timeDifference, PlayerScore score) {
            if (this.Note.TypedRomaji == string.Empty && this.Note.Typed == string.Empty) {
                this.Note.HitResult = timeDifference switch {
                    < Player.TIMING_EXCELLENT => HitResult.Excellent,
                    < Player.TIMING_GOOD      => HitResult.Good,
                    < Player.TIMING_FAIR      => HitResult.Fair,
                    < Player.TIMING_POOR      => HitResult.Poor,
                    _                         => this.Note.HitResult
                };
            }

            //Types the next character
            this.Note.TypedRomaji += romaji[this.Note.TypedRomaji.Length];

            //Checks if we have finished typing the current romaji
            if (string.Equals(this.Note.TypedRomaji, romaji)) {
                this.Note.Typed += hiragana;
                score.AddScore(Player.SCORE_PER_CHARACTER);
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

        public override void Draw(GameTime time, DrawableBatch batch, DrawableManagerArgs args) {
            batch.SpriteBatch.Draw(
            this.Texture,
            args.Position * FurballGame.VerticalRatio,
            null,
            args.Color,
            args.Rotation,
            Vector2.Zero,
            args.Scale * FurballGame.VerticalRatio,
            args.Effects,
            0f
            );

            // FIXME: this is a bit of a hack, it should definitely be done differently
            args.Scale = new(1f);
            // tempArgs.Position   -= this.LabelTextDrawable.Size / 2f + this.Size / 2f;
            args.Position.Y += 100f;
            args.Position.X += this.LabelTextDrawable.Size.X / 4f;
            args.Color = new(this.LabelTextDrawable.ColorOverride.R, this.LabelTextDrawable.ColorOverride.G, this.LabelTextDrawable.ColorOverride.B, args.Color.A);
            this.LabelTextDrawable.Draw(time, batch, args);
        }
    }
}
