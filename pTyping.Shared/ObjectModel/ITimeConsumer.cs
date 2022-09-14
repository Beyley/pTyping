using Furball.Engine.Engine.Timing;

namespace pTyping.Shared.ObjectModel;

public interface ITimeConsumer {
    public void Update(ITimeSource timeSource);
}
