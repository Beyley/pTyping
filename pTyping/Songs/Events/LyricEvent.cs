using Newtonsoft.Json;

namespace pTyping.Songs.Events {
    [JsonObject(MemberSerialization.OptIn)]
    public class LyricEvent : Event {
        [JsonProperty]
        public override EventType Type => EventType.Lyric;

        [JsonProperty]
        public string Lyric { get; set; } = "";
    }
}
