using System.Numerics;
using Furball.Engine.Engine.Graphics.Drawables;

namespace pTyping.Graphics.OldEditor;

public class EditorDrawable : CompositeDrawable {
	public Vector2 OverrideSize;

	public override Vector2 Size => this.OverrideSize;
}
