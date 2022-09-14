using System.ComponentModel;
using Realms;

namespace pTyping.Shared.Beatmaps;

public class BeatmapSet : RealmObject {
    [Description("A list of all beatmaps in the set")]
    public IList<Beatmap> Beatmaps { get; }
}
