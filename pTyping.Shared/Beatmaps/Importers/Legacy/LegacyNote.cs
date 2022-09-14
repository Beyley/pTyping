using Furball.Vixie.Backends.Shared;
using Newtonsoft.Json;

namespace pTyping.Shared.Beatmaps.Importers.Legacy;

public enum NoteType {
    UTyping
}

public class LegacyNote {
    [JsonProperty]
    public Color Color = Color.Red;

    [JsonProperty]
    public NoteType Type = NoteType.UTyping;

    [JsonProperty]
    public string Text;
    [JsonProperty]
    public double Time;
}
