using Realms;

namespace pTyping.Shared.Beatmaps;

#nullable enable

public class BeatmapFileCollection : RealmObject {
    public PathHashTuple? Audio           { get; set; }
    public PathHashTuple? Background      { get; set; }
    public PathHashTuple? BackgroundVideo { get; set; }
}
