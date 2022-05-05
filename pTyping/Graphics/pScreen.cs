using Furball.Engine.Engine;

namespace pTyping.Graphics;

// ReSharper disable once InconsistentNaming
public abstract class pScreen : Screen {
    public abstract string Name    { get; }
    public abstract string State   { get; }
    public abstract string Details { get; }

    /// <summary>
    /// Should speed be reset to 0 when this screen is loaded
    /// </summary>
    public abstract bool ForceSpeedReset { get; }
    
    /// <summary>
    /// How much to fade the background image to, -1 is dont fade at all and let the screen itself decide
    /// </summary>
    public abstract float BackgroundFadeAmount { get; }

    /// <summary>
    /// Should music automatically loop after a song ends
    /// </summary>
    public abstract MusicLoopState LoopState { get; }
}

public enum MusicLoopState {
    Loop,
    LoopFromPreviewPoint,
    NewSong,
    None
}