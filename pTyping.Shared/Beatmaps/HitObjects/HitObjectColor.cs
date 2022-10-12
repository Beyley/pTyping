using Furball.Vixie.Backends.Shared;
using Newtonsoft.Json;
using pTyping.Shared.ObjectModel;
using Realms;

namespace pTyping.Shared.Beatmaps.HitObjects;

[JsonObject(MemberSerialization.OptIn)]
public class HitObjectColor : EmbeddedObject, ICloneable<HitObjectColor> {
	private HitObjectColor(float colorRf, float colorGf, float colorBf, float colorAf) {
		this.R = colorRf;
		this.G = colorGf;
		this.B = colorBf;
		this.A = colorAf;
	}

	public HitObjectColor() {}

	[JsonProperty]
	public float R { get; set; }
	[JsonProperty]
	public float G { get; set; }
	[JsonProperty]
	public float B { get; set; }
	[JsonProperty]
	public float A { get; set; }

	public static implicit operator Color(HitObjectColor color) {
		return new(color.R, color.G, color.B, color.A);
	}
	public static implicit operator HitObjectColor(Color color) {
		return new(color.Rf, color.Gf, color.Bf, color.Af);
	}
	public HitObjectColor Clone() {
		HitObjectColor color = new HitObjectColor {
			R = this.R,
			G = this.G,
			B = this.B,
			A = this.A
		};

		return color;
	}
}
