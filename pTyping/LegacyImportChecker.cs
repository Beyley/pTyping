using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Furball.Engine;
using pTyping.Engine;
using pTyping.Shared;
using pTyping.Shared.Beatmaps.Importers;

namespace pTyping;

public static class LegacyImportChecker {
	private static string LegacySongFolder               => Path.Combine(FurballGame.AssemblyPath, "songs/");
	private static string LegacySongFolderImported       => Path.Combine(FurballGame.AssemblyPath, "songs-imported/");
	private static string LegacySongFolderImportedFailed => Path.Combine(LegacySongFolderImported, "failed/");

	public static void CheckAndImportLegacyMaps() {
		Task.Factory.StartNew(ImportLegacyMaps);
	}

	private static void ImportLegacyMaps() {
		if (!Directory.Exists(LegacySongFolder))
			return;

		Directory.CreateDirectory(LegacySongFolderImported);
		Directory.CreateDirectory(LegacySongFolderImportedFailed);

		BeatmapDatabase database      = new BeatmapDatabase();
		ScoreDatabase   scoreDatabase = new ScoreDatabase();

		database.Realm.Write(
			() => {
				DirectoryInfo info = new DirectoryInfo(LegacySongFolder);

				DirectoryInfo[] legacySongDirs = info.GetDirectories();

				if (legacySongDirs.Length != 0)
					FurballGame.GameTimeScheduler.ScheduleMethod(
						_ => {
							pTypingGame.NotificationManager.CreateNotification(
								NotificationManager.NotificationImportance.Info,
								$"Importing {legacySongDirs.Length} beatmaps..."
							);
						}
					);

				LegacyBeatmapImporter       legacyImporter       = new LegacyBeatmapImporter();
				UTypingBeatmapScoreImporter uTypingScoreImporter = new UTypingBeatmapScoreImporter();

				foreach (DirectoryInfo legacySongDir in legacySongDirs)
					try {
						if (legacySongDir.GetFiles().Any(x => x.Name.Contains("info.txt")))
							uTypingScoreImporter.ImportBeatmaps(database, scoreDatabase, pTypingGame.FileDatabase, legacySongDir);
						else
							//Attempt to import the beatmap
							legacyImporter.ImportBeatmaps(database, scoreDatabase, pTypingGame.FileDatabase, legacySongDir);

						try {
							legacySongDir.MoveTo(Path.Combine(LegacySongFolderImported, legacySongDir.Name));
						}
						catch { /* do nothing :^) */
						}
					}
					catch (Exception ex) {
						legacySongDir.MoveTo(Path.Combine(LegacySongFolderImportedFailed, legacySongDir.Name));

						FurballGame.GameTimeScheduler.ScheduleMethod(
							_ => {
								pTypingGame.NotificationManager.CreateNotification(
									NotificationManager.NotificationImportance.Error,
									$"Failed to import {legacySongDir.Name}! Reason: {ex.GetType()}"
								);
							}
						);
					}
			}
		);

		//Refresh at the end to make sure it notifies the other threads
		database.Realm.Refresh();
		scoreDatabase.Realm.Refresh();

		FurballGame.GameTimeScheduler.ScheduleMethod(
			_ => {
				pTypingGame.BeatmapDatabase.Realm.Refresh();
				pTypingGame.ScoreDatabase.Realm.Refresh();
			}
		);
	}
}
