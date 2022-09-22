using System.ComponentModel;
using pTyping.Shared.ObjectModel;
using Realms;

namespace pTyping.Shared.Beatmaps;

public class BeatmapSet : RealmObject, IClonable<BeatmapSet> {
    [Description("A list of all beatmaps in the set")]
    public IList<Beatmap> Beatmaps { get; }

    [Description("The title of the song.")]
    public AsciiUnicodeTuple Title { get; set; }

    [Description("The artist of the song.")]
    public AsciiUnicodeTuple Artist { get; set; }

    [Description("The source of the song.")]
    public string Source { get; set; }

    public BeatmapSet Clone() {
        BeatmapSet set = new() {
            Title  = this.Title.Clone(),
            Artist = this.Artist.Clone(),
            Source = this.Source
        };
        foreach (Beatmap beatmap in this.Beatmaps)
            set.Beatmaps.Add(beatmap.Clone());
        return set;
    }
    
    public override bool Equals(object obj) {
        return obj is BeatmapSet other && this.Beatmaps.SequenceEqual(other.Beatmaps);
    }
    
    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), this.Beatmaps);
}
