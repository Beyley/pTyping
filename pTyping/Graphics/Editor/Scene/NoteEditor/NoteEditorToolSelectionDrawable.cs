using System;
using System.Numerics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Vixie.Backends.Shared;

namespace pTyping.Graphics.Editor.Scene.NoteEditor;

public class NoteEditorToolSelectionDrawable : CompositeDrawable {
	private readonly Action _toolChanged;

	private readonly RectanglePrimitiveDrawable _titleBarBackground;
	private readonly TextDrawable               _titleBarText;

	private readonly RectanglePrimitiveDrawable _contentsBackground;

	private const float WIDTH            = 200;
	private const float TITLE_BAR_HEIGHT = 30;
	private const float CONTENTS_HEIGHT  = 250 - TITLE_BAR_HEIGHT;

	private const float CONTENT_MARGIN = 5f;

	private static readonly Color TitleBarColor           = new Color(120, 120, 120);
	private static readonly Color ContentsBackgroundColor = new Color(100, 100, 100);

	public override Vector2 Size => new Vector2(WIDTH, TITLE_BAR_HEIGHT + CONTENTS_HEIGHT) * this.Scale;

	public NoteEditorToolSelectionDrawable(Vector2 position, Action toolChanged) {
		this._toolChanged = toolChanged;
		this.Position     = position;

		//Title bar
		this.Children.Add(this._titleBarBackground = new RectanglePrimitiveDrawable(Vector2.Zero, new Vector2(WIDTH, TITLE_BAR_HEIGHT), 0, true) {
			ColorOverride = TitleBarColor
		});
		this.Children.Add(this._titleBarText = new TextDrawable(new Vector2(WIDTH / 2f, TITLE_BAR_HEIGHT / 2f), pTypingGame.JapaneseFont, "Tool Selection", TITLE_BAR_HEIGHT - 4f) {
			OriginType = OriginType.Center
		});

		//Contents
		this.Children.Add(this._contentsBackground = new RectanglePrimitiveDrawable(new Vector2(0, TITLE_BAR_HEIGHT), new Vector2(WIDTH, CONTENTS_HEIGHT), 0, true) {
			ColorOverride = ContentsBackgroundColor
		});
	}
}
