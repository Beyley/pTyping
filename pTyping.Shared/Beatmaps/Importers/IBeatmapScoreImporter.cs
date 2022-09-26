namespace pTyping.Shared.Beatmaps.Importers;

public interface IBeatmapScoreImporter {
	public void ImportBeatmaps(BeatmapDatabase database, ScoreDatabase scoreDatabase, FileDatabase fileDatabase, DirectoryInfo beatmapPath);
}
