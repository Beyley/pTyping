using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Vixie.Backends.Shared;

namespace pTyping.Graphics.Player;

public class AccuracyBarDrawable : CompositeDrawable {
    private readonly Player                     _player;
    private readonly RectanglePrimitiveDrawable _poorRect;
    private readonly RectanglePrimitiveDrawable _fairRect;
    private readonly RectanglePrimitiveDrawable _goodRect;
    private readonly RectanglePrimitiveDrawable _excellentRect;

    public float Width = 300;
    public float Height;
    
    public override Vector2 Size => new(this.Width, this.Height);

    public AccuracyBarDrawable(Vector2 position, Player player, float height = 10) {
        this._player  = player;
        this.Position = position;

        this.Height = height;
        
        this._player.OnCorrectCharTyped += this.OnChar;

        Vector2 pos = new(Width / 2f, 0);

        float fairRatio = this._player.TIMING_FAIR / this._player.TIMING_POOR;
        float goodRatio = this._player.TIMING_GOOD / this._player.TIMING_POOR;
        float excellentRatio = this._player.TIMING_EXCELLENT / this._player.TIMING_POOR;

        this._poorRect = new RectanglePrimitiveDrawable(pos, new Vector2(Width, height), 1f, true) {
            OriginType = OriginType.TopCenter,
            ColorOverride = Player.COLOR_POOR
        };
        this._fairRect = new RectanglePrimitiveDrawable(pos, new Vector2(Width * fairRatio, height), 1f, true) {
            OriginType    = OriginType.TopCenter,
            ColorOverride = Player.COLOR_FAIR

        };
        this._goodRect = new RectanglePrimitiveDrawable(pos, new Vector2(Width * goodRatio, height), 1f, true) {
            OriginType    = OriginType.TopCenter,
            ColorOverride = Player.COLOR_GOOD
        };
        this._excellentRect = new RectanglePrimitiveDrawable(pos, new Vector2(Width * excellentRatio, height), 1f, true) {
            OriginType = OriginType.TopCenter,
            ColorOverride = Player.COLOR_EXCELLENT
        };

        this.Drawables.Add(this._poorRect);
        this.Drawables.Add(this._fairRect);
        this.Drawables.Add(this._goodRect);
        this.Drawables.Add(this._excellentRect);
    }

    private void OnChar(object sender, double e) {
        const float fadeoutTime = 2000;
        
        RectanglePrimitiveDrawable rect = new(new Vector2((float) ((this.Width / 2f) + ((this.Width / 2) * (e / this._player.TIMING_POOR))), this.Height), new(this.Width / 50f, this.Height * 2f), 1f, true) {
            ColorOverride = new(255, 255, 255, 100),
            OriginType = OriginType.BottomCenter
        };
        
        rect.Tweens.Add(new ColorTween(TweenType.Color, rect.ColorOverride, new Color(0, 0, 0, 0), FurballGame.Time, FurballGame.Time + fadeoutTime));
        
        this.Drawables.Add(rect);
        
        FurballGame.GameTimeScheduler.ScheduleMethod(
        _ => {
            this.Drawables.Remove(rect);
        }, FurballGame.Time + fadeoutTime);
    }

    public override void Dispose() {
        this._player.OnCorrectCharTyped -= this.OnChar;

        base.Dispose();
    }
}
