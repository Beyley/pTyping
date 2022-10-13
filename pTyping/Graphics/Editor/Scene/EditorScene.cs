using System.Numerics;
using Furball.Engine.Engine.Graphics.Drawables;

namespace pTyping.Graphics.Editor.Scene;

/// <summary>
///     A scene placed within the editor
/// </summary>
public abstract class EditorScene : CompositeDrawable {
	private readonly EditorScreen _editor;
	public EditorScene(EditorScreen editor) {
		this._editor = editor;
	}

	public override Vector2 Size => this._editor.SceneSize;

	public abstract void Opening();
	public abstract void Closing();
	public abstract void Relayout(float newWidth, float newHeight);
}
