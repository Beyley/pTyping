using System.Numerics;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;

namespace pTyping.Graphics.Editor.Scene;

public sealed class NoteEditorScene : EditorScene {
	private readonly RectanglePrimitiveDrawable _outline;

	public NoteEditorScene(EditorScreen editor) : base(editor) {
		this.Children.Add(this._outline = new RectanglePrimitiveDrawable(Vector2.Zero, this.Size, 1, false));
	}

	public override void Opening() {
		//
	}

	public override void Closing() {
		//
	}

	public override void Relayout(float newWidth, float newHeight) {
		this._outline.RectSize = this.Size;
	}
}
