using Newtonsoft.Json;

namespace pTyping.Songs.Events {
    [JsonObject(MemberSerialization.OptIn)]
    public class RestEvent : Event {
        [JsonProperty]
        public override EventType Type => EventType.Rest;
    }
}
