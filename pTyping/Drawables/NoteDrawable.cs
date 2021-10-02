using SpriteFontPlus;
using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using pTyping.Songs;
using pTyping.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace pTyping.Drawables {
	public class NoteDrawable : TexturedDrawable {
		public TextDrawable LabelTextDrawable;

		public Note Note;

		public NoteDrawable(Vector2 position, Texture2D texture, byte[] font, float size, CharacterRange[] range = null) : base(texture, position) {
			this.LabelTextDrawable       = new TextDrawable(Vector2.Zero, font, "", size, new []{CharacterRange.BasicLatin, CharacterRange.Hiragana});
			this.CircleRadius            = 40f;
			this.Circular                = false;
			this.LabelTextDrawable.Scale = new(1);
		}

		public bool Type(string romaji, double timeDifference) {
			if (this.Note.TypedRomaji == string.Empty && this.Note.Typed == string.Empty) {
				if (timeDifference < PlayerScreen.TimingExcellent)
					this.Note.HitResult = HitResult.Excellent;
				else if (timeDifference < PlayerScreen.TimingGood)
					this.Note.HitResult = HitResult.Good;
				else if (timeDifference < PlayerScreen.TimingFair)
					this.Note.HitResult = HitResult.Fair;
				else if (timeDifference < PlayerScreen.TimingPoor)
					this.Note.HitResult = HitResult.Poor;
			}
			
			
			this.Note.TypedRomaji += romaji[this.Note.TypedRomaji.Length];

			if (string.Equals(this.Note.TypedRomaji, romaji)) {
				this.Note.Typed += this.Note.Text[this.Note.Typed.Length];
				this.Note.TypedRomaji = string.Empty;

				if(string.Equals(this.Note.Typed, this.Note.Text)) {
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
			this.Note.HitResult = HitResult.Miss;
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
				args.LayerDepth);
			
			// FIXME: this is a bit of a hack, it should definitely be done differently
			DrawableManagerArgs tempArgs = args;
			tempArgs.Scale      =  new(1f);
			// tempArgs.Position   -= this.LabelTextDrawable.Size / 2f + this.Size / 2f;
			tempArgs.Position.Y += 100f;
			tempArgs.Position.X += this.LabelTextDrawable.Size.X / 4f;
			tempArgs.Color      =  this.LabelTextDrawable.ColorOverride;
			this.LabelTextDrawable.Draw(time, batch, tempArgs);
		}
	}
}
