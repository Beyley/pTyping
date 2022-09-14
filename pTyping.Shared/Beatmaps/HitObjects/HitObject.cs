using Furball.Vixie.Backends.Shared;

namespace pTyping.Shared.Beatmaps.HitObjects;

public abstract class HitObject {
    public double Time;
    public Color  Color;

    public abstract bool Complete { get; }
}
