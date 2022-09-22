using pTyping.Shared.ObjectModel;
using pTyping.Shared.Scores;
using Realms;
using Realms.Schema;

namespace pTyping.Shared;

public class ScoreDatabase {
    public Realm Realm;

    public ScoreDatabase() {
        RealmSchema.Builder builder = new() {
            typeof(Score),
            typeof(DatabaseUser),
            typeof(ReplayFrame)
        };

        RealmConfiguration config = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scores.db")) {
            Schema = builder.Build()
        };

        this.Realm = Realm.GetInstance(config);
    }
}
