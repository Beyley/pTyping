using System;
using System.Collections.Generic;
using System.Linq;
using Furball.Engine.Engine.Audio;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using pTyping.Drawables;
using pTyping.Screens;

namespace pTyping.Player.Mods {
    public class EaseInMod : PlayerMod {
        public override List<Type> IncompatibleMods() => new() {
            typeof(EaseOutMod)
        };

        public override string Name()            => "Ease In";
        public override string ShorthandName()   => "EI";
        public override double ScoreMultiplier() => 1.025d;

        public override void OnMapStart(AudioStream musicTrack, List<NoteDrawable> notes, PlayerScreen player) {
            foreach (Tween noteTween in notes.SelectMany(note => note.Tweens.Where(noteTween => noteTween.TweenType == TweenType.Movement)))
                noteTween.Easing = Easing.In;

            base.OnMapStart(musicTrack, notes, player);
        }
    }
}
