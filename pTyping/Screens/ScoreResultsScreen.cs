using pTyping.Player;
using Furball.Engine;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Graphics.Drawables.UiElements;
using Microsoft.Xna.Framework;

namespace pTyping.Screens {
	public class ScoreResultsScreen : Screen {
		public PlayerScore Score;
		
		public ScoreResultsScreen(PlayerScore score) {
			this.Score = score;
		}
		
		public override void Initialize() {
			#region Title
			TextDrawable songTitleText   = new(new(10, 10), FurballGame.DEFAULT_FONT, $"{pTypingGame.CurrentSong.Value.Artist} - {pTypingGame.CurrentSong.Value.Name} [{pTypingGame.CurrentSong.Value.Difficulty}]", 40);
			TextDrawable songCreatorText = new(new(10, songTitleText.Size.Y + 20), FurballGame.DEFAULT_FONT, $"Created by {pTypingGame.CurrentSong.Value.Creator}", 30);
			
			this.Manager.Add(songTitleText);
			this.Manager.Add(songCreatorText);
			#endregion
			
			#region Score info
			float y = 175;
			
			TextDrawable score = new(new(100, y), FurballGame.DEFAULT_FONT, $"Score: {this.Score.Score:00000000}", 35);
			y += 20 + score.Size.Y;
			TextDrawable accuracy = new(new(100, y), FurballGame.DEFAULT_FONT, $"Accuracy: {this.Score.Accuracy * 100:0.00}%", 35);
			y += 20 + accuracy.Size.Y;
			TextDrawable combo = new(new(100, y), FurballGame.DEFAULT_FONT, $"Combo: {this.Score.Combo}x", 35);
			
			this.Manager.Add(score);
			this.Manager.Add(accuracy);
			this.Manager.Add(combo);
			#endregion

			#region Buttons
			UiButtonDrawable exitButton = new(new(FurballGame.DEFAULT_WINDOW_WIDTH - 20f, FurballGame.DEFAULT_WINDOW_HEIGHT - 20f), "Exit", FurballGame.DEFAULT_FONT, 40, Color.Red, Color.White, Color.White, Vector2.Zero) {
				OriginType = OriginType.BottomRight
			};
			
			exitButton.OnClick += delegate {
				pTypingGame.MenuClickSound.Play();
				FurballGame.Instance.ChangeScreen(new SongSelectionScreen(false));
			};

			this.Manager.Add(exitButton);
			#endregion
			
			#region background image
			this.Manager.Add(pTypingGame.CurrentSongBackground);
			
			pTypingGame.CurrentSongBackground.Tweens.Add(new ColorTween(TweenType.Color, pTypingGame.CurrentSongBackground.ColorOverride, new(0.5f, 0.5f, 0.5f), pTypingGame.CurrentSongBackground.TimeSource.GetCurrentTime(), pTypingGame.CurrentSongBackground.TimeSource.GetCurrentTime() + 1000));
			pTypingGame.LoadBackgroundFromSong(pTypingGame.CurrentSong.Value);
			#endregion
			
			base.Initialize();
		}
	}
}
