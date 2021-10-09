using System.IO;
using System.Linq;
using Portable.Text;
using Newtonsoft.Json;
using pTyping.Songs.Events;
using pTyping.LoggingLevels;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Furball.Engine.Engine.Helpers.Logger;

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
		/// A list of all timing points
		/// </summary>
		[JsonProperty]
		public List<TimingPoint> TimingPoints { get; set; } = new();
		/// <summary>
		/// A list of all events that happen in the song
		/// </summary>
		[JsonProperty]
		public List<Event> Events { get; set; } = new();

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
			
			song.Notes.Sort((x, y) => (int)((x.Time - y.Time) * 1000));

			Logger.Log($"pTyping song loaded! notecount:{song.Notes.Count} eventcount:{song.Events.Count} {song.Artist}-{song.Name} [{song.Difficulty}] by {song.Creator}", new LoggerLevelSongInfo());
			
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
						
						song.Notes.Add(new () {
							Time = time, 
							Text = text.Trim(),
							Color = Color.Red
						});
						
						break;
					}
					//Contains the next set of lyrics in the format of
					//*TimeInSeconds Lyrics
					//ex. *16.100000 だいてだいてだいてだいて　もっと
					case "*": {
						string[] splitLine = line.Split(" ");

						double time = double.Parse(splitLine[0]) * 1000d;
						string text = splitLine[1];
						
						song.Events.Add(new LyricEvent {
							Lyric = text.Trim(),
							Time  = time
						});
						
						break;
					}
					//Prevents you from typing the previous note in the format of
					// /TimeInSeconds
					//ex. /17.982353
					case "/": {
						song.Events.Add(new TypingCutoffEvent {
							Time = double.Parse(line) * 1000d
						});
						
						break;
					}
					//A rest in the song (what this does im still not sure) in the format of
					//-TimeInSeconds
					//ex. -17.747059
					case "-": {
						song.Events.Add(new RestEvent {
							Time = double.Parse(line) * 1000d
						});
						
						break;
					}
				}
				
			} while (line != null);
			
			Logger.Log($"UTyping song loaded! notecount:{song.Notes.Count} eventcount:{song.Events.Count} {song.Artist}-{song.Name} diff:{song.Difficulty}", new LoggerLevelSongInfo());
			
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
			
			Logger.Log($"Saved pTyping song! {this.Artist}-{this.Name} [{this.Difficulty}] by {this.Creator}", new LoggerLevelSongInfo());
		}

		public void Save(FileStream stream) {
			this.Notes.Sort((x,        y) => (int)(x.Time - y.Time));
			this.TimingPoints.Sort((x, y) => (int)(x.Time - y.Time));

			StreamWriter writer = new(stream);

			writer.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
			writer.Flush();
			
			Logger.Log($"Saved pTyping song! {this.Artist}-{this.Name} [{this.Difficulty}] by {this.Creator}", new LoggerLevelSongInfo());
		}

		public bool AllNotesHit() => this.Notes.All(note => note.IsHit);
	}
}
