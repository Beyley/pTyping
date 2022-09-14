using Newtonsoft.Json;

namespace pTyping.Shared.Beatmaps.Importers.Legacy;

[JsonObject(MemberSerialization.OptIn)]
public abstract class LegacyEvent {
    [JsonProperty]
    public abstract LegacyEventType Type { get; }
    [JsonProperty]
    public double Time { get; set; }
}

public enum LegacyEventType {
    Lyric,
    TypingCutoff,
    BeatLineBar,
    BeatLineBeat
}
