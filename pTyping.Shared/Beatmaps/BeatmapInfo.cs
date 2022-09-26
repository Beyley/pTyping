using System.ComponentModel;
using pTyping.Shared.ObjectModel;
using Realms;

namespace pTyping.Shared.Beatmaps;

public class BeatmapInfo : EmbeddedObject, IEquatable<BeatmapInfo>, IClonable<BeatmapInfo> {
	[Description("A description of the beatmap given by the creator.")]
	public string Description { get; set; }

	[Description("The name given by the creator for this specific difficulty.")]
	public AsciiUnicodeTuple DifficultyName { get; set; }
	[Description("The person who created the map, aka the mapper.")]
	public string Mapper { get; set; }

	[Description("The time set to preview the song.")]
	public double PreviewTime { get; set; }

	public BeatmapInfo Clone() {
		BeatmapInfo info = new BeatmapInfo {
			Description    = this.Description,
			Mapper         = this.Mapper,
			DifficultyName = this.DifficultyName.Clone(),
			PreviewTime    = this.PreviewTime
		};

		return info;
	}


	public bool Equals(BeatmapInfo other) {
		if (ReferenceEquals(null, other))
			return false;
		if (ReferenceEquals(this, other))
			return true;
		return base.Equals(other) && this.Description == other.Description && Equals(this.DifficultyName, other.DifficultyName) && this.Mapper == other.Mapper &&
			   this.PreviewTime.Equals(other.PreviewTime);
	}
}
