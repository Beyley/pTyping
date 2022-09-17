using System.IO;
using Furball.Engine;
using pTyping.Shared.Beatmaps.Importers;

namespace pTyping;

public static class LegacyImportChecker {
    private static string LegacySongFolder               => Path.Combine(FurballGame.AssemblyPath, "songs/");
    private static string LegacySongFolderImported       => Path.Combine(FurballGame.AssemblyPath, "songs-imported/");
    private static string LegacySongFolderImportedFailed => Path.Combine(LegacySongFolderImported, "failed/");

    public static void CheckAndImportLegacyMaps() {
        if (!Directory.Exists(LegacySongFolder))
            return;

        Directory.CreateDirectory(LegacySongFolderImported);
        Directory.CreateDirectory(LegacySongFolderImportedFailed);

        pTypingGame.BeatmapDatabase.Realm.Write(
        () => {
            DirectoryInfo info = new(LegacySongFolder);

            DirectoryInfo[] legacySongDirs = info.GetDirectories();

            LegacyBeatmapImporter importer = new();

            foreach (DirectoryInfo legacySongDir in legacySongDirs)
                try {
                    //Attempt to import the beatmap
                    importer.ImportBeatmaps(pTypingGame.BeatmapDatabase, pTypingGame.FileDatabase, legacySongDir);

                    legacySongDir.MoveTo(Path.Combine(LegacySongFolderImported, legacySongDir.Name));
                }
                catch {
                    // TODO: notify the user of the failed maps

                    legacySongDir.MoveTo(Path.Combine(LegacySongFolderImportedFailed, legacySongDir.Name));
                }
        }
        );

        //After we are done importing, just refresh it to disk
        pTypingGame.BeatmapDatabase.Realm.RefreshAsync();
    }
}
