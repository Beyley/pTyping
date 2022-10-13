using System;
using System.Collections.Generic;
using System.Numerics;
using Furball.Vixie.Backends.Shared;
using pTyping.Shared.ObjectModel;
using pTyping.UiElements;

namespace pTyping.Graphics.Menus;

public class UiElementsTestScreen : pScreen {
	public override void Initialize() {
		base.Initialize();

		this.Manager.Add(new ButtonDrawable(new TextDrawable(Localizations.Changelog, pTypingGame.JapaneseFont, 30), new Vector2(200, 50)) {
			Position = new Vector2(50)
		});

		this.Manager.Add(new ButtonDrawable(new TextDrawable(Localizations.Changelog, pTypingGame.JapaneseFont, 30), new Vector2(200, 50)) {
			Position   = new Vector2(10, 110),
			BaseColor  = new Color(0.8f, 0.3f, 0.3f),
			HoverColor = new Color(1.2f, 0.5f, 0.5f)
		});

		this.Manager.Add(new SliderDrawable<float>(new BoundNumber<float> {
			MaxValue  = 10,
			MinValue  = 0,
			Precision = 1,
			Value     = 5
		}) {
			Position = new Vector2(10, 190)
		});

		this.Manager.Add(new SliderDrawable<float>(new BoundNumber<float> {
			MaxValue  = 10,
			MinValue  = 0,
			Precision = 0.1f,
			Value     = 5
		}) {
			Position = new Vector2(10, 230)
		});

		this.Manager.Add(new SliderDrawable<float>(new BoundNumber<float> {
			MaxValue  = 10,
			MinValue  = 0,
			Precision = 0.01f,
			Value     = 5
		}) {
			Position = new Vector2(10, 270)
		});

		this.Manager.Add(new ContextMenuDrawable(new Vector2(10, 340), new List<(string, Delegate)> {
			("Option 1", new Action(delegate {
				pTypingGame.NotificationManager.CreatePopup("Clicked Option 1");
			})),
			("Option 2", new Action(delegate {
				pTypingGame.NotificationManager.CreatePopup("Clicked Option 2");
			})),
			("This is option 3!", new Action(delegate {
				pTypingGame.NotificationManager.CreatePopup("Clicked Option 3");
			}))
		}, pTypingGame.JapaneseFont, 24));

		this.Manager.Add(new ToolbarDrawable(new Dictionary<string, Delegate> {
			{
				"Option 1", new Action(delegate {
					pTypingGame.NotificationManager.CreatePopup("Clicked Option 1");
				})
			}, {
				"Option 2", new Action(delegate {
					pTypingGame.NotificationManager.CreatePopup("Clicked Option 2");
				})
			}, {
				"Option 3", new Action(delegate {
					pTypingGame.NotificationManager.CreatePopup("Clicked Option 3");
				})
			}
		}, pTypingGame.JapaneseFont));
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
