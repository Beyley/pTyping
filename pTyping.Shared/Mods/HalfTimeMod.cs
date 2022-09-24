namespace pTyping.Shared.Mods;

public class HalfTimeMod : Mod {
	public override double ScoreMultiplier => 0.95;
	public override string Name            => "Half Time";
	public override string ToolTip         => "Slowin' down, are we?";
	public override string ShorthandName   => "HT";

	public override void PreStart(IGameState state) {
		state.EffectSpeed(0.5);

		base.PreStart(state);
	}
	public override bool IsIncompatible(Mod mod) {
		return mod is DoubleTimeMod;
	}
}
