using System;
using System.Collections.Generic;
using System.Linq;
using Kettu;
using pTyping.Engine;
using pTyping.Songs;
using sowelipisona;

// using Furball.Engine.Engine.Audio;

namespace pTyping.Graphics.Player.Mods;

public abstract class PlayerMod {
    public static List<PlayerMod> RegisteredMods = new() {
        new HalfTimeMod(),
        new DoubleTimeMod(),
        new HiddenMod(),
        new EaseInMod(),
        new EaseOutMod(),
        new HardRockMod(),
        new EasyMod(),
        new RotateMod(),
        new RandomHeightMod()
    };

    public static double ScoreMultiplier(List<PlayerMod> mods) => mods.Aggregate<PlayerMod, double>(1f, (current, mod) => current * mod.ScoreMultiplier());

    public static string GetModString(List<PlayerMod> mods) => mods.Aggregate("", (current, playerMod) => current + playerMod.ShorthandName());

    public abstract List<Type> IncompatibleMods();

    public abstract string Name();
    public abstract string ToolTip();
    public abstract string ShorthandName();
    public abstract double ScoreMultiplier();
    public virtual  double SpeedMultiplier() => 1f;

    public abstract string IconFilename();

    public virtual void OnMapStart(AudioStream musicTrack, List<NoteDrawable> notes, Player player) {
        Logger.Log($"Mod {this.Name()} ({this.ShorthandName()}) initialized!", LoggerLevelModInfo.Instance);
    }

    public virtual void OnMapEnd(AudioStream musicTrack, List<NoteDrawable> notes, Player player) {
        Logger.Log($"Mod {this.Name()} ({this.ShorthandName()}) uninitialized!", LoggerLevelModInfo.Instance);
    }

    public virtual void OnCharacterTyped(Note note, string character, bool correct) {}

    public virtual void OnNoteHit(Note note) {}

    public virtual void Update(double time) {}

    public virtual void BeforeNoteCreate(Player player) {}
}