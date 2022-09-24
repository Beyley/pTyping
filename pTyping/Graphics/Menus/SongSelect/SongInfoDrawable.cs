using System.Numerics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using pTyping.Shared.Beatmaps;

namespace pTyping.Graphics.Menus.SongSelect;

public class SongInfoDrawable : CompositeDrawable {
	private readonly RectanglePrimitiveDrawable _backgroundDrawable;
	private readonly RichTextDrawable           _descriptionDrawable;
	private readonly ScrollableContainer        _scrollableContainer;

	public sealed override Vector2 Size => new Vector2(300, 475) * this.Scale;

	public SongInfoDrawable() {
		this.Drawables.Add(
			this._backgroundDrawable = new RectanglePrimitiveDrawable(Vector2.Zero, this.Size, 2, true) {
				ColorOverride = new(100, 100, 100, 100)
			}
		);

		this.Drawables.Add(this._scrollableContainer = new ScrollableContainer(this.Size));

		this._scrollableContainer.Add(
			this._descriptionDrawable = new RichTextDrawable(Vector2.Zero, "", pTypingGame.JapaneseFont, 20) {
				Clickable   = false,
				CoverClicks = false
			}
		);
		this._descriptionDrawable.Layout.Width = (int?)this.Size.X;
	}

	public void SetSong(Beatmap song) {
		this._descriptionDrawable.Layout.Text = song.Info.Description ?? "";

		this._scrollableContainer.RecalculateMax();
	}
}
