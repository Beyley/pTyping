using System.Text;
using Furball.Engine.Engine.Platform;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;

namespace pTyping.Shared.Beatmaps.Exporters;

// ReSharper disable once InconsistentNaming
public class pTypingBeatmapExporter : IBeatmapExporter {
	public void ExportBeatmapSet(BeatmapSet set, Stream output, FileDatabase fileDatabase) {
		string setJson = JsonConvert.SerializeObject(set, RuntimeInfo.IsDebug() ? Formatting.Indented : Formatting.None, new JsonSerializerSettings());

		//The temporary folder used to store the beatmap contents to be zipped
		string tempFolderPath = Path.Combine(Path.GetTempPath(), $"{set.Id.ToString()}/");

		if (Directory.Exists(tempFolderPath))
			throw new Exception($"Temporary folder {tempFolderPath} already exists! Did an import get aborted in the middle?");

		//Create the temp folder we will zip up at the end
		Directory.CreateDirectory(tempFolderPath);

		//Write the song to the folder
		File.WriteAllText(Path.Combine(tempFolderPath, "song"), setJson, Encoding.UTF8);

		string filesDir = Path.Combine(tempFolderPath, "files/");

		//Create the files directory
		Directory.CreateDirectory(filesDir);
		
		//Write the files the beatmaps need to the folder
		foreach (Beatmap beatmap in set.Beatmaps) {
			if (beatmap.FileCollection.Audio != null)
				File.WriteAllBytes(Path.Combine(filesDir, beatmap.FileCollection.Audio.Hash), fileDatabase.GetFile(beatmap.FileCollection.Audio.Hash));
			if (beatmap.FileCollection.Background != null)
				File.WriteAllBytes(Path.Combine(filesDir, beatmap.FileCollection.Background.Hash), fileDatabase.GetFile(beatmap.FileCollection.Background.Hash));
			if (beatmap.FileCollection.BackgroundVideo != null)
				File.WriteAllBytes(Path.Combine(filesDir, beatmap.FileCollection.BackgroundVideo.Hash), fileDatabase.GetFile(beatmap.FileCollection.BackgroundVideo.Hash));
		}

		FastZip z = new FastZip();

		//Create the zip file
		z.CreateZip(output, tempFolderPath, true, "", "");

		//Delete the temporary directory once we are done
		Directory.Delete(tempFolderPath, true);
	}
}
