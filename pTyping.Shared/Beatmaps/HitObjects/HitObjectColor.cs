using Furball.Vixie.Backends.Shared;
using Realms;

namespace pTyping.Shared.Beatmaps.HitObjects;

public class HitObjectColor : RealmObject {
    private HitObjectColor(float colorRf, float colorGf, float colorBf, float colorAf) {
        this.R = colorRf;
        this.G = colorGf;
        this.B = colorBf;
        this.A = colorAf;
    }

    public HitObjectColor() {}

    public float R { get; set; }
    public float G { get; set; }
    public float B { get; set; }
    public float A { get; set; }

    public static implicit operator Color(HitObjectColor color) => new(color.R, color.G, color.B, color.A);
    public static implicit operator HitObjectColor(Color color) => new(color.Rf, color.Gf, color.Bf, color.Af);
}
