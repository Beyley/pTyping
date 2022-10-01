namespace pTyping.Shared.Beatmaps.Exporters;

/// <summary>
///     Used for exporting beatmaps to zip files
/// </summary>
public interface IBeatmapExporter {
	/// <summary>
	///     Exports the beatmap set as a zip file to the specified stream
	/// </summary>
	/// <param name="set">The beatmap set to export</param>
	/// <param name="output">The output stream to write the zip file to</param>
	/// <param name="fileDatabase"></param>
	public void ExportBeatmapSet(BeatmapSet set, Stream output, FileDatabase fileDatabase);
}
