using System.ComponentModel;
using Newtonsoft.Json;
using pTyping.Shared.ObjectModel;
using Realms;

namespace pTyping.Shared.Beatmaps;

[JsonObject(MemberSerialization.OptIn)]
public class Break : EmbeddedObject, IClonable<Break> {
	[Description("The end of the break"), JsonProperty]
	public double End { get; set; }
	[Description("The start of the break"), JsonProperty]
	public double Start { get; set; }

	[Ignored, Description("The length of the break"), JsonProperty]
	public double Length => this.End - this.Start;

	public Break Clone() {
		Break @break = new Break {
			Start = this.Start,
			End   = this.End
		};

		return @break;
	}
}
