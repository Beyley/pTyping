using System;
using System.Collections.Generic;
using pTyping.Screens;

namespace pTyping.Player.Mods {
    public class EasyMod : PlayerMod {
        public override List<Type> IncompatibleMods() => new() {
            typeof(HardRockMod)
        };
        public override string Name()            => "Easy";
        public override string ShorthandName()   => "EZ";
        public override double ScoreMultiplier() => 0.75d;

        public override void BeforeNoteCreate(PlayerScreen player) {
            player.BaseApproachTime = (int)(player.BaseApproachTime * 1.4d);

            base.BeforeNoteCreate(player);
        }
    }
}
