using System.Numerics;
using Furball.Vixie.Backends.Shared;
using pTyping.Shared.ObjectModel;
using pTyping.UiElements;

namespace pTyping.Graphics.Menus;

public class UiElementsTestScreen : pScreen {
	public override void Initialize() {
		base.Initialize();

		this.Manager.Add(new ButtonDrawable(new TextDrawable(Localizations.Changelog, pTypingGame.JapaneseFont, 30), new Vector2(200, 50)) {
			Position = new Vector2(10)
		});

		this.Manager.Add(new ButtonDrawable(new TextDrawable(Localizations.Changelog, pTypingGame.JapaneseFont, 30), new Vector2(200, 50)) {
			Position   = new Vector2(10, 70),
			BaseColor  = new Color(0.8f, 0.3f, 0.3f),
			HoverColor = new Color(1.2f, 0.5f, 0.5f)
		});

		this.Manager.Add(new SliderDrawable<float>(new BoundNumber<float> {
			MaxValue  = 10,
			MinValue  = 0,
			Precision = 1,
			Value     = 5
		}) {
			Position = new Vector2(10, 150)
		});
		
		this.Manager.Add(new SliderDrawable<float>(new BoundNumber<float> {
			MaxValue  = 10,
			MinValue  = 0,
			Precision = 0.1f,
			Value     = 5
		}) {
			Position = new Vector2(10, 190)
		});
		
		this.Manager.Add(new SliderDrawable<float>(new BoundNumber<float> {
			MaxValue  = 10,
			MinValue  = 0,
			Precision = 0.01f,
			Value     = 5
		}) {
			Position = new Vector2(10, 230)
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
