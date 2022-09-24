using System.Numerics;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using Furball.Vixie;
using Furball.Vixie.Backends.Shared;
using pTyping.Graphics.Player;
using pTyping.Shared.Events;

namespace pTyping.Graphics.Drawables.Events;

public class LyricEventDrawable : TexturedDrawable {
	public readonly Event Event;

	public LyricEventDrawable(Texture texture, Event @event) : base(texture, new Vector2(0)) {
		this.Scale         = new Vector2(0.3f, 0.6f);
		this.OriginType    = OriginType.Center;
		this.ColorOverride = Color.CornflowerBlue;
		this.Event         = @event;
		this.TimeSource    = pTypingGame.MusicTrackTimeSource;
	}

	public void CreateTweens(GameplayDrawableTweenArgs tweenArgs) {
		this.Tweens.Clear();

		Vector2 noteStartPos  = Player.Player.NOTE_START_POS;
		Vector2 noteEndPos    = Player.Player.NOTE_END_POS;
		Vector2 recepticlePos = Player.Player.RECEPTICLE_POS;

		float travelDistance = noteStartPos.X - recepticlePos.X;
		float travelRatio    = (float)(tweenArgs.ApproachTime / travelDistance);

		float afterTravelTime = (recepticlePos.X - noteEndPos.X) * travelRatio;

		this.Tweens.Add(
			new VectorTween(
				TweenType.Movement,
				new Vector2(noteStartPos.X, noteStartPos.Y),
				recepticlePos,
				(int)(this.Event.Start - tweenArgs.ApproachTime),
				(int)this.Event.Start
			) {
				KeepAlive = tweenArgs.TweenKeepAlive
			}
		);

		this.Tweens.Add(
			new VectorTween(
				TweenType.Movement,
				recepticlePos,
				new Vector2(noteEndPos.X, recepticlePos.Y),
				(int)this.Event.Start,
				(int)(this.Event.Start + afterTravelTime)
			) {
				KeepAlive = tweenArgs.TweenKeepAlive
			}
		);
	}
}
