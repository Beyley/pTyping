using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using pTyping.Shared.Mods.Attributes;
using pTyping.Shared.ObjectModel;

namespace pTyping.Shared.Mods;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global"), JsonObject]
public class DoubleTimeMod : Mod {
	public override double ScoreMultiplier => 1.05;
	public override string Name            => "Double Time";
	public override string ToolTip         => "A whole 3 fasts per second!";
	public override string ShorthandName   => "DT";

	[ModSetting("Speed", "The speed of the beatmap", 0)]
	public BoundNumber<double> Speed = new BoundNumber<double> {
		Value     = 1.5,
		MaxValue  = 2,
		MinValue  = 1.1,
		Precision = 0.1
	};
	
	public override void PreStart(IGameState state) {
		state.EffectSpeed(this.Speed);

		base.PreStart(state);
	}
	public override bool IsIncompatible(Mod mod) {
		return mod is HalfTimeMod;
	}
}
