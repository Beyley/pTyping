namespace pTyping.Graphics.Editor;

public partial class EditorScreen {
	public override string               Name                 => "Editor";
	public override string               State                => "Editing...";
	public override string               Details              => "Editing a map!";
	public override bool                 ForceSpeedReset      => true;
	public override float                BackgroundFadeAmount => 0.5f;
	public override MusicLoopState       LoopState            => MusicLoopState.None;
	public override ScreenType           ScreenType           => ScreenType.Menu;
	public override ScreenUserActionType OnlineUserActionType => ScreenUserActionType.Editing;
}
