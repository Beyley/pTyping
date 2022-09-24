using System.Drawing;

namespace pTyping.Graphics.Drawables;

public static class TexturePositions {
	public static readonly Rectangle MENU_PLAY_BUTTON    = new(0, 0, 300, 100);
	public static readonly Rectangle MENU_EDIT_BUTTON    = new(0, 100, 300, 100);
	public static readonly Rectangle MENU_OPTIONS_BUTTON = new(0, 200, 300, 100);
	public static readonly Rectangle MENU_EXIT_BUTTON    = new(0, 300, 300, 100);

	public static readonly Rectangle EDITOR_PLAY  = new(0, 0, 80, 80);
	public static readonly Rectangle EDITOR_PAUSE = new(80, 0, 80, 80);
	public static readonly Rectangle EDITOR_RIGHT = new(160, 0, 80, 80);
	public static readonly Rectangle EDITOR_LEFT  = new(240, 0, 80, 80);
}
