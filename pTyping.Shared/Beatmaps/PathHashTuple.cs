using System.ComponentModel;
using Newtonsoft.Json;
using pTyping.Shared.ObjectModel;
using Realms;

namespace pTyping.Shared.Beatmaps;

[JsonObject(MemberSerialization.OptIn)]
public class PathHashTuple : EmbeddedObject, ICloneable<PathHashTuple> {
	[Description("The path to the file"), JsonProperty]
	public string Path { get; set; }
	[Description("The hash of the file"), JsonProperty]
	public string Hash { get; set; }

	public PathHashTuple() {}

	public PathHashTuple(string path, string hash) {
		this.Path = path;
		this.Hash = hash;
	}
	public PathHashTuple Clone() {
		PathHashTuple tuple = new PathHashTuple(this.Path, this.Hash);

		return tuple;
	}
}
