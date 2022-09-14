using System.ComponentModel;
using Newtonsoft.Json;
using pTyping.Shared.Beatmaps.HitObjects;
using pTyping.Shared.Events;
using Realms;

namespace pTyping.Shared.Beatmaps;

public class Beatmap : RealmObject {
    [Description("All of the breaks that happen during the Beatmap.")]
    public IReadOnlyList<Break> Breaks;
    [Description("All of the objects contained within the Beatmap.")]
    public IReadOnlyList<HitObject> HitObjects;
    [Description("All of the events container within the Beatmap.")]
    public IReadOnlyList<Event> Events;
    [Description("All of the timing points within the Beatmap.")]
    public IReadOnlyList<TimingPoint> TimingPoints;

    public readonly BeatmapDifficulty     Difficulty;
    public readonly BeatmapInfo           Info;
    public readonly BeatmapMetadata       Metadata;
    public readonly BeatmapFileCollection FileCollection;

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
            Languages = new[] {
                SongLanguage.Unknown
            },
            Tags = ArraySegment<string>.Empty
        };
        this.FileCollection = new BeatmapFileCollection {
            AudioHash      = new PathHashTuple("", ""),
            BackgroundHash = new PathHashTuple("", "")
        };

        this.HitObjects = ArraySegment<HitObject>.Empty;
        this.Breaks     = ArraySegment<Break>.Empty;
        this.Events     = ArraySegment<Event>.Empty;
    }

    [Description("The total duration of all the breaks in this Beatmap."), JsonIgnore]
    public double TotalBreakDuration => this.Breaks.Sum(b => b.Length);

    public Beatmap Clone() => (Beatmap)this.MemberwiseClone();
}
