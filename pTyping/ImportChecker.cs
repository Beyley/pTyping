#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Furball.Engine;
using Furball.Vixie.Helpers;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using pTyping.Engine;
using pTyping.Shared;
using pTyping.Shared.Beatmaps;
using pTyping.Shared.Beatmaps.Importers;
using pTyping.Shared.Scores;

namespace pTyping;

public static class ImportChecker {
	private static string LegacySongFolder               => Path.Combine(FurballGame.DataFolder, "songs/");
	private static string LegacySongFolderImported       => Path.Combine(FurballGame.DataFolder, "songs-imported/");
	private static string LegacySongFolderImportedFailed => Path.Combine(LegacySongFolderImported, "failed/");

	public static void ImportMaps() {
		Task.Factory.StartNew(ImportMapsAndScoresRun);
		Task.Factory.StartNew(ImportLegacyMapsRun);
	}

	private static void ImportMapsAndScoresRun() {
		BeatmapDatabase database      = new BeatmapDatabase(FurballGame.DataFolder);
		ScoreDatabase   scoreDatabase = new ScoreDatabase(FurballGame.DataFolder);
		FileDatabase    fileDatabase  = new FileDatabase(FurballGame.DataFolder);

		DirectoryInfo info = new DirectoryInfo(Path.Combine(FurballGame.DataFolder, "songs"));

		if (!info.Exists)
			info.Create();

		string tempPath = Path.GetTempPath();

		FileInfo[] foundArchives = info.GetFiles("*.ptm", SearchOption.AllDirectories);

		FastZip z = new FastZip();
		int importedCount = database.Realm.Write(() => {
			int importedMaps = 0;

			foreach (FileInfo archive in foundArchives) {
				string tempExtractPath = Path.Combine(tempPath, archive.Name);

				if (Directory.Exists(tempExtractPath))
					Directory.Delete(tempExtractPath, true);

				Directory.CreateDirectory(tempExtractPath);

				//Extract the map archive to the temp dir
				z.ExtractZip(archive.FullName, tempExtractPath, "");

				string fullSongPath = Path.Combine(tempExtractPath, "song");

				//Make sure the song file exists
				if (!File.Exists(fullSongPath))
					continue; //TODO: notify the user something went wrong with this archive

				//Deserialize the map from the JSON
				BeatmapSet? set = JsonConvert.DeserializeObject<BeatmapSet>(File.ReadAllText(fullSongPath));

				//Check if it deserialized correctly
				if (set == null)
					continue; //TODO: notify user it failed to parse the song

				FileInfo[] fileDependencies = new DirectoryInfo(Path.Combine(tempExtractPath, "files")).GetFiles();

				foreach (FileInfo fileDependency in fileDependencies) {
					Guard.Assert(File.Exists(fileDependency.FullName), "File.Exists(fileDepedency.FullName)");

					//Add the file asyncronously, dont block here
					_ = fileDatabase.AddFile(File.ReadAllBytes(fileDependency.FullName));
				}

				//Add the set to the realm, or overwrite an existing map with the same ID
				database.Realm.Add(set, true);

				//Indicate that we have imported another map successfully
				importedMaps++;
			}

			return importedMaps;
		});

		int importedScores = scoreDatabase.Realm.Write(() => {
			int imported = 0;

			info = new DirectoryInfo(Path.Combine(FurballGame.DataFolder, "scores"));

			if (!info.Exists)
				info.Create();

			FileInfo[] scoreFiles = info.GetFiles("*.pts");
			foreach (FileInfo scoreFile in scoreFiles) {
				Score? score = JsonConvert.DeserializeObject<Score>(File.ReadAllText(scoreFile.FullName));

				if (score == null)
					continue; //TODO: tell the user something went wrong

				scoreDatabase.Realm.Add(score);
			}

			return imported;
		});

		database.Realm.Refresh();
		scoreDatabase.Realm.Refresh();

		if (importedCount != 0)
			FurballGame.GameTimeScheduler.ScheduleMethod(_ => {
				pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Info, $"Imported {importedCount} beatmap archive{(importedCount == 1 ? "" : "s")}!");
				pTypingGame.BeatmapDatabase.Realm.Refresh();
			});
		if (importedScores != 0)
			FurballGame.GameTimeScheduler.ScheduleMethod(_ => {
				pTypingGame.NotificationManager.CreateNotification(NotificationManager.NotificationImportance.Info, $"Imported {importedScores} score{(importedScores == 1 ? "" : "s")}!");
				pTypingGame.ScoreDatabase.Realm.Refresh();
			});
	}

	private static void ImportLegacyMapsRun() {
		if (!Directory.Exists(LegacySongFolder))
			return;

		Directory.CreateDirectory(LegacySongFolderImported);
		Directory.CreateDirectory(LegacySongFolderImportedFailed);

		BeatmapDatabase database      = new BeatmapDatabase(FurballGame.DataFolder);
		ScoreDatabase   scoreDatabase = new ScoreDatabase(FurballGame.DataFolder);

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
