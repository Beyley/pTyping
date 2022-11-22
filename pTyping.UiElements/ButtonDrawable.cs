using System.Drawing;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Helpers;
using Furball.Vixie.Backends.Shared.Renderers;
using Color = Furball.Vixie.Backends.Shared.Color;

namespace pTyping.UiElements;

public class ButtonDrawable : CompositeDrawable {
	private readonly TextDrawable _text;

	public readonly Bindable<Vector2> ButtonSize;

	public override Vector2 Size => this.ButtonSize.Value * this.Scale;

	private Color  _colorAdd = new Color(1f, 1f, 1f, 1f);
	private ColorTween? _hoverColorTween;

	public Color BaseColor = new Color(1f, 1f, 1f);
	public Color HoverColor = new Color(1.4f, 1.4f, 1.4f);

	public ButtonDrawable(TextDrawable text, Vector2 size) {
		this._text      = text;
		this.ButtonSize = new Bindable<Vector2>(size);

		//Make the text origin center
		this._text.OriginType = OriginType.Center;

		this.Children!.Add(this._text);

		//When the user changes the button size, relayout the text
		this.ButtonSize.OnChange += (_, _) => this.Relayout();

		this.Relayout();

		const float fadeTime = 250;
		this.OnHover += (_, _) => {
			this._hoverColorTween = new ColorTween(TweenType.Color, this._colorAdd, this.HoverColor, FurballGame.Time, FurballGame.Time + fadeTime);
		};
		this.OnHoverLost += (_, _) => {
			this._hoverColorTween = new ColorTween(TweenType.Color, this._colorAdd, this.BaseColor, FurballGame.Time, FurballGame.Time + fadeTime);
		};

		this.ChildrenInvisibleToInput = true;
	}

	private void Relayout() {
		//Set the position of the text to the center of the button
		this._text.Position = this.ButtonSize.Value / 2f;
	}

	public override void Update(double time) {
		base.Update(time);
		
		this._hoverColorTween?.Update(FurballGame.Time);
		this._colorAdd = this._hoverColorTween?.GetCurrent() ?? this.BaseColor;
	}

	public override void Draw(double time, DrawableBatch batch, DrawableManagerArgs args) {
		batch.ScissorPush(this, new Rectangle(this.RealPosition.ToPoint(), this.RealSize.ToSize()));

		MappedData mappedData = batch.Reserve(4, 6, FurballGame.WhitePixel);

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

			for (int i = 0; i < mappedData.VertexCount; i++)
				mappedData.VertexPtr[i].TexId = mappedData.TextureId;

			mappedData.VertexPtr[topLeft].Color     = new Color((byte)Math.Min(255, 200 * this._colorAdd.Rf * args.Color.Rf), (byte)Math.Min(255, 200 * this._colorAdd.Gf * args.Color.Gf), (byte)Math.Min(255, 200 * this._colorAdd.Bf * args.Color.Bf), (byte)Math.Min(255, 255 * this._colorAdd.Af * args.Color.Af));
			mappedData.VertexPtr[bottomLeft].Color  = new Color((byte)Math.Min(255, 200 * this._colorAdd.Rf * args.Color.Rf), (byte)Math.Min(255, 200 * this._colorAdd.Gf * args.Color.Gf), (byte)Math.Min(255, 200 * this._colorAdd.Bf * args.Color.Bf), (byte)Math.Min(255, 255 * this._colorAdd.Af * args.Color.Af));
			mappedData.VertexPtr[topRight].Color    = new Color((byte)Math.Min(255, 100 * this._colorAdd.Rf * args.Color.Rf), (byte)Math.Min(255, 100 * this._colorAdd.Gf * args.Color.Gf), (byte)Math.Min(255, 100 * this._colorAdd.Bf * args.Color.Bf), (byte)Math.Min(255, 255 * this._colorAdd.Af * args.Color.Af));
			mappedData.VertexPtr[bottomRight].Color = new Color((byte)Math.Min(255, 100 * this._colorAdd.Rf * args.Color.Rf), (byte)Math.Min(255, 100 * this._colorAdd.Gf * args.Color.Gf), (byte)Math.Min(255, 100 * this._colorAdd.Bf * args.Color.Bf), (byte)Math.Min(255, 255 * this._colorAdd.Af * args.Color.Af));

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
