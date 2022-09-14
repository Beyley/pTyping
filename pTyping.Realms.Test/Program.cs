using pTyping.Shared;
using pTyping.Shared.Beatmaps;
using pTyping.Shared.Beatmaps.Importers;
using Realms;

public static class Program {
    public static void Main(string[] args) {
        BeatmapDatabase database = new();

        UTypingBeatmapImporter importer = new();

        Transaction transaction = database.Realm.BeginWrite();

        try {
            importer.ImportBeatmaps(database, new DirectoryInfo("concyclic"));

            transaction.Commit();
        }
        catch {
            transaction.Rollback();
        } finally {
            transaction.Dispose();
        }

        IQueryable<Beatmap> queryable = database.Realm.All<Beatmap>();

        foreach (Beatmap beatmap in queryable)
            Console.WriteLine(beatmap.Info.Artist);
    }
}
