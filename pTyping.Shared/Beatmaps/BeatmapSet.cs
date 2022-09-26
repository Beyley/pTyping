using System.ComponentModel;
using pTyping.Shared.ObjectModel;
using Realms;

namespace pTyping.Shared.Beatmaps;

public class BeatmapSet : RealmObject, IClonable<BeatmapSet>, IEquatable<BeatmapSet> {
	public BeatmapSet() {
		this.Id = Guid.NewGuid();
	}

	[Description("The unique ID for the beatmap set"), PrimaryKey]
	public Guid Id { get; set; }

	[Description("A list of all beatmaps in the set")]
	public IList<Beatmap> Beatmaps { get; }

	[Description("The title of the song.")]
	public AsciiUnicodeTuple Title { get; set; }

	[Description("The artist of the song.")]
	public AsciiUnicodeTuple Artist { get; set; }

	[Description("The source of the song.")]
	public string Source { get; set; }

	public BeatmapSet Clone() {
		BeatmapSet set = new BeatmapSet {
			Title  = this.Title.Clone(),
			Artist = this.Artist.Clone(),
			Source = this.Source,
			Id     = this.Id
		};
		foreach (Beatmap beatmap in this.Beatmaps)
			set.Beatmaps.Add(beatmap.Clone());
		return set;
	}

	public bool Equals(BeatmapSet obj) {
		if (ReferenceEquals(null, obj))
			return false;
		if (ReferenceEquals(this, obj))
			return true;

		return this.Id == obj.Id;
	}
}
