using System;
using Furball.Engine;
using MonoGame.Extended;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using pTyping.Songs;
using SpriteFontPlus;

namespace pTyping.Drawables {
	public class NoteDrawable : ManagedDrawable {
		public TextDrawable TextDrawable;

		public Note Note;

		public NoteDrawable(byte[] font, string text, float size, CharacterRange[] range = null) {
			this.TextDrawable = new TextDrawable(font, text, size, range);
		}

		public bool Type() {
			this.Note.Typed += this.Note.TextToShow[this.Note.Typed.Length];

			if (string.Equals(this.Note.Typed, this.Note.TextToType)) {
				this.Hit();
				return true;
			} 
			
			return false;
		}

		public void Hit() {
			this.Note.Hit = HitResult.Hit;
			
			this.Visible = false;
			Console.WriteLine($"{this.Note.Time} ({this.Note.TextToShow}): Is hit!");
		}

		public void Miss() {
			this.Visible  = false;
			this.Note.Hit = HitResult.Miss;
			Console.WriteLine($"{this.Note.Time} ({this.Note.TextToShow}): Missed!");
		}
		
		public override void Draw(GameTime time, SpriteBatch batch, DrawableManagerArgs args) {
			batch.DrawCircle(args.Position, 40f, 10, args.Color, 1f, args.LayerDepth);
			
			// FIXME: this is a bit of a hack, it should definitely be done differently
			DrawableManagerArgs tempArgs = args;
			tempArgs.Position   -= this.TextDrawable.Size / 2f;
			tempArgs.Position.Y += 80f;
			tempArgs.Color      =  this.TextDrawable.ColorOverride;
			tempArgs.Scale      /= FurballGame.VerticalRatio;
			this.TextDrawable.Draw(time, batch, tempArgs);
		}
	}
}
