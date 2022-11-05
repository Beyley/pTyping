using Furball.Engine;
using Kettu;
using pTyping.Shared.Beatmaps;
using pTyping.Shared.Beatmaps.HitObjects;
using pTyping.Shared.Difficulty;
using pTyping.Shared.Events;
using pTyping.Shared.ObjectModel;
using Realms;
using Realms.Schema;

namespace pTyping.Shared;

public class BeatmapDatabase {
	public readonly Realm Realm;

	public const ulong SCHEMA_VERSION = 4;

	public BeatmapDatabase(string dataFolder) {
		RealmSchema.Builder builder = new RealmSchema.Builder {
			typeof(BeatmapSet),
			typeof(Beatmap),
			typeof(Break),
			typeof(BeatmapDifficulty),
			typeof(BeatmapFileCollection),
			typeof(BeatmapInfo),
			typeof(BeatmapMetadata),
			typeof(DatabaseUser),
			typeof(PathHashTuple),
			typeof(TimingPoint),
			typeof(HitObject),
			typeof(Event),
			typeof(AsciiUnicodeTuple),
			typeof(HitObjectColor),
			typeof(HitObjectSettings),
			typeof(CalculatedMapDifficulty),
			typeof(DifficultySection)
		};

		RealmConfiguration config = new RealmConfiguration(Path.Combine(dataFolder, "songs.db")) {
			Schema            = builder.Build(),
			SchemaVersion     = SCHEMA_VERSION,
			MigrationCallback = this.Migrate
		};

		this.Realm = Realm.GetInstance(config);
	}

	public void TriggerDifficultyRecalculation(Beatmap beatmap) {
		string id = beatmap.Id;

		//Get a working copy of the beatmap
		Beatmap working = beatmap.Clone();

		_ = Task.Factory.StartNew(() => {
			Logger.Log($"Calculating difficulty of map {id}", LoggerLevelDifficultyCalculation.Instance);

			//Calculate the difficulty
			CalculatedMapDifficulty calculatedDifficulty = new DifficultyCalculator(working).Calculate();

			Logger.Log($"Difficulty of map {id} is {calculatedDifficulty.OverallDifficulty}", LoggerLevelDifficultyCalculation.Instance);

			//Get a new beatmap instance from the database
			BeatmapDatabase beatmapDatabase = new BeatmapDatabase(FurballGame.DataFolder);
			Beatmap         toSet           = beatmapDatabase.Realm.Find<Beatmap>(id);

			beatmapDatabase.Realm.Write(() => {
				//Set the beatmap instance difficulty
				toSet.CalculatedDifficulty = calculatedDifficulty;
			});
			
			//Refresh the database to make sure other threads get the update
			beatmapDatabase.Realm.Refresh();
		});
	}

	private void Migrate(Migration migration, ulong oldSchemaVersion) {
		List<dynamic>          oldSets = migration.OldRealm.DynamicApi.All("BeatmapSet").ToList();
		IQueryable<BeatmapSet> newSets = migration.NewRealm.All<BeatmapSet>();

		for (int i = 0; i < newSets.Count(); i++) {
			dynamic    oldSet = oldSets.ElementAt(i);
			BeatmapSet newSet = newSets.ElementAt(i);

			//If the old version is less than 1, than migrate to proper backlinks
			if (oldSchemaVersion < 1) {
				//This is blank because nothing needs to be done here, this is just here to keep track of the change
			}

			//In version 2, we added typing conversion to `HitObject`, so we need to set a default value for all of them
			if (oldSchemaVersion < 2)
				foreach (Beatmap newSetBeatmap in newSet.Beatmaps) {
					foreach (HitObject hitObject in newSetBeatmap.HitObjects)
						hitObject.TypingConversion = TypingConversions.ConversionType.StandardHiragana;
				}

			//In version 3, we changed Beatmap.Info.Mapper from a string to a DatabaseUser
			if (oldSchemaVersion < 3)
				for (int j = 0; j < newSet.Beatmaps.Count; j++) {
					//The new beatmap
					Beatmap newSetBeatmap = newSet.Beatmaps[j];
					//The old beatmap
					dynamic oldSetBeatmap = oldSet.Beatmaps[j];

					//Set the mapper to a new DatabaseUser with the old mapper's name
					newSetBeatmap.Info.Mapper = new DatabaseUser(oldSetBeatmap.Info.Mapper);
				}

			//In version 4, we added map difficulty calculation
			if (oldSchemaVersion < 4)
				foreach (Beatmap newSetBeatmap in newSet.Beatmaps)
					newSetBeatmap.CalculatedDifficulty = null;
		}
	}
}
