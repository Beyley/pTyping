using System;
using pTyping.Online;
using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Helpers;
using Microsoft.Xna.Framework;

namespace pTyping.Drawables {
    public class UserCardDrawable : CompositeDrawable {
        public Bindable<OnlinePlayer> Player;

        private TexturedDrawable _backgroundDrawable;
        private TextDrawable     _usernameDrawable;
        private TextDrawable     _mainTextDrawable;
        private TextDrawable     _statusTextDrawable;
        private TexturedDrawable _modeIconDrawable;

        public override Vector2 Size => this._backgroundDrawable.Size * this.Scale;

        public UserCardDrawable(Vector2 position, OnlinePlayer player) {
            this.Player   = new(player);
            this.Position = position;
            
            this.Drawables.Add(this._backgroundDrawable = new(ContentManager.LoadTextureFromFile("user-card.png", ContentSource.User), new(0f)));
            this.Drawables.Add(this._usernameDrawable = new(new(15f), FurballGame.DEFAULT_FONT_STROKED, "", 55) {
                Scale = new(1.7f)
            });
            this.Drawables.Add(this._mainTextDrawable = new(new(this._usernameDrawable.Position.X, 100), FurballGame.DEFAULT_FONT_STROKED, "", 45) {
                Scale = new(1.7f),
                Visible = true
            });
            this.Drawables.Add(this._statusTextDrawable = new(new(this._usernameDrawable.Position.X, 100), pTypingGame.JapaneseFontStroked, "", 45) {
                Scale   = new(1.7f),
                Visible = true
            });
            this.Drawables.Add(this._modeIconDrawable = new(ContentManager.LoadTextureFromFile(GetFilenameForModeIcon(player.Action.Value.Mode), ContentSource.User), new(0f)) {
                Scale = new(0.175f)
            });
            
            this._modeIconDrawable.MoveTo(new Vector2(this._backgroundDrawable.Size.X - this._modeIconDrawable.Size.X - 10, 10));
            
            this.UpdateDrawable();
        }

        public override void Update(GameTime time) {
            this._mainTextDrawable.Visible = !this.IsHovered;
            this._statusTextDrawable.Visible = this.IsHovered;
            
            base.Update(time);
        }

        public static string GetFilenameForModeIcon(PlayMode mode) {
            return mode switch {
                PlayMode.Standard => "standard-mode-icon.png",
                PlayMode.Taiko    => "taiko-mode.icon.png",
                PlayMode.Catch    => "catch-mode-icon.png",
                PlayMode.Mania    => "mania-mode-icon.png",
                PlayMode.pTyping  => "ptyping-mode.icon.png",
                _                 => throw new ArgumentOutOfRangeException(nameof (mode), mode, null)
            };
        }

        public void UpdateDrawable() {
            this._usernameDrawable.Text = $@"{this.Player.Value.Username}";
            this._mainTextDrawable.Text = $"Total Score: {this.Player.Value.TotalScore}\nRanked Score: {this.Player.Value.RankedScore}\nAccuracy: {this.Player.Value.Accuracy * 100f:00.00}% Play Count: {this.Player.Value.PlayCount}";
            this._statusTextDrawable.Text = $"{this.Player.Value.Action.Value.ActionText}";
            
            this._modeIconDrawable.SetTexture(ContentManager.LoadTextureFromFile(GetFilenameForModeIcon(this.Player.Value.Action.Value.Mode), ContentSource.User));
            
            Color color = this.Player.Value.Action.Value.Action.Value switch {
                UserActionType.Idle    => Color.White,
                UserActionType.Ingame  => Color.Green,
                UserActionType.Editing => Color.Red,
                UserActionType.Leaving => Color.Orange,
                UserActionType.Unknown => Color.White,
                _                      => Color.White
            };

            this._backgroundDrawable.Tweens.Clear();
            
            this._backgroundDrawable.Tweens.Add(
            new ColorTween(TweenType.Color, this._backgroundDrawable.ColorOverride, color, FurballGame.Time, FurballGame.Time + 100)
            );
        }
    }
}
