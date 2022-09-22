using System.ComponentModel;
using pTyping.Shared.ObjectModel;
using Realms;

namespace pTyping.Shared.Beatmaps;

public class PathHashTuple : EmbeddedObject, IClonable<PathHashTuple> {
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
