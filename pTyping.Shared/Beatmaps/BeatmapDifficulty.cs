using System.ComponentModel;
using pTyping.Shared.ObjectModel;
using Realms;

namespace pTyping.Shared.Beatmaps;

public class BeatmapDifficulty : EmbeddedObject, IClonable<BeatmapDifficulty> {
    [Description("How strict timing is for the song")]
    public float Strictness { get; set; } = 5;

    public BeatmapDifficulty Clone() => (BeatmapDifficulty)this.MemberwiseClone();
}
