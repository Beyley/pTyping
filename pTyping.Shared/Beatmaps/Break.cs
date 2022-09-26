using System.ComponentModel;
using pTyping.Shared.ObjectModel;
using Realms;

namespace pTyping.Shared.Beatmaps;

public class Break : EmbeddedObject, IClonable<Break> {
	[Description("The end of the break")]
	public double End { get; set; }
	[Description("The start of the break")]
	public double Start { get; set; }

	[Ignored, Description("The length of the break")]
	public double Length => this.End - this.Start;

	public Break Clone() {
		Break @break = new Break {
			Start = this.Start,
			End   = this.End
		};

		return @break;
	}
}
