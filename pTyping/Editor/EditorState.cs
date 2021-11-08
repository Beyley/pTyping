using System.Collections.Generic;
using System.Collections.ObjectModel;
using pTyping.Drawables;
using pTyping.Songs;

namespace pTyping.Editor {
    public class EditorState {
        public readonly List<NoteDrawable>                 Notes         = new();
        public readonly ObservableCollection<NoteDrawable> SelectedNotes = new();

        public double CurrentTime;
        public double MouseTime;

        public Song Song;
    }
}