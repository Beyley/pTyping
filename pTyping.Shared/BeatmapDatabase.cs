using pTyping.Shared.Beatmaps;
using pTyping.Shared.Beatmaps.HitObjects;
using pTyping.Shared.Events;
using Realms;
using Realms.Schema;

namespace pTyping.Shared;

public class BeatmapDatabase {
    public readonly Realm Realm;

    public BeatmapDatabase() {
        RealmSchema.Builder builder = new() {
            typeof(BeatmapSet),
            typeof(Beatmap),
            typeof(Break),
            typeof(BeatmapDifficulty),
            typeof(BeatmapFileCollection),
            typeof(BeatmapInfo),
            typeof(BeatmapMetadata),
            typeof(PathHashTuple),
            typeof(TimingPoint),
            typeof(HitObject),
            typeof(Event),
            typeof(AsciiUnicodeTuple),
            typeof(HitObjectColor)
        };

        RealmConfiguration config = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "songs.db")) {
            Schema = builder.Build()
        };

        this.Realm = Realm.GetInstance(config);
    }
}
