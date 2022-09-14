using Realms;

namespace pTyping.Shared.Beatmaps;

public class BeatmapMetadata : RealmObject {
    public IReadOnlyList<SongLanguage> Languages;
    public IReadOnlyList<string>       Tags;

    public BeatmapMetadata Clone() => (BeatmapMetadata)this.MemberwiseClone();
}
