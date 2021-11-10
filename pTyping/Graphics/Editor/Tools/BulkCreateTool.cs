using System.Collections.Generic;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Helpers;
using Furball.Engine.Engine.Input;
using Microsoft.Xna.Framework;
using pTyping.Engine;
using pTyping.Screens.Player;
using pTyping.Songs;
using WebSocketSharp;

namespace pTyping.Screens.Editor.Tools {
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

        private readonly List<NoteDrawable> _previewNotes = new();

        public override void Initialize() {
            this.LyricsToAdd.OnChange += this.OnTextChange;
            this.Delimiter.OnChange   += this.OnTextChange;
            this.Color.OnChange       += this.OnTextChange;
            this.Spacing.OnChange     += this.OnIntChange;

            this.Update();
        }

        public override void Deinitialize() {
            this.LyricsToAdd.OnChange -= this.OnTextChange;
            this.Delimiter.OnChange   -= this.OnTextChange;
            this.Color.OnChange       -= this.OnTextChange;
            this.Spacing.OnChange     -= this.OnIntChange;

            this._previewNotes.ForEach(x => this.DrawableManager.Remove(x));
            this._previewNotes.Clear();
        }

        private void OnTextChange(object? sender, string e) {
            this.Update();
        }

        private void OnIntChange(object? sender, int e) {
            this.Update();
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
                    TimeSource = pTypingGame.MusicTrack,
                    LabelTextDrawable = {
                        Text  = $"{note.Text}",
                        Scale = new(1f)
                    },
                    Scale         = new(0.55f, 0.55f),
                    OriginType    = OriginType.Center,
                    Note          = note,
                    ColorOverride = new(255, 255, 255, 100)
                };

                drawable.Tweens.Add(
                new VectorTween(
                TweenType.Movement,
                PlayerScreen.NOTE_START_POS,
                PlayerScreen.RECEPTICLE_POS,
                (int)(note.Time - ConVars.BaseApproachTime.Value),
                (int)note.Time
                )
                );

                this._previewNotes.Add(drawable);
                this.DrawableManager.Add(drawable);
            }
        }

        private List<Note> GenerateNotes() {
            string[] splitText = this.LyricsToAdd.Value.Split(this.Delimiter.Value);

            double time = this.EditorInstance.State.CurrentTime;

            double spacing = this.EditorInstance.State.Song.CurrentTimingPoint(time).Tempo / this.Spacing.Value;

            Color color = Microsoft.Xna.Framework.Color.Red;

            try {
                color = ColorConverter.FromHexString(this.Color.Value);
            }
            catch {/* */
            }

            List<Note> notes = new();
            
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
