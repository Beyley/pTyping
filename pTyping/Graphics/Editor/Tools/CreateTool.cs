using System;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Helpers;
using Furball.Engine.Engine.Input;
using Microsoft.Xna.Framework;
using pTyping.Engine;
using pTyping.Songs;

namespace pTyping.Graphics.Editor.Tools {
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CreateTool : EditorTool {
        public override string Name    => "Create Note";
        public override string Tooltip => "Create notes on the timeline.";

        private LinePrimitiveDrawable _createLine;

        [ToolOption("Default Text", "The default text in new notes.")]
        public Bindable<string> DefaultNoteText = new("a");
        [ToolOption("Default Colour", "The default colour in new notes.")]
        public Bindable<Color> DefaultNoteColor = new(new(255, 0, 0));

        public override void Initialize() {
            this._createLine = new LinePrimitiveDrawable(new Vector2(0, 0), 80f, (float)Math.PI / 2f) {
                Visible    = false,
                TimeSource = pTypingGame.MusicTrack
            };

            this.DrawableManager.Add(this._createLine);

            base.Initialize();
        }

        public override void OnMouseMove(Point position) {
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
                (int)(this.EditorInstance.EditorState.MouseTime - ConVars.BaseApproachTime.Value),
                (int)this.EditorInstance.EditorState.MouseTime
                )
                );
            }

            base.OnMouseMove(position);
        }

        public override void OnMouseClick((MouseButton mouseButton, Point position) args) {
            if (!EditorScreen.InPlayfield(args.position)) return;
            if (args.mouseButton != MouseButton.LeftButton) return;

            Note noteToAdd = new() {
                Time  = this.EditorInstance.EditorState.MouseTime,
                Text  = this.DefaultNoteText.Value.Trim(),
                Color = this.DefaultNoteColor
            };

            this.EditorInstance.CreateNote(noteToAdd, true);

            base.OnMouseClick(args);
        }

        public override void Deinitialize() {
            this.DrawableManager.Remove(this._createLine);

            base.Deinitialize();
        }
    }
}
