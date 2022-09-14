using Realms;

namespace pTyping.Shared.Beatmaps.HitObjects;

public class HitObject : RealmObject {
    public double Time { get; set; }

    public HitObjectColor Color { get; set; }

    public string Text { get; set; }

    public bool IsComplete() => throw new NotImplementedException();
}
