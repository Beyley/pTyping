using System.Collections.ObjectModel;
using System.Numerics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Helpers;
using Furball.Vixie.Backends.Shared;
using pTyping.Graphics.Drawables;
using pTyping.Shared.Events;

namespace pTyping.Graphics.Editor.Scene.LyricEditor;

public sealed class LyricEditorContents : CompositeDrawable {
	private readonly EditorScreen          _editorScreen;
	private readonly LinePrimitiveDrawable _topLine;
	private readonly LinePrimitiveDrawable _bottomLine;

	public const float  HEIGHT                = 100f;
	public const double PIXELS_PER_MILISECOND = 0.25;

	private Vector2 _size;

	private readonly Bindable<bool>                                    _areLyricsSelectable;
	private readonly ObservableCollection<SelectableCompositeDrawable> _selectedLyrics = new ObservableCollection<SelectableCompositeDrawable>();

	public override Vector2 Size => this._size * this.Scale;

	public LyricEditorContents(EditorScreen editorScreen) {
		this._editorScreen = editorScreen;

		this.InvisibleToInput = true;

		this._topLine    = new LinePrimitiveDrawable(Vector2.Zero, Vector2.Zero, Color.White);
		this._bottomLine = new LinePrimitiveDrawable(Vector2.Zero, Vector2.Zero, Color.White);

		this.Children.Add(this._topLine);
		this.Children.Add(this._bottomLine);

		this._areLyricsSelectable = new Bindable<bool>(true);
		foreach (Event @event in this._editorScreen.Beatmap.Events)
			if (@event.Type == EventType.Lyric) {
				LyricDrawable lyric = new LyricDrawable(editorScreen, @event, this._selectedLyrics, this._areLyricsSelectable);

				//Make sure we are already off-screen when the event starts
				lyric.Position.X = this._size.X;

				lyric.Relayout(this.Size.X);

				this.Children.Add(lyric);
			}
	}

	public override void Update(double time) {
		base.Update(time);

		double audioPosition = pTypingGame.MusicTrack.CurrentPosition;

		for (int i = 0; i < this.Children.Count; i++) {
			Drawable drawable = this.Children[i];
			if (drawable is LyricDrawable lyric) {
				// drawable.Visible = audioPosition < lyric.Event.End && audioPosition > lyric.Event.Start - this._size.X / PIXELS_PER_MILISECOND;
			}
		}
	}

	public void Relayout(float newWidth, float newHeight) {
		this._topLine.Position    = new Vector2(0, newHeight / 2f - HEIGHT / 2f);
		this._topLine.EndPosition = new Vector2(newWidth, 0);

		this._bottomLine.Position    = new Vector2(0, newHeight / 2f + HEIGHT / 2f);
		this._bottomLine.EndPosition = new Vector2(newWidth, 0);

		this._size = new Vector2(newWidth, newHeight);

		foreach (Drawable drawable in this.Children)
			if (drawable is LyricDrawable lyric) {
				lyric.Relayout(this.Size.X);

				lyric.Position.Y = newHeight / 2f - HEIGHT / 2f;
			}
	}
}
