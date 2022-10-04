using System.ComponentModel;
using Newtonsoft.Json;
using pTyping.Shared.ObjectModel;
using Realms;

namespace pTyping.Shared.Beatmaps;

[JsonObject(MemberSerialization.OptIn)]
public class BeatmapSet : RealmObject, IClonable<BeatmapSet>, IEquatable<BeatmapSet>, ICopyable<BeatmapSet> {
	public BeatmapSet() {
		this.Id = Guid.NewGuid();
	}

	[Description("The unique ID for the beatmap set"), PrimaryKey, JsonProperty]
	public Guid Id { get; set; }

	[Description("A list of all beatmaps in the set"), JsonProperty]
	public IList<Beatmap> Beatmaps { get; }

	[Description("The title of the song."), JsonProperty]
	public AsciiUnicodeTuple Title { get; set; }

	[Description("The artist of the song."), JsonProperty]
	public AsciiUnicodeTuple Artist { get; set; }

	[Description("The source of the song."), JsonProperty]
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
	public void CopyInto(BeatmapSet into) {
		into.Artist = this.Artist.Clone();

		//If something has been added to one of the lists, just throw an exeption for now, we'll work this out later
		if (into.Beatmaps.Count != this.Beatmaps.Count)
			throw new Exception();

		//Iterate the beatmaps and copy all beatmaps from one list into the other
		for (int i = 0; i < this.Beatmaps.Count; i++) {
			if (this.Beatmaps[i].Id != into.Beatmaps[i].Id)
				throw new Exception();

			this.Beatmaps[i].CopyInto(into.Beatmaps[i]);
		}

		into.Id     = this.Id;
		into.Source = this.Source;
		into.Title  = this.Title.Clone();
	}
}
