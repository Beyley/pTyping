namespace pTyping.Shared.Scores.Exporters;

public interface IScoreExporter {
	public void ExportScore(Score score, BeatmapDatabase beatmapDatabase, ScoreDatabase scoreDatabase, FileDatabase fileDatabase);
}
