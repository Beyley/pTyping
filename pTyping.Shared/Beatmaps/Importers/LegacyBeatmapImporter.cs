namespace pTyping.Shared.Beatmaps.Importers;

public class LegacyBeatmapImporter : IBeatmapImporter {
    public void ImportBeatmaps(BeatmapDatabase database, FileDatabase fileDatabase, DirectoryInfo beatmapPath) {
        FileInfo[] files = beatmapPath.GetFiles("*.pts");

        foreach (FileInfo file in files) {}
    }
}
