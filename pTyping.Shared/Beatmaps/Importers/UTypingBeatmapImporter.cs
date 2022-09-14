#nullable enable
using pTyping.Shared.Beatmaps.Importers.UTyping;

namespace pTyping.Shared.Beatmaps.Importers;

#pragma warning disable CS4014

public class UTypingBeatmapImporter : IBeatmapImporter {
    public void ImportBeatmaps(BeatmapDatabase database, FileDatabase fileDatabase, DirectoryInfo beatmapPath) {
        IEnumerable<FileInfo> files = beatmapPath.GetFiles("*.txt", SearchOption.AllDirectories).Where(x => !x.Name.EndsWith("_src.txt") && x.Name.StartsWith("info"));

        BeatmapSet set = new();
        foreach (FileInfo file in files) {
            Beatmap? map = UTypingSongParser.ParseUTypingBeatmap(file);

            //TODO: handle failed imports better, maybe add an error message of some sort and tell the user?
            if (map == null)
                continue;

            //TODO: this should never happen, what the fuck?
            if (string.IsNullOrEmpty(map.Id))
                continue;

            if (map.FileCollection.Audio == null)
                continue;

            string audioPath = map.FileCollection.Audio.Path;
            fileDatabase.AddFile(File.ReadAllBytes(Path.Combine(beatmapPath.FullName, audioPath)));

            set.Beatmaps.Add(map);
        }

        database.Realm.Add(set);
    }
}
