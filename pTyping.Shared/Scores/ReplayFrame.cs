using Newtonsoft.Json;
using pTyping.Shared.ObjectModel;
using Realms;

namespace pTyping.Shared.Scores;

[JsonObject(MemberSerialization.OptIn)]
public class ReplayFrame : EmbeddedObject, ICloneable<ReplayFrame> {
	[JsonProperty]
	public char   Character { get; set; }
	[JsonProperty]
	public double Time      { get; set; }

	[Ignored, JsonIgnore]
	public bool Used {
		get;
		set;
	}
	public ReplayFrame Clone() {
		ReplayFrame frame = new ReplayFrame {
			Used      = this.Used,
			Time      = this.Time,
			Character = this.Character
		};

		return frame;
	}
}
