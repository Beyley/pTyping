using Realms;

namespace pTyping.Shared.Beatmaps;

public class Break : RealmObject {
    public double End;
    public double Start;

    public double Length => this.End - this.Start;

    public Break Clone() => (Break)this.MemberwiseClone();
}
