using System;
using System.Collections.Generic;
using Furball.Engine.Engine.Audio;
using pTyping.Screens;
using pTyping.Songs;

namespace pTyping.Player.Mods {
    public class HalfTimeMod : PlayerMod {
        public override List<Type> IncompatibleMods() => new() {
            typeof(DoubleTimeMod)
        };

        public override string Name()            => "Half Time";
        public override string ShorthandName()   => "HT";
        public override double ScoreMultiplier() => 0.6d;

        public override void OnMapStart(AudioStream musicTrack, List<Note> notes, PlayerScreen player) {
            musicTrack.AudioRate = 0.5f;

            base.OnMapStart(musicTrack, notes, player);
        }

        public override void OnMapEnd(AudioStream musicTrack, List<Note> notes, PlayerScreen player) {
            musicTrack.AudioRate = 1f;

            base.OnMapEnd(musicTrack, notes, player);
        }
    }
}
