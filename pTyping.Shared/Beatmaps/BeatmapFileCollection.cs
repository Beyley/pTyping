using Realms;

namespace pTyping.Shared.Beatmaps;

public class BeatmapFileCollection : RealmObject {
    public PathHashTuple AudioHash;
    public PathHashTuple BackgroundHash;
}
