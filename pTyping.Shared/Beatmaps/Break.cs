using System.ComponentModel;
using Realms;

namespace pTyping.Shared.Beatmaps;

public class Break : RealmObject {
    [Description("The end of the break")]
    public double End { get; set; }
    [Description("The start of the break")]
    public double Start { get; set; }

    [Ignored, Description("The length of the break")]
    public double Length => this.End - this.Start;

    public Break Clone() => (Break)this.MemberwiseClone();
}
