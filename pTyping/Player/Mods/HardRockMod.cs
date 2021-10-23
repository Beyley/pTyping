using System;
using System.Collections.Generic;
using pTyping.Screens;

namespace pTyping.Player.Mods {
    public class HardRockMod : PlayerMod {
        public override List<Type> IncompatibleMods() => new() {
            typeof(EasyMod)
        };

        public override string Name()            => "Hard Rock";
        public override string ShorthandName()   => "HR";
        public override double ScoreMultiplier() => 1.1d;

        public override void BeforeNoteCreate(PlayerScreen player) {
            player.BaseApproachTime = (int)(player.BaseApproachTime / 1.4d);

            base.BeforeNoteCreate(player);
        }
    }
}