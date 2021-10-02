using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.Xna.Framework;
using Portable.Text;

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
		public List<TimingPoint> TimingPoints { get; set; } = new();

		[JsonProperty]
		public string AudioPath { get; set; }
		[JsonProperty]
		public string BackgroundPath { get; set; }

		[JsonProperty]
		public int    TimeSignature = 4;
		
		public FileInfo FileInfo;
		
		public static Song LoadFromFile(FileInfo fileInfo) {
			Song song = JsonConvert.DeserializeObject<Song>(File.ReadAllText(fileInfo.FullName));
			song.FileInfo = fileInfo;

			return song;
		}

		public static Song LoadUTypingSong(FileInfo fileInfo) {
			Song song = new() {
				Name       = "",
				Artist     = "",
				Creator    = "",
				Difficulty = "",
				FileInfo   = fileInfo,
				TimingPoints = new() {new(){Tempo = 500}}
			};

			string infoData = Encoding.GetEncoding(932).GetString(File.ReadAllBytes(fileInfo.FullName));

			infoData = infoData.Replace("\r", "");
			string[] info = infoData.Split("\n");

			song.Name       = info[0];
			song.Artist     = info[1];
			song.Difficulty = info[3];

			string dataFilename = info[4];

			string mapData = Encoding.GetEncoding(932).GetString(File.ReadAllBytes(Path.Combine(fileInfo.DirectoryName, dataFilename)));
			
			if (mapData[0] != '@') return null;
			
			using StringReader reader = new(mapData);
			string             line;
			do
			{
				line = reader.ReadLine();
				
				if (line == null || line.Trim() == string.Empty)
					continue;

				string firstChar = line[0].ToString();

				line = line[1..];
				
				switch (firstChar) {
					case "@": {
						song.AudioPath = line;
						break;
					}
					case "+": {
						string[] splitLine = line.Split(" ");

						double time = double.Parse(splitLine[0]) * 1000;
						string text = splitLine[1];
						
						song.Notes.Add(new () {
							Time = time, 
							Text = text,
							Color = Color.Red
						});
						
						break;
					}
				}
				
			} while (line != null);
			
			
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

		public void Save() {
			this.Notes.Sort((x,        y) => (int)(x.Time - y.Time));
			this.TimingPoints.Sort((x, y) => (int)(x.Time - y.Time));
			
			File.WriteAllText(this.FileInfo.FullName, JsonConvert.SerializeObject(this, Formatting.Indented));
		}

		public void Save(FileStream stream) {
			this.Notes.Sort((x,        y) => (int)(x.Time - y.Time));
			this.TimingPoints.Sort((x, y) => (int)(x.Time - y.Time));

			StreamWriter writer = new(stream);

			writer.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
			writer.Flush();
		}

		public bool AllNotesHit() => this.Notes.All(note => note.IsHit);
	}
}
