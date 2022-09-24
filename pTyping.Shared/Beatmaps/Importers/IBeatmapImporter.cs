namespace pTyping.Shared.Beatmaps.Importers;

public interface IBeatmapImporter {
	public void ImportBeatmaps(BeatmapDatabase database, FileDatabase fileDatabase, DirectoryInfo beatmapPath);
}
