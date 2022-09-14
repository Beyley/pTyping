using System.ComponentModel;
using Realms;

namespace pTyping.Shared.Beatmaps;

public class BeatmapInfo : RealmObject, IEquatable<BeatmapInfo> {
    [Description("The artist of the song.")]
    public AsciiUnicodeTuple Artist { get; set; }
    [Description("A description of the beatmap given by the creator.")]
    public string Description { get; set; }

    [Description("The name given by the creator for this specific difficulty.")]
    public AsciiUnicodeTuple DifficultyName { get; set; }
    [Description("The person who created the map, aka the mapper.")]
    public string Mapper { get; set; }
    [Description("The title of the song.")]
    public AsciiUnicodeTuple Title { get; set; }
    [Description("The source of the song.")]
    public string Source { get; set; }

    [Description("The time set to preview the song.")]
    public double PreviewTime { get; set; }

    public BeatmapInfo Clone() => (BeatmapInfo)this.MemberwiseClone();

    public bool Equals(BeatmapInfo other) {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return this.Artist.Equals(other.Artist) && this.Description == other.Description && this.DifficultyName.Equals(other.DifficultyName) &&
               this.Mapper == other.Mapper && this.Title.Equals(other.Title) && this.Source == other.Source && this.PreviewTime.Equals(other.PreviewTime);
    }
}
