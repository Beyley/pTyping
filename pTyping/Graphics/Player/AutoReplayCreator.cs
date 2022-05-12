using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using pTyping.Scores;
using pTyping.Songs;

namespace pTyping.Graphics.Player;

public static class AutoReplayCreator {
    [Pure]
    public static PlayerScore CreateReplay(Song song) {
        PlayerScore score = new(song.MapHash, "p!auto");

        score.ExcellentHits = song.Notes.Count;
        score.Combo         = song.Notes.Count;

        score.AddScore(69420);

        List<ReplayFrame> frames = new();

        for (int i = 0; i < song.Notes.Count; i++) {
            Note note = song.Notes[i];

            double time = note.Time;
            string text = "";
            for (int i2 = 0; i2 < note.Text.Length; i2++) {
                char[] currentRomaji = note.GetTypableRomaji(text).Romaji.First().ToCharArray();
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