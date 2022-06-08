using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Furball.Engine.Engine.Helpers;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace pTyping.Songs;

public enum SongType {
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    pTyping,
    UTyping
}

[JsonObject(MemberSerialization.OptIn)]
public class Song {
    [JsonProperty]
    public int TimeSignature = 4;

    [JsonProperty]
    public string FilePath;
    [JsonProperty]
    public string FolderPath;
    
    public SongType Type;
    [JsonProperty]
    public List<Note> Notes { get; set; } = new();

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
    public List<TimingPoint> TimingPoints { get; set; } = new();
    /// <summary>
    ///     A list of all events that happen in the song
    /// </summary>
    [JsonProperty]
    public List<Event> Events { get; set; } = new();

    [JsonProperty]
    public string AudioPath { get; set; }
    [JsonProperty]
    public string BackgroundPath { get; set; }

    [JsonObject(MemberSerialization.OptIn)]
    public class SongSettings {
        [JsonProperty]
        public int Strictness = 5;
        [JsonProperty]
        public double GlobalApproachMultiplier = 1d;
    }

    [JsonProperty]
    public SongSettings Settings { get; set; } = new();

    public string MapHash => CryptoHelper.GetSha256(File.ReadAllBytes(this.FilePath)); //todo: decide if SHA256 is the right choice, it might be wise to go with SHA512 while we still have the chance

    public double BeatsPerMinute => 60000 / this.TimingPoints[0].Tempo;

    [Pure]
    public TimingPoint CurrentTimingPoint(double currentTime) {
        for (int i = this.TimingPoints.Count - 1; i >= 0; i--) {
            TimingPoint timingPoint = this.TimingPoints[i];
            if (timingPoint.Time < currentTime)
                return timingPoint;
        }

        return this.TimingPoints.First();
    }

    [Pure]
    public double DividedNoteLength(double currentTime) {
        TimingPoint timingPoint = this.CurrentTimingPoint(currentTime);

        return timingPoint.Tempo / this.TimeSignature;
    }

    public bool AllNotesHit() => this.Notes.All(note => note.IsHit);
}