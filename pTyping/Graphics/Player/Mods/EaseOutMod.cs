using System;
using System.Collections.Generic;
using System.Linq;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using sowelipisona;
// using Furball.Engine.Engine.Audio;

namespace pTyping.Graphics.Player.Mods;

public class EaseOutMod : PlayerMod {
    public override List<Type> IncompatibleMods() => new() {
        typeof(EaseInMod)
    };

    public override string Name()            => "Ease Out";
    public override string ToolTip()         => "Someone parse the barf bag...";
    public override string ShorthandName()   => "EO";
    public override double ScoreMultiplier() => 1.025d;

    public override string IconFilename() => "mod-ease-out.png";
    public override void OnMapStart(AudioStream musicTrack, List<NoteDrawable> notes, Player player) {
        foreach (Tween noteTween in notes.SelectMany(note => note.Tweens.Where(noteTween => noteTween.TweenType == TweenType.Movement)))
            noteTween.Easing = Easing.Out;

        base.OnMapStart(musicTrack, notes, player);
    }
}