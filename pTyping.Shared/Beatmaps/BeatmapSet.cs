using System.ComponentModel;
using Realms;

namespace pTyping.Shared.Beatmaps;

public class BeatmapSet : RealmObject {
    [Description("A list of all beatmaps in the set")]
    public IList<Beatmap> Beatmaps { get; }

    [Description("The title of the song.")]
    public AsciiUnicodeTuple Title { get; set; }

    [Description("The artist of the song.")]
    public AsciiUnicodeTuple Artist { get; set; }

    [Description("The source of the song.")]
    public string Source { get; set; }
    
    public override bool Equals(object obj) {
        return obj is BeatmapSet other && this.Beatmaps.SequenceEqual(other.Beatmaps);
    }
    
    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), this.Beatmaps);
}
