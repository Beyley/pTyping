using pTyping.Shared.ObjectModel;
using pTyping.Shared.Scores;
using Realms;
using Realms.Schema;

namespace pTyping.Shared;

public class ScoreDatabase {
	public Realm Realm;

	public ScoreDatabase(string dataFolder) {
		RealmSchema.Builder builder = new RealmSchema.Builder {
			typeof(Score),
			typeof(DatabaseUser),
			typeof(ReplayFrame)
		};

		RealmConfiguration config = new RealmConfiguration(Path.Combine(dataFolder, "scores.db")) {
			Schema = builder.Build()
		};

		this.Realm = Realm.GetInstance(config);
	}
}
