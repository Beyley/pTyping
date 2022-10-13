using System.Numerics;
using FontStashSharp;
using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Input.Events;
using Furball.Vixie.Backends.Shared;
using Furball.Vixie.Helpers;

namespace pTyping.UiElements;

public class ToolbarDrawable : CompositeDrawable {
	private readonly Dictionary<string, Action> _buttons;

	public const float HEIGHT      = 30f;
	public const float MARGIN      = 5f;
	public const float ITEM_HEIGHT = HEIGHT - MARGIN * 2;
	public const float ITEM_MARGIN = (ITEM_HEIGHT - FONT_SIZE) / 2f;

	public const float FONT_SIZE = 15f;

	private static DynamicSpriteFont? _Font;

	public static Color BackgroundColor = new Color(100, 100, 100);
	public static Color ItemColor       = new Color(120, 120, 120);

	private         Vector2 _size;
	public override Vector2 Size => this._size;

	public ToolbarDrawable(Dictionary<string, Action> buttons, FontSystem font) {
		_Font ??= font.GetFont(FONT_SIZE);

		this._buttons = buttons;

		float x = 0;
		foreach ((string item, Action action) in this._buttons) {
			ToolbarEntryDrawable drawable = new ToolbarEntryDrawable(item, action) {
				Position = new Vector2(x + MARGIN, MARGIN)
			};

			this.Children.Add(drawable);

			x += drawable.Size.X + MARGIN;
		}

		this._size = new Vector2(FurballGame.WindowWidth, HEIGHT);

		FurballGame.Instance.OnRelayout += this.Relayout;
	}

	public override void Draw(double time, DrawableBatch batch, DrawableManagerArgs args) {
		//Just manually draw the background rectangle i do NOT want to use a drawable for this it's just a rectangle
		//NOTE: we do some overfill here to make sure that theres no gap due to rounding point errors
		batch.FillRectangle(args.Position, new Vector2(FurballGame.WindowWidth + 10, HEIGHT * args.Scale.Y), BackgroundColor);

		base.Draw(time, batch, args);
	}

	private void Relayout(object? sender, Vector2 e) {
		this._size = new Vector2(FurballGame.WindowWidth, HEIGHT);
	}

	public override void Dispose() {
		base.Dispose();

		FurballGame.Instance.OnRelayout -= this.Relayout;
	}

	private sealed class ToolbarEntryDrawable : CompositeDrawable {
		private readonly Action _action;

		private readonly float _textWidth;

		public override Vector2 Size => new Vector2(this._textWidth + ITEM_MARGIN * 2f, ITEM_HEIGHT);

		public ToolbarEntryDrawable(string text, Action action) {
			this._action = action;

			this.ChildrenInvisibleToInput = true;

			Guard.EnsureNonNull(_Font, "_Font");

			Vector2 measureString = _Font!.MeasureString(text);

			this._textWidth = measureString.X;

			this.Children.Add(new RectanglePrimitiveDrawable(Vector2.Zero, this.Size, 0, true) {
				ColorOverride = ItemColor
			});
			this.Children.Add(new Furball.Engine.Engine.Graphics.Drawables.TextDrawable(this.Size / 2f, _Font.FontSystem, text, _Font.FontSize) {
				OriginType = OriginType.Center
			});

			this.OnClick += this.ItemClick;
		}

		public override void Dispose() {
			base.Dispose();

			this.OnClick -= this.ItemClick;
		}

		private void ItemClick(object? sender, MouseButtonEventArgs e) {
			this._action.Invoke();
		}
	}
}
