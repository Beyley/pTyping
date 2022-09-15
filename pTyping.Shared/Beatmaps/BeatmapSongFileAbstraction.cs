using System.Diagnostics;
using File=TagLib.File;

namespace pTyping.Shared.Beatmaps;

public class BeatmapSongFileAbstraction : File.IFileAbstraction {
    private readonly MemoryStream _memoryStream;
    public BeatmapSongFileAbstraction(FileDatabase database, Beatmap map) {
        Debug.Assert(map.FileCollection.Audio != null, "map.FileCollection.Audio != null");
        this._memoryStream = new MemoryStream(database.GetFile(map.FileCollection.Audio.Hash));

        this.ReadStream  = this._memoryStream;
        this.WriteStream = this._memoryStream;
    }

    public void CloseStream(Stream stream) {
        stream.Close();
    }
    public string Name {
        get;
    }
    public Stream ReadStream {
        get;
    }
    public Stream WriteStream {
        get;
    }
}
