using System.Diagnostics.CodeAnalysis;

namespace pTyping.Shared.Mods;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class EasyMod : Mod {
	public override double ScoreMultiplier => 0.9;
	public override string Name            => "Easy";
	public override string ToolTip         => "えーマジ？ Easy Mode？";
	public override string ShorthandName   => "EZ";

	public override void PreStart(IGameState state) {
		//Applies a global 1.4x multiplier to the approach time
		state.EffectApproachTime(1.4);

		base.PreStart(state);
	}
	public override bool IsIncompatible(Mod mod) {
		return mod is HardRockMod;
	}
}
