using System.Diagnostics.CodeAnalysis;
using pTyping.Shared.Mods.Attributes;
using pTyping.Shared.ObjectModel;

namespace pTyping.Shared.Mods;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class HalfTimeMod : Mod {
	public override double ScoreMultiplier => 0.95;
	public override string Name            => "Half Time";
	public override string ToolTip         => "Slowin' down, are we?";
	public override string ShorthandName   => "HT";

	[ModSetting("Speed", "The speed of the beatmap", 0)]
	public BoundNumber<double> Speed = new BoundNumber<double> {
		Value     = 0.5,
		MaxValue  = 1,
		MinValue  = 0.1,
		Precision = 0.1
	};

	public override void PreStart(IGameState state) {
		state.EffectSpeed(this.Speed);

		base.PreStart(state);
	}
	public override bool IsIncompatible(Mod mod) {
		return mod is DoubleTimeMod;
	}
}
