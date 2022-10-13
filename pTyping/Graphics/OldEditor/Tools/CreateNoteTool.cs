using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Vixie.Backends.Shared;
using pTyping.Shared.Beatmaps.HitObjects;
using pTyping.UiGenerator;
using Silk.NET.Input;

namespace pTyping.Graphics.OldEditor.Tools;

// ReSharper disable once ClassNeverInstantiated.Global
public class CreateNoteTool : EditorTool {
	public override string Name    => "Create Note";
	public override string Tooltip => "Create notes on the timeline.";

	private UiElement _defaultNoteText;
	private UiElement _defaultNoteTextLabel;
	private UiElement _defaultNoteColor;
	private UiElement _defaultNoteColorLabel;

	private TexturedDrawable _createLine;

	public override void Initialize() {
		this._createLine = new TexturedDrawable(FurballGame.WhitePixel, new Vector2(0, 0)) {
			Visible    = false,
			TimeSource = pTypingGame.MusicTrackTimeSource,
			Scale      = new Vector2(1, 80)
		};

		this.DrawableManager.Children.Add(this._createLine);

		this._defaultNoteTextLabel            = UiElement.CreateText(pTypingGame.JapaneseFont, "Text", LABELTEXTSIZE);
		this._defaultNoteTextLabel.SpaceAfter = LABELAFTERDISTANCE;
		this._defaultNoteText                 = UiElement.CreateTextBox(pTypingGame.JapaneseFont, "", ITEMTEXTSIZE, TEXTBOXWIDTH);

		this._defaultNoteColorLabel            = UiElement.CreateText(pTypingGame.JapaneseFont, "Color", LABELTEXTSIZE);
		this._defaultNoteColorLabel.SpaceAfter = LABELAFTERDISTANCE;
		this._defaultNoteColor                 = UiElement.CreateColorPicker(pTypingGame.JapaneseFont, ITEMTEXTSIZE, Color.Red);

		this.OldEditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this._defaultNoteTextLabel);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this._defaultNoteText);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this._defaultNoteColorLabel);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this._defaultNoteColor);

		base.Initialize();
	}

	public override void OnMouseMove(Vector2 position) {
		//Only show the create line if we are inside of the playfield, as thats the only time we are able to place notes
		this._createLine.Visible = this.OldEditorInstance.InPlayfield(position);

		//Update the position of the preview line
		if (this.OldEditorInstance.InPlayfield(position)) {
			this._createLine.Tweens.Clear();
			this._createLine.Tweens.Add(
				new VectorTween(
					TweenType.Movement,
					new Vector2(Player.Player.NOTE_START_POS.X, Player.Player.NOTE_START_POS.Y - 40),
					new Vector2(Player.Player.RECEPTICLE_POS.X, Player.Player.RECEPTICLE_POS.Y - 40),
					(int)(this.OldEditorInstance.EditorState.MouseTime - this.OldEditorInstance.CurrentApproachTime(this.OldEditorInstance.EditorState.MouseTime)),
					(int)this.OldEditorInstance.EditorState.MouseTime
				)
			);
		}

		base.OnMouseMove(position);
	}

	public override void OnMouseClick((MouseButton mouseButton, Vector2 position) args) {
		if (!this.OldEditorInstance.InPlayfield(args.position)) return;
		if (args.mouseButton != MouseButton.Left) return;

		HitObject noteToAdd = new HitObject {
			Time  = this.OldEditorInstance.EditorState.MouseTime,
			Text  = this._defaultNoteText.AsTextBox().Text.Trim(),
			Color = this._defaultNoteColor.AsColorPicker().Color.Value
		};

		this.OldEditorInstance.CreateNote(noteToAdd, true);

		base.OnMouseClick(args);
	}

	public override void Deinitialize() {
		this.DrawableManager.Children.Remove(this._createLine);

		this.OldEditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this._defaultNoteTextLabel);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this._defaultNoteText);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this._defaultNoteColorLabel);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this._defaultNoteColor);

		base.Deinitialize();
	}
}
