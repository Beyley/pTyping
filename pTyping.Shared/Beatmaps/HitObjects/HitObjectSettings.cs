using Realms;

namespace pTyping.Shared.Beatmaps.HitObjects;

public class HitObjectSettings : RealmObject {
    public double ApproachModifier { get; set; } = 1d;

}
