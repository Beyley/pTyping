using Realms;

namespace pTyping.Shared.Beatmaps;

public class BeatmapDifficulty : RealmObject {
    public float Strictness = 5;

    public BeatmapDifficulty Clone() => (BeatmapDifficulty)this.MemberwiseClone();
}
