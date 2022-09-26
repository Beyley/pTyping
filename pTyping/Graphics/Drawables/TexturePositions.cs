using System.Drawing;

namespace pTyping.Graphics.Drawables;

public static class TexturePositions {
	public static readonly Rectangle MENU_PLAY_BUTTON    = new Rectangle(0, 0, 300, 100);
	public static readonly Rectangle MENU_EDIT_BUTTON    = new Rectangle(0, 100, 300, 100);
	public static readonly Rectangle MENU_OPTIONS_BUTTON = new Rectangle(0, 200, 300, 100);
	public static readonly Rectangle MENU_EXIT_BUTTON    = new Rectangle(0, 300, 300, 100);

	public static readonly Rectangle EDITOR_PLAY  = new Rectangle(0, 0, 80, 80);
	public static readonly Rectangle EDITOR_PAUSE = new Rectangle(80, 0, 80, 80);
	public static readonly Rectangle EDITOR_RIGHT = new Rectangle(160, 0, 80, 80);
	public static readonly Rectangle EDITOR_LEFT  = new Rectangle(240, 0, 80, 80);
}
