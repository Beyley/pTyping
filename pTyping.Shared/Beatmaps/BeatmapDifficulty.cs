using Realms;

namespace pTyping.Shared.Beatmaps;

public class BeatmapDifficulty : RealmObject {
    public float Strictness { get; set; } = 5;

    public BeatmapDifficulty Clone() => (BeatmapDifficulty)this.MemberwiseClone();
}
