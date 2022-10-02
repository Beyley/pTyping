using Furball.Engine.Engine.Platform;
using Newtonsoft.Json;
using pTyping.Shared.Beatmaps;

namespace pTyping.Shared.Scores.Exporters;

public class pTypingScoreExporter : IScoreExporter {
	public void ExportScore(Score score, BeatmapDatabase beatmapDatabase, ScoreDatabase scoreDatabase, FileDatabase fileDatabase) {
		//The serialized version of the score
		string scoreJson = JsonConvert.SerializeObject(score, RuntimeInfo.IsDebug() ? Formatting.Indented : Formatting.None);

		//The export directory
		string exportPath = Path.Combine(AppContext.BaseDirectory, "exports");

		//Create the export directory if it does not exist
		if (!Directory.Exists(exportPath))
			Directory.CreateDirectory(exportPath);

		//Get the beatmap and set info
		Beatmap    beatmap = beatmapDatabase.Realm.Find<Beatmap>(score.BeatmapId);
		BeatmapSet set     = beatmap.Parent.First();

		//Write the score data to a 'pts' file in the exports folder
		File.WriteAllText(Path.Combine(exportPath, $"{score.User.Username} - {set.Artist}-{set.Title}.pts"), scoreJson);
	}
}
