using Furball.Engine.Engine;

namespace pTyping.Graphics;

// ReSharper disable once InconsistentNaming
public abstract class pScreen : Screen {
    public abstract string Name    { get; }
    public abstract string State   { get; }
    public abstract string Details { get; }
}