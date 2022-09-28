using System.Numerics;
using Furball.Vixie.Backends.Shared;
using pTyping.UiElements;

namespace pTyping.Graphics.Menus;

public class UiElementsTestScreen : pScreen {
	public override void Initialize() {
		base.Initialize();

		this.Manager.Add(new ButtonDrawable(new TextDrawable(Localizations.Changelog, pTypingGame.JapaneseFontStroked, 30), new Vector2(200, 50)) {
			Position = new Vector2(10)
		});
		
		this.Manager.Add(new ButtonDrawable(new TextDrawable(Localizations.Changelog, pTypingGame.JapaneseFontStroked, 30), new Vector2(200, 50)) {
			Position = new Vector2(10, 70),
			BaseColor = new Color(0.8f, 0.3f, 0.3f),
			HoverColor = new Color(1.2f, 0.5f, 0.5f)
		});
	}

	public override string               Name                 => "UI Elements Test Screen";
	public override string               State                => "Testing the UI";
	public override string               Details              => "Testing away...";
	public override bool                 ForceSpeedReset      => true;
	public override float                BackgroundFadeAmount => 0.5f;
	public override MusicLoopState       LoopState            => MusicLoopState.None;
	public override ScreenType           ScreenType           => ScreenType.Menu;
	public override ScreenUserActionType OnlineUserActionType => ScreenUserActionType.Listening;
}
