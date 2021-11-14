using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using pTyping.Graphics.Editor;
using pTyping.Graphics.Player;
using pTyping.Songs;

namespace pTyping.Graphics.Drawables.Events {
    public class TypingCutoffEventDrawable : TexturedDrawable {
        private readonly Event _event;

        public TypingCutoffEventDrawable(Texture2D texture, Event @event) : base(texture, Vector2.Zero) {
            this._event        = @event;
            this.TimeSource    = pTypingGame.MusicTrack;
            this.ColorOverride = Color.LightBlue;
            this.Scale         = new(0.3f);
            this.OriginType    = OriginType.Center;
        }

        public void CreateTweens(GameplayDrawableTweenArgs tweenArgs) {
            this.Tweens.Clear();

            Vector2 noteStartPos  = tweenArgs.IsEditor ? EditorScreen.NOTE_START_POS : Player.Player.NOTE_START_POS;
            Vector2 noteEndPos    = tweenArgs.IsEditor ? EditorScreen.NOTE_END_POS : Player.Player.NOTE_END_POS;
            Vector2 recepticlePos = tweenArgs.IsEditor ? EditorScreen.RECEPTICLE_POS : Player.Player.RECEPTICLE_POS;

            float travelDistance = noteStartPos.X - recepticlePos.X;
            float travelRatio    = tweenArgs.ApproachTime / travelDistance;

            float afterTravelTime = (recepticlePos.X - noteEndPos.X) * travelRatio;

            this.Tweens.Add(
            new VectorTween(
            TweenType.Movement,
            new(noteStartPos.X, noteStartPos.Y),
            recepticlePos,
            (int)(this._event.Time - tweenArgs.ApproachTime),
            (int)this._event.Time
            ) {
                KeepAlive = tweenArgs.TweenKeepAlive
            }
            );

            this.Tweens.Add(
            new VectorTween(TweenType.Movement, recepticlePos, new(noteEndPos.X, recepticlePos.Y), (int)this._event.Time, (int)(this._event.Time + afterTravelTime)) {
                KeepAlive = tweenArgs.TweenKeepAlive
            }
            );
        }
    }
}
