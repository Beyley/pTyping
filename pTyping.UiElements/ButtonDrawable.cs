using System.Drawing;
using System.Numerics;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using Furball.Engine.Engine.Helpers;
using Furball.Vixie.Backends.Shared.Renderers;

namespace pTyping.UiElements;

public class ButtonDrawable : CompositeDrawable {
	private readonly TextDrawable _text;

	public readonly Bindable<Vector2> ButtonSize;

	public override Vector2 Size => this.ButtonSize.Value * this.Scale;

	public ButtonDrawable(TextDrawable text, Vector2 size) {
		this._text      = text;
		this.ButtonSize = new Bindable<Vector2>(size);

		//Make the text origin center
		this._text.OriginType = OriginType.Center;

		this.Drawables.Add(this._text);

		//When the user changes the button size, relayout the text
		this.ButtonSize.OnChange += (_, _) => this.Relayout();

		this.Relayout();
	}

	private void Relayout() {
		//Set the position of the text to the center of the button
		this._text.Position = this.ButtonSize.Value / 2f;
	}

	public override void Draw(double time, DrawableBatch batch, DrawableManagerArgs args) {
		batch.ScissorPush(this, new Rectangle(this.RealPosition.ToPoint(), this.RealSize.ToSize()));
		
		MappedData mappedData = batch.Reserve(4, 6);

		//TODO

		base.Draw(time, batch, args);

		batch.ScissorPop(this);
	}
}
