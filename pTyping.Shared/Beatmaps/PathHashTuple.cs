using System.ComponentModel;
using Realms;

namespace pTyping.Shared.Beatmaps;

public class PathHashTuple : RealmObject {
    [Description("The path to the file")]
    public string Path { get; set; }
    [Description("The hash of the file")]
    public string Hash { get; set; }

    public PathHashTuple() {}

    public PathHashTuple(string path, string hash) {
        this.Path = path;
        this.Hash = hash;
    }
    public PathHashTuple Clone() {
        PathHashTuple tuple = new(this.Path, this.Hash);

        return tuple;
    }
}
