using Newtonsoft.Json;

namespace pTyping.Songs.Events;

[JsonObject(MemberSerialization.OptIn)]
public class LyricEvent : Event {
    [JsonProperty]
    public override EventType Type => EventType.Lyric;

    [JsonProperty]
    public double EndTime { get; set; } = double.PositiveInfinity;
    
    [JsonProperty]
    public string Lyric { get; set; } = "";
    
}