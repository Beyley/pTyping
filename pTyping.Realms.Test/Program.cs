using pTyping.Shared;
using pTyping.Shared.Beatmaps;
using pTyping.Shared.Beatmaps.Importers;

public static class Program {
    public static void Main(string[] args) {
        BeatmapDatabase database     = new();
        FileDatabase    fileDatabase = new();

        database.Realm.Write(
        () => {
            IBeatmapImporter importer = new UTypingBeatmapImporter();
            importer.ImportBeatmaps(database, fileDatabase, new DirectoryInfo("concyclic"));
            importer = new LegacyBeatmapImporter();
            importer.ImportBeatmaps(database, fileDatabase, new DirectoryInfo("legacymap"));
        }
        );

        IQueryable<Beatmap> queryable = database.Realm.All<Beatmap>();

        foreach (Beatmap beatmap in queryable)
            if (beatmap.FileCollection.Audio != null) {
                byte[] arr = fileDatabase.GetFile(beatmap.FileCollection.Audio.Hash);

                Console.WriteLine(arr.Length);
            }
    }
}
