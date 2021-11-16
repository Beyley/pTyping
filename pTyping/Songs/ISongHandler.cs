using System.IO;
using JetBrains.Annotations;

namespace pTyping.Songs {
    public interface ISongHandler {
        public SongType Type { get; }

        [Pure, CanBeNull]
        public Song LoadSong(FileInfo fileInfo);
        public void SaveSong(Song song);
    }
}
