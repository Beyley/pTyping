using System.Collections.Generic;
using System.Numerics;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Vixie.Backends.Shared;
using pTyping.Engine;
using pTyping.Graphics.Player;
using pTyping.Shared.Beatmaps.HitObjects;
using pTyping.UiGenerator;
using Silk.NET.Input;

namespace pTyping.Graphics.OldEditor.Tools;

public class BulkCreateTool : EditorTool {
	public override string Name    => "Bulk Create Notes";
	public override string Tooltip => "Create multiple notes with a set spacing.";

	// [ToolOption("Lyrics to add", "The lyrics to split and add.")]
	// public readonly Bindable<string> LyricsToAdd = new("");
	// [ToolOption("Time spacing", "The time spacing between notes.")]
	// public readonly Bindable<int> Spacing = new(4);
	// [ToolOption("Text Delimiter", "Splits the text every this character.")]
	// public readonly Bindable<string> Delimiter = new(";");
	// [ToolOption("Color", "The color of all the notes.")]
	// public readonly Bindable<Color> Color = new(new(255, 0, 0));
	public UiElement LyricsToAdd;
	public UiElement LyricsToAddLabel;
	public UiElement Spacing;
	public UiElement SpacingLabel;
	public UiElement Delimiter;
	public UiElement DelimiterLabel;
	public UiElement Color;
	public UiElement ColorLabel;

	private readonly List<NoteDrawable> _previewNotes = new List<NoteDrawable>();

	public override void Initialize() {
		this.LyricsToAddLabel            = UiElement.CreateText(pTypingGame.JapaneseFont, "Lyrics", LABELTEXTSIZE);
		this.LyricsToAddLabel.SpaceAfter = LABELAFTERDISTANCE;
		this.LyricsToAdd                 = UiElement.CreateTextBox(pTypingGame.JapaneseFont, "", ITEMTEXTSIZE, TEXTBOXWIDTH);
		this.DelimiterLabel              = UiElement.CreateText(pTypingGame.JapaneseFont, "Delimiter", LABELTEXTSIZE);
		this.DelimiterLabel.SpaceAfter   = LABELAFTERDISTANCE;
		this.Delimiter                   = UiElement.CreateTextBox(pTypingGame.JapaneseFont, ";", ITEMTEXTSIZE, TEXTBOXWIDTH);
		this.SpacingLabel                = UiElement.CreateText(pTypingGame.JapaneseFont, "Spacing", LABELTEXTSIZE);
		this.SpacingLabel.SpaceAfter     = LABELAFTERDISTANCE;
		this.Spacing                     = UiElement.CreateTextBox(pTypingGame.JapaneseFont, "4", ITEMTEXTSIZE, TEXTBOXWIDTH);
		this.ColorLabel                  = UiElement.CreateText(pTypingGame.JapaneseFont, "Color", LABELTEXTSIZE);
		this.ColorLabel.SpaceAfter       = LABELAFTERDISTANCE;
		this.Color                       = UiElement.CreateColorPicker(pTypingGame.JapaneseFont, ITEMTEXTSIZE, Furball.Vixie.Backends.Shared.Color.Red);

		this.OldEditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.LyricsToAddLabel);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.LyricsToAdd);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.DelimiterLabel);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.Delimiter);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.SpacingLabel);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.Spacing);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.ColorLabel);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.Color);

		this.Update();
	}

	public override void Deinitialize() {
		this.OldEditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.LyricsToAddLabel);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.LyricsToAdd);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.DelimiterLabel);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.Delimiter);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.SpacingLabel);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.Spacing);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.ColorLabel);
		this.OldEditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.Color);

		this._previewNotes.ForEach(x => this.DrawableManager.Children.Remove(x));
		this._previewNotes.Clear();
	}

	public override void OnMouseMove(Vector2 position) {
		this.Update();
	}

	public override void OnTimeChange(double time) {
		this.Update();
	}

	private void Update() {
		this._previewNotes.ForEach(x => this.DrawableManager.Children.Remove(x));
		this._previewNotes.Clear();

		List<HitObject> notes = this.GenerateNotes();
		foreach (HitObject note in notes) {
			NoteDrawable drawable = new NoteDrawable(Vector2.Zero, this.OldEditorInstance.NoteTexture, pTypingGame.JapaneseFont, 50, null, PlayerStateArguments.DefaultPlayer) {
				TimeSource = pTypingGame.MusicTrackTimeSource,
				RawTextDrawable = {
					Text = $"{note.Text}"
				},
				Scale      = new Vector2(0.55f, 0.55f),
				OriginType = OriginType.Center,
				Note       = note,
				NoteTexture = {
					ColorOverride = new Color(255, 255, 255, 100)
				},
				Clickable   = false,
				CoverClicks = false,
				Hoverable   = false,
				CoverHovers = false
			};

			drawable.Tweens.Add(
				new VectorTween(
					TweenType.Movement,
					Player.Player.NOTE_START_POS,
					Player.Player.RECEPTICLE_POS,
					(int)(note.Time - ConVars.BASE_APPROACH_TIME),
					(int)note.Time
				)
			);

			this._previewNotes.Add(drawable);
			this.DrawableManager.Children.Add(drawable);
		}
	}

	private List<HitObject> GenerateNotes() {
		string[] splitText = this.LyricsToAdd.AsTextBox().Text.Split(this.Delimiter.AsTextBox().Text);

		double time = this.OldEditorInstance.EditorState.CurrentTime;

		try {
			double spacing = this.OldEditorInstance.EditorState.Song.CurrentTimingPoint(time).Tempo / double.Parse(this.Spacing.AsTextBox().Text);

			List<HitObject> notes = new List<HitObject>();

			foreach (string text in splitText) {
				if (string.IsNullOrEmpty(text.Trim())) {
					time += spacing;

					continue;
				}

				HitObject note = new HitObject {
					Text  = text.Trim(),
					Time  = time,
					Color = this.Color.AsColorPicker().Color.Value
				};

				notes.Add(note);

				time += spacing;
			}

			return notes;
		}
		catch {
			return new List<HitObject>();
		}
	}

	public override void OnMouseClick((MouseButton mouseButton, Vector2 position) args) {
		if (!this.OldEditorInstance.InPlayfield(args.position)) return;

		List<HitObject> notes = this.GenerateNotes();
		notes.ForEach(x => this.OldEditorInstance.CreateNote(x, true));
	}
}
