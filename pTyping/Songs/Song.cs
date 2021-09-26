using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace pTyping.Songs {
	[JsonObject(MemberSerialization.OptIn)]
	public class Song {
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
		/// A dictionary of all timing points, the key is the time and the value is the tempo
		/// </summary>
		[JsonProperty]
		public List<TimingPoint> TimingPoints { get; set; }

		[JsonProperty]
		public string AudioPath { get; set; }

		[JsonProperty]
		public int    TimeSignature = 4;
		
		public FileInfo FileInfo;
		
		public static Song LoadFromFile(FileInfo fileInfo) {
			Song song = JsonConvert.DeserializeObject<Song>(File.ReadAllText(fileInfo.FullName));
			song.FileInfo = fileInfo;

			return song;
		}

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

		public double DividedNoteLength(double currentTime) {
			TimingPoint timingPoint = this.CurrentTimingPoint(currentTime);

			return timingPoint.Tempo / this.TimeSignature;
		}

		public void Save() => File.WriteAllText(this.FileInfo.FullName, JsonConvert.SerializeObject(this, Formatting.Indented));
	}
}
