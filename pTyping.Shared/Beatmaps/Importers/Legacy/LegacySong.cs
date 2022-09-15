using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace pTyping.Shared.Beatmaps.Importers.Legacy;

public enum SongType {
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    pTyping,
    UTyping
}

[JsonObject(MemberSerialization.OptIn)]
internal class LegacySong {
    [JsonProperty]
    public int TimeSignature = 4;

    [JsonProperty]
    public string FilePath;
    [JsonProperty]
    public string FolderPath;
    [JsonProperty]
    public SongType Type;
    [JsonProperty]
    public List<LegacyNote> Notes { get; set; } = new();

    [JsonProperty]
    public string Description { get; set; }
    [JsonProperty]
    public string Name { get; set; }
    [JsonProperty]
    public string Artist { get; set; }
    [JsonProperty]
    public string Creator { get; set; }
    [JsonProperty]
    public string Difficulty { get; set; }
    /// <summary>
    ///     A list of all timing points
    /// </summary>
    [JsonProperty]
    public List<LegacyTimingPoint> TimingPoints { get; set; } = new();
    /// <summary>
    ///     A list of all events that happen in the song
    /// </summary>
    [JsonProperty]
    public List<LegacyEvent> Events { get; set; } = new();

    [JsonProperty]
    public string AudioPath { get; set; }
    [JsonProperty]
    public string BackgroundPath { get; set; }
    [JsonProperty]
    public string VideoPath { get; set; }

    [JsonProperty]
    public double PreviewPoint { get; set; } = 0;

    [JsonObject(MemberSerialization.OptIn)]
    public class SongSettings {
        [JsonProperty]
        public int Strictness = 5;
        [JsonProperty]
        public double GlobalApproachMultiplier = 1d;
    }

    [JsonProperty]
    public SongSettings Settings { get; set; } = new();
}
