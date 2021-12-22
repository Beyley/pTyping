using System;
using System.Collections.Generic;
using sowelipisona;
// using Furball.Engine.Engine.Audio;

namespace pTyping.Graphics.Player.Mods {
    public class HalfTimeMod : PlayerMod {
        public override List<Type> IncompatibleMods() => new() {
            typeof(DoubleTimeMod)
        };

        public override string Name()            => "Half Time";
        public override string ShorthandName()   => "HT";
        public override double ScoreMultiplier() => 0.75d;

        public override void OnMapStart(AudioStream musicTrack, List<NoteDrawable> notes, Player player) {
            musicTrack.SetSpeed(0.5f);

            base.OnMapStart(musicTrack, notes, player);
        }

        public override void OnMapEnd(AudioStream musicTrack, List<NoteDrawable> notes, Player player) {
            musicTrack.SetSpeed(1f);

            base.OnMapEnd(musicTrack, notes, player);
        }
    }
}
