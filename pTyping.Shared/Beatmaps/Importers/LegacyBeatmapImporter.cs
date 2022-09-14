#nullable enable
using pTyping.Shared.Beatmaps.Importers.Legacy;

namespace pTyping.Shared.Beatmaps.Importers;

#pragma warning disable CS4014

public class LegacyBeatmapImporter : IBeatmapImporter {
    public void ImportBeatmaps(BeatmapDatabase database, FileDatabase fileDatabase, DirectoryInfo beatmapPath) {
        FileInfo[] files = beatmapPath.GetFiles("*.pts");

        BeatmapSet set = new();
        foreach (FileInfo file in files) {
            Beatmap? map = LegacySongParser.ParseLegacySong(file);

            if (map == null)
                continue;

            if (map.FileCollection.Audio == null)
                continue;

            string audioPath = map.FileCollection.Audio.Path;
            fileDatabase.AddFile(File.ReadAllBytes(Path.Combine(beatmapPath.FullName, audioPath)));

            if (map.FileCollection.Background != null) {
                string backgrondPath = map.FileCollection.Background.Path;
                fileDatabase.AddFile(File.ReadAllBytes(Path.Combine(beatmapPath.FullName, backgrondPath)));
            }

            if (map.FileCollection.BackgroundVideo != null) {
                string videoPath = map.FileCollection.BackgroundVideo.Path;
                fileDatabase.AddFile(File.ReadAllBytes(Path.Combine(beatmapPath.FullName, videoPath)));
            }

            set.Beatmaps.Add(map);
        }

        database.Realm.Add(set);
    }
}
