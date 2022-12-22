#nullable enable
using pTyping.Shared.Beatmaps.Importers.UTyping;

namespace pTyping.Shared.Beatmaps.Importers;

#pragma warning disable CS4014

public class UTypingBeatmapScoreImporter : IBeatmapScoreImporter {
	public void ImportBeatmaps(BeatmapDatabase database, ScoreDatabase scoreDatabase, FileDatabase fileDatabase, DirectoryInfo beatmapPath) {
		IEnumerable<FileInfo> files = beatmapPath.GetFiles("*.txt", SearchOption.AllDirectories).Where(x => !x.Name.EndsWith("_src.txt") && x.Name.StartsWith("info"));

		BeatmapSet set = new BeatmapSet();
		foreach (FileInfo file in files) {
			Beatmap? map = UTypingSongParser.ParseUTypingBeatmapAndScores(file, out AsciiUnicodeTuple artist, out string source, out AsciiUnicodeTuple title, scoreDatabase);
			set.Artist = artist;
			set.Source = source;
			set.Title  = title;

			//TODO: handle failed imports better, maybe add an error message of some sort and tell the user?
			if (map == null)
				continue;

			// map.Parent = set;

			//TODO: this should never happen, what the fuck?
			if (string.IsNullOrEmpty(map.Id))
				continue;

			if (map.FileCollection.Audio == null)
				continue;

			string audioPath = map.FileCollection.Audio.Path;
			fileDatabase.AddFile(File.ReadAllBytes(Path.Combine(beatmapPath.FullName, audioPath)));

			set.Beatmaps.Add(map);
		}

		//Do nothing if we found no beatmaps
		if (set.Beatmaps.Count == 0)
			return;

		database.Realm.Add(set, true);
	}
}
