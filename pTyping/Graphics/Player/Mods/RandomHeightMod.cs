using System;
using System.Collections.Generic;
using Furball.Engine;

namespace pTyping.Graphics.Player.Mods;

public class RandomHeightMod : PlayerMod {
    public override List<Type> IncompatibleMods() => new();
    public override string     Name()             => "Random Height";
    public override string     ToolTip()          => "From all sides now?";
    public override string     ShorthandName()    => "RH";
    public override double     ScoreMultiplier()  => 1.0d;
    public override string     IconFilename()     => "mod-random-height.png";

    public override void BeforeNoteCreate(Player player) {
        player.Song.Notes.ForEach(x => x.YOffset = FurballGame.Random.Next(-400, 400));

        base.BeforeNoteCreate(player);
    }
}