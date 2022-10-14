using System;
using System.Numerics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Input.Events;
using Furball.Vixie.Backends.Shared;
using pTyping.Graphics.Editor.Scene.NoteEditor.Tools;
using pTyping.UiGenerator;

namespace pTyping.Graphics.Editor.Scene.NoteEditor;

public class NoteEditorToolSelectionDrawable : CompositeDrawable {
	public NoteEditorTool CurrentTool { get; private set; }

	private readonly Action _toolChanged;

	private readonly RectanglePrimitiveDrawable _titleBarBackground;
	private readonly TextDrawable               _titleBarText;

	private readonly RectanglePrimitiveDrawable _contentsBackground;
	private readonly UiContainer                _uiContainer;

	private readonly UiElement _noteTool;
	private readonly UiElement _typingCutoffTool;
	private readonly UiElement _selectTool;

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

		this.InvisibleToInput = true;

		//Title bar
		this.Children.Add(this._titleBarBackground = new RectanglePrimitiveDrawable(Vector2.Zero, new Vector2(WIDTH, TITLE_BAR_HEIGHT), 0, true) {
			ColorOverride = TitleBarColor,
			Clickable     = false,
			CoverClicks   = false,
			Hoverable     = false,
			CoverHovers   = false
		});
		this.Children.Add(this._titleBarText = new TextDrawable(new Vector2(WIDTH / 2f, TITLE_BAR_HEIGHT / 2f), pTypingGame.JapaneseFont, "Tool Selection", TITLE_BAR_HEIGHT - 4f) {
			OriginType  = OriginType.Center,
			Clickable   = false,
			CoverClicks = false,
			Hoverable   = false,
			CoverHovers = false
		});

		//Contents
		this.Children.Add(this._contentsBackground = new RectanglePrimitiveDrawable(new Vector2(0, TITLE_BAR_HEIGHT), new Vector2(WIDTH, CONTENTS_HEIGHT), 0, true) {
			ColorOverride = ContentsBackgroundColor,
			Clickable     = false,
			CoverClicks   = false,
			Hoverable     = false,
			CoverHovers   = false
		});

		this._uiContainer          = new UiContainer(OriginType.TopLeft);
		this._uiContainer.Position = new Vector2(CONTENT_MARGIN, TITLE_BAR_HEIGHT + CONTENT_MARGIN);

		this._selectTool       = UiElement.CreateTickBox("Select Tool", 24, false, true);
		this._noteTool         = UiElement.CreateTickBox("Note Tool", 24, false, true);
		this._typingCutoffTool = UiElement.CreateTickBox("Typing Cutoff Tool", 24, false, true);

		this._selectTool.SpaceAfter       = CONTENT_MARGIN / 2f;
		this._noteTool.SpaceAfter         = CONTENT_MARGIN / 2f;
		this._typingCutoffTool.SpaceAfter = CONTENT_MARGIN / 2f;

		this._selectTool.Drawable.OnClick       += this.SelectToolSelected;
		this._noteTool.Drawable.OnClick         += this.NoteToolSelected;
		this._typingCutoffTool.Drawable.OnClick += this.TypingCutoffToolSelected;

		this._uiContainer.RegisterElement(this._selectTool);
		this._uiContainer.RegisterElement(this._noteTool);
		this._uiContainer.RegisterElement(this._typingCutoffTool);

		this.Children.Add(this._uiContainer);

		this.CurrentTool = new SelectTool();
		this.UpdateTickBoxes();
	}

	private void SelectToolSelected(object sender, MouseButtonEventArgs e) {
		this.CurrentTool = new SelectTool();

		this.UpdateTickBoxes();

		this._toolChanged();
	}

	private void TypingCutoffToolSelected(object sender, MouseButtonEventArgs mouseButtonEventArgs) {
		this.CurrentTool = new TypingCutoffTool();

		this.UpdateTickBoxes();

		this._toolChanged();
	}

	private void NoteToolSelected(object sender, MouseButtonEventArgs mouseButtonEventArgs) {
		this.CurrentTool = new NoteTool();

		this.UpdateTickBoxes();

		this._toolChanged();
	}

	private void UpdateTickBoxes() {
		this._selectTool.AsTickBox().Selected.Value       = this.CurrentTool is SelectTool;
		this._noteTool.AsTickBox().Selected.Value         = this.CurrentTool is NoteTool;
		this._typingCutoffTool.AsTickBox().Selected.Value = this.CurrentTool is TypingCutoffTool;
	}
}
