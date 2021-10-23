using System;
using System.Collections.Generic;
using Furball.Engine.Engine.Audio;
using Furball.Engine.Engine.Graphics.Drawables.Tweens;
using Furball.Engine.Engine.Graphics.Drawables.Tweens.TweenTypes;
using pTyping.Drawables;
using pTyping.Screens;

namespace pTyping.Player.Mods {
    public class RotateMod : PlayerMod {
        public override List<Type> IncompatibleMods() => new();
        public override string     Name()             => "Rotate";
        public override string     ShorthandName()    => "RT";
        public override double     ScoreMultiplier()  => 1.15d;

        public override void OnMapStart(AudioStream musicTrack, List<NoteDrawable> notes, PlayerScreen player) {
            foreach (NoteDrawable note in notes)
                note.Tweens.Add(
                new FloatTween(TweenType.Rotation, 0f, (float)(Math.PI * 2d), (int)(note.Note.Time - ConVars.BaseApproachTime.Value), (int)note.Note.Time)
                );

            base.OnMapStart(musicTrack, notes, player);
        }
    }
}