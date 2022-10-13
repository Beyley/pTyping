using System.Drawing;
using System.Numerics;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using Furball.Engine.Engine.Helpers;

namespace pTyping.Graphics.Editor.Scene;

/// <summary>
///     A scene placed within the editor
/// </summary>
public abstract class EditorScene : CompositeDrawable {
	private readonly EditorScreen _editor;
	public EditorScene(EditorScreen editor) {
		this._editor = editor;
	}

	public override Vector2 Size => this._editor.SceneSize * this.Scale;

	public abstract void Opening();
	public abstract void Closing();
	public abstract void Relayout(float newWidth, float newHeight);

	public override void Draw(double time, DrawableBatch batch, DrawableManagerArgs args) {
		//Scissor to the scene size
		batch.ScissorPush(this, new Rectangle(this.RealPosition.ToPoint(), this.RealSize.ToSize()));

		base.Draw(time, batch, args);

		batch.ScissorPop(this);
	}
}
