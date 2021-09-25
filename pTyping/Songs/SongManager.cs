using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace pTyping.Songs {
	public static class SongManager {
		public static string SongFolder          = "songs";
		public static string QualifiedSongFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new Exception(), SongFolder);

		private static List<Song> _Songs = new();
		public static  List<Song> Songs => _Songs;
		
		public static void UpdateSongs() {
			List<Song> songs = new();
			
			DirectoryInfo dirInfo = new(QualifiedSongFolder);

			foreach (FileInfo file in dirInfo.GetFiles("*.pts")) {
				songs.Add(Song.LoadFromFile(file));
			}
			
			_Songs = songs;
		}
	}
}
