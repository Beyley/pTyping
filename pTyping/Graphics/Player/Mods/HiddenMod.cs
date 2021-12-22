using System;
using System.Collections.Generic;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using pTyping.Engine;
using sowelipisona;
// using Furball.Engine.Engine.Audio;

namespace pTyping.Graphics.Player.Mods {
    public class HiddenMod : PlayerMod {
        public override List<Type> IncompatibleMods() => new();

        public override string Name()          => "Hidden";
        public override string ShorthandName() => "HD";

        public override double ScoreMultiplier() => 1.025d;

        public override void OnMapStart(AudioStream musicTrack, List<NoteDrawable> notes, Player player) {
            foreach (NoteDrawable note in notes)
                note.Tweens.Add(new FloatTween(TweenType.Fade, 1f, 0f, (int)(note.Note.Time - ConVars.BaseApproachTime.Value), (int)(note.Note.Time - 500)));

            base.OnMapStart(musicTrack, notes, player);
        }
    }
}
