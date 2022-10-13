using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using Furball.Engine.Engine.Graphics.Drawables;
using JetBrains.Annotations;
using pTyping.Graphics.Player;
using pTyping.Shared.Beatmaps;
using pTyping.UiGenerator;

namespace pTyping.Graphics.OldEditor;

public class EditorState {
	public readonly List<NoteDrawable> Notes  = new List<NoteDrawable>();
	public readonly List<Drawable>     Events = new List<Drawable>();

	public readonly ObservableCollection<Drawable> SelectedObjects = new ObservableCollection<Drawable>();

	public double CurrentTime;
	public double MouseTime;

	[NotNull]
	public readonly Beatmap Song;
	public readonly BeatmapSet Set;

	public EditorState(Beatmap song, BeatmapSet set) {
		this.Song = song;
		this.Set  = set;
	}

	public readonly UiContainer EditorToolUiContainer = new UiContainer(OriginType.TopRight) {
		Position         = new Vector2(10, 10),
		Depth            = -1f,
		ScreenOriginType = OriginType.TopRight
	};
}
