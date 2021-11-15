using Furball.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using pTyping.Engine;
using pTyping.Graphics.Drawables.Events;
using pTyping.Graphics.Player;

namespace pTyping.Graphics.Editor.Tools {
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SelectTool : EditorTool {
        [ToolOption("Note Colour", "The colour tint of the note.")]
        public Bindable<Color> NoteColour = new(new(255, 0, 0));
        [ToolOption("Note Text", "The note's text you are going to type ingame.")]
        public Bindable<string> NoteText = new(string.Empty);
        public override string Name    => "Select";
        public override string Tooltip => "Select, move, and change notes in the timeline.";

        private bool _dragging = false;

        private static readonly GameplayDrawableTweenArgs _TweenArgs = new(ConVars.BaseApproachTime.Value, true, true);
        
        public override void Initialize() {
            foreach (NoteDrawable note in this.EditorInstance.EditorState.Notes) {
                note.OnClick     += this.OnObjectClick;
                note.OnDragBegin += this.OnObjectDragBegin;
                note.OnDrag      += this.OnObjectDrag;
                note.OnDragEnd   += this.OnObjectDragEnd;
            }
            foreach (ManagedDrawable @event in this.EditorInstance.EditorState.Events) {
                @event.OnClick     += this.OnObjectClick;
                @event.OnDragBegin += this.OnObjectDragBegin;
                @event.OnDrag      += this.OnObjectDrag;
                @event.OnDragEnd   += this.OnObjectDragEnd;
            }

            base.Initialize();
        }

        public override void Deinitialize() {
            foreach (NoteDrawable note in this.EditorInstance.EditorState.Notes) {
                note.OnClick     -= this.OnObjectClick;
                note.OnDragBegin -= this.OnObjectDragBegin;
                note.OnDrag      -= this.OnObjectDrag;
                note.OnDragEnd   -= this.OnObjectDragEnd;
            }
            foreach (ManagedDrawable @event in this.EditorInstance.EditorState.Events) {
                @event.OnClick     -= this.OnObjectClick;
                @event.OnDragBegin -= this.OnObjectDragBegin;
                @event.OnDrag      -= this.OnObjectDrag;
                @event.OnDragEnd   -= this.OnObjectDragEnd;
            }

            base.Deinitialize();
        }

        private void OnObjectDragBegin(object sender, Point e) {
            if (!FurballGame.InputManager.HeldKeys.Contains(Keys.LeftShift)) return;

            this._dragging     = true;
            this._lastDragTime = this.EditorInstance.EditorState.MouseTime;
        }

        private void OnObjectDragEnd(object sender, Point e) {
            this._dragging = false;
        }

        private double _lastDragTime;
        private void OnObjectDrag(object sender, Point e) {
            if (!FurballGame.InputManager.HeldKeys.Contains(Keys.LeftShift)) {
                this._dragging = false;
                return;
            }

            if (!this._dragging) return;

            // We disable this because we are directly setting it, so if its not equal, its not the same value
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (this.EditorInstance.EditorState.MouseTime != this._lastDragTime) {
                double timeDifference = this.EditorInstance.EditorState.MouseTime - this._lastDragTime;

                foreach (ManagedDrawable selectedObject in this.EditorInstance.EditorState.SelectedObjects)
                    switch (selectedObject) {
                        case NoteDrawable noteDrawable:
                            noteDrawable.Note.Time += timeDifference;

                            noteDrawable.CreateTweens(_TweenArgs);
                            break;
                        case BeatLineBarEventDrawable beatLineBarEventDrawable:
                            beatLineBarEventDrawable.Event.Time += timeDifference;

                            beatLineBarEventDrawable.CreateTweens(_TweenArgs);
                            break;
                        case BeatLineBeatEventDrawable beatLineBeatEventDrawable:
                            beatLineBeatEventDrawable.Event.Time += timeDifference;

                            beatLineBeatEventDrawable.CreateTweens(_TweenArgs);
                            break;
                        case TypingCutoffEventDrawable typingCutoffEventDrawable:
                            typingCutoffEventDrawable.Event.Time += timeDifference;

                            typingCutoffEventDrawable.CreateTweens(_TweenArgs);
                            break;
                    }

                this.EditorInstance.UpdateSelectionRects(this, null);

                this.EditorInstance.SaveNeeded = true;
            }
            
            this._lastDragTime = this.EditorInstance.EditorState.MouseTime;
        }

        private void OnObjectClick(object sender, Point e) {
            if (FurballGame.InputManager.HeldKeys.Contains(Keys.LeftShift)) return;

            bool ctrlHeld = FurballGame.InputManager.HeldKeys.Contains(Keys.LeftControl) || FurballGame.InputManager.HeldKeys.Contains(Keys.RightControl);

            if (sender is not ManagedDrawable drawable) return;

            if (ctrlHeld) {
                if (!this.EditorInstance.EditorState.SelectedObjects.Remove(drawable))
                    this.EditorInstance.EditorState.SelectedObjects.Add(drawable);
            } else {
                this.EditorInstance.EditorState.SelectedObjects.Clear();
                this.EditorInstance.EditorState.SelectedObjects.Add(drawable);
            }
        }
    }
}
