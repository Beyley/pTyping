using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Furball.Engine.Engine.Helpers;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace pTyping.Songs {
    public enum SongType {
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        pTyping,
        UTyping
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Song {

        public FileInfo FileInfo;

        [JsonProperty]
        public int TimeSignature = 4;

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

        public string MapHash {
            get {
                this.Notes.Sort((x, y) => (int)(x.Time - y.Time));

                StringBuilder notes  = new();
                const string  format = "{0}:{1}:{2}:{3}:{4}";
                foreach (Note note in this.Notes)
                    notes.AppendFormat(format, note.Time, note.Color, note.Text, note.YOffset, note.Type);

                return CryptoHelper.GetSha256(Encoding.Unicode.GetBytes(notes.ToString()));
            }
        }

        public double BeatsPerMinute => 60000 / this.TimingPoints[0].Tempo;

        [Pure]
        public TimingPoint CurrentTimingPoint(double currentTime) {
            List<TimingPoint> sortedTimingPoints = this.TimingPoints.ToList();
            sortedTimingPoints.Sort((pair, valuePair) => (int)(pair.Time - valuePair.Time));

            for (int i = 0; i < sortedTimingPoints.Count; i++) {
                TimingPoint timingPoint = sortedTimingPoints[i];
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
}
