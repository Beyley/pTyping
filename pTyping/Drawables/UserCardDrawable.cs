using pTyping.Online;
using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Helpers;
using Microsoft.Xna.Framework;

namespace pTyping.Drawables {
    public class UserCardDrawable : CompositeDrawable {
        public Bindable<OnlinePlayer> Player;

        private TexturedDrawable _backgroundDrawable;
        private TextDrawable     _usernameDrawable;
        private TextDrawable     _mainTextDrawable;
        
        public UserCardDrawable(Vector2 position, OnlinePlayer player) {
            this.Player   = new(player);
            this.Position = position;
            
            this.Drawables.Add(this._backgroundDrawable = new(ContentManager.LoadTextureFromFile("user-card.png"), new(0)));
            this.Drawables.Add(this._usernameDrawable = new(new(5), FurballGame.DEFAULT_FONT, "", 50) {
                Scale = new(1.8f)
            });
            this.Drawables.Add(this._mainTextDrawable = new(new(this._usernameDrawable.Position.X, 30), FurballGame.DEFAULT_FONT, "", 40) {
                Scale = new(1.8f)
            });
            
            this.UpdateText();
        }

        public void UpdateText() {
            this._usernameDrawable.Text = $@"{this.Player.Value.Username}";
            this._mainTextDrawable.Text = $"Total Score: {this.Player.Value.TotalScore}\nRanked Score: {this.Player.Value.RankedScore}\nAccuracy: {this.Player.Value.Accuracy * 100:00.00} Play Count: {this.Player.Value.PlayCount}";
        }
    }
}
