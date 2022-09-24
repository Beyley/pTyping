using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using Furball.Engine.Engine.Graphics.Drawables;
using JetBrains.Annotations;
using pTyping.Graphics.Player;
using pTyping.Shared.Beatmaps;
using pTyping.UiGenerator;

namespace pTyping.Graphics.Editor;

public class EditorState {
	public readonly List<NoteDrawable> Notes  = new();
	public readonly List<Drawable>     Events = new();

	public readonly ObservableCollection<Drawable> SelectedObjects = new();

	public double CurrentTime;
	public double MouseTime;

	[NotNull]
	public readonly Beatmap Song;

	public EditorState(Beatmap song) {
		this.Song = song;
	}

	public readonly UiContainer EditorToolUiContainer = new(OriginType.TopRight) {
		Position         = new Vector2(10, 10),
		Depth            = -1f,
		ScreenOriginType = OriginType.TopRight
	};
}
