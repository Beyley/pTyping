using Furball.Vixie.Backends.Shared;
using Newtonsoft.Json;

namespace pTyping.Shared.Beatmaps.Importers.Legacy;

internal enum LegacyNoteType {
    UTyping
}

internal class LegacyNote {
    [JsonProperty]
    public Color Color = Color.Red;

    [JsonProperty]
    public LegacyNoteType Type = LegacyNoteType.UTyping;

    [JsonProperty]
    public string Text;
    [JsonProperty]
    public double Time;
}
