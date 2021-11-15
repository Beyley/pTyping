using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Furball.Engine.Engine.Helpers;
using JetBrains.Annotations;
using Kettu;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using pTyping.Engine;
using pTyping.Songs.Events;

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
                const string  format = "{0}:{1}:{2}:{3}";
                foreach (Note note in this.Notes)
                    notes.AppendFormat(format, note.Time, note.Color, note.Text, note.YOffset);

                return CryptoHelper.GetSha256(Encoding.Unicode.GetBytes(notes.ToString()));
            }
        }

        public double BeatsPerMinute => 60000 / this.TimingPoints[0].Tempo;

        [Pure]
        public static Song LoadFromFile(FileInfo fileInfo) {
            Song song = JsonConvert.DeserializeObject<Song>(
            File.ReadAllText(fileInfo.FullName),
            new JsonSerializerSettings {
                TypeNameHandling = TypeNameHandling.Auto
            }
            );
            song.FileInfo = fileInfo;

            song.Notes.Sort((x, y) => (int)((x.Time - y.Time) * 1000));

            Logger.Log(
            $"pTyping song loaded! notecount:{song.Notes.Count} eventcount:{song.Events.Count} {song.Artist}-{song.Name} [{song.Difficulty}] by {song.Creator}",
            LoggerLevelSongInfo.Instance
            );

            song.Type = SongType.pTyping;

            return song;
        }

        [CanBeNull]
        public static Song LoadUTypingSong(FileInfo fileInfo) {
            Song song = new() {
                Name       = "",
                Artist     = "",
                Creator    = "",
                Difficulty = "",
                FileInfo   = fileInfo
            };

            string infoData = Encoding.GetEncoding(932).GetString(File.ReadAllBytes(fileInfo.FullName));

            infoData = infoData.Replace("\r", "");
            string[] info = infoData.Split("\n");

            song.Name       = info[0];
            song.Artist     = info[1];
            song.Creator    = info[2];
            song.Difficulty = info[3];
            song.Type       = SongType.UTyping;

            string dataFilename = info[4];

            string mapData = Encoding.GetEncoding(932).GetString(File.ReadAllBytes(Path.Combine(fileInfo.DirectoryName!, dataFilename)));

            if (mapData[0] != '@') return null;

            using StringReader reader = new(mapData);
            string             line;
            do {
                line = reader.ReadLine();

                if (line == null || line.Trim() == string.Empty)
                    continue;

                string firstChar = line[0].ToString();

                line = line[1..];

                switch (firstChar) {
                    //Contains the relative path to the song file in the format of
                    //@path
                    //ex. @animariot.ogg
                    case "@": {
                        song.AudioPath = line;
                        break;
                    }
                    //Contains a note in the format of
                    //TimeInSeconds CharacterToType
                    //ex. +109.041176 だい
                    case "+": {
                        string[] splitLine = line.Split(" ");

                        double time = double.Parse(splitLine[0]) * 1000;
                        string text = splitLine[1];

                        song.Notes.Add(
                        new() {
                            Time  = time,
                            Text  = text.Trim(),
                            Color = Color.Red
                        }
                        );

                        break;
                    }
                    //Contains the next set of lyrics in the format of
                    //*TimeInSeconds Lyrics
                    //ex. *16.100000 だいてだいてだいてだいて　もっと
                    case "*": {
                        string[] splitLine = line.Split(" ");

                        double time = double.Parse(splitLine[0]) * 1000d;
                        string text = splitLine[1];

                        song.Events.Add(
                        new LyricEvent {
                            Lyric = text.Trim(),
                            Time  = time
                        }
                        );

                        break;
                    }
                    //Prevents you from typing the previous note in the format of
                    // /TimeInSeconds
                    //ex. /17.982353
                    case "/": {
                        song.Events.Add(
                        new TypingCutoffEvent {
                            Time = double.Parse(line) * 1000d
                        }
                        );

                        break;
                    }
                    //A beatline beat (happens every 1/4th beat except for full beats)
                    //-TimeInSeconds
                    //ex. -17.747059
                    case "-": {
                        song.Events.Add(
                        new BeatLineBeatEvent {
                            Time = double.Parse(line) * 1000d
                        }
                        );

                        break;
                    }
                    //A beatline bar (happens every full beat)
                    //=TimeInSeconds
                    //ex. =4.544444
                    case "=": {
                        song.Events.Add(
                        new BeatLineBarEvent {
                            Time = double.Parse(line) * 1000d
                        }
                        );

                        break;
                    }
                }

            } while (line != null);

            List<BeatLineBarEvent> beatLineBarEvents = song.Events.Where(x => x is BeatLineBarEvent).Cast<BeatLineBarEvent>().ToList();

            double tempo = beatLineBarEvents[1].Time - beatLineBarEvents[0].Time;

            song.TimingPoints.Add(
            new TimingPoint {
                Time  = song.Events.First(x => x is BeatLineBarEvent).Time,
                Tempo = tempo / 4d
            }
            );
            
            Logger.Log(
            $"UTyping song loaded! notecount:{song.Notes.Count} eventcount:{song.Events.Count} {song.Artist}-{song.Name} diff:{song.Difficulty} creator:{song.Creator}",
            LoggerLevelSongInfo.Instance
            );

            return song;
        }

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

        public void Save() {
            this.Notes.Sort((x,        y) => (int)(x.Time - y.Time));
            this.TimingPoints.Sort((x, y) => (int)(x.Time - y.Time));

            File.WriteAllText(
            this.FileInfo.FullName,
            JsonConvert.SerializeObject(
            this,
            new JsonSerializerSettings {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting       = Formatting.Indented
            }
            )
            );

            Logger.Log($"Saved pTyping song! {this.Artist}-{this.Name} [{this.Difficulty}] by {this.Creator}", LoggerLevelSongInfo.Instance);
        }

        public void Save(FileStream stream) {
            this.Notes.Sort((x,        y) => (int)(x.Time - y.Time));
            this.TimingPoints.Sort((x, y) => (int)(x.Time - y.Time));

            StreamWriter writer = new(stream);

            writer.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
            writer.Flush();

            Logger.Log($"Saved pTyping song! {this.Artist}-{this.Name} [{this.Difficulty}] by {this.Creator}", LoggerLevelSongInfo.Instance);
        }

        public bool AllNotesHit() => this.Notes.All(note => note.IsHit);
    }
}
