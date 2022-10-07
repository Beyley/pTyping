using System.Numerics;
using FontStashSharp;
using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using Furball.Engine.Engine.Input.Events;
using Furball.Vixie.Backends.Shared;

namespace pTyping.UiElements;

public class ContextMenuDrawable : Drawable {
	private readonly List<(string, Delegate)> _items;
	private readonly DynamicSpriteFont        Font;

	public override Vector2 Size {
		get;
	}

	private const float OUTSIDE_PADDING    = 3f;
	private const float SIZE_BETWEEN_ITEMS = 3f;

	public ContextMenuDrawable(Vector2 position, List<(string, Delegate)> items, FontSystem fontSystem, float fontSize) {
		this._items = items;

		this.Font = fontSystem.GetFont(fontSize);

		float x = 0;
		foreach ((string item, Delegate _) in items) {
			Vector2 itemSize = this.Font.MeasureString(item);

			if (itemSize.X > x)
				x = itemSize.X;
		}

		float y = items.Count * this.Font.LineHeight + (items.Count - 1) * SIZE_BETWEEN_ITEMS;

		this.Size     = new Vector2(x + OUTSIDE_PADDING * 2, y + OUTSIDE_PADDING * 2);
		this.Position = position;

		this.OnClick += this.HandleClick;
	}

	private void HandleClick(object? sender, MouseButtonEventArgs e) {
		Vector2 relativePos = e.Mouse.Position - this.RealPosition;

		int index = (int)(relativePos.Y / (this.Font.LineHeight + SIZE_BETWEEN_ITEMS));

		if (index < 0 || index >= this._items.Count)
			return;

		(string _, Delegate action) = this._items[index];

		action.DynamicInvoke();
	}

	public override void Draw(double time, DrawableBatch batch, DrawableManagerArgs args) {
		batch.Draw(FurballGame.WhitePixel, args.Position, this.Size, new Color(150, 150, 150, 170));

		float y = OUTSIDE_PADDING;
		foreach ((string item, Delegate _) in this._items) {
			Vector2 itemSize = this.Font.MeasureString(item);

			batch.DrawString(this.Font, item, args.Position + new Vector2(OUTSIDE_PADDING, y), Color.White);

			y += this.Font.LineHeight + SIZE_BETWEEN_ITEMS;
		}
	}
}
