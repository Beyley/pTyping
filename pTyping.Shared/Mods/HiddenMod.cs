using System.Diagnostics.CodeAnalysis;
using Furball.Engine.Engine.Graphics.Drawables;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using pTyping.Shared.Beatmaps.HitObjects;

namespace pTyping.Shared.Mods;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global"), SuppressMessage("ReSharper", "UnusedType.Global")]
public class HiddenMod : Mod {
	public override double ScoreMultiplier => 1.02;
	public override string Name            => "Hidden";
	public override string ToolTip         => "Can someone find me my glasses?";
	public override string ShorthandName   => "HD";

	public override void NoteCreate(IGameState state, Drawable drawable, HitObject hitObject) {
		base.NoteCreate(state, drawable, hitObject);

		double approachTime = state.ApproachTimeAt(hitObject.Time);

		//Add a fade tween from the start of the note appearing, to just before the notes time (scaling with approach time)
		drawable.Tweens.Add(new FloatTween(TweenType.Fade, 1, 0, hitObject.Time - approachTime, hitObject.Time - approachTime / 10d));
	}

	public override bool IsIncompatible(Mod mod) {
		return false;
	}
}
