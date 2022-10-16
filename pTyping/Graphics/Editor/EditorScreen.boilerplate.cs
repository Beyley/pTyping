using JetBrains.Annotations;
using pTyping.UiElements;

namespace pTyping.Graphics.Editor;

public partial class EditorScreen {
	[CanBeNull]
	private ContextMenuDrawable _openContextMenu;

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

		contextMenu.Depth = -10;

		//Add the context menu
		this.Manager.Add(contextMenu);

		//Mark the context menu as open
		this._openContextMenu = contextMenu;
	}

	public override string               Name                 => "Editor";
	public override string               State                => "Editing...";
	public override string               Details              => "Editing a map!";
	public override bool                 ForceSpeedReset      => true;
	public override float                BackgroundFadeAmount => 0.5f;
	public override MusicLoopState       LoopState            => MusicLoopState.None;
	public override ScreenType           ScreenType           => ScreenType.Menu;
	public override ScreenUserActionType OnlineUserActionType => ScreenUserActionType.Editing;
}
