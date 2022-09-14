using Newtonsoft.Json;

namespace pTyping.Shared.Beatmaps.Importers.Legacy;

[JsonObject(MemberSerialization.OptIn)]
public class LegacyTimingPoint {
    [JsonProperty]
    public double Time { get; set; }
    [JsonProperty]
    public double Tempo { get; set; }
    [JsonProperty]
    public double ApproachMultiplier { get; set; } = 1d;
}
