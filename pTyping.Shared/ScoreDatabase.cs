using pTyping.Shared.ObjectModel;
using pTyping.Shared.Scores;
using Realms;
using Realms.Schema;

namespace pTyping.Shared;

public class ScoreDatabase {
	public Realm Realm;

	private const ulong DATABASE_VERSION = 1;
	
	public ScoreDatabase(string dataFolder) {
		RealmSchema.Builder builder = new RealmSchema.Builder {
			typeof(Score),
			typeof(DatabaseUser),
			typeof(ReplayFrame)
		};

		RealmConfiguration config = new RealmConfiguration(Path.Combine(dataFolder, "scores.db")) {
			Schema            = builder.Build(),
			SchemaVersion     = DATABASE_VERSION,
			MigrationCallback = this.MigrationCallback
		};

		this.Realm = Realm.GetInstance(config);
	}

	private void MigrationCallback(Migration migration, ulong oldschemaversion) {
		List<dynamic>     oldScores = migration.OldRealm.DynamicApi.All("Score").ToList();
		IQueryable<Score> newScores = migration.NewRealm.All<Score>();

		for (int i = 0; i < newScores.Count(); i++) {
			dynamic oldScore = oldScores.ElementAt(i);
			Score   newScore = newScores.ElementAt(i);

			//In version 1, we added a bool to the user determining if the user originates from online or not.
			if (oldschemaversion < 1)
				newScore.User.Online = false;
		}
	}
}
