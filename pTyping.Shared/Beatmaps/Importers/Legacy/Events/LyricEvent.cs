using Newtonsoft.Json;

namespace pTyping.Shared.Beatmaps.Importers.Legacy.Events;

[JsonObject(MemberSerialization.OptIn)]
public class LyricEvent : LegacyEvent {
    [JsonProperty]
    public override LegacyEventType Type => LegacyEventType.Lyric;

    [JsonProperty]
    public double EndTime { get; set; } = double.PositiveInfinity;

    [JsonProperty]
    public string Lyric { get; set; } = "";

}
