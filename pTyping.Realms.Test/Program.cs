using pTyping.Shared;
using pTyping.Shared.Beatmaps;
using pTyping.Shared.Beatmaps.Importers;

public static class Program {
    public static void Main(string[] args) {
        BeatmapDatabase database     = new();
        FileDatabase    fileDatabase = new();

        UTypingBeatmapImporter importer = new();

        importer.ImportBeatmaps(database, fileDatabase, new DirectoryInfo("concyclic"));

        IQueryable<Beatmap> queryable = database.Realm.All<Beatmap>();

        foreach (Beatmap beatmap in queryable)
            if (beatmap.FileCollection.Audio != null) {
                Task<byte[]> task = fileDatabase.GetFile(beatmap.FileCollection.Audio.Hash);
                task.Wait();
                byte[] arr = task.Result;

                Console.WriteLine(arr.Length);
            }
    }
}
