using System.Collections.Specialized;
using System.Drawing;
using Furball.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using pTyping.Graphics.Drawables.Events;
using pTyping.Graphics.Player;
using pTyping.UiGenerator;
using Silk.NET.Input;
using Color=Furball.Vixie.Backends.Shared.Color;

namespace pTyping.Graphics.Editor.Tools;

// ReSharper disable once ClassNeverInstantiated.Global
public class SelectTool : EditorTool {
    public UiElement ObjectText;
    public UiElement ObjectTextLabel;
    public UiElement ObjectColour;
    public UiElement ObjectColourLabel;

    public override string Name    => "Select";
    public override string Tooltip => "Select, move, and change notes in the timeline.";

    private bool _dragging = false;

    private GameplayDrawableTweenArgs GetTweenArgs(double time) => new(this.EditorInstance.CurrentApproachTime(time), true, true);

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

        this.EditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.ObjectTextLabel);
        this.EditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.ObjectText);
        this.EditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.ObjectColourLabel);
        this.EditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.ObjectColour);

        this.ObjectText.AsTextBox().OnFocusChange        += this.OnObjectTextCommit;
        this.ObjectColour.AsColorPicker().Color.OnChange += this.OnObjectColourChange;

        this.EditorInstance.EditorState.SelectedObjects.CollectionChanged += this.OnSelectedObjectsChange;

        base.Initialize();
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

    private void ShowUiElements(bool text, bool colour) {
        this.ObjectTextLabel.Visible.Value   = text;
        this.ObjectText.Visible.Value        = text;
        this.ObjectColourLabel.Visible.Value = colour;
        this.ObjectColour.Visible.Value      = colour;
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
                    lyric.Event.Lyric              = this.ObjectText.AsTextBox().Text;
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
                this.ObjectText.AsTextBox().Text              = note.Note.Text;
                this.ObjectColour.AsColorPicker().Color.Value = note.Note.Color;

                this.ShowUiElements(true, true);
                break;
            case LyricEventDrawable lyric:
                this.ObjectText.AsTextBox().Text = lyric.Event.Lyric;

                this.ShowUiElements(true, false);
                break;
            default:
                this.ShowUiElements(false, false);
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

        this.ObjectText.AsTextBox().OnCommit             -= this.OnObjectTextCommit;
        this.ObjectColour.AsColorPicker().Color.OnChange -= this.OnObjectColourChange;

        this.EditorInstance.EditorState.SelectedObjects.CollectionChanged -= this.OnSelectedObjectsChange;

        base.Deinitialize();
    }

    private void OnObjectDragBegin(object sender, Point e) {
        if (!FurballGame.InputManager.HeldKeys.Contains(Key.ShiftLeft)) return;

        this._dragging     = true;
        this._lastDragTime = this.EditorInstance.EditorState.MouseTime;
    }

    private void OnObjectDragEnd(object sender, Point e) {
        this._dragging = false;
    }

    private double _lastDragTime;
    private void OnObjectDrag(object sender, Point e) {
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
                        beatLineBarEventDrawable.Event.Time = this.EditorInstance.EditorState.MouseTime;

                        beatLineBarEventDrawable.CreateTweens(this.GetTweenArgs(this.EditorInstance.EditorState.MouseTime));
                        break;
                    case BeatLineBeatEventDrawable beatLineBeatEventDrawable:
                        beatLineBeatEventDrawable.Event.Time = this.EditorInstance.EditorState.MouseTime;

                        beatLineBeatEventDrawable.CreateTweens(this.GetTweenArgs(this.EditorInstance.EditorState.MouseTime));
                        break;
                    case TypingCutoffEventDrawable typingCutoffEventDrawable:
                        typingCutoffEventDrawable.Event.Time = this.EditorInstance.EditorState.MouseTime;

                        typingCutoffEventDrawable.CreateTweens(this.GetTweenArgs(this.EditorInstance.EditorState.MouseTime));
                        break;
                    case LyricEventDrawable lyricEventDrawable:
                        lyricEventDrawable.Event.Time = this.EditorInstance.EditorState.MouseTime;

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
                            beatLineBarEventDrawable.Event.Time += timeDifference;

                            beatLineBarEventDrawable.CreateTweens(this.GetTweenArgs(beatLineBarEventDrawable.Event.Time));
                            break;
                        case BeatLineBeatEventDrawable beatLineBeatEventDrawable:
                            beatLineBeatEventDrawable.Event.Time += timeDifference;

                            beatLineBeatEventDrawable.CreateTweens(this.GetTweenArgs(beatLineBeatEventDrawable.Event.Time));
                            break;
                        case TypingCutoffEventDrawable typingCutoffEventDrawable:
                            typingCutoffEventDrawable.Event.Time += timeDifference;

                            typingCutoffEventDrawable.CreateTweens(this.GetTweenArgs(typingCutoffEventDrawable.Event.Time));
                            break;
                        case LyricEventDrawable lyricEventDrawable:
                            lyricEventDrawable.Event.Time += timeDifference;

                            lyricEventDrawable.CreateTweens(this.GetTweenArgs(lyricEventDrawable.Event.Time));
                            break;
                    }

            this.EditorInstance.UpdateSelectionRects(this, null);

            this.EditorInstance.SaveNeeded = true;
        }

        this._lastDragTime = this.EditorInstance.EditorState.MouseTime;
    }

    private void OnObjectClick(object sender, (MouseButton button, Point pos) tuple) {
        if (FurballGame.InputManager.HeldKeys.Contains(Key.ShiftLeft)) return;

        bool ctrlHeld = FurballGame.InputManager.HeldKeys.Contains(Key.ControlLeft) || FurballGame.InputManager.HeldKeys.Contains(Key.ControlRight);

        if (sender is not Drawable drawable) return;

        if (ctrlHeld) {
            if (!this.EditorInstance.EditorState.SelectedObjects.Remove(drawable))
                this.EditorInstance.EditorState.SelectedObjects.Add(drawable);
        } else {
            this.EditorInstance.EditorState.SelectedObjects.Clear();
            this.EditorInstance.EditorState.SelectedObjects.Add(drawable);
        }
    }
}