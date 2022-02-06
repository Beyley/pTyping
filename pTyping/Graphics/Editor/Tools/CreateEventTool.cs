using System.Collections.Generic;
using System.Numerics;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using pTyping.Songs;
using pTyping.Songs.Events;
using pTyping.UiGenerator;
using Silk.NET.Input;

namespace pTyping.Graphics.Editor.Tools;

public class CreateEventTool : EditorTool {
    public override string Name    => "Create Event";
    public override string Tooltip => "Create events on the timeline";

    private LinePrimitiveDrawable _createLine;

    private const string BEAT_LINE_BEAT = "Beat Line Event";
    private const string BEAT_LINE_BAR  = "Bar Line Event";
    private const string LYRIC          = "Lyric Event";
    private const string TYPING_CUTOFF  = "Typing Cutoff Event";

    // [ToolOption("Selected Event", "The selected event to create", BEAT_LINE_BEAT, BEAT_LINE_BAR, LYRIC, TYPING_CUTOFF)]
    // public Bindable<string> SelectedEvent = new("");
    public UiElement SelectedEvent;
    public UiElement SelectedEventLabel;
    public UiElement LyricInput;
    public UiElement LyricInputLabel;

    public override void Initialize() {
        this._createLine = new LinePrimitiveDrawable(new Vector2(0, 0), Vector2.Zero, Color.White) {
            Visible    = false,
            TimeSource = pTypingGame.MusicTrackTimeSource
        };

        this.DrawableManager.Add(this._createLine);

        this.SelectedEventLabel            = UiElement.CreateText(pTypingGame.JapaneseFont, "Event", LABELTEXTSIZE);
        this.SelectedEventLabel.SpaceAfter = LABELAFTERDISTANCE;
        this.SelectedEvent = UiElement.CreateDropdown(
        new List<string> {
            // BEAT_LINE_BEAT, // We dont allow the user to create this right now, it should be auto-generated
            // BEAT_LINE_BAR,  // ^
            LYRIC,
            TYPING_CUTOFF
        },
        DROPDOWNBUTTONSIZE,
        pTypingGame.JapaneseFont,
        ITEMTEXTSIZE
        );
        this.LyricInputLabel            = UiElement.CreateText(pTypingGame.JapaneseFont, "Lyric", LABELTEXTSIZE);
        this.LyricInputLabel.SpaceAfter = LABELAFTERDISTANCE;
        this.LyricInput                 = UiElement.CreateTextBox(pTypingGame.JapaneseFont, "lyric", ITEMTEXTSIZE, TEXTBOXWIDTH);

        this.EditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.SelectedEventLabel);
        this.EditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.SelectedEvent);

        this.SelectedEvent.AsDropdown().SelectedItem.OnChange += this.OnSelectedEventChange;

        this.OnSelectedEventChange(null, this.SelectedEvent.AsDropdown().SelectedItem);

        base.Initialize();
    }

    private void OnSelectedEventChange(object sender, string e) {
        switch (e) {
            case LYRIC: {
                this.EditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.LyricInputLabel);
                this.EditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.LyricInput);
                break;
            }
            default: {
                this.EditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.LyricInputLabel);
                this.EditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.LyricInput);
                break;
            }
        }
    }

    public override void OnMouseClick((MouseButton mouseButton, Vector2 position) args) {
        if (!EditorScreen.InPlayfield(args.position)) return;
        if (args.mouseButton != MouseButton.Left) return;

        Event @event = null;

        switch (this.SelectedEvent.AsDropdown().SelectedItem.Value) {
            case BEAT_LINE_BAR: {
                @event = new BeatLineBarEvent {
                    Time = this.EditorInstance.EditorState.MouseTime
                };

                break;
            }
            case BEAT_LINE_BEAT: {
                @event = new BeatLineBeatEvent {
                    Time = this.EditorInstance.EditorState.MouseTime
                };

                break;
            }
            case LYRIC: {
                @event = new LyricEvent {
                    Time  = this.EditorInstance.EditorState.MouseTime,
                    Lyric = this.LyricInput.AsTextBox().Text
                };

                break;
            }
            case TYPING_CUTOFF: {
                @event = new TypingCutoffEvent {
                    Time = this.EditorInstance.EditorState.MouseTime
                };

                break;
            }
        }

        if (@event != null)
            this.EditorInstance.CreateEvent(@event, true);

        base.OnMouseClick(args);
    }

    public override void OnMouseMove(Vector2 position) {
        //Only show the create line if we are inside of the playfield, as thats the only time we are able to place notes
        this._createLine.Visible = EditorScreen.InPlayfield(position);

        //Update the position of the preview line
        if (EditorScreen.InPlayfield(position)) {
            this._createLine.Tweens.Clear();
            this._createLine.Tweens.Add(
            new VectorTween(
            TweenType.Movement,
            new(EditorScreen.NOTE_START_POS.X, EditorScreen.NOTE_START_POS.Y - 40),
            new(EditorScreen.RECEPTICLE_POS.X, EditorScreen.RECEPTICLE_POS.Y - 40),
            (int)(this.EditorInstance.EditorState.MouseTime - this.EditorInstance.CurrentApproachTime(this.EditorInstance.EditorState.MouseTime)),
            (int)this.EditorInstance.EditorState.MouseTime
            )
            );
        }

        base.OnMouseMove(position);
    }

    public override void Deinitialize() {
        this.DrawableManager.Remove(this._createLine);

        this.EditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.SelectedEventLabel);
        this.EditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.SelectedEvent);
        this.EditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.LyricInputLabel);
        this.EditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.LyricInput);

        this.SelectedEvent.AsDropdown().SelectedItem.OnChange -= this.OnSelectedEventChange;

        base.Deinitialize();
    }
}