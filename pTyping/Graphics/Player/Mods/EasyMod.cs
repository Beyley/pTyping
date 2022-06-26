using System;
using System.Collections.Generic;

namespace pTyping.Graphics.Player.Mods;

public class EasyMod : PlayerMod {
    public override List<Type> IncompatibleMods() => new() {
        typeof(HardRockMod)
    };
    public override string Name()            => "Easy";
    public override string ToolTip()         => "えーマジ？ Easy Mode？";
    public override string ShorthandName()   => "EZ";
    public override double ScoreMultiplier() => 0.9d;
    public override string IconFilename()    => "mod-easy.png";

    public override void BeforeNoteCreate(Player player) {
        player.BaseApproachTime = (int)(player.BaseApproachTime * 1.4d);

        base.BeforeNoteCreate(player);
    }
}