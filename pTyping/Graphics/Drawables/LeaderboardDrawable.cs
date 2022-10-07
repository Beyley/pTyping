using System;
using System.Collections.Generic;
using System.Numerics;
using Furball.Engine.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Vixie;
using pTyping.Graphics.Player;
using pTyping.Shared.Mods;
using pTyping.Shared.Scores;

namespace pTyping.Graphics.Drawables;

public class LeaderboardDrawable : CompositeDrawable {
	private readonly List<LeaderboardElementDrawable> _leaderboardElementDrawables = new List<LeaderboardElementDrawable>();

	private readonly List<Score> _scores;

	public LeaderboardDrawable(List<Score> scores) {
		Texture tex = ContentManager.LoadTextureFromFileCached("song-button-background.png", ContentSource.User);

		this.Clickable   = false;
		this.CoverClicks = false;
		this.Hoverable   = false;
		this.CoverHovers = false;

		this._scores = scores;

		float y = 0;
		for (int i = 0; i < this._scores.GetRange(0, Math.Min(8, scores.Count)).Count; i++) {
			Score score = this._scores.GetRange(0, Math.Min(8, scores.Count))[i];
			LeaderboardElementDrawable drawable = new LeaderboardElementDrawable(score, tex) {
				Position = new Vector2(0, y)
			};

			this._leaderboardElementDrawables.Add(drawable);
			this.Children.Add(drawable);

			drawable.OnClick += delegate {
				ScreenManager.ChangeScreen(new ScoreResultsScreen(score));
			};

			y += drawable.Size.Y + 5f;
		}
	}

	private class LeaderboardElementDrawable : CompositeDrawable {
		private readonly TexturedDrawable _backgroundDrawable;
		private readonly TextDrawable     _infoTextDrawable;
		private readonly TextDrawable     _usernameInfoDrawable;

		public readonly Score Score;

		public LeaderboardElementDrawable(Score score, Texture tex) {
			this.Score = score;

			this.Children.Add(
				this._backgroundDrawable = new TexturedDrawable(tex, Vector2.Zero) {
					Scale = new Vector2(0.2f)
				}
			);
			this.Children.Add(this._usernameInfoDrawable = new TextDrawable(new Vector2(3f), pTypingGame.JapaneseFont, "a", 30));
			this.Children.Add(this._infoTextDrawable     = new TextDrawable(new Vector2(3f, this._usernameInfoDrawable.Size.Y + 8f), pTypingGame.JapaneseFont, "a", 25));

			this.UpdateText();
		}

		public override Vector2 Size => this._backgroundDrawable.Size * this.Scale;

		public void UpdateText() {
			this._usernameInfoDrawable.Text = this.Score.User.Username;
			this._infoTextDrawable.Text =
				$"Score: {this.Score.AchievedScore} | Accuracy: {this.Score.Accuracy * 100:00.##} | {this.Score.MaxCombo}x | {Mod.ModsShorthandString(this.Score.Mods)}";
		}
	}
}
