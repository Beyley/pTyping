using pTyping.Shared;
using pTyping.Shared.Beatmaps;
using pTyping.Shared.Beatmaps.Importers;

public static class Program {
	public static void Main(string[] args) {
		BeatmapDatabase database      = new BeatmapDatabase();
		ScoreDatabase   scoreDatabase = new ScoreDatabase();
		FileDatabase    fileDatabase  = new FileDatabase();

		database.Realm.Write(
			() => {
				IBeatmapScoreImporter importer = new UTypingBeatmapScoreImporter();
				importer.ImportBeatmaps(database, scoreDatabase, fileDatabase, new DirectoryInfo("concyclic"));
				importer = new LegacyBeatmapImporter();
				importer.ImportBeatmaps(database, scoreDatabase, fileDatabase, new DirectoryInfo("legacymap"));
			}
		);

		IQueryable<BeatmapSet> queryable = database.Realm.All<BeatmapSet>();

		foreach (BeatmapSet set in queryable)
			foreach (Beatmap beatmap in set.Beatmaps)
				if (beatmap.FileCollection.Audio != null) {
					byte[] arr = fileDatabase.GetFile(beatmap.FileCollection.Audio.Hash);

					Console.WriteLine(arr.Length);
				}
	}
}
