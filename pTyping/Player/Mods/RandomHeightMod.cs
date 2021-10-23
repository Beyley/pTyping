using System;
using System.Collections.Generic;
using Furball.Engine;
using pTyping.Screens;

namespace pTyping.Player.Mods {
    public class RandomHeightMod : PlayerMod {
        public override List<Type> IncompatibleMods() => new();
        public override string     Name()             => "Random Height";
        public override string     ShorthandName()    => "RH";
        public override double     ScoreMultiplier()  => 1.0d;

        public override void BeforeNoteCreate(PlayerScreen player) {
            player.Song.Notes.ForEach(x => x.YOffset = FurballGame.Random.Next(-400, 400));

            base.BeforeNoteCreate(player);
        }
    }
}
