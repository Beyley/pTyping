using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Furball.Engine;
using pTyping.Shared;
using pTyping.Shared.Beatmaps.Importers;

namespace pTyping;

public static class LegacyImportChecker {
    private static string LegacySongFolder               => Path.Combine(FurballGame.AssemblyPath, "songs/");
    private static string LegacySongFolderImported       => Path.Combine(FurballGame.AssemblyPath, "songs-imported/");
    private static string LegacySongFolderImportedFailed => Path.Combine(LegacySongFolderImported, "failed/");

    public static void CheckAndImportLegacyMaps() {
        Task.Factory.StartNew(
        () => {
            if (!Directory.Exists(LegacySongFolder))
                return;

            Directory.CreateDirectory(LegacySongFolderImported);
            Directory.CreateDirectory(LegacySongFolderImportedFailed);

            BeatmapDatabase database = new();

            database.Realm.Write(
            () => {
                DirectoryInfo info = new(LegacySongFolder);

                DirectoryInfo[] legacySongDirs = info.GetDirectories();

                LegacyBeatmapImporter  legacyImporter  = new();
                UTypingBeatmapImporter uTypingImporter = new();

                foreach (DirectoryInfo legacySongDir in legacySongDirs)
                    try {
                        if (legacySongDir.GetFiles().Any(x => x.Name.Contains("info.txt")))
                            uTypingImporter.ImportBeatmaps(database, pTypingGame.FileDatabase, legacySongDir);
                        else
                            //Attempt to import the beatmap
                            legacyImporter.ImportBeatmaps(database, pTypingGame.FileDatabase, legacySongDir);

                        legacySongDir.MoveTo(Path.Combine(LegacySongFolderImported, legacySongDir.Name));
                    }
                    catch {
                        // TODO: notify the user of the failed maps

                        legacySongDir.MoveTo(Path.Combine(LegacySongFolderImportedFailed, legacySongDir.Name));
                    }
            }
            );

            //Refresh at the end to make sure it notifies the other threads
            database.Realm.Refresh();

            FurballGame.GameTimeScheduler.ScheduleMethod(
            _ => {
                pTypingGame.BeatmapDatabase.Realm.Refresh();
            }
            );
        }
        );
    }
}
