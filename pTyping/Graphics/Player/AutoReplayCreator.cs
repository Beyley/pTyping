using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using pTyping.Scores;
using pTyping.Shared.Beatmaps;
using pTyping.Shared.Beatmaps.HitObjects;

namespace pTyping.Graphics.Player;

//TODO: Create a `ReplayFactory` class that takes in something and spits back out a replay
//This will let us have a single interface for auto replays and replay conversions and such
public static class AutoReplayCreator {
    [Pure]
    public static PlayerScore CreateReplay(Beatmap song) {
        PlayerScore score = new(song.Id, "p!auto") {
            ExcellentHits = (ushort)song.HitObjects.Count,
            Combo         = (ushort)song.HitObjects.Count
        };

        score.AddScore(69420);

        List<ReplayFrame> frames = new();

        for (int i = 0; i < song.HitObjects.Count; i++) {
            HitObject note = song.HitObjects[i];

            double time = note.Time;
            string text = "";
            for (int i2 = 0; i2 < note.Text.Length; i2++) {
                string currentRomaji = note.GetTypableRomaji(text).Romaji.First();
                text += note.Text[i2];

                foreach (char s in currentRomaji) {
                    frames.Add(
                    new ReplayFrame {
                        Character = s,
                        Time      = time,
                        Used      = false
                    }
                    );

                    time += 1d;
                }
            }
        }

        score.ReplayFrames = frames.ToArray();

        return score;
    }
}