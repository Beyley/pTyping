using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace pTyping.Songs {
	[JsonObject(MemberSerialization.OptIn)]
	public class Song {
		[JsonProperty]
		public List<Note> Notes { get; set; } = new();
		
		[JsonProperty]
		public string Name    { get; set; }
		[JsonProperty]
		public string Artist  { get; set; }
		[JsonProperty]
		public string Creator { get; set; }

		[JsonProperty]
		public string AudioPath { get; set; }

		public FileInfo FileInfo;
		
		public static Song LoadFromFile(FileInfo fileInfo) {
			Song song = JsonConvert.DeserializeObject<Song>(File.ReadAllText(fileInfo.FullName));
			song.FileInfo = fileInfo;

			return song;
		}

		public void Save() => File.WriteAllText(this.FileInfo.FullName, JsonConvert.SerializeObject(this, Formatting.Indented));
	}
}
