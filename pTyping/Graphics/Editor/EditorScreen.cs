using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using pTyping.UiElements;

namespace pTyping.Graphics.Editor;

public partial class EditorScreen : pScreen {
	[CanBeNull]
	private ContextMenuDrawable _openContextMenu;

	public override void Initialize() {
		base.Initialize();

		this.Manager.Add(new ToolbarDrawable(new Dictionary<string, Action> {
			{ "File", this.OpenFileMenu }
		}, pTypingGame.JapaneseFont));
	}

	private void CloseCurrentContextMenu() {
		//Do nothing if there is no context menu open
		if (this._openContextMenu == null)
			return;

		//Remove the context menu
		this.Manager.Remove(this._openContextMenu);

		//Mark no context menu is open
		this._openContextMenu = null;
	}


	private void OpenContextMenu([NotNull] ContextMenuDrawable contextMenu) {
		this.CloseCurrentContextMenu();

		//Add the context menu
		this.Manager.Add(contextMenu);

		//Mark the context menu as open
		this._openContextMenu = contextMenu;
	}
}
