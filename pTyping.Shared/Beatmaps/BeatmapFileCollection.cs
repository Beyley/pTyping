using System.ComponentModel;
using Newtonsoft.Json;
using pTyping.Shared.ObjectModel;
using Realms;

namespace pTyping.Shared.Beatmaps;

#nullable enable

[JsonObject(MemberSerialization.OptIn)]
public class BeatmapFileCollection : EmbeddedObject, IClonable<BeatmapFileCollection> {
	[Description("The hash and file path for the audio."), JsonProperty]
	public PathHashTuple? Audio { get; set; }
	[Description("The hash and file path for the background image."), JsonProperty]
	public PathHashTuple? Background { get; set; }
	[Description("The hash and file path for the background video."), JsonProperty]
	public PathHashTuple? BackgroundVideo { get; set; }

	public BeatmapFileCollection Clone() {
		BeatmapFileCollection collection = new BeatmapFileCollection {
			Audio           = this.Audio?.Clone(),
			Background      = this.Background?.Clone(),
			BackgroundVideo = this.BackgroundVideo?.Clone()
		};

		return collection;
	}
}
