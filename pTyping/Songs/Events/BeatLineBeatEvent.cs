namespace pTyping.Songs.Events;

/// <summary>
///     Happens every quarter beat
/// </summary>
public class BeatLineBeatEvent : Event {
    public override EventType Type => EventType.BeatLineBeat;
}
