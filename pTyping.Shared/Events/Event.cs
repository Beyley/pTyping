using Realms;

namespace pTyping.Shared.Events;

public class Event : RealmObject {
    public double End   { get; set; }
    public double Start { get; set; }

    public string Text { get; set; }

    public int BackingType { get; set; }
    [Ignored]
    public EventType Type {
        get => (EventType)this.BackingType;
        set => this.BackingType = (int)value;
    }

    [Ignored]
    public double Length => this.End - this.Start;

    public Event Copy() => (Event)this.MemberwiseClone();
}
