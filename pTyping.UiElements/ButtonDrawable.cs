using System.Drawing;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using Furball.Engine.Engine.Helpers;
using Furball.Vixie.Backends.Shared.Renderers;
using Color = Furball.Vixie.Backends.Shared.Color;

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

		unsafe {
			const int topLeft     = 0;
			const int topRight    = 1;
			const int bottomLeft  = 2;
			const int bottomRight = 3;

			const float offset = 7.5f;
			
			mappedData.VertexPtr[topLeft].Position = args.Position with {
				X = args.Position.X + offset
			};
			mappedData.VertexPtr[topRight].Position = args.Position with {
				X = args.Position.X + this.RealSize.X
			};
			mappedData.VertexPtr[bottomLeft].Position = args.Position with {
				Y = args.Position.Y + this.RealSize.Y
			};
			mappedData.VertexPtr[bottomRight].Position = new(args.Position.X - offset + this.RealSize.X, args.Position.Y + this.RealSize.Y);

			long texId = batch.GetTextureIdForReserve(FurballGame.WhitePixel);
			for (int i = 0; i < mappedData.VertexCount; i++)
				mappedData.VertexPtr[i].TexId = texId;

			mappedData.VertexPtr[topLeft].Color     = new Color(200, 200, 200);
			mappedData.VertexPtr[bottomLeft].Color  = new Color(200, 200, 200);
			mappedData.VertexPtr[topRight].Color    = new Color(100, 100, 100);
			mappedData.VertexPtr[bottomRight].Color = new Color(100, 100, 100);

			mappedData.IndexPtr[0] = (ushort)(topLeft     + mappedData.IndexOffset);
			mappedData.IndexPtr[1] = (ushort)(bottomLeft  + mappedData.IndexOffset);
			mappedData.IndexPtr[2] = (ushort)(topRight    + mappedData.IndexOffset);
			mappedData.IndexPtr[3] = (ushort)(bottomRight + mappedData.IndexOffset);
			mappedData.IndexPtr[4] = (ushort)(topRight    + mappedData.IndexOffset);
			mappedData.IndexPtr[5] = (ushort)(bottomLeft  + mappedData.IndexOffset);
		}

		base.Draw(time, batch, args);

		batch.ScissorPop(this);
	}
}
