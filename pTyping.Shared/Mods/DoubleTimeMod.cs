namespace pTyping.Shared.Mods;

public class DoubleTimeMod : Mod {
	public override double ScoreMultiplier => 1.05;
	public override string Name            => "Double Time";
	public override string ToolTip         => "A whole 3 fasts per second!";
	public override string ShorthandName   => "DT";

	public override void PreStart(IGameState state) {
		state.EffectSpeed(1.5);

		base.PreStart(state);
	}
	public override bool IsIncompatible(Mod mod) {
		return mod is HalfTimeMod;
	}
}
