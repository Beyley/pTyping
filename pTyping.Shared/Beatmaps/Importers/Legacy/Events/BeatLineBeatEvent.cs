namespace pTyping.Shared.Beatmaps.Importers.Legacy.Events;

/// <summary>
///     Happens every quarter beat
/// </summary>
public class BeatLineBeatEvent : LegacyEvent {
    public override LegacyEventType Type => LegacyEventType.BeatLineBeat;
}
