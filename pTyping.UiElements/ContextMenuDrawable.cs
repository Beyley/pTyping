using System.Numerics;
using FontStashSharp;
using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using Furball.Engine.Engine.Input.Events;
using Furball.Vixie.Backends.Shared;
using Furball.Vixie.Helpers.Helpers;

namespace pTyping.UiElements;

public class ContextMenuDrawable : Drawable {
	private readonly List<(string, Action)> _items;
	private readonly DynamicSpriteFont      Font;

	private int _hoverIndex = -1;
	
	public override Vector2 Size {
		get;
	}

	private const float OUTSIDE_PADDING    = 3f;
	private const float SIZE_BETWEEN_ITEMS = 3f;

	public ContextMenuDrawable(Vector2 position, List<(string, Action)> items, FontSystem fontSystem, float fontSize) {
		this._items = items;

		this.Font = fontSystem.GetFont(fontSize);

		float x = 0;
		foreach ((string item, Action _) in items) {
			Vector2 itemSize = this.Font.MeasureString(item);

			if (itemSize.X > x)
				x = itemSize.X;
		}

		float y = items.Count * this.Font.LineHeight + (items.Count - 1) * SIZE_BETWEEN_ITEMS;

		this.Size     = new Vector2(x + OUTSIDE_PADDING * 2, y + OUTSIDE_PADDING * 2);
		this.Position = position;

		this.OnClick += this.HandleClick;
		
		this.OnHover += this.HoverBegin;
		this.OnHoverLost += this.HoverLost;
		this.RegisterForInput();
	}
	
	private void HoverBegin(object? sender, EventArgs e) {
		FurballGame.InputManager.OnMouseMove += this.MouseMove;
	}
	
	private void MouseMove(object? sender, MouseMoveEventArgs e) {
		//Dont do anything if we arent hovered on
		if (!this.IsHovered)
			return;
		
		Vector2 relativeMouse = e.Position - this.RealPosition;

		//Account for the padding
		relativeMouse.Y -= OUTSIDE_PADDING;
		
		this._hoverIndex = (int)(relativeMouse.Y / (this.Font.LineHeight + SIZE_BETWEEN_ITEMS));

		this._hoverIndex = this._hoverIndex.Clamp(0, this._items.Count - 1);
	}

	private void HoverLost(object? sender, EventArgs e) {
		FurballGame.InputManager.OnMouseMove -= this.MouseMove;

		this._hoverIndex = -1;
	}

	private void HandleClick(object? sender, MouseButtonEventArgs e) {
		Vector2 relativePos = e.Mouse.Position - this.RealPosition;

		//Account for the padding
		relativePos.Y -= OUTSIDE_PADDING;
		
		int index = (int)(relativePos.Y / (this.Font.LineHeight + SIZE_BETWEEN_ITEMS));

		if (index < 0 || index >= this._items.Count)
			return;

		(string _, Action action) = this._items[index];

		action.Invoke();
	}

	public override void Draw(double time, DrawableBatch batch, DrawableManagerArgs args) {
		batch.Draw(FurballGame.WhitePixel, args.Position, this.Size, new Color(150, 150, 150, 170));

		float y = OUTSIDE_PADDING;
		for (int i = 0; i < this._items.Count; i++) {
			(string item, Action _) = this._items[i];

			if (i == this._hoverIndex) {
				batch.Draw(FurballGame.WhitePixel, args.Position + new Vector2(OUTSIDE_PADDING, y - SIZE_BETWEEN_ITEMS / 2f), new Vector2(this.Size.X - OUTSIDE_PADDING * 2, this.Font.LineHeight + SIZE_BETWEEN_ITEMS), new Color(255, 255, 255, 100));
			}
			
			batch.DrawString(this.Font, item, args.Position + new Vector2(OUTSIDE_PADDING, y), Color.White);
			
			y += this.Font.LineHeight + SIZE_BETWEEN_ITEMS;
		}
	}
}
