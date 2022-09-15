namespace pTyping.Shared.Beatmaps.Importers.Legacy.Events;

/// <summary>
///     Happens every full beat
/// </summary>
internal abstract class LegacyBeatLineBarEvent : LegacyEvent {
    public override LegacyEventType Type => LegacyEventType.BeatLineBar;
}
