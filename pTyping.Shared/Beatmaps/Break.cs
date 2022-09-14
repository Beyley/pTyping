using Realms;

namespace pTyping.Shared.Beatmaps;

public class Break : RealmObject {
    public double End   { get; set; }
    public double Start { get; set; }

    public double Length => this.End - this.Start;

    public Break Clone() => (Break)this.MemberwiseClone();
}
