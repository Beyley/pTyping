using Furball.Engine;
using Microsoft.Xna.Framework;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using pTyping.Songs;
using SpriteFontPlus;

namespace pTyping.Drawables {
	public class NoteDrawable : ManagedDrawable {
		public TextDrawable LabelTextDrawable;

		public Note Note;

		public NoteDrawable(byte[] font, float size, CharacterRange[] range = null) {
			this.LabelTextDrawable = new TextDrawable(Vector2.Zero, font, "", size, range);
			this.CircleRadius      = 40f;
			this.Circular          = true;
		}

		public bool Type() {
			this.Note.Typed += this.Note.TextToType[this.Note.Typed.Length];

			if (string.Equals(this.Note.Typed, this.Note.TextToType)) {
				this.Hit();
				return true;
			} 
			
			return false;
		}

		public void Hit() {
			this.Visible  = false;
			this.Note.Hit = HitResult.Hit;
		}

		public void Miss() {
			this.Visible  = false;
			this.Note.Hit = HitResult.Miss;
		}
		
		public override void Draw(GameTime time, DrawableBatch batch, DrawableManagerArgs args) {
			batch.ShapeBatch.DrawCircle(
				args.Position * FurballGame.VerticalRatio, 
				40f * FurballGame.VerticalRatio, 
				Color.Transparent, 
				args.Color, 
				1f * FurballGame.VerticalRatio
			);
			
			// FIXME: this is a bit of a hack, it should definitely be done differently
			DrawableManagerArgs tempArgs = args;
			tempArgs.Position   -= this.LabelTextDrawable.Size / 2f;
			tempArgs.Position.Y += 80f;
			tempArgs.Color      =  this.LabelTextDrawable.ColorOverride;
			this.LabelTextDrawable.Draw(time, batch, tempArgs);
		}
	}
}
