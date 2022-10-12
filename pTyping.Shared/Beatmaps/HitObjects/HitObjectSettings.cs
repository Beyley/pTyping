using System.ComponentModel;
using Newtonsoft.Json;
using pTyping.Shared.ObjectModel;
using Realms;

namespace pTyping.Shared.Beatmaps.HitObjects;

[JsonObject(MemberSerialization.OptIn)]
public class HitObjectSettings : EmbeddedObject, ICloneable<HitObjectSettings> {
	[Description("The approach modifier of the note"), JsonProperty]
	public double ApproachModifier { get; set; } = 1d;

	public HitObjectSettings Clone() {
		HitObjectSettings settings = new HitObjectSettings {
			ApproachModifier = this.ApproachModifier
		};

		return settings;
	}
}
