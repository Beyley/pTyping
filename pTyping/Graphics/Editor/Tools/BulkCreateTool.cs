using System.Collections.Generic;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Input;
using Microsoft.Xna.Framework;
using pTyping.Engine;
using pTyping.Graphics.Player;
using pTyping.Songs;
using pTyping.UiGenerator;

namespace pTyping.Graphics.Editor.Tools {
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

        private readonly List<NoteDrawable> _previewNotes = new();

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
            this.Color                       = UiElement.CreateColorPicker(pTypingGame.JapaneseFont, ITEMTEXTSIZE, Microsoft.Xna.Framework.Color.Red);

            this.EditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.LyricsToAddLabel);
            this.EditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.LyricsToAdd);
            this.EditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.DelimiterLabel);
            this.EditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.Delimiter);
            this.EditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.SpacingLabel);
            this.EditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.Spacing);
            this.EditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.ColorLabel);
            this.EditorInstance.EditorState.EditorToolUiContainer.RegisterElement(this.Color);

            this.Update();
        }

        public override void Deinitialize() {

            this.EditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.LyricsToAddLabel);
            this.EditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.LyricsToAdd);
            this.EditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.DelimiterLabel);
            this.EditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.Delimiter);
            this.EditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.SpacingLabel);
            this.EditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.Spacing);
            this.EditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.ColorLabel);
            this.EditorInstance.EditorState.EditorToolUiContainer.UnRegisterElement(this.Color);

            this._previewNotes.ForEach(x => this.DrawableManager.Remove(x));
            this._previewNotes.Clear();
        }

        public override void OnMouseMove(Point position) {
            this.Update();
        }

        public override void OnTimeChange(double time) {
            this.Update();
        }

        private void Update() {
            this._previewNotes.ForEach(x => this.DrawableManager.Remove(x));
            this._previewNotes.Clear();

            List<Note> notes = this.GenerateNotes();
            foreach (Note note in notes) {
                NoteDrawable drawable = new(Vector2.Zero, this.EditorInstance.NoteTexture, pTypingGame.JapaneseFont, 50) {
                    TimeSource = pTypingGame.MusicTrackTimeSource,
                    RawTextDrawable = {
                        Text = $"{note.Text}"
                    },
                    Scale         = new(0.55f, 0.55f),
                    OriginType    = OriginType.Center,
                    Note          = note,
                    NoteTexture = {
                        ColorOverride = new(255, 255, 255, 100)
                    }
                };

                drawable.Tweens.Add(
                new VectorTween(
                TweenType.Movement,
                EditorScreen.NOTE_START_POS,
                EditorScreen.RECEPTICLE_POS,
                (int)(note.Time - ConVars.BaseApproachTime.Value),
                (int)note.Time
                )
                );

                this._previewNotes.Add(drawable);
                this.DrawableManager.Add(drawable);
            }
        }

        private List<Note> GenerateNotes() {
            string[] splitText = this.LyricsToAdd.AsTextBox().Text.Split(this.Delimiter.AsTextBox().Text);

            double time = this.EditorInstance.EditorState.CurrentTime;

            double spacing = this.EditorInstance.EditorState.Song.CurrentTimingPoint(time).Tempo / double.Parse(this.Spacing.AsTextBox().Text);
            
            List<Note> notes = new();

            foreach (string text in splitText) {
                if (string.IsNullOrEmpty(text.Trim())) {
                    time += spacing;

                    continue;
                }

                Note note = new() {
                    Text  = text.Trim(),
                    Time  = time,
                    Color = this.Color.AsColorPicker().Color
                };

                notes.Add(note);

                time += spacing;
            }
            
            return notes;
        }

        public override void OnMouseClick((MouseButton mouseButton, Point position) args) {
            if (!EditorScreen.InPlayfield(args.position)) return;

            List<Note> notes = this.GenerateNotes();
            notes.ForEach(x => this.EditorInstance.CreateNote(x, true));
        }
    }
}
