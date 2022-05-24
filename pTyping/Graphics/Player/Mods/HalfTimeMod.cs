using System;
using System.Collections.Generic;
// using Furball.Engine.Engine.Audio;

namespace pTyping.Graphics.Player.Mods;

public class HalfTimeMod : PlayerMod {
    public override List<Type> IncompatibleMods() => new() {
        typeof(DoubleTimeMod)
    };

    public override string Name()            => "Half Time";
    public override string ShorthandName()   => "HT";
    public override double ScoreMultiplier() => 0.75d;
    public override double SpeedMultiplier() => 0.5d;
}