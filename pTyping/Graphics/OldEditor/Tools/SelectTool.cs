using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Furball.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Input.Events;
using Furball.Vixie.Backends.Shared;
using pTyping.Graphics.Drawables.Events;
using pTyping.Graphics.Player;
using pTyping.Shared;
using pTyping.UiGenerator;

namespace pTyping.Graphics.OldEditor.Tools;

// ReSharper disable once ClassNeverInstantiated.Global
public class SelectTool : EditorTool {
	public  UiElement ObjectText;
	public  UiElement ObjectTextLabel;
	public  UiElement ObjectColour;
	public  UiElement ObjectColourLabel;
	private UiElement ObjectLanguageLabel;
	private UiElement ObjectLanguage;

	private Dictionary<object, string> _langaugeDictionary;

	public override string Name    => "Select";
	public override string Tooltip => "Select, move, and change notes in the timeline.";

	private bool _dragging;

	private GameplayDrawableTweenArgs GetTweenArgs(double time) {
		return new(this.OldEditorInstance.CurrentApproachTime(time), true, true);
	}

	public override void Initialize() {
		foreach (NoteDrawable note in this.OldEditorInstance.EditorState.Notes) {
			note.OnClick     += this.OnObjectClick;
			note.OnDragBegin += this.OnObjectDragBegin;
			note.OnDrag      += this.OnObjectDrag;
			note.OnDragEnd   += this.OnObjectDragEnd;
		}
		foreach (Drawable @event in this.OldEditorInstance.EditorState.Events) {
			@event.OnClick     += this.OnObjectClick;
			@event.OnDragBegin += this.OnObjectDragBegin;
			@event.OnDrag      += this.OnObjectDrag;
			@event.OnDragEnd   += this.OnObjectDragEnd;
		}

		this.ObjectTextLabel            = UiElement.CreateText(pTypingGame.JapaneseFont, "Text", LABELTEXTSIZE);
		this.ObjectTextLabel.SpaceAfter = LABELAFTERDISTANCE;
		this.ObjectText                 = UiElement.CreateTextBox(pTypingGame.JapaneseFont, "", ITEMTEXTSIZE, TEXTBOXWIDTH);

		this.ObjectColourLabel            = UiElement.CreateText(pTypingGame.JapaneseFont, "Color", LABELTEXTSIZE);
		this.ObjectColourLabel.SpaceAfter = LABELAFTERDISTANCE;
		this.ObjectColour                 = UiElement.CreateColorPicker(pTypingGame.JapaneseFont, ITEMTEXTSIZE, Color.White);

		this.ObjectLanguageLabel            = UiElement.CreateText(pTypingGame.JapaneseFont, "Language", LABELTEXTSIZE);
		this.ObjectLanguageLabel.SpaceAfter = LABELAFTERDISTANCE;
		this.ObjectLanguage = UiElement.CreateDropdown(this._langaugeDictionary = new Dictionary<object, string> {
			{ TypingConversions.ConversionType.StandardLatin, "Standard Latin Only" },
			{ TypingConversions.ConversionType.StandardHiragana, "Japanese/Hiragana" },
			{ TypingConversions.ConversionType.StandardRussian, "Russian" },
			{ TypingConversions.ConversionType.StandardEsperanto, "Esperanto" }
		}, DROPDOWNBUTTONSIZE, pTypingGame.JapaneseFont, ITEMTEXTSIZE);

		this.OldEditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.ObjectTextLabel);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.ObjectText);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.ObjectColourLabel);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.ObjectColour);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.ObjectLanguageLabel);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.ObjectLanguage);

		this.ObjectText.AsTextBox().OnFocusChange              += this.OnObjectTextCommit;
		this.ObjectColour.AsColorPicker().Color.OnChange       += this.OnObjectColourChange;
		this.ObjectLanguage.AsDropdown().SelectedItem.OnChange += this.OnObjectLanguageChange;

		this.OldEditorInstance.EditorState.SelectedObjects.CollectionChanged += this.OnSelectedObjectsChange;

		base.Initialize();
	}
	private void OnObjectLanguageChange(object sender, KeyValuePair<object, string> e) {
		if (this.OldEditorInstance.EditorState.SelectedObjects.Count == 0) return;

		foreach (Drawable @object in this.OldEditorInstance.EditorState.SelectedObjects) {
			if (@object is not NoteDrawable note)
				continue;

			note.Note.TypingConversion        = (TypingConversions.ConversionType)e.Key;
			this.OldEditorInstance.SaveNeeded = true;
		}
	}

	private void OnObjectTextCommit(object sender, bool selected) {
		if (!selected)
			this.OnObjectTextCommit(sender, this.ObjectText.AsTextBox().Text);
	}

	public override void OnNoteCreate(NoteDrawable note, bool isNew) {
		if (isNew) {
			note.OnClick     += this.OnObjectClick;
			note.OnDragBegin += this.OnObjectDragBegin;
			note.OnDrag      += this.OnObjectDrag;
			note.OnDragEnd   += this.OnObjectDragEnd;
		}

		base.OnNoteCreate(note, isNew);
	}

	public override void OnNoteDelete(NoteDrawable note) {
		note.OnClick     -= this.OnObjectClick;
		note.OnDragBegin -= this.OnObjectDragBegin;
		note.OnDrag      -= this.OnObjectDrag;
		note.OnDragEnd   -= this.OnObjectDragEnd;

		base.OnNoteDelete(note);
	}

	private void ShowUiElements(bool text, bool colour, bool language) {
		this.ObjectTextLabel.Visible.Value     = text;
		this.ObjectText.Visible.Value          = text;
		this.ObjectColourLabel.Visible.Value   = colour;
		this.ObjectColour.Visible.Value        = colour;
		this.ObjectLanguageLabel.Visible.Value = language;
		this.ObjectLanguageLabel.Visible.Value = language;
	}

	private void OnObjectColourChange(object sender, Color color) {
		if (this.OldEditorInstance.EditorState.SelectedObjects.Count == 0) return;

		foreach (Drawable selectedObject in this.OldEditorInstance.EditorState.SelectedObjects)
			if (selectedObject is NoteDrawable note) {
				note.Note.Color                   = color;
				this.OldEditorInstance.SaveNeeded = true;
				note.Reset();
			}
	}

	private void OnObjectTextCommit(object sender, string e) {
		if (this.OldEditorInstance.EditorState.SelectedObjects.Count == 0) return;

		foreach (Drawable selectedObject in this.OldEditorInstance.EditorState.SelectedObjects)
			switch (selectedObject) {
				case NoteDrawable note:
					note.Note.Text                    = this.ObjectText.AsTextBox().Text;
					this.OldEditorInstance.SaveNeeded = true;

					note.Reset();
					break;
				case LyricEventDrawable lyric:
					lyric.Event.Text                  = this.ObjectText.AsTextBox().Text;
					this.OldEditorInstance.SaveNeeded = true;
					break;
			}
	}

	private void OnSelectedObjectsChange(object sender, NotifyCollectionChangedEventArgs e) {
		if (this.OldEditorInstance.EditorState.SelectedObjects.Count == 0) {
			this.ObjectText.AsTextBox().Text              = string.Empty;
			this.ObjectColour.AsColorPicker().Color.Value = Color.White;

			return;
		}

		Drawable selectedObject = this.OldEditorInstance.EditorState.SelectedObjects[0];

		switch (selectedObject) {
			case NoteDrawable note:
				this.ObjectText.AsTextBox().Text                    = note.Note.Text;
				this.ObjectColour.AsColorPicker().Color.Value       = note.Note.Color;
				this.ObjectLanguage.AsDropdown().SelectedItem.Value = this._langaugeDictionary.First(x => (TypingConversions.ConversionType)x.Key == note.Note.TypingConversion);
				this.ShowUiElements(true, true, true);
				break;
			case LyricEventDrawable lyric:
				this.ObjectText.AsTextBox().Text = lyric.Event.Text;

				this.ShowUiElements(true, false, false);
				break;
			default:
				this.ShowUiElements(false, false, false);
				break;
		}
	}

	public override void Deinitialize() {
		foreach (NoteDrawable note in this.OldEditorInstance.EditorState.Notes) {
			note.OnClick     -= this.OnObjectClick;
			note.OnDragBegin -= this.OnObjectDragBegin;
			note.OnDrag      -= this.OnObjectDrag;
			note.OnDragEnd   -= this.OnObjectDragEnd;
		}
		foreach (Drawable @event in this.OldEditorInstance.EditorState.Events) {
			@event.OnClick     -= this.OnObjectClick;
			@event.OnDragBegin -= this.OnObjectDragBegin;
			@event.OnDrag      -= this.OnObjectDrag;
			@event.OnDragEnd   -= this.OnObjectDragEnd;
		}

		this.OldEditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.ObjectTextLabel);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.ObjectText);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.ObjectColourLabel);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.ObjectColour);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.ObjectLanguageLabel);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.ObjectLanguage);

		this.ObjectText.AsTextBox().OnCommit                   -= this.OnObjectTextCommit;
		this.ObjectColour.AsColorPicker().Color.OnChange       -= this.OnObjectColourChange;
		this.ObjectLanguage.AsDropdown().SelectedItem.OnChange -= this.OnObjectLanguageChange;

		this.OldEditorInstance.EditorState.SelectedObjects.CollectionChanged -= this.OnSelectedObjectsChange;

		base.Deinitialize();
	}

	private void OnObjectDragBegin(object sender, MouseDragEventArgs mouseDragEventArgs) {
		if (!FurballGame.InputManager.ShiftHeld) return;

		this._dragging     = true;
		this._lastDragTime = this.OldEditorInstance.EditorState.MouseTime;
	}

	private void OnObjectDragEnd(object sender, MouseDragEventArgs mouseDragEventArgs) {
		this._dragging = false;
	}

	private double _lastDragTime;
	private void OnObjectDrag(object sender, MouseDragEventArgs mouseDragEventArgs) {
		if (!FurballGame.InputManager.ShiftHeld) {
			this._dragging = false;
			return;
		}

		if (!this._dragging) return;

		// We disable this because we are directly setting it, so if its not equal, its not the same value
		// ReSharper disable once CompareOfFloatsByEqualityOperator
		if (this.OldEditorInstance.EditorState.MouseTime != this._lastDragTime) {
			double timeDifference = this.OldEditorInstance.EditorState.MouseTime - this._lastDragTime;

			if (this.OldEditorInstance.EditorState.SelectedObjects.Count == 1)
				switch (this.OldEditorInstance.EditorState.SelectedObjects[0]) {
					case NoteDrawable noteDrawable:
						noteDrawable.Note.Time = this.OldEditorInstance.EditorState.MouseTime;

						noteDrawable.CreateTweens(this.GetTweenArgs(this.OldEditorInstance.EditorState.MouseTime));
						break;
					case BeatLineBarEventDrawable beatLineBarEventDrawable:
						beatLineBarEventDrawable.Event.Start = this.OldEditorInstance.EditorState.MouseTime;

						beatLineBarEventDrawable.CreateTweens(this.GetTweenArgs(this.OldEditorInstance.EditorState.MouseTime));
						break;
					case BeatLineBeatEventDrawable beatLineBeatEventDrawable:
						beatLineBeatEventDrawable.Event.Start = this.OldEditorInstance.EditorState.MouseTime;

						beatLineBeatEventDrawable.CreateTweens(this.GetTweenArgs(this.OldEditorInstance.EditorState.MouseTime));
						break;
					case TypingCutoffEventDrawable typingCutoffEventDrawable:
						typingCutoffEventDrawable.Event.Start = this.OldEditorInstance.EditorState.MouseTime;

						typingCutoffEventDrawable.CreateTweens(this.GetTweenArgs(this.OldEditorInstance.EditorState.MouseTime));
						break;
					case LyricEventDrawable lyricEventDrawable:
						lyricEventDrawable.Event.Start = this.OldEditorInstance.EditorState.MouseTime;

						lyricEventDrawable.CreateTweens(this.GetTweenArgs(this.OldEditorInstance.EditorState.MouseTime));
						break;
				}
			else
				foreach (Drawable selectedObject in this.OldEditorInstance.EditorState.SelectedObjects)
					switch (selectedObject) {
						case NoteDrawable noteDrawable:
							noteDrawable.Note.Time += timeDifference;

							noteDrawable.CreateTweens(this.GetTweenArgs(noteDrawable.Note.Time));
							break;
						case BeatLineBarEventDrawable beatLineBarEventDrawable:
							beatLineBarEventDrawable.Event.Start += timeDifference;

							beatLineBarEventDrawable.CreateTweens(this.GetTweenArgs(beatLineBarEventDrawable.Event.Start));
							break;
						case BeatLineBeatEventDrawable beatLineBeatEventDrawable:
							beatLineBeatEventDrawable.Event.Start += timeDifference;

							beatLineBeatEventDrawable.CreateTweens(this.GetTweenArgs(beatLineBeatEventDrawable.Event.Start));
							break;
						case TypingCutoffEventDrawable typingCutoffEventDrawable:
							typingCutoffEventDrawable.Event.Start += timeDifference;

							typingCutoffEventDrawable.CreateTweens(this.GetTweenArgs(typingCutoffEventDrawable.Event.Start));
							break;
						case LyricEventDrawable lyricEventDrawable:
							lyricEventDrawable.Event.Start += timeDifference;

							lyricEventDrawable.CreateTweens(this.GetTweenArgs(lyricEventDrawable.Event.Start));
							break;
					}

			this.OldEditorInstance.UpdateSelectionRects(this, null);

			this.OldEditorInstance.SaveNeeded = true;
		}

		this._lastDragTime = this.OldEditorInstance.EditorState.MouseTime;
	}

	private void OnObjectClick(object sender, MouseButtonEventArgs mouseButtonEventArgs) {
		if (FurballGame.InputManager.ShiftHeld) return;

		bool ctrlHeld = FurballGame.InputManager.ControlHeld;

		if (sender is not Drawable drawable) return;

		if (ctrlHeld) {
			if (!this.OldEditorInstance.EditorState.SelectedObjects.Remove(drawable))
				this.OldEditorInstance.EditorState.SelectedObjects.Add(drawable);
		}
		else {
			this.OldEditorInstance.EditorState.SelectedObjects.Clear();
			this.OldEditorInstance.EditorState.SelectedObjects.Add(drawable);
		}
	}
}
