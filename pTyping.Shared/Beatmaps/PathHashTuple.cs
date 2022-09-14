using Realms;

namespace pTyping.Shared.Beatmaps;

public class PathHashTuple : RealmObject {
    public string Path;
    public string Hash;

    public PathHashTuple(string path, string hash) {
        this.Path = path;
        this.Hash = hash;
    }
}
