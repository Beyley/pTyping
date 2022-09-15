using System.Numerics;
using Furball.Engine;
using Furball.Engine.Engine.Graphics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Managers;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Vixie.Backends.Shared;
using pTyping.Graphics.Player;
using pTyping.Shared.Events;

namespace pTyping.Graphics.Drawables.Events;

public class BeatLineBarEventDrawable : TexturedDrawable {
    public readonly Event Event;

    public override Vector2 Size => new Vector2(5, 100) * this.Scale;

    // public BeatLineBarEventDrawable(Event @event) : base(Vector2.Zero, 100, (float)Math.PI / 2f) {
    public BeatLineBarEventDrawable(Event @event) : base(FurballGame.WhitePixel, Vector2.Zero) {
        this.Event         = @event;
        this.Scale         = new Vector2(4, 100);
        this.TimeSource    = pTypingGame.MusicTrackTimeSource;
        this.OriginType    = OriginType.Center;
        this.ColorOverride = new Color(1f, 1f, 1f, 0.7f);
    }

    public void CreateTweens(GameplayDrawableTweenArgs tweenArgs) {
        this.Tweens.Clear();

        Vector2 startPos      = Player.Player.NOTE_START_POS;
        Vector2 endPos        = Player.Player.NOTE_END_POS;
        Vector2 recepticlePos = Player.Player.RECEPTICLE_POS;

        float travelDistance = startPos.X - recepticlePos.X;
        float travelRatio    = (float)(tweenArgs.ApproachTime / travelDistance);

        float afterTravelTime = (recepticlePos.X - endPos.X) * travelRatio;

        this.Tweens.Add(
        new VectorTween(
        TweenType.Movement,
        new Vector2(startPos.X, startPos.Y),
        recepticlePos,
        (int)(this.Event.Start - tweenArgs.ApproachTime),
        (int)this.Event.Start
        ) {
            KeepAlive = tweenArgs.TweenKeepAlive
        }
        );

        this.Tweens.Add(
        new VectorTween(TweenType.Movement, recepticlePos, new Vector2(endPos.X, recepticlePos.Y), (int)this.Event.Start, (int)(this.Event.Start + afterTravelTime)) {
            KeepAlive = tweenArgs.TweenKeepAlive
        }
        );
    }

    public override void Draw(double time, DrawableBatch batch, DrawableManagerArgs args) {
        args.Position.X += this.Size.X;

        base.Draw(time, batch, args);
    }
}