using System;
using System.Collections.Generic;
// using Furball.Engine.Engine.Audio;

namespace pTyping.Graphics.Player.Mods;

public class ThreeQuarterTimeMod : PlayerMod {
    public override List<Type> IncompatibleMods() => new() {
        typeof(DoubleTimeMod),
        typeof(HalfTimeMod)
    };

    public override string Name()            => "3 Quarter Time";
    public override string ToolTip()         => "A little less vroom...";
    public override string ShorthandName()   => "3Q";
    public override double ScoreMultiplier() => 0.85d;
    public override double SpeedMultiplier() => 0.75d;
    public override string IconFilename()    => "mod-half-time.png";
}
