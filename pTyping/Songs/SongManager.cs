using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kettu;
using Newtonsoft.Json;
using pTyping.Engine;
using pTyping.Songs.SongLoaders;

namespace pTyping.Songs;

public static class SongManager {
    public static string SongFolder          = "songs/";
    public static string QualifiedSongFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new Exception(), SongFolder);

    public static readonly ISongHandler PTYPING_SONG_HANDLER = new pTypingSongHandler();
    public static readonly ISongHandler UTYPING_SONG_HANDLER = new UTypingSongHandler();

    public static readonly List<Song> Songs = new();

    public const string SONG_DATABASE_NAME = "songs.json";

    public static void LoadDatabase() {
        if (!File.Exists(SONG_DATABASE_NAME)) {
            Logger.Log("Creating song database!", LoggerLevelSongManagerUpdateInfo.Instance);

            //Get the songs into the list
            UpdateSongs();
            
            //Save the list to disk
            SaveDatabase();
        }

        string databaseRaw = File.ReadAllText(SONG_DATABASE_NAME);

        List<Song> songs = JsonConvert.DeserializeObject<List<Song>>(databaseRaw);

        //todo: should we do this? i dont know if we should scan for songs if we have an empty database, but im putting this here now
        if (songs.Count == 0) {
            UpdateSongs();
        }
        
        Songs.Clear();
        Songs.AddRange(songs);
    }

    public static void SaveDatabase() {
        //todo

        string databaseSerialized = JsonConvert.SerializeObject(Songs);
        
        if(File.Exists(SONG_DATABASE_NAME))
            File.Delete(SONG_DATABASE_NAME);
        
        File.WriteAllText(SONG_DATABASE_NAME, databaseSerialized);
    }

    public static Song LoadFullSong(Song song) {
        using FileStream stream = File.OpenRead(song.FilePath);
        using StreamReader reader = new(stream);

        Song tempSong = null;
        switch (song.Type) {
            case SongType.pTyping: {
                tempSong = PTYPING_SONG_HANDLER.LoadSong(new FileInfo(song.FilePath));
                break;
            }
            case SongType.UTyping:
                tempSong = UTYPING_SONG_HANDLER.LoadSong(new FileInfo(song.FilePath));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        //todo: handle failed loading
        
        //Inherit these from the main song
        tempSong!.FilePath  = song.FilePath;
        tempSong.FolderPath = song.FolderPath;

        return tempSong;
    }

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
                tempSong.Notes  = null;
                tempSong.Events = null;

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
                tempSong.Notes  = null;
                tempSong.Events = null;

                Songs.Add(tempSong);
            }
            else
                Logger.Log(
                $"Song {file.Name} has failed to load!",
                LoggerLevelSongManagerUpdateInfo.Instance
                );//todo: move folders with failed songs to "failed" folder
        }

        Logger.Log($"Loaded {Songs.Count} songs!", LoggerLevelSongManagerUpdateInfo.Instance);
        
        SaveDatabase();
    }
}