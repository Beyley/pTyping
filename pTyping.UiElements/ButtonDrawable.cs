using System.Numerics;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using Furball.Vixie.Backends.Shared.Renderers;

namespace pTyping.UiElements;

public class ButtonDrawable : CompositeDrawable {
	private readonly TextDrawable _text;
	public ButtonDrawable(TextDrawable text, Vector2 size) {
		this._text = text;

		this.Drawables.Add(this._text);
	}

	public override void Draw(double time, DrawableBatch batch, DrawableManagerArgs args) {
		MappedData mappedData = batch.Reserve(4, 6);

		base.Draw(time, batch, args);
	}
}
