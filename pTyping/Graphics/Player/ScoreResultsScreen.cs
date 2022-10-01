using System;
using System.Linq;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Furball.Engine.Engine.Input.Events;
using Furball.Vixie.Backends.Shared;
using pTyping.Engine;
using pTyping.Graphics.Drawables;
using pTyping.Graphics.Menus.SongSelect;
using pTyping.Shared.Beatmaps;
using pTyping.Shared.Scores;

namespace pTyping.Graphics.Player;

public class ScoreResultsScreen : pScreen {
	public Score Score;

	public ScoreResultsScreen(Score score) {
		this.Score = score;
	}

	public override void Initialize() {
		base.Initialize();

		#region Title

		BeatmapSet set = pTypingGame.CurrentSong.Value.Parent.First();

		TextDrawable songTitleText   = new TextDrawable(new Vector2(10, 10), pTypingGame.JapaneseFont, $"{set.Artist} - {set.Title} [{pTypingGame.CurrentSong.Value.Info.DifficultyName}]", 40);
		TextDrawable songCreatorText = new TextDrawable(new Vector2(10, songTitleText.Size.Y + 20), pTypingGame.JapaneseFont, $"Created by {pTypingGame.CurrentSong.Value.Info.Mapper}", 30);

		this.Manager.Add(songTitleText);
		this.Manager.Add(songCreatorText);

		#endregion

		#region Score info

		ScoreResultsDrawable playerResults = new ScoreResultsDrawable(this.Score) {
			Position = new Vector2(0, FurballGame.DEFAULT_WINDOW_HEIGHT / 2f)
		};

		this.Manager.Add(playerResults);

		#endregion

		#region Buttons

		EventHandler<MouseButtonEventArgs> exitOnClick = delegate {
			pTypingGame.MenuClickSound.PlayNew();
			ScreenManager.ChangeScreen(new SongSelectionScreen(false));
		};

		DrawableButton exitButton = new DrawableButton(new Vector2(20f, 20f), FurballGame.DefaultFont, 40, "Exit", Color.Red, Color.White, Color.White, Vector2.Zero, exitOnClick) {
			OriginType       = OriginType.BottomRight,
			ScreenOriginType = OriginType.BottomRight
		};

		this.Manager.Add(exitButton);

		EventHandler<MouseButtonEventArgs> watchReplayOnClick = delegate {
			pTypingGame.MenuClickSound.PlayNew();
			if (this.Score.ReplayFrames.Count == 0) {
				pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Error, "This score has no replay data!");
				return;
			}
			ScreenManager.ChangeScreen(new PlayerScreen(this.Score));
		};

		DrawableButton watchReplayButton = new DrawableButton(new Vector2(20f, 80f), FurballGame.DefaultFont, 40, "Watch Replay", Color.Blue, Color.White, Color.White, Vector2.Zero, watchReplayOnClick) {
			OriginType       = OriginType.BottomRight,
			ScreenOriginType = OriginType.BottomRight
		};

		this.Manager.Add(watchReplayButton);

		#endregion

		#region background image

		this.Manager.Add(pTypingGame.CurrentSongBackground);

		#endregion
	}

	public override ScreenUserActionType OnlineUserActionType => ScreenUserActionType.Listening;

	public override string Name  => "Score Results";
	public override string State => "Looking at scores!";

	public override string Details {
		get {
			BeatmapSet set = pTypingGame.CurrentSong.Value.Parent.First();
			return $@"{set.Artist} - {set.Title} [{pTypingGame.CurrentSong.Value.Info.DifficultyName}]
Played by {this.Score.User.Username}
Score: {this.Score.AchievedScore:0000000} Accuracy: {100d * this.Score.Accuracy:00.##}%";
		}
	}
	public override bool           ForceSpeedReset      => false;
	public override float          BackgroundFadeAmount => 0.5f;
	public override MusicLoopState LoopState            => MusicLoopState.None;
	public override ScreenType     ScreenType           => ScreenType.Menu;
}
