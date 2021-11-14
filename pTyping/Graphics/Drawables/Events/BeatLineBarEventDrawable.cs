using System;
using Furball.Engine.Engine.Graphics.Drawables.Primitives;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Microsoft.Xna.Framework;
using pTyping.Graphics.Editor;
using pTyping.Graphics.Player;
using pTyping.Songs;

namespace pTyping.Graphics.Drawables.Events {
    public class BeatLineBarEventDrawable : LinePrimitiveDrawable {
        private readonly Event _event;

        public BeatLineBarEventDrawable(Event @event) : base(Vector2.Zero, 100, (float)Math.PI / 2f) {
            this._event     = @event;
            this.Thickness  = 3f;
            this.TimeSource = pTypingGame.MusicTrack;
        }

        public void CreateTweens(GameplayDrawableTweenArgs tweenArgs) {
            this.Tweens.Clear();

            Vector2 startPos      = tweenArgs.IsEditor ? EditorScreen.NOTE_START_POS : Player.Player.NOTE_START_POS;
            Vector2 endPos        = tweenArgs.IsEditor ? EditorScreen.NOTE_END_POS : Player.Player.NOTE_END_POS;
            Vector2 recepticlePos = tweenArgs.IsEditor ? EditorScreen.RECEPTICLE_POS : Player.Player.RECEPTICLE_POS;

            startPos.Y      -= 50;
            endPos.Y        -= 50;
            recepticlePos.Y -= 50;

            float travelDistance = startPos.X - recepticlePos.X;
            float travelRatio    = tweenArgs.ApproachTime / travelDistance;

            float afterTravelTime = (recepticlePos.X - endPos.X) * travelRatio;

            this.Tweens.Add(
            new VectorTween(TweenType.Movement, new(startPos.X, startPos.Y), recepticlePos, (int)(this._event.Time - tweenArgs.ApproachTime), (int)this._event.Time) {
                KeepAlive = tweenArgs.TweenKeepAlive
            }
            );

            this.Tweens.Add(
            new VectorTween(TweenType.Movement, recepticlePos, new(endPos.X, recepticlePos.Y), (int)this._event.Time, (int)(this._event.Time + afterTravelTime)) {
                KeepAlive = tweenArgs.TweenKeepAlive
            }
            );
        }
    }
}
