using System.Numerics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;

namespace pTyping.Graphics.Editor.Scene.NoteEditor;

public sealed class NoteEditorPlayFieldContainer : CompositeDrawable {
	private readonly RectanglePrimitiveDrawable _outline;

	public override Vector2 Size => this.SizeOverride;

	public Vector2 SizeOverride;

	public NoteEditorPlayFieldContainer(Vector2 position, Vector2 size) {
		this.Position     = position;
		this.SizeOverride = size;

		this._outline = new RectanglePrimitiveDrawable(Vector2.Zero, this.Size, 1, false);

		this.Children.Add(this._outline);
	}
	public void Relayout() {
		this._outline.RectSize = this.Size;
	}
}
