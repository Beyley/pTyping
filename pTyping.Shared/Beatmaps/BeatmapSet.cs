using Realms;

namespace pTyping.Shared.Beatmaps;

public class BeatmapSet : RealmObject {
    public IReadOnlyList<Beatmap> Beatmaps;
}
