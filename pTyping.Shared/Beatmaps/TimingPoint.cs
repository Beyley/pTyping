using System.ComponentModel;
using Newtonsoft.Json;
using pTyping.Shared.ObjectModel;
using Realms;

namespace pTyping.Shared.Beatmaps;

[JsonObject(MemberSerialization.OptIn)]
public class TimingPoint : EmbeddedObject, ICloneable<TimingPoint> {
	[Description("The exact time where the timing segment starts."), JsonProperty]
	public double Time { get; set; }
	[Description("The time between full beats."), JsonProperty]
	public double Tempo { get; set; }

	[Description("The time division of the song, aka N beat divisions per full beat."), JsonProperty]
	public double TimeSignature { get; set; } = 4;

	public TimingPoint() {}

	[Ignored, JsonIgnore]
	public double BeatsPerMinute => 60000d / this.Tempo;

	public TimingPoint(double time, double tempo) {
		this.Time          = time;
		this.Tempo         = tempo;
		this.TimeSignature = 4;
	}
	public TimingPoint Clone() {
		TimingPoint point = new TimingPoint {
			Tempo         = this.Tempo,
			Time          = this.Time,
			TimeSignature = this.TimeSignature
		};

		return point;
	}
}
