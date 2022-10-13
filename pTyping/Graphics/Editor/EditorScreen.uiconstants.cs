using System.Numerics;
using pTyping.UiElements;

namespace pTyping.Graphics.Editor;

public partial class EditorScreen {
	public const float MARGIN_AROUND_SCENE = 5f;

	public Vector2 ScenePosition = new Vector2(MARGIN_AROUND_SCENE, ToolbarDrawable.HEIGHT + MARGIN_AROUND_SCENE);
	public Vector2 SceneSize     = new Vector2(1280                                        - MARGIN_AROUND_SCENE * 2, 720 - ToolbarDrawable.HEIGHT - MARGIN_AROUND_SCENE * 2);
}
