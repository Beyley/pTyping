using System.ComponentModel;
using pTyping.Shared.ObjectModel;
using Realms;

namespace pTyping.Shared.Beatmaps;

public class BeatmapMetadata : EmbeddedObject, IClonable<BeatmapMetadata> {
	public IList<int> BackingLanguages { get; }
	[Ignored, Description("The languages contained within the song")]
	public IList<SongLanguage> Languages {
		get {
			List<SongLanguage> list = new();

			foreach (uint u in this.BackingLanguages)
				list.Add((SongLanguage)u);

			return list;
		}
		set {
			this.BackingLanguages.Clear();

			foreach (SongLanguage language in value)
				this.BackingLanguages.Add((int)language);
		}
	}
	[Description("The tags given for the song by the beatmap creator.")]
	public IList<string> Tags { get; }

	public BeatmapMetadata Clone() {
		BeatmapMetadata metadata = new() {
			Languages = this.Languages
		};

		foreach (string tag in this.Tags)
			metadata.Tags.Add(tag);

		return metadata;
	}
}
