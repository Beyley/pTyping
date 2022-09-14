using Furball.Engine.Engine.Timing;

namespace pTyping.Shared.Events;

public class LyricEvent : Event {
    public readonly string Text;

    public LyricEvent(string text) => this.Text = text;

    public override void Update(double      delta)      {}
    public override void Update(ITimeSource timeSource) {}
}
