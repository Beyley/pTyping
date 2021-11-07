using Furball.Engine.Engine.Helpers;
using Furball.Engine.Engine.Input;
using Microsoft.Xna.Framework;
using pTyping.Screens;
using pTyping.Songs;
using WebSocketSharp;

namespace pTyping.Editor.Tools {
    public class BulkCreateTool : EditorTool {
        public override string Name    => "Bulk Create Notes";
        public override string Tooltip => "Create multiple notes with a set spacing.";

        [ToolOption("Lyrics to add", "The lyrics to split and add.")]
        public readonly Bindable<string> LyricsToAdd = new("");
        [ToolOption("Time spacing", "The time spacing between notes.")]
        public readonly Bindable<int> Spacing = new(4);
        [ToolOption("Text Delimiter", "Splits the text every this character.")]
        public readonly Bindable<string> Delimiter = new(";");
        [ToolOption("Color", "The color of all the notes.")]
        public readonly Bindable<string> Color = new("#FF0000");

        public override void OnMouseClick((MouseButton mouseButton, Point position) args) {
            if (!EditorScreen.InPlayfield(args.position)) return;

            string[] splitText = this.LyricsToAdd.Value.Split(this.Delimiter.Value);

            double time = this.EditorInstance.State.CurrentTime;

            double spacing = this.EditorInstance.State.Song.CurrentTimingPoint(time).Tempo / this.Spacing.Value;

            Color color = ColorConverter.FromHexString(this.Color.Value);

            foreach (string text in splitText) {
                if (text.Trim().IsNullOrEmpty()) {
                    time += spacing;

                    continue;
                }

                Note note = new() {
                    Text  = text.Trim(),
                    Time  = time,
                    Color = color
                };

                this.EditorInstance.CreateNote(note, true);

                time += spacing;
            }
        }
    }
}
