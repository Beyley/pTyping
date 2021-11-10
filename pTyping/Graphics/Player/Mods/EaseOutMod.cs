using System;
using System.Collections.Generic;
using System.Linq;
using Furball.Engine.Engine.Audio;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;

namespace pTyping.Screens.Player.Mods {
    public class EaseOutMod : PlayerMod {
        public override List<Type> IncompatibleMods() => new() {
            typeof(EaseInMod)
        };

        public override string Name()            => "Ease Out";
        public override string ShorthandName()   => "EO";
        public override double ScoreMultiplier() => 1.025d;

        public override void OnMapStart(AudioStream musicTrack, List<NoteDrawable> notes, PlayerScreen player) {
            foreach (Tween noteTween in notes.SelectMany(note => note.Tweens.Where(noteTween => noteTween.TweenType == TweenType.Movement)))
                noteTween.Easing = Easing.Out;

            base.OnMapStart(musicTrack, notes, player);
        }
    }
}
