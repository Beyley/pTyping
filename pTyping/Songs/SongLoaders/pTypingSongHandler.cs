using System.IO;
using Kettu;
using Newtonsoft.Json;
using pTyping.Engine;

namespace pTyping.Songs.SongLoaders;

// ReSharper disable once InconsistentNaming
public class pTypingSongHandler : ISongHandler {
    public SongType Type => SongType.pTyping;

    public Song LoadSong(FileInfo fileInfo) {
        Song song = JsonConvert.DeserializeObject<Song>(
        File.ReadAllText(fileInfo.FullName),
        new JsonSerializerSettings {
            TypeNameHandling = TypeNameHandling.Auto
        }
        );

        song.Notes.Sort((x, y) => (int)((x.Time - y.Time) * 1000));

        Logger.Log(
        $"pTyping song loaded! notecount:{song.Notes.Count} eventcount:{song.Events.Count} {song.Artist}-{song.Name} [{song.Difficulty}] by {song.Creator}",
        LoggerLevelSongInfo.Instance
        );

        song.Type = this.Type;

        song.Notes.ForEach(x => x.Text = x.Text.Trim());

        return song;
    }

    public void SaveSong(Song song) {
        song.Notes.Sort((x,        y) => (int)(x.Time - y.Time));
        song.TimingPoints.Sort((x, y) => (int)(x.Time - y.Time));

        song.Notes.ForEach(x => x.Text = x.Text.Trim());

        File.WriteAllText(
        song.FilePath,
        JsonConvert.SerializeObject(
        song,
        new JsonSerializerSettings {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting       = Formatting.Indented
        }
        )
        );

        Logger.Log($"Saved pTyping song! {song.Artist}-{song.Name} [{song.Difficulty}] by {song.Creator}", LoggerLevelSongInfo.Instance);
    }
}