using Newtonsoft.Json;

namespace pTyping.Songs.Events {
    [JsonObject(MemberSerialization.OptIn)]
    public class TypingCutoffEvent : Event {
        [JsonProperty]
        public override EventType Type => EventType.TypingCutoff;
    }
}
