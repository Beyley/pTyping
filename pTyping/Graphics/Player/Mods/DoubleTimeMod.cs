using System;
using System.Collections.Generic;
using sowelipisona;
// using Furball.Engine.Engine.Audio;

namespace pTyping.Graphics.Player.Mods;

public class DoubleTimeMod : PlayerMod {
    public override List<Type> IncompatibleMods() => new() {
        typeof(HalfTimeMod)
    };

    public override string Name()            => "Double Time";
    public override string ShorthandName()   => "DT";
    public override double ScoreMultiplier() => 1.05d;
    public override double SpeedMultiplier() => 1.5d;
}