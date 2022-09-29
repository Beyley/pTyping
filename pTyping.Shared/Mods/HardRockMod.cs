using System.Diagnostics.CodeAnalysis;

namespace pTyping.Shared.Mods;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class HardRockMod : Mod {
	public override double ScoreMultiplier => 1.1;
	public override string Name            => "Hard Rock";
	public override string ToolTip         => "6801 Hollywood Blvd Suite 105";
	public override string ShorthandName   => "HR";

	public override void PreStart(IGameState state) {
		//Applies a global effect to the approach time, making it shorter
		state.EffectApproachTime(1.0 / 1.4);

		base.PreStart(state);
	}
	public override bool IsIncompatible(Mod mod) {
		return mod is EasyMod;
	}
}
