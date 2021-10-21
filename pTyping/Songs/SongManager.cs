using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Kettu;
using pTyping.LoggingLevels;

namespace pTyping.Songs {
    public static class SongManager {
        public static string SongFolder          = "songs/";
        public static string QualifiedSongFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new Exception(), SongFolder);

        private static List<Song> _Songs = new();
        public static  List<Song> Songs => _Songs;

        public static void UpdateSongs() {
            List<Song> songs = new();

            DirectoryInfo dirInfo = new(QualifiedSongFolder);

            //Create the songs folder if it does not exist
            if (!dirInfo.Exists)
                dirInfo.Create();

            foreach (FileInfo file in dirInfo.GetFiles("*.pts", SearchOption.AllDirectories))
                songs.Add(Song.LoadFromFile(file));

            foreach (FileInfo file in dirInfo.GetFiles("info.txt", SearchOption.AllDirectories)) {
                Song tempSong = Song.LoadUTypingSong(file);

                if (tempSong is not null)
                    songs.Add(tempSong);
            }

            _Songs = songs;

            Logger.Log($"Loaded {Songs.Count} songs!", new LoggerLevelSongManagerUpdateInfo());
        }
    }
}
