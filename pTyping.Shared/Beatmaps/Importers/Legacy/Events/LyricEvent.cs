using Newtonsoft.Json;

namespace pTyping.Shared.Beatmaps.Importers.Legacy.Events;

[JsonObject(MemberSerialization.OptIn)]
internal class LyricEvent : LegacyEvent {
    [JsonProperty]
    public override LegacyEventType Type => LegacyEventType.Lyric;

    [JsonProperty]
    public double EndTime { get; set; } = double.PositiveInfinity;

    [JsonProperty]
    public string Lyric { get; set; } = "";

}
