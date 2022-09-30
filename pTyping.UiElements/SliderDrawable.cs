using System.Drawing;
using System.Globalization;
using System.Numerics;
using FontStashSharp;
using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Helpers;
using Furball.Engine.Engine.Input.Events;
using Furball.Vixie.Backends.Shared.Renderers;
using pTyping.Shared.ObjectModel;
using Color = Furball.Vixie.Backends.Shared.Color;

namespace pTyping.UiElements;

public class SliderDrawable <T> : Drawable where T : struct, IComparable, IConvertible, IEquatable<T>, IComparable<T>, IFormattable {
	public readonly BoundNumber<T> Value;

	public const float LINE_WIDTH         = 1.5f;
	public const float STOPPER_LINE_WIDTH = 1;
	public const float HEIGHT             = 35;
	public const float MIDDLE             = HEIGHT / 2f;

	private const float HANDLE_SIZE                = 7.5f;
	private const float STOPPER_HEIGHT_FROM_MIDDLE = 7;

	public float Width;

	private bool _drawPreview;
	private bool _dragging;

	private readonly DynamicSpriteFont _previewFont;

	private float      _currentHandleX;
	private FloatTween _handleMovementTween;

	public override Vector2 Size => new Vector2(this.Width, HEIGHT) * this.Scale;

	public SliderDrawable(BoundNumber<T> num, float width = 300) {
		this.Value = num;
		this.Width = width;

		this.OnHover     += this.HoverBegin;
		this.OnHoverLost += this.HoverLost;

		this.OnDrag      += this.Drag;
		this.OnDragBegin += this.DragBegin;
		this.OnDragEnd   += this.DragEnd;

		this._previewFont = FurballGame.DefaultFont.GetFont(20);

		float handleX = this.GetHandleX();
		this._handleMovementTween = new FloatTween(TweenType.Fade, handleX, handleX, 0, 0);
		this._currentHandleX      = this._handleMovementTween.GetCurrent();
	}

	private void DragEnd(object? sender, MouseDragEventArgs e) {
		this._dragging = false;
	}
	private void DragBegin(object? sender, MouseDragEventArgs e) {
		this._dragging = true;
	}

	private void Drag(object? sender, MouseDragEventArgs e) {
		float mouseProgress = (e.Position.X - this.RealPosition.X) / this.RealSize.X;

		float startVal = this.Value.MinValue.ToSingle(CultureInfo.InvariantCulture);
		float endVal   = this.Value.MaxValue.ToSingle(CultureInfo.InvariantCulture);

		float range = endVal - startVal;

		this.Value.Value = (T)Convert.ChangeType(mouseProgress * range + startVal, typeof(T));

		//Set the handle movement tween
		this._handleMovementTween = new FloatTween(
			TweenType.Fade,
			this._currentHandleX,
			this.GetHandleX(),
			FurballGame.Time,
			FurballGame.Time + 75,
			Easing.In
		);
	}

	public override void Update(double time) {
		base.Update(time);

		//Update the handle movement tween
		this._handleMovementTween.Update(FurballGame.Time);
		//Save the current handle X
		this._currentHandleX = this._handleMovementTween.GetCurrent();
	}

	private void HoverLost(object? sender, EventArgs e) {
		FurballGame.InputManager.OnMouseMove -= this.MouseMove;
	}

	private void HoverBegin(object? sender, EventArgs e) {
		FurballGame.InputManager.OnMouseMove += this.MouseMove;
	}

	private void MouseMove(object? sender, MouseMoveEventArgs e) {
		this._drawPreview = this.PointInHandle(e.Position);
	}

	private bool PointInHandle(Vector2 p) {
		float handleX = this.GetHandleX();

		RectangleF handleRect = new RectangleF(handleX - HANDLE_SIZE, MIDDLE - HANDLE_SIZE, HANDLE_SIZE * 2f, HANDLE_SIZE * 2f);
		handleRect.X += this.RealPosition.X;
		handleRect.Y += this.RealPosition.Y;

		return handleRect.Contains(p.ToPointF());
	}

	public override void Draw(double time, DrawableBatch batch, DrawableManagerArgs args) {
		base.Draw(time, batch, args);


		//Draw the main line that you drag across
		batch.Draw(FurballGame.WhitePixel, args.Position with {
			Y = args.Position.Y + MIDDLE - LINE_WIDTH / 2f
		}, new Vector2(this.Width, LINE_WIDTH));

		//Draw the left stopper
		batch.Draw(FurballGame.WhitePixel, args.Position with {
			Y = args.Position.Y + MIDDLE - STOPPER_HEIGHT_FROM_MIDDLE
		}, new Vector2(STOPPER_LINE_WIDTH, STOPPER_HEIGHT_FROM_MIDDLE * 2f));

		//Draw the right stopper
		batch.Draw(FurballGame.WhitePixel, new Vector2(args.Position.X + this.RealSize.X, args.Position.Y + MIDDLE - STOPPER_HEIGHT_FROM_MIDDLE), new Vector2(STOPPER_LINE_WIDTH, STOPPER_HEIGHT_FROM_MIDDLE * 2f));

		float handleX = this._currentHandleX;

		MappedData mappedData = batch.Reserve(4, 6);

		unsafe {
			const int top    = 0;
			const int right  = 1;
			const int left   = 2;
			const int bottom = 3;

			mappedData.VertexPtr[top].Position    = new Vector2(args.Position.X           + handleX, args.Position.Y + MIDDLE - HANDLE_SIZE);
			mappedData.VertexPtr[right].Position  = new Vector2(args.Position.X           + handleX                           + HANDLE_SIZE, args.Position.Y + MIDDLE);
			mappedData.VertexPtr[left].Position   = new Vector2(args.Position.X + handleX - HANDLE_SIZE, args.Position.Y      + MIDDLE);
			mappedData.VertexPtr[bottom].Position = new Vector2(args.Position.X           + handleX, args.Position.Y          + MIDDLE + HANDLE_SIZE);

			long texId = batch.GetTextureIdForReserve(FurballGame.WhitePixel);
			for (int i = 0; i < mappedData.VertexCount; i++) {
				mappedData.VertexPtr[i].TexId = texId;
				mappedData.VertexPtr[i].Color = new Color(0.75f, 0.75f, 0.75f);
			}

			mappedData.IndexPtr[0] = (ushort)(top    + mappedData.IndexOffset);
			mappedData.IndexPtr[1] = (ushort)(left   + mappedData.IndexOffset);
			mappedData.IndexPtr[2] = (ushort)(right  + mappedData.IndexOffset);
			mappedData.IndexPtr[3] = (ushort)(bottom + mappedData.IndexOffset);
			mappedData.IndexPtr[4] = (ushort)(right  + mappedData.IndexOffset);
			mappedData.IndexPtr[5] = (ushort)(left   + mappedData.IndexOffset);
		}

		if (this._drawPreview || this._dragging) {
			string text = $"{this.Value.Value:N2}";

			const float margin    = 3f;
			const float posOffset = 10f;

			Vector2 size = this._previewFont.MeasureString(text);

			Vector2 mousePos = FurballGame.InputManager.Mice[0].Position;

			//Draw the preview background
			batch.Draw(FurballGame.WhitePixel, mousePos + new Vector2(posOffset), size + new Vector2(margin * 2f), new Color(0.3f, 0.3f, 0.3f));
			//Draw the preview text
			batch.DrawString(this._previewFont, text, mousePos + new Vector2(posOffset) + new Vector2(margin), Color.White);
		}
	}

	private float GetHandleX() {
		//The current value of the slider as a float
		float currVal = this.Value.Value.ToSingle(CultureInfo.InvariantCulture);
		//The range of values (Max - Min)
		float range = this.Value.MaxValue.ToSingle(CultureInfo.InvariantCulture) - this.Value.MinValue.ToSingle(CultureInfo.InvariantCulture);
		//0-1 progress of how far we are along the slider
		float progress = (currVal - this.Value.MinValue.ToSingle(CultureInfo.InvariantCulture)) / range;

		return progress * this.Width;
	}
}
