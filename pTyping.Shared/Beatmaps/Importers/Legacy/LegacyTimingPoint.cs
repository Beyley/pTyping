using Newtonsoft.Json;

namespace pTyping.Shared.Beatmaps.Importers.Legacy;

[JsonObject(MemberSerialization.OptIn)]
internal class LegacyTimingPoint {
	[JsonProperty]
	public double Time { get; set; }
	[JsonProperty]
	public double Tempo { get; set; }
	[JsonProperty]
	public double ApproachMultiplier { get; set; } = 1d;
}
