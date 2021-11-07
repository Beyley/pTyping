using Furball.Engine;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using pTyping.Drawables;
using pTyping.Screens;

namespace pTyping.Editor.Tools {
    public class SelectTool : EditorTool {
        private double _lastDragTime = 0;
        private bool   _isDragging   = false;

        public override string Name    => "Select";
        public override string Tooltip => "Select, move, and change notes in the timeline.";

        public override void Initialize() {
            foreach (NoteDrawable note in this.EditorInstance.State.Notes) {
                note.OnClick     += this.OnNoteClick;
                note.OnDragBegin += this.OnNoteDragBegin;
                note.OnDrag      += this.OnNoteDrag;
                note.OnDragEnd   += this.OnNoteDragEnd;
            }
        }

        public override void Deinitialize() {
            foreach (NoteDrawable note in this.EditorInstance.State.Notes) {
                note.OnClick     -= this.OnNoteClick;
                note.OnDragBegin -= this.OnNoteDragBegin;
                note.OnDrag      -= this.OnNoteDrag;
                note.OnDragEnd   -= this.OnNoteDragEnd;
            }
        }

        private void OnNoteDragBegin(object sender, Point e) {
            //We only allow dragging notes around when shift is held
            if (!FurballGame.InputManager.HeldKeys.Contains(Keys.LeftShift) || this.EditorInstance.State.SelectedNotes.Count == 0) {
                this._isDragging = false;
                return;
            }

            this._lastDragTime = this.EditorInstance.State.MouseTime;

            this._isDragging = true;
        }

        private void OnNoteDrag(object sender, Point e) {
            //We only allow dragging notes around when shift is held
            if (!FurballGame.InputManager.HeldKeys.Contains(Keys.LeftShift) || !this._isDragging) {
                this._isDragging = false;
                return;
            }

            double difference = this.EditorInstance.State.MouseTime - this._lastDragTime;

            foreach (NoteDrawable noteDrawable in this.EditorInstance.State.SelectedNotes) {
                noteDrawable.Tweens.Clear();

                noteDrawable.Note.Time += difference;

                noteDrawable.Tweens.Add(
                new VectorTween(
                TweenType.Movement,
                new Vector2(PlayerScreen.NOTE_START_POS.X, PlayerScreen.NOTE_START_POS.Y + noteDrawable.Note.YOffset),
                PlayerScreen.RECEPTICLE_POS,
                (int)(noteDrawable.Note.Time - ConVars.BaseApproachTime.Value),
                (int)noteDrawable.Note.Time
                ) {
                    KeepAlive = true
                }
                );
            }

            this.EditorInstance.UpdateSelectionRects(null, null);

            this._lastDragTime = this.EditorInstance.State.MouseTime;
        }

        private void OnNoteDragEnd(object sender, Point e) {
            this._isDragging = false;
        }

        private void OnNoteClick(object sender, Point e) {
            //Somehow the sender isnt the note? ignore as otherwise we'd get a crash
            if (sender is not NoteDrawable note) return;
            //Shift is used for dragging notes, so block selection when shift is held
            if (FurballGame.InputManager.HeldKeys.Contains(Keys.LeftShift)) return;

            //If the user is holding control, then let them select multiple notes, otherwise, only allow selecting one
            if (FurballGame.InputManager.HeldKeys.Contains(Keys.LeftControl) || FurballGame.InputManager.HeldKeys.Contains(Keys.RightControl)) {
                if (!this.EditorInstance.State.SelectedNotes.Remove(note))
                    this.EditorInstance.State.SelectedNotes.Add(note);
            } else {
                this.EditorInstance.State.SelectedNotes.Clear();
                this.EditorInstance.State.SelectedNotes.Add(note);
            }
        }
    }
}
