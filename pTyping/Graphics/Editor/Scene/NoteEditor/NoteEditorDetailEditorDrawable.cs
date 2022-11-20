using System.Numerics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Vixie.Backends.Shared;
using pTyping.UiGenerator;

namespace pTyping.Graphics.Editor.Scene.NoteEditor;

public class NoteEditorDetailEditorDrawable : CompositeDrawable {
	private readonly NoteEditorPlayFieldContainer _playFieldContainer;

	private const float CONTENTS_HEIGHT = 250 - NoteEditorToolSelectionDrawable.TITLE_BAR_HEIGHT;

	private static readonly Color                      TitleBarColor           = new Color(120, 120, 120);
	private static readonly Color                      ContentsBackgroundColor = new Color(100, 100, 100);
	private readonly        RectanglePrimitiveDrawable _titleBarBackground;
	private readonly        TextDrawable               _titleBarText;
	private readonly        RectanglePrimitiveDrawable _contentsBackground;
	private readonly        UiContainer                _uiContainer;

	public override Vector2 Size => new Vector2(NoteEditorToolSelectionDrawable.WIDTH, NoteEditorToolSelectionDrawable.TITLE_BAR_HEIGHT + CONTENTS_HEIGHT) * this.Scale;

	public NoteEditorDetailEditorDrawable(Vector2 position, NoteEditorPlayFieldContainer playFieldContainer) {
		this._playFieldContainer = playFieldContainer;
		this.Position            = position;

		this.InvisibleToInput = true;

		//Title bar
		this.Children.Add(this._titleBarBackground = new RectanglePrimitiveDrawable(Vector2.Zero, new Vector2(NoteEditorToolSelectionDrawable.WIDTH, NoteEditorToolSelectionDrawable.TITLE_BAR_HEIGHT), 0, true) {
			ColorOverride = TitleBarColor,
			Clickable     = false,
			CoverClicks   = false,
			Hoverable     = false,
			CoverHovers   = false
		});
		this.Children.Add(this._titleBarText = new TextDrawable(new Vector2(NoteEditorToolSelectionDrawable.WIDTH / 2f, NoteEditorToolSelectionDrawable.TITLE_BAR_HEIGHT / 2f), pTypingGame.JapaneseFont, "Details", NoteEditorToolSelectionDrawable.TITLE_BAR_HEIGHT - 4f) {
			OriginType  = OriginType.Center,
			Clickable   = false,
			CoverClicks = false,
			Hoverable   = false,
			CoverHovers = false
		});

		//Contents
		this.Children.Add(this._contentsBackground = new RectanglePrimitiveDrawable(new Vector2(0, NoteEditorToolSelectionDrawable.TITLE_BAR_HEIGHT), new Vector2(NoteEditorToolSelectionDrawable.WIDTH, CONTENTS_HEIGHT), 0, true) {
			ColorOverride = ContentsBackgroundColor,
			Clickable     = false,
			CoverClicks   = false,
			Hoverable     = false,
			CoverHovers   = false
		});

		this._uiContainer          = new UiContainer(OriginType.TopLeft);
		this._uiContainer.Position = new Vector2(NoteEditorToolSelectionDrawable.CONTENT_MARGIN, NoteEditorToolSelectionDrawable.TITLE_BAR_HEIGHT + NoteEditorToolSelectionDrawable.CONTENT_MARGIN);

		this.Children.Add(this._uiContainer);
	}
}
