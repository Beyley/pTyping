using System;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Helpers;
using Furball.Engine.Engine.Input;
using Microsoft.Xna.Framework;
using pTyping.Engine;
using pTyping.Songs;
using pTyping.Songs.Events;

namespace pTyping.Graphics.Editor.Tools {
    public class CreateEventTool : EditorTool {
        public override string Name    => "Create Event";
        public override string Tooltip => "Create events on the timeline";

        private LinePrimitiveDrawable _createLine;

        private const string BEAT_LINE_BEAT = "Beat Line Event";
        private const string BEAT_LINE_BAR  = "Bar Line Event";
        private const string LYRIC          = "Lyric Event";
        private const string TYPING_CUTOFF  = "Typing Cutoff Event";

        [ToolOption("Selected Event", "The selected event to create", BEAT_LINE_BEAT, BEAT_LINE_BAR, LYRIC, TYPING_CUTOFF)]
        public Bindable<string> SelectedEvent = new("");

        public override void Initialize() {
            this._createLine = new LinePrimitiveDrawable(new Vector2(0, 0), 80f, (float)Math.PI / 2f) {
                Visible    = false,
                TimeSource = pTypingGame.MusicTrack
            };

            this.DrawableManager.Add(this._createLine);

            base.Initialize();
        }

        public override void OnMouseClick((MouseButton mouseButton, Point position) args) {
            if (!EditorScreen.InPlayfield(args.position)) return;
            if (args.mouseButton != MouseButton.LeftButton) return;

            Event @event = null;

            switch (this.SelectedEvent.Value) {
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

        public override void Deinitialize() {
            this.DrawableManager.Remove(this._createLine);

            base.Deinitialize();
        }
    }
}
