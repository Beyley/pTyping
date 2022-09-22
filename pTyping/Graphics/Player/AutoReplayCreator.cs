using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using pTyping.Shared.Beatmaps;
using pTyping.Shared.Beatmaps.HitObjects;
using pTyping.Shared.Scores;

namespace pTyping.Graphics.Player;

//TODO: Create a `ReplayFactory` class that takes in something and spits back out a replay
//This will let us have a single interface for auto replays and replay conversions and such
public static class AutoReplayCreator {
    [Pure]
    public static Score CreateReplay(Beatmap song) {
        Score score = new() {
            BeatmapId = song.Id,
            User = {
                Username = "p!auto"
            },
            ExcellentHits = (ushort)song.HitObjects.Count,
            MaxCombo      = (ushort)song.HitObjects.Count
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
                        Time      = time
                    }
                    );

                    time += 1d;
                }
            }
        }

        foreach (ReplayFrame frame in frames)
            score.ReplayFrames.Add(frame);

        return score;
    }
}