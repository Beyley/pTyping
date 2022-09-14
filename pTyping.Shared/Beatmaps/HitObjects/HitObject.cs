using System.ComponentModel;
using Realms;

namespace pTyping.Shared.Beatmaps.HitObjects;

public class HitObject : RealmObject {
    [Description("The time the hit object occurs at")]
    public double Time { get; set; }

    [Description("The colour of the hit object")]
    public HitObjectColor Color { get; set; }

    [Description("The text in the hit object")]
    public string Text { get; set; }

    public bool IsComplete() => throw new NotImplementedException();
}
