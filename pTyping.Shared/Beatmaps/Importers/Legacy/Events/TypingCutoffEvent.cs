using Newtonsoft.Json;

namespace pTyping.Shared.Beatmaps.Importers.Legacy.Events;

[JsonObject(MemberSerialization.OptIn)]
internal class TypingCutoffEvent : LegacyEvent {
    [JsonProperty]
    public override LegacyEventType Type => LegacyEventType.TypingCutoff;
}
