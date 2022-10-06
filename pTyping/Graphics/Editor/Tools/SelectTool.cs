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
using Silk.NET.Input;

namespace pTyping.Graphics.Editor.Tools;

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
		return new(this.EditorInstance.CurrentApproachTime(time), true, true);
	}

	public override void Initialize() {
		foreach (NoteDrawable note in this.EditorInstance.EditorState.Notes) {
			note.OnClick     += this.OnObjectClick;
			note.OnDragBegin += this.OnObjectDragBegin;
			note.OnDrag      += this.OnObjectDrag;
			note.OnDragEnd   += this.OnObjectDragEnd;
		}
		foreach (Drawable @event in this.EditorInstance.EditorState.Events) {
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

		this.EditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.ObjectTextLabel);
		this.EditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.ObjectText);
		this.EditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.ObjectColourLabel);
		this.EditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.ObjectColour);
		this.EditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.ObjectLanguageLabel);
		this.EditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.ObjectLanguage);

		this.ObjectText.AsTextBox().OnFocusChange              += this.OnObjectTextCommit;
		this.ObjectColour.AsColorPicker().Color.OnChange       += this.OnObjectColourChange;
		this.ObjectLanguage.AsDropdown().SelectedItem.OnChange += this.OnObjectLanguageChange;

		this.EditorInstance.EditorState.SelectedObjects.CollectionChanged += this.OnSelectedObjectsChange;

		base.Initialize();
	}
	private void OnObjectLanguageChange(object sender, KeyValuePair<object, string> e) {
		if (this.EditorInstance.EditorState.SelectedObjects.Count == 0) return;

		foreach (Drawable @object in this.EditorInstance.EditorState.SelectedObjects) {
			if (@object is not NoteDrawable note)
				continue;

			note.Note.TypingConversion     = (TypingConversions.ConversionType)e.Key;
			this.EditorInstance.SaveNeeded = true;
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
		if (this.EditorInstance.EditorState.SelectedObjects.Count == 0) return;

		foreach (Drawable selectedObject in this.EditorInstance.EditorState.SelectedObjects)
			if (selectedObject is NoteDrawable note) {
				note.Note.Color                = color;
				this.EditorInstance.SaveNeeded = true;
				note.Reset();
			}
	}

	private void OnObjectTextCommit(object sender, string e) {
		if (this.EditorInstance.EditorState.SelectedObjects.Count == 0) return;

		foreach (Drawable selectedObject in this.EditorInstance.EditorState.SelectedObjects)
			switch (selectedObject) {
				case NoteDrawable note:
					note.Note.Text                 = this.ObjectText.AsTextBox().Text;
					this.EditorInstance.SaveNeeded = true;

					note.Reset();
					break;
				case LyricEventDrawable lyric:
					lyric.Event.Text               = this.ObjectText.AsTextBox().Text;
					this.EditorInstance.SaveNeeded = true;
					break;
			}
	}

	private void OnSelectedObjectsChange(object sender, NotifyCollectionChangedEventArgs e) {
		if (this.EditorInstance.EditorState.SelectedObjects.Count == 0) {
			this.ObjectText.AsTextBox().Text              = string.Empty;
			this.ObjectColour.AsColorPicker().Color.Value = Color.White;

			return;
		}

		Drawable selectedObject = this.EditorInstance.EditorState.SelectedObjects[0];

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
		foreach (NoteDrawable note in this.EditorInstance.EditorState.Notes) {
			note.OnClick     -= this.OnObjectClick;
			note.OnDragBegin -= this.OnObjectDragBegin;
			note.OnDrag      -= this.OnObjectDrag;
			note.OnDragEnd   -= this.OnObjectDragEnd;
		}
		foreach (Drawable @event in this.EditorInstance.EditorState.Events) {
			@event.OnClick     -= this.OnObjectClick;
			@event.OnDragBegin -= this.OnObjectDragBegin;
			@event.OnDrag      -= this.OnObjectDrag;
			@event.OnDragEnd   -= this.OnObjectDragEnd;
		}

		this.EditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.ObjectTextLabel);
		this.EditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.ObjectText);
		this.EditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.ObjectColourLabel);
		this.EditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.ObjectColour);
		this.EditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.ObjectLanguageLabel);
		this.EditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.ObjectLanguage);

		this.ObjectText.AsTextBox().OnCommit                   -= this.OnObjectTextCommit;
		this.ObjectColour.AsColorPicker().Color.OnChange       -= this.OnObjectColourChange;
		this.ObjectLanguage.AsDropdown().SelectedItem.OnChange -= this.OnObjectLanguageChange;

		this.EditorInstance.EditorState.SelectedObjects.CollectionChanged -= this.OnSelectedObjectsChange;

		base.Deinitialize();
	}

	private void OnObjectDragBegin(object sender, MouseDragEventArgs mouseDragEventArgs) {
		if (!FurballGame.InputManager.HeldKeys.Contains(Key.ShiftLeft)) return;

		this._dragging     = true;
		this._lastDragTime = this.EditorInstance.EditorState.MouseTime;
	}

	private void OnObjectDragEnd(object sender, MouseDragEventArgs mouseDragEventArgs) {
		this._dragging = false;
	}

	private double _lastDragTime;
	private void OnObjectDrag(object sender, MouseDragEventArgs mouseDragEventArgs) {
		if (!FurballGame.InputManager.HeldKeys.Contains(Key.ShiftLeft)) {
			this._dragging = false;
			return;
		}

		if (!this._dragging) return;

		// We disable this because we are directly setting it, so if its not equal, its not the same value
		// ReSharper disable once CompareOfFloatsByEqualityOperator
		if (this.EditorInstance.EditorState.MouseTime != this._lastDragTime) {
			double timeDifference = this.EditorInstance.EditorState.MouseTime - this._lastDragTime;

			if (this.EditorInstance.EditorState.SelectedObjects.Count == 1)
				switch (this.EditorInstance.EditorState.SelectedObjects[0]) {
					case NoteDrawable noteDrawable:
						noteDrawable.Note.Time = this.EditorInstance.EditorState.MouseTime;

						noteDrawable.CreateTweens(this.GetTweenArgs(this.EditorInstance.EditorState.MouseTime));
						break;
					case BeatLineBarEventDrawable beatLineBarEventDrawable:
						beatLineBarEventDrawable.Event.Start = this.EditorInstance.EditorState.MouseTime;

						beatLineBarEventDrawable.CreateTweens(this.GetTweenArgs(this.EditorInstance.EditorState.MouseTime));
						break;
					case BeatLineBeatEventDrawable beatLineBeatEventDrawable:
						beatLineBeatEventDrawable.Event.Start = this.EditorInstance.EditorState.MouseTime;

						beatLineBeatEventDrawable.CreateTweens(this.GetTweenArgs(this.EditorInstance.EditorState.MouseTime));
						break;
					case TypingCutoffEventDrawable typingCutoffEventDrawable:
						typingCutoffEventDrawable.Event.Start = this.EditorInstance.EditorState.MouseTime;

						typingCutoffEventDrawable.CreateTweens(this.GetTweenArgs(this.EditorInstance.EditorState.MouseTime));
						break;
					case LyricEventDrawable lyricEventDrawable:
						lyricEventDrawable.Event.Start = this.EditorInstance.EditorState.MouseTime;

						lyricEventDrawable.CreateTweens(this.GetTweenArgs(this.EditorInstance.EditorState.MouseTime));
						break;
				}
			else
				foreach (Drawable selectedObject in this.EditorInstance.EditorState.SelectedObjects)
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

			this.EditorInstance.UpdateSelectionRects(this, null);

			this.EditorInstance.SaveNeeded = true;
		}

		this._lastDragTime = this.EditorInstance.EditorState.MouseTime;
	}

	private void OnObjectClick(object sender, MouseButtonEventArgs mouseButtonEventArgs) {
		if (FurballGame.InputManager.HeldKeys.Contains(Key.ShiftLeft)) return;

		bool ctrlHeld = FurballGame.InputManager.HeldKeys.Contains(Key.ControlLeft) || FurballGame.InputManager.HeldKeys.Contains(Key.ControlRight);

		if (sender is not Drawable drawable) return;

		if (ctrlHeld) {
			if (!this.EditorInstance.EditorState.SelectedObjects.Remove(drawable))
				this.EditorInstance.EditorState.SelectedObjects.Add(drawable);
		}
		else {
			this.EditorInstance.EditorState.SelectedObjects.Clear();
			this.EditorInstance.EditorState.SelectedObjects.Add(drawable);
		}
	}
}
