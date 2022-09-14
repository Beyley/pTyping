using Furball.Engine.Engine.Timing;
using pTyping.Shared.ObjectModel;

namespace pTyping.Shared.Events;

public abstract class Event : IUpdatable, ITimeConsumer {
    public double End;
    public double Start;

    public double Length => this.End - this.Start;

    public abstract void Update(double      delta);
    public abstract void Update(ITimeSource timeSource);

    public Event Copy() => (Event)this.MemberwiseClone();
}
