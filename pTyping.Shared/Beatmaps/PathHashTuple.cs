using Realms;

namespace pTyping.Shared.Beatmaps;

public class PathHashTuple : RealmObject {
    public string Path { get; set; }
    public string Hash { get; set; }

    public PathHashTuple() {}
    
    public PathHashTuple(string path, string hash) {
        this.Path = path;
        this.Hash = hash;
    }
}
