using System.Collections.Generic;
using Furball.Engine.Engine.Graphics.Drawables;
using Microsoft.Xna.Framework;
using pTyping.Player;

namespace pTyping.Drawables {
    public class LeaderboardDrawable : CompositeDrawable {

        private readonly List<LeaderboardElementDrawable> _leaderboardElementDrawables = new();

        private readonly List<PlayerScore> _scores;

        public LeaderboardDrawable(List<PlayerScore> scores) {
            this._scores = scores;

            float y = 0;
            foreach (PlayerScore score in this._scores) {
                LeaderboardElementDrawable drawable = new(score) {
                    Position = new(0, y)
                };

                this._leaderboardElementDrawables.Add(drawable);
                this.Drawables.Add(drawable);

                y += drawable.Size.Y;
            }
        }

        private class LeaderboardElementDrawable : CompositeDrawable {
            private readonly TextDrawable _infoTextDrawable;
            private readonly TextDrawable _usernameInfoDrawable;

            public readonly PlayerScore Score;

            public LeaderboardElementDrawable(PlayerScore score) {
                this.Score = score;

                this.Drawables.Add(this._usernameInfoDrawable = new TextDrawable(new(0f),                                        pTypingGame.JapaneseFont, "a", 30));
                this.Drawables.Add(this._infoTextDrawable     = new TextDrawable(new(0f, this._usernameInfoDrawable.Size.Y + 5), pTypingGame.JapaneseFont, "a", 25));

                this.UpdateText();
            }

            public override Vector2 Size => this._infoTextDrawable.Position + this._infoTextDrawable.Size;

            public void UpdateText() {
                this._usernameInfoDrawable.Text = this.Score.Username;
                this._infoTextDrawable.Text     = $"Score: {this.Score.Score} | Accuracy: {this.Score.Accuracy * 100:00.##} | {this.Score.MaxCombo}x";
            }
        }
    }
}
