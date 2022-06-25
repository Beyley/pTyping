using System;
using System.Collections.Generic;
using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Engine.Engine.Helpers;
using Furball.Vixie.Backends.Shared;
using pTyping.Online;

namespace pTyping.Graphics.Online;

public class UserCardDrawable : CompositeDrawable {

    private readonly TexturedDrawable       _backgroundDrawable;
    private readonly TextDrawable           _mainTextDrawable;
    private readonly TexturedDrawable       _modeIconDrawable;
    private readonly TextDrawable           _statusTextDrawable;
    private readonly TextDrawable           _usernameDrawable;
    private readonly TextDrawable           _rankDrawable;
    public           Bindable<OnlinePlayer> Player;

    public UserCardDrawable(Vector2 position, OnlinePlayer player) {
        this.Player   = new Bindable<OnlinePlayer>(player);
        this.Position = position;

        this.Drawables.Add(
        this._backgroundDrawable = new TexturedDrawable(ContentManager.LoadTextureFromFile("user-card.png", ContentSource.User), new Vector2(0f)) {
            Clickable   = false,
            CoverClicks = false,
            CoverHovers = false,
            Hoverable   = false
        }
        );
        this.Drawables.Add(
        this._usernameDrawable = new TextDrawable(new Vector2(15f), pTypingGame.FurballFontRegular, "", 55) {
            Scale       = new Vector2(1.7f),
            Clickable   = false,
            CoverClicks = false,
            CoverHovers = false,
            Hoverable   = false
        }
        );
        this.Drawables.Add(
        this._modeIconDrawable = new TexturedDrawable(FurballGame.WhitePixel, new Vector2(0f)) {
            Scale       = new Vector2(0f),
            Clickable   = false,
            CoverClicks = false,
            CoverHovers = false,
            Hoverable   = false,
            OriginType  = OriginType.TopRight
        }
        );

        this.Drawables.Add(
        this._rankDrawable = new TextDrawable(new Vector2(0, 0), pTypingGame.FurballFontRegular, "", 175) {
            Scale         = new Vector2(2f),
            ColorOverride = new Color(255, 255, 255, 100),
            Clickable     = false,
            CoverClicks   = false,
            CoverHovers   = false,
            Hoverable     = false
        }
        );

        this._rankDrawable.MoveTo(new Vector2(this._backgroundDrawable.Size.X - 370, 0));

        this.Drawables.Add(
        this._mainTextDrawable = new TextDrawable(new Vector2(this._usernameDrawable.Position.X, 100), pTypingGame.FurballFontRegular, "", 45) {
            Scale       = new Vector2(1.7f),
            Visible     = true,
            Clickable   = false,
            CoverClicks = false,
            CoverHovers = false,
            Hoverable   = false
        }
        );
        this.Drawables.Add(
        this._statusTextDrawable = new TextDrawable(new Vector2(this._usernameDrawable.Position.X, 100), pTypingGame.JapaneseFont, "", 45) {
            Scale       = new Vector2(1.7f),
            Visible     = true,
            Clickable   = false,
            CoverClicks = false,
            CoverHovers = false,
            Hoverable   = false
        }
        );

        this._modeIconDrawable.MoveTo(new Vector2(this._backgroundDrawable.Size.X - this._modeIconDrawable.Size.X - 10, 10));

        this.UpdateDrawable();
    }

    public override Vector2 Size => this._backgroundDrawable.Size * this.Scale;

    public override void Update(double time) {
        this._mainTextDrawable.Visible   = !this.IsHovered;
        this._statusTextDrawable.Visible = this.IsHovered;

        base.Update(time);
    }

    public override void Draw(double time, DrawableBatch batch, DrawableManagerArgs args) {
        batch.End();
        //
        // Rectangle originalRect = FurballGame.Instance.GraphicsDevice.ScissorRectangle;
        //
        // FurballGame.Instance.GraphicsDevice.ScissorRectangle = new(
        // (int)(this.RealRectangle.X      * FurballGame.VerticalRatio),
        // (int)(this.RealRectangle.Y      * FurballGame.VerticalRatio),
        // (int)(this.RealRectangle.Width  * FurballGame.VerticalRatio),
        // (int)(this.RealRectangle.Height * FurballGame.VerticalRatio)
        // );

        batch.Begin();
        base.Draw(time, batch, args);
        batch.End();

        // FurballGame.Instance.GraphicsDevice.ScissorRectangle = originalRect;

        batch.Begin();
    }

    public static string GetFilenameForModeIcon(PlayMode mode) {
        return mode switch {
            PlayMode.Standard => "standard-mode-icon.png",
            PlayMode.Taiko    => "taiko-mode.icon.png",
            PlayMode.Catch    => "catch-mode-icon.png",
            PlayMode.Mania    => "mania-mode-icon.png",
            PlayMode.pTyping  => "ptyping-mode-icon.png",
            PlayMode.Unknown  => "none",
            PlayMode.Adofai   => "none",//TODO: add adofai icon
            _                 => throw new ArgumentOutOfRangeException(nameof (mode), mode, null)
        };
    }

    private static readonly Dictionary<string, Texture> _TextureCache = new();

    private static Texture GetTextureForMode(string mode) {
        if (_TextureCache.TryGetValue(mode, out Texture tex))
            return tex;

        tex = ContentManager.LoadTextureFromFile(mode, ContentSource.User);

        _TextureCache[mode] = tex;

        return tex;
    }
    
    public void UpdateDrawable() {
        this._usernameDrawable.Text = $@"{this.Player.Value.Username}";
        this._mainTextDrawable.Text = $@"Total Score: {this.Player.Value.TotalScore}
Ranked Score: {this.Player.Value.RankedScore}
Accuracy: {this.Player.Value.Accuracy * 100f:00.00}% Play Count: {this.Player.Value.PlayCount}";
        this._statusTextDrawable.Text = $"{this.Player.Value.Action.Value.ActionText}";
        this._rankDrawable.Text       = this.Player.Value.Rank == 0 ? "" : $"#{this.Player.Value.Rank.Value}";

        string f = GetFilenameForModeIcon(this.Player.Value.Action.Value.Mode);
        if (f == "none") {
            this._modeIconDrawable.SetTexture(FurballGame.WhitePixel);
            this._modeIconDrawable.Scale = new Vector2(0f);
        } else {
            this._modeIconDrawable.SetTexture(GetTextureForMode(f));
            this._modeIconDrawable.Scale = new Vector2(0.175f);
        }

        Color color = this.Player.Value.Action.Value.Action.Value switch {
            UserActionType.Idle    => Color.White,
            UserActionType.Ingame  => Color.Green,
            UserActionType.Editing => Color.Red,
            UserActionType.Leaving => Color.Orange,
            UserActionType.Unknown => Color.White,
            _                      => Color.White
        };

        this._backgroundDrawable.Tweens.Clear();

        this._backgroundDrawable.Tweens.Add(new ColorTween(TweenType.Color, this._backgroundDrawable.ColorOverride, color, FurballGame.Time, FurballGame.Time + 100));
    }
}