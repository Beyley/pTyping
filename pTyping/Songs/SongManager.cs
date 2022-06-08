using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kettu;
using pTyping.Engine;
using pTyping.Songs.SongLoaders;

namespace pTyping.Songs;

public static class SongManager {
    public static string SongFolder          = "songs/";
    public static string QualifiedSongFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new Exception(), SongFolder);

    public static readonly ISongHandler PTYPING_SONG_HANDLER = new pTypingSongHandler();
    public static readonly ISongHandler UTYPING_SONG_HANDLER = new UTypingSongHandler();

    public static List<Song> Songs {
        get;
    } = new();

    public const string SONG_DATABASE_NAME = "songs.json";

    public static void LoadDatabase() {
        if (!File.Exists(SONG_DATABASE_NAME)) {
            Logger.Log("Creating song database!", LoggerLevelSongManagerUpdateInfo.Instance);

            SaveDatabase();
        }


    }

    public static void SaveDatabase() {}

    public static Song LoadFullSong(Song song) {}

    public static void UpdateSongs() {
        Songs.Clear();

        DirectoryInfo dirInfo = new(QualifiedSongFolder);

        //Create the songs folder if it does not exist
        if (!dirInfo.Exists) {
            dirInfo.Create();

            Logger.Log("Song folder created!", LoggerLevelSongManagerUpdateInfo.Instance);
        }

        foreach (FileInfo file in dirInfo.GetFiles("*.pts", SearchOption.AllDirectories)) {
            Song tempSong = PTYPING_SONG_HANDLER.LoadSong(file);

            if (tempSong is not null) {
                tempSong.FilePath   = file.FullName;
                tempSong.FolderPath = file.Directory!.FullName;//todo: handle this being null (why would it ever be?)

                //This saves a lot of memory, as the list can be freed
                tempSong.Notes = null;

                Songs.Add(tempSong);
            }
            else
                Logger.Log(
                $"Song {file.Name} has failed to load!",
                LoggerLevelSongManagerUpdateInfo.Instance
                );//todo: move folders with failed songs to "failed" folder
        }

        IEnumerable<FileInfo> utypingSongs =
            dirInfo.GetFiles("*.txt", SearchOption.AllDirectories).Where(x => !x.Name.EndsWith("_src.txt") && x.Name.StartsWith("info"));
        foreach (FileInfo file in utypingSongs) {
            Song tempSong = UTYPING_SONG_HANDLER.LoadSong(file);

            if (tempSong is not null) {
                tempSong.FilePath   = file.FullName;
                tempSong.FolderPath = file.Directory!.FullName;//todo: handle this being null (why would it ever be?)

                //This saves a lot of memory, as the list can be freed
                tempSong.Notes = null;

                Songs.Add(tempSong);
            }
            else
                Logger.Log(
                $"Song {file.Name} has failed to load!",
                LoggerLevelSongManagerUpdateInfo.Instance
                );//todo: move folders with failed songs to "failed" folder
        }

        Logger.Log($"Loaded {Songs.Count} songs!", LoggerLevelSongManagerUpdateInfo.Instance);
    }
}