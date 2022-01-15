using System;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Microsoft.Xna.Framework;
using pTyping.Graphics.Editor;
using pTyping.Graphics.Player;
using pTyping.Songs;

namespace pTyping.Graphics.Drawables.Events;

public class BeatLineBeatEventDrawable : LinePrimitiveDrawable {
    public readonly Event Event;

    public override Vector2 Size => new(5, 100);

    public BeatLineBeatEventDrawable(Event @event) : base(Vector2.Zero, 100, (float)Math.PI / 2f) {
        this.Event      = @event;
        this.TimeSource = pTypingGame.MusicTrackTimeSource;
        this.OriginType = OriginType.Center;
    }

    public void CreateTweens(GameplayDrawableTweenArgs tweenArgs) {
        this.Tweens.Clear();

        Vector2 startPos      = tweenArgs.IsEditor ? EditorScreen.NOTE_START_POS : Player.Player.NOTE_START_POS;
        Vector2 endPos        = tweenArgs.IsEditor ? EditorScreen.NOTE_END_POS : Player.Player.NOTE_END_POS;
        Vector2 recepticlePos = tweenArgs.IsEditor ? EditorScreen.RECEPTICLE_POS : Player.Player.RECEPTICLE_POS;

        float travelDistance = startPos.X - recepticlePos.X;
        float travelRatio    = (float)(tweenArgs.ApproachTime / travelDistance);

        float afterTravelTime = (recepticlePos.X - endPos.X) * travelRatio;

        this.Tweens.Add(
        new VectorTween(TweenType.Movement, new(startPos.X, startPos.Y), recepticlePos, (int)(this.Event.Time - tweenArgs.ApproachTime), (int)this.Event.Time) {
            KeepAlive = tweenArgs.TweenKeepAlive
        }
        );

        this.Tweens.Add(
        new VectorTween(TweenType.Movement, recepticlePos, new(endPos.X, recepticlePos.Y), (int)this.Event.Time, (int)(this.Event.Time + afterTravelTime)) {
            KeepAlive = tweenArgs.TweenKeepAlive
        }
        );
    }
}