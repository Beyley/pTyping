using System.ComponentModel;
using pTyping.Shared.ObjectModel;
using Realms;

namespace pTyping.Shared.Beatmaps;

#nullable enable

public class BeatmapFileCollection : EmbeddedObject, IClonable<BeatmapFileCollection> {
    [Description("The hash and file path for the audio.")]
    public PathHashTuple? Audio { get; set; }
    [Description("The hash and file path for the background image.")]
    public PathHashTuple? Background { get; set; }
    [Description("The hash and file path for the background video.")]
    public PathHashTuple? BackgroundVideo { get; set; }

    public BeatmapFileCollection Clone() {
        BeatmapFileCollection collection = new() {
            Audio           = this.Audio?.Clone(),
            Background      = this.Background?.Clone(),
            BackgroundVideo = this.BackgroundVideo?.Clone()
        };

        return collection;
    }
}
