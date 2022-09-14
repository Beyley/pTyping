using pTyping.Shared.Beatmaps.Importers.UTyping;

namespace pTyping.Shared.Beatmaps.Importers;

public class UTypingBeatmapImporter : IBeatmapImporter {
    public void ImportBeatmaps(BeatmapDatabase database, DirectoryInfo beatmapPath) {
        IEnumerable<FileInfo> files = beatmapPath.GetFiles("*.txt", SearchOption.AllDirectories).Where(x => !x.Name.EndsWith("_src.txt") && x.Name.StartsWith("info"));

        List<Beatmap> maps = new();
        foreach (FileInfo file in files) {
            Beatmap? map = UTypingSongParser.ParseUTypingBeatmap(file);

            //TODO: handle failed imports better, maybe add an error message of some sort and tell the user?
            if (map == null)
                continue;

            maps.Add(map);
        }

        BeatmapSet set = new() {
            //Put all the collected maps into the beatmap set
            Beatmaps = maps
        };
        //Add the BeatmapSet into the realm
        database.Realm.Add(set);
    }
}
