using pTyping.Shared.ObjectModel;
using Realms;

namespace pTyping.Shared.Scores;

public class ReplayFrame : EmbeddedObject, IClonable<ReplayFrame> {
    public char   Character { get; set; }
    public double Time      { get; set; }

    [Ignored]
    public bool Used {
        get;
        set;
    }
    public ReplayFrame Clone() {
        ReplayFrame frame = new() {
            Used      = this.Used,
            Time      = this.Time,
            Character = this.Character
        };

        return frame;
    }
}
