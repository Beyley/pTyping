using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Vixie.Backends.Shared;
using pTyping.Graphics.Drawables;
using pTyping.Graphics.Player;
using pTyping.Shared;
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
	private readonly UiElement _colorPicker;
	private readonly UiElement _conversion;

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

		UiElement textLabel = UiElement.CreateText(pTypingGame.JapaneseFont, "Text:", 24);
		this._textInput = UiElement.CreateTextBox(pTypingGame.JapaneseFont, "", 24, this.Size.X - NoteEditorToolSelectionDrawable.CONTENT_MARGIN * 2);
		UiElement colorLabel = UiElement.CreateText(pTypingGame.JapaneseFont, "Color:", 24);
		this._colorPicker = UiElement.CreateColorPicker(pTypingGame.JapaneseFont, 24, Color.White);
		UiElement conversionLabel = UiElement.CreateText(pTypingGame.JapaneseFont, "Conversion:", 24);
		this._conversion = UiElement.CreateDropdown(new Dictionary<object, string> {
			{ TypingConversions.ConversionType.StandardLatin, "Latin" },
			{ TypingConversions.ConversionType.StandardHiragana, "Hiragana" },
			{ TypingConversions.ConversionType.StandardEsperanto, "Esperanto" },
			{ TypingConversions.ConversionType.StandardRussian, "Russian" },
		}, new Vector2(this.Size.X - NoteEditorToolSelectionDrawable.CONTENT_MARGIN * 2, 28), pTypingGame.JapaneseFont, 24);

		textLabel.SpaceAfter       = NoteEditorToolSelectionDrawable.CONTENT_MARGIN / 2f;
		colorLabel.SpaceAfter      = NoteEditorToolSelectionDrawable.CONTENT_MARGIN / 2f;
		conversionLabel.SpaceAfter = NoteEditorToolSelectionDrawable.CONTENT_MARGIN / 2f;

		this._conversion.AsDropdown().OutlineThickness = 2f;

		this._uiContainer.RegisterElement(textLabel);
		this._uiContainer.RegisterElement(this._textInput);

		this._uiContainer.RegisterElement(colorLabel);
		this._uiContainer.RegisterElement(this._colorPicker);

		this._uiContainer.RegisterElement(conversionLabel);
		this._uiContainer.RegisterElement(this._conversion);

		this.Children.Add(this._uiContainer);

		this._playFieldContainer.Arguments.SelectedNotes.Object.CollectionChanged += this.SelectedNotesChanged;

		this._textInput.AsTextBox().OnCommit                += this.TextCommit;
		this._colorPicker.AsColorPicker().Color.OnChange    += this.ColorPicked;
		this._conversion.AsDropdown().SelectedItem.OnChange += this.ConversionChanged;

		this.SelectedNotesChanged(null, null);
	}

	private void ConversionChanged(object sender, KeyValuePair<object, string> e) {
		if (this._suppressEventsFromSelection)
			return;

		this._playFieldContainer.Arguments.SelectedNotes.Lock.EnterReadLock();
		//Set the text of all selected notes
		foreach (SelectableCompositeDrawable selected in this._playFieldContainer.Arguments.SelectedNotes.Object) {
			NoteDrawable note = (NoteDrawable)selected;

			note.Note.TypingConversion = (TypingConversions.ConversionType)e.Key;

			note.UpdateDrawables();
		}
		this._playFieldContainer.Arguments.SelectedNotes.Lock.ExitReadLock();

		//When the user changes a note, mark that a save is needed
		this._playFieldContainer.Editor.SaveNeeded = true;
	}

	private void ColorPicked(object sender, Color e) {
		if (this._suppressEventsFromSelection)
			return;

		this._playFieldContainer.Arguments.SelectedNotes.Lock.EnterReadLock();
		//Set the text of all selected notes
		foreach (SelectableCompositeDrawable selected in this._playFieldContainer.Arguments.SelectedNotes.Object) {
			NoteDrawable note = (NoteDrawable)selected;

			note.Note.Color = e;

			note.UpdateDrawables();
		}
		this._playFieldContainer.Arguments.SelectedNotes.Lock.ExitReadLock();

		//When the user changes a note, mark that a save is needed
		this._playFieldContainer.Editor.SaveNeeded = true;
	}

	private void TextCommit(object sender, string e) {
		this._playFieldContainer.Arguments.SelectedNotes.Lock.EnterReadLock();
		//Set the text of all selected notes
		foreach (SelectableCompositeDrawable selected in this._playFieldContainer.Arguments.SelectedNotes.Object) {
			NoteDrawable note = (NoteDrawable)selected;

			note.Note.Text = e;

			note.UpdateDrawables();
		}
		this._playFieldContainer.Arguments.SelectedNotes.Lock.ExitReadLock();

		//When the user changes a note, mark that a save is needed
		this._playFieldContainer.Editor.SaveNeeded = true;
	}

	private bool _suppressEventsFromSelection = false;
	private void SelectedNotesChanged(object _, NotifyCollectionChangedEventArgs __) {
		//If there are 0 notes selected, then we shouldn't allow you to select the text box
		this._textInput.AsTextBox().Clickable       = this._playFieldContainer.Arguments.SelectedNotes.Object.Count != 0;
		this._colorPicker.AsColorPicker().Clickable = this._playFieldContainer.Arguments.SelectedNotes.Object.Count != 0;
		this._conversion.AsDropdown().Clickable     = this._playFieldContainer.Arguments.SelectedNotes.Object.Count != 0;

		//If the user's selection changes, release text focus, as they arent actively using it anymore,
		//and it will be confusing if they are using shortcuts and the text changes
		FurballGame.InputManager.ReleaseTextFocus();

		this._textInput.AsTextBox().Text =
			this._playFieldContainer.Arguments.SelectedNotes.Object.Count == 1
				//If the user has selected only a single note, then we can show the text of that note
				? ((NoteDrawable)this._playFieldContainer.Arguments.SelectedNotes.Object[0]).Note.Text
				//Else, just blank it out
				: "";

		this._suppressEventsFromSelection = true;
		this._colorPicker.AsColorPicker().Color.Value =
			this._playFieldContainer.Arguments.SelectedNotes.Object.Count == 1
				//If the user has selected only a single note, then we can show the color of that note
				? ((NoteDrawable)this._playFieldContainer.Arguments.SelectedNotes.Object[0]).Note.Color
				//Else, just blank it out
				: Color.White;

		this._conversion.AsDropdown().SelectedItem.Value =
			this._playFieldContainer.Arguments.SelectedNotes.Object.Count == 1
				//If the user has selected only a single note, then we can show the conversion of that note
				? this._conversion.AsDropdown().Items.First(x => (TypingConversions.ConversionType)x.Key == ((NoteDrawable)this._playFieldContainer.Arguments.SelectedNotes.Object[0]).Note.TypingConversion)
				//Else, just blank it out
				: this._conversion.AsDropdown().Items.First(x => (TypingConversions.ConversionType)x.Key == TypingConversions.ConversionType.StandardLatin);
		this._suppressEventsFromSelection = false;
	}

	public override void Dispose() {
		base.Dispose();

		this._playFieldContainer.Arguments.SelectedNotes.Object.CollectionChanged -= this.SelectedNotesChanged;

		this._textInput.AsTextBox().OnCommit -= this.TextCommit;
	}
}
