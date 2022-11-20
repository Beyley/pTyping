using System.Collections.Specialized;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Vixie.Backends.Shared;
using pTyping.Graphics.Drawables;
using pTyping.Graphics.Player;
using pTyping.UiGenerator;

namespace pTyping.Graphics.Editor.Scene.NoteEditor;

public sealed class NoteEditorDetailEditorDrawable : CompositeDrawable {
	private readonly NoteEditorPlayFieldContainer _playFieldContainer;

	private const float CONTENTS_HEIGHT = 250 - NoteEditorToolSelectionDrawable.TITLE_BAR_HEIGHT;

	private static readonly Color                      TitleBarColor           = new Color(120, 120, 120);
	private static readonly Color                      ContentsBackgroundColor = new Color(100, 100, 100);
	private readonly        RectanglePrimitiveDrawable _titleBarBackground;
	private readonly        TextDrawable               _titleBarText;
	private readonly        RectanglePrimitiveDrawable _contentsBackground;
	private readonly        UiContainer                _uiContainer;

	private readonly UiElement _textInput;

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

		this._uiContainer.RegisterElement(UiElement.CreateText(pTypingGame.JapaneseFont, "Text:", 24));
		this._uiContainer.RegisterElement(this._textInput = UiElement.CreateTextBox(pTypingGame.JapaneseFont, "", 24, this.Size.X - NoteEditorToolSelectionDrawable.CONTENT_MARGIN * 2));

		this.Children.Add(this._uiContainer);

		this._playFieldContainer.Arguments.SelectedNotes.CollectionChanged += this.SelectedNotesChanged;

		this._textInput.AsTextBox().OnCommit += this.TextCommit;

		SelectedNotesChanged(null, null);
	}

	private void TextCommit(object sender, string e) {
		//Set the text of all selected notes
		foreach (SelectableCompositeDrawable selected in this._playFieldContainer.Arguments.SelectedNotes) {
			NoteDrawable note = (NoteDrawable)selected;

			note.Note.Text = e;
		}

		//When the user changes a note, mark that a save is needed
		this._playFieldContainer.Editor.SaveNeeded = true;
	}

	private void SelectedNotesChanged(object _, NotifyCollectionChangedEventArgs __) {
		//If there are 0 notes selected, then we shouldn't allow you to select the text box
		this._textInput.AsTextBox().Clickable = this._playFieldContainer.Arguments.SelectedNotes.Count != 0;

		//If the user's selection changes, release text focus, as they arent actively using it anymore,
		//and it will be confusing if they are using shortcuts and the text changes
		FurballGame.InputManager.ReleaseTextFocus();

		this._textInput.AsTextBox().Text =
			this._playFieldContainer.Arguments.SelectedNotes.Count == 1
				//If the user has selected only a single note, then we can show the text of that note
				? ((NoteDrawable)this._playFieldContainer.Arguments.SelectedNotes[0]).Note.Text
				//Else, just blank it out
				: "";
	}

	public override void Dispose() {
		base.Dispose();

		this._playFieldContainer.Arguments.SelectedNotes.CollectionChanged -= this.SelectedNotesChanged;

		this._textInput.AsTextBox().OnCommit -= this.TextCommit;
	}
}
