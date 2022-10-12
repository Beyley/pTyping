using System.ComponentModel;
using Newtonsoft.Json;
using pTyping.Shared.ObjectModel;
using Realms;

namespace pTyping.Shared.Beatmaps;

[JsonObject(MemberSerialization.OptIn)]
public class BeatmapDifficulty : EmbeddedObject, ICloneable<BeatmapDifficulty> {
	[Description("How strict timing is for the song"), JsonProperty]
	public float Strictness { get; set; } = 5;

	public BeatmapDifficulty Clone() {
		return (BeatmapDifficulty)this.MemberwiseClone();
	}
}
