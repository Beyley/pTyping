namespace pTyping.Songs.Events;

/// <summary>
///     Happens every full beat
/// </summary>
public class BeatLineBarEvent : Event {
    public override EventType Type => EventType.BeatLineBar;
}
