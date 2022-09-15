using System.ComponentModel;
using Newtonsoft.Json;
using Realms;

namespace pTyping.Shared.Beatmaps.HitObjects;

[JsonObject(MemberSerialization.OptIn)]
public class HitObjectSettings : RealmObject {
    [Description("The approach modifier of the note"), JsonProperty]
    public double ApproachModifier { get; set; } = 1d;

    public HitObjectSettings Clone() {
        HitObjectSettings settings = new() {
            ApproachModifier = this.ApproachModifier
        };

        return settings;
    }
}
