using Realms;

namespace pTyping.Shared.Beatmaps;

public class BeatmapSet : RealmObject {
    public IList<Beatmap> Beatmaps { get; }
}
