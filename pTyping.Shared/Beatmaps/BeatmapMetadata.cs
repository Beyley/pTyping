using System.ComponentModel;
using Newtonsoft.Json;
using pTyping.Shared.ObjectModel;
using Realms;

namespace pTyping.Shared.Beatmaps;

[JsonObject(MemberSerialization.OptIn)]
public class BeatmapMetadata : EmbeddedObject, IClonable<BeatmapMetadata> {
	[JsonProperty]
	public IList<int> BackingLanguages { get; }
	[Ignored, Description("The languages contained within the song"), JsonIgnore]
	public IReadOnlyList<SongLanguage> Languages {
		get {
			List<SongLanguage> list = new List<SongLanguage>();

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
	[Description("The tags given for the song by the beatmap creator."), JsonProperty]
	public IList<string> Tags { get; }

	public BeatmapMetadata Clone() {
		BeatmapMetadata metadata = new BeatmapMetadata();
		
		foreach (int backingLanguage in this.BackingLanguages) {
			metadata.BackingLanguages.Add(backingLanguage);
		}

		foreach (string tag in this.Tags)
			metadata.Tags.Add(tag);

		return metadata;
	}
}
