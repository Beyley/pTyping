using System.ComponentModel;
using Realms;

namespace pTyping.Shared.Beatmaps;

public class BeatmapInfo : RealmObject, IEquatable<BeatmapInfo> {
    [Description("The artist of the song.")]
    public AsciiUnicodeTuple Artist;
    [Description("A description of the beatmap given by the creator.")]
    public string Description;

    [Description("The name given by the creator for this specific difficulty.")]
    public AsciiUnicodeTuple DifficultyName;
    [Description("The person who created the map, aka the mapper.")]
    public string Mapper;
    [Description("The title of the song.")]
    public AsciiUnicodeTuple Title;
    [Description("The source of the song.")]
    public string Source;

    [Description("The time set to preview the song.")]
    public double PreviewTime;

    public BeatmapInfo Clone() => (BeatmapInfo)this.MemberwiseClone();

    public bool Equals(BeatmapInfo? other) {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return this.Artist.Equals(other.Artist) && this.Description == other.Description && this.DifficultyName.Equals(other.DifficultyName) &&
               this.Mapper == other.Mapper && this.Title.Equals(other.Title) && this.Source == other.Source && this.PreviewTime.Equals(other.PreviewTime);
    }
}
