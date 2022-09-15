namespace pTyping.Shared.Beatmaps.Importers.Legacy.Events;

/// <summary>
///     Happens every quarter beat
/// </summary>
internal class BeatLineBeatEvent : LegacyEvent {
    public override LegacyEventType Type => LegacyEventType.BeatLineBeat;
}
