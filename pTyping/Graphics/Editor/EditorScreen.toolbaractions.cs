using System;
using System.Collections.Generic;
using Furball.Engine;
using Furball.Engine.Engine;
using pTyping.Graphics.Menus.SongSelect;
using pTyping.UiElements;

namespace pTyping.Graphics.Editor;

public partial class EditorScreen {
	private void OpenFileMenu() {
		ContextMenuDrawable contextMenu = new ContextMenuDrawable(FurballGame.InputManager.Mice[0].Position, new List<(string, Action)> {
			("Save", () => {
				this.CloseCurrentContextMenu();

				//Save the song
				this.Save();
			}),
			("Exit", () => {
				this.CloseCurrentContextMenu();

				//Switch back to song select
				ScreenManager.ChangeScreen(new SongSelectionScreen(true));
			})
		}, pTypingGame.JapaneseFont, 24);

		this.OpenContextMenu(contextMenu);
	}
}
