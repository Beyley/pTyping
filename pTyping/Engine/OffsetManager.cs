using System;
using Furball.Engine.Engine.Config;
using Furball.Volpe.Evaluation;
using pTyping.Songs;

namespace pTyping.Engine;

public static class OffsetManager {
    private class OffsetConfig : VolpeConfig {
        public override string Name => "offsets";

        public static readonly OffsetConfig Instance = new();
    }

    public static void Initialize() {
        OffsetConfig.Instance.Load();
    }

    public static void Save() {
        OffsetConfig.Instance.Save();
    }

    public static double GetOffset(Song song) {
        if (OffsetConfig.Instance.Values.TryGetValue(song.MapHash, out Value offset))
            return offset.ToNumber().Value;

        return 0;
    }

    public static void SetOffset(Song song, double offset) {
        offset = Math.Clamp(offset, -200, 200);

        OffsetConfig.Instance.Values[song.MapHash] = new Value.Number(offset);
    }
}
