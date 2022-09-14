using System.ComponentModel;
using Newtonsoft.Json;
using pTyping.Shared.Beatmaps.HitObjects;
using pTyping.Shared.Events;
using Realms;

namespace pTyping.Shared.Beatmaps;

public class Beatmap : RealmObject {
    [PrimaryKey]
    public string Id { get; set; }
    
    [Description("All of the breaks that happen during the Beatmap.")]
    public IList<Break> Breaks { get; }
    [Description("All of the objects contained within the Beatmap.")]
    public IList<HitObject> HitObjects { get; }
    [Description("All of the events container within the Beatmap.")]
    public IList<Event> Events { get; }
    [Description("All of the timing points within the Beatmap.")]
    public IList<TimingPoint> TimingPoints { get; }

    public BeatmapDifficulty     Difficulty     { get; set; }
    public BeatmapInfo           Info           { get; set; }
    public BeatmapMetadata       Metadata       { get; set; }
    public BeatmapFileCollection FileCollection { get; set; }

    public Beatmap() {
        this.Info = new BeatmapInfo {
            Artist         = new AsciiUnicodeTuple("Unknown"),
            Description    = "",
            Mapper         = "Unknown Creator",
            Title          = new AsciiUnicodeTuple("Unknown"),
            DifficultyName = new AsciiUnicodeTuple(""),
            PreviewTime    = 0,
            Source         = "Unknown"
        };
        this.Difficulty = new BeatmapDifficulty();
        this.Metadata = new BeatmapMetadata {
            Languages = {
                SongLanguage.Unknown
            }
        };
        this.FileCollection = new BeatmapFileCollection {
            Audio           = null,
            Background      = null,
            BackgroundVideo = null
        };
    }

    [Description("The total duration of all the breaks in this Beatmap."), JsonIgnore]
    public double TotalBreakDuration => this.Breaks.Sum(b => b.Length);

    public Beatmap Clone() => (Beatmap)this.MemberwiseClone();
}
