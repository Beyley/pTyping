using System.Collections.Generic;
using System.Collections.ObjectModel;
using Furball.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using JetBrains.Annotations;
using pTyping.Graphics.Player;
using pTyping.Songs;
using pTyping.UiGenerator;

namespace pTyping.Graphics.Editor {
    public class EditorState {
        public readonly List<NoteDrawable>    Notes  = new();
        public readonly List<ManagedDrawable> Events = new();

        public readonly ObservableCollection<ManagedDrawable> SelectedObjects = new();

        public double CurrentTime;
        public double MouseTime;

        [NotNull]
        public readonly Song Song;

        public EditorState(Song song) => this.Song = song;

        public readonly UiContainer EditorToolUiContainer = new(OriginType.TopRight) {
            // OriginType = OriginType.TopRight,
            Position = new(FurballGame.DEFAULT_WINDOW_WIDTH - 10, 10),
            Depth    = -1f
        };
    }
}
