using System;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Input;
using Microsoft.Xna.Framework;
using pTyping.Screens;
using pTyping.Songs;

namespace pTyping.Editor.Tools {
    public class CreateTool : EditorTool {
        public override string Name    => "Create Note";
        public override string Tooltip => "Create notes on the timeline.";

        private LinePrimitiveDrawable _createLine;

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
                new(PlayerScreen.NOTE_START_POS.X, PlayerScreen.NOTE_START_POS.Y - 40),
                new(PlayerScreen.RECEPTICLE_POS.X, PlayerScreen.RECEPTICLE_POS.Y - 40),
                (int)(this.EditorInstance.State.MouseTime - ConVars.BaseApproachTime.Value),
                (int)this.EditorInstance.State.MouseTime
                )
                );
            }

            base.OnMouseMove(position);
        }

        public override void OnMouseClick((MouseButton mouseButton, Point position) args) {
            if (!EditorScreen.InPlayfield(args.position)) return;
            if (args.mouseButton != MouseButton.LeftButton) return;

            Note noteToAdd = new() {
                Time = this.EditorInstance.State.MouseTime,
                Text = "a"
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
