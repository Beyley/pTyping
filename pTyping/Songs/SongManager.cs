using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Kettu;
using pTyping.Engine;
using pTyping.Songs.SongLoader;

namespace pTyping.Songs {
    public static class SongManager {
        public static string SongFolder          = "songs/";
        public static string QualifiedSongFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new Exception(), SongFolder);

        public static readonly ISongHandler PTYPING_SONG_HANDLER = new pTypingSongHandler();
        public static readonly ISongHandler UTYPING_SONG_HANDLER = new UTypingSongHandler();
        
        public static List<Song> Songs {
            get;
        } = new();

        public static void UpdateSongs() {
            Songs.Clear();

            DirectoryInfo dirInfo = new(QualifiedSongFolder);

            //Create the songs folder if it does not exist
            if (!dirInfo.Exists)
                dirInfo.Create();

            foreach (FileInfo file in dirInfo.GetFiles("*.pts", SearchOption.AllDirectories)) {
                Song tempSong = PTYPING_SONG_HANDLER.LoadSong(file);

                if (tempSong is not null)
                    Songs.Add(tempSong);
                else
                    Logger.Log($"Song {file.Name} has failed to load!", LoggerLevelSongManagerUpdateInfo.Instance);
            }

            foreach (FileInfo file in dirInfo.GetFiles("info.txt", SearchOption.AllDirectories)) {
                Song tempSong = UTYPING_SONG_HANDLER.LoadSong(file);

                if (tempSong is not null)
                    Songs.Add(tempSong);
                else
                    Logger.Log($"Song {file.Name} has failed to load!", LoggerLevelSongManagerUpdateInfo.Instance);
            }

            Logger.Log($"Loaded {Songs.Count} songs!", LoggerLevelSongManagerUpdateInfo.Instance);
        }
    }
}
