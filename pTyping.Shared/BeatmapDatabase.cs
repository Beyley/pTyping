using pTyping.Shared.Beatmaps;
using Realms;
using Realms.Schema;

namespace pTyping.Shared;

public class BeatmapDatabase {
    public readonly Realm Realm;

    public BeatmapDatabase() {
        RealmSchema.Builder builder = new();
        ObjectSchema.Builder oSchema = new(typeof(BeatmapSet)) {
            Name = "BeatmapSet"
        };

        builder.Add(oSchema);

        RealmConfiguration config = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "songs.db")) {
            Schema = builder.Build()
        };

        this.Realm = Realm.GetInstance(config);
    }
}
