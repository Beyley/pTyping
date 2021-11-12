using System.Collections.Specialized;
using System.Linq;
using Furball.Engine;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using pTyping.Engine;
using pTyping.Graphics.Player;

namespace pTyping.Graphics.Editor.Tools {
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SelectTool : EditorTool {
        public override string Name    => "Select";
        public override string Tooltip => "Select, move, and change notes in the timeline.";

        [ToolOption("Note Text", "The note's text you are going to type ingame.")]
        public Bindable<string> NoteText = new(string.Empty);
        [ToolOption("Note Colour", "The colour tint of the note.")]
        public Bindable<Color> NoteColour = new(new(255, 0, 0));
        
        private double _lastDragTime = 0;
        private bool   _isDragging   = false;

        public override void Initialize() {
            foreach (NoteDrawable note in this.EditorInstance.State.Notes) {
                note.OnClick     += this.OnNoteClick;
                note.OnDragBegin += this.OnNoteDragBegin;
                note.OnDrag      += this.OnNoteDrag;
                note.OnDragEnd   += this.OnNoteDragEnd;
            }

            this.EditorInstance.State.SelectedNotes.CollectionChanged += this.OnSelectedNotesChanged;

            this.NoteText.OnChange   += this.UpdateNoteText;
            this.NoteColour.OnChange += this.UpdateNoteColor;
        }

        public override void OnNoteCreate(NoteDrawable note, bool isNew) {
            note.OnClick     += this.OnNoteClick;
            note.OnDragBegin += this.OnNoteDragBegin;
            note.OnDrag      += this.OnNoteDrag;
            note.OnDragEnd   += this.OnNoteDragEnd;
        }

        public override void OnNoteDelete(NoteDrawable note) {
            note.OnClick     -= this.OnNoteClick;
            note.OnDragBegin -= this.OnNoteDragBegin;
            note.OnDrag      -= this.OnNoteDrag;
            note.OnDragEnd   -= this.OnNoteDragEnd;
        }

        private void UpdateNoteText(object __, string _) {
            if (this.EditorInstance.State.SelectedNotes.Count != 1) {
                this.NoteText.Value   = "";
                this.NoteColour.Value = new(255, 0, 0);
                return;
            }

            NoteDrawable note = this.EditorInstance.State.SelectedNotes.First();

            note.Note.Text              = this.NoteText.Value.Trim();
            note.LabelTextDrawable.Text = $"{note.Note.Text}";

            this.EditorInstance.SaveNeeded = true;
        }

        private void UpdateNoteColor(object __, Color _) {
            if (this.EditorInstance.State.SelectedNotes.Count != 1)// this.NoteText.Value   = "";
                // this.NoteColour.Value = new(255, 0, 0);
                return;

            NoteDrawable note = this.EditorInstance.State.SelectedNotes.First();

            note.Note.Color    = this.NoteColour.Value;
            note.ColorOverride = note.Note.Color;

            this.EditorInstance.SaveNeeded = true;
        }

        private void OnSelectedNotesChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (this.EditorInstance.State.SelectedNotes.Count != 1)// this.NoteText.Value   = "";
                // this.NoteColour.Value = new(255, 0, 0);
                return;

            NoteDrawable note = this.EditorInstance.State.SelectedNotes.First();

            this.NoteText.Value   = note.Note.Text;
            this.NoteColour.Value = note.Note.Color;
        }

        public override void Deinitialize() {
            foreach (NoteDrawable note in this.EditorInstance.State.Notes) {
                note.OnClick     -= this.OnNoteClick;
                note.OnDragBegin -= this.OnNoteDragBegin;
                note.OnDrag      -= this.OnNoteDrag;
                note.OnDragEnd   -= this.OnNoteDragEnd;
            }

            this.EditorInstance.State.SelectedNotes.CollectionChanged -= this.OnSelectedNotesChanged;

            this.NoteText.OnChange   -= this.UpdateNoteText;
            this.NoteColour.OnChange -= this.UpdateNoteColor;
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

            if (this.EditorInstance.State.SelectedNotes.Count == 1) {
                NoteDrawable noteDrawable = this.EditorInstance.State.SelectedNotes[0];

                noteDrawable.Tweens.Clear();

                noteDrawable.Note.Time = this.EditorInstance.State.MouseTime;

                noteDrawable.Tweens.Add(
                new VectorTween(
                TweenType.Movement,
                new Vector2(EditorScreen.NOTE_START_POS.X, EditorScreen.NOTE_START_POS.Y + noteDrawable.Note.YOffset),
                EditorScreen.RECEPTICLE_POS,
                (int)(noteDrawable.Note.Time - ConVars.BaseApproachTime.Value),
                (int)noteDrawable.Note.Time
                ) {
                    KeepAlive = true
                }
                );

                this.EditorInstance.SaveNeeded = true;
            } else {
                foreach (NoteDrawable noteDrawable in this.EditorInstance.State.SelectedNotes) {
                    noteDrawable.Tweens.Clear();

                    noteDrawable.Note.Time += difference;

                    noteDrawable.Tweens.Add(
                    new VectorTween(
                    TweenType.Movement,
                    new Vector2(EditorScreen.NOTE_START_POS.X, EditorScreen.NOTE_START_POS.Y + noteDrawable.Note.YOffset),
                    EditorScreen.RECEPTICLE_POS,
                    (int)(noteDrawable.Note.Time - ConVars.BaseApproachTime.Value),
                    (int)noteDrawable.Note.Time
                    ) {
                        KeepAlive = true
                    }
                    );
                }

                this.EditorInstance.SaveNeeded = true;
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
