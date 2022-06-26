using System;
using System.Collections.Generic;

namespace pTyping.Graphics.Player.Mods;

public class HardRockMod : PlayerMod {
    public override List<Type> IncompatibleMods() => new() {
        typeof(EasyMod)
    };

    public override string Name()            => "Hard Rock";
    public override string ToolTip()         => "6801 Hollywood Blvd Suite 105";
    public override string ShorthandName()   => "HR";
    public override double ScoreMultiplier() => 1.05d;
    public override string IconFilename()    => "mod-hard-rock.png";

    public override void BeforeNoteCreate(Player player) {
        player.BaseApproachTime = (int)(player.BaseApproachTime / 1.4d);

        base.BeforeNoteCreate(player);
    }
}