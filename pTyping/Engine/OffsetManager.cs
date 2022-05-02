using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using pTyping.Songs;

namespace pTyping.Engine;

public static class OffsetManager {
    public static Dictionary<string, double> Offsets = new();

    public const string FILENAME = "offsets.json";

    public static void Initialize() {
        if (!File.Exists(FILENAME)) {
            File.Create(FILENAME).Close();
        } else {
            string fileContents = File.ReadAllText(FILENAME);
            Offsets = JsonConvert.DeserializeObject<Dictionary<string, double>>(fileContents);
        }
        
        Offsets ??= new Dictionary<string, double>();
    }

    public static void Save() {
        if (!File.Exists(FILENAME))
            File.Create(FILENAME).Close();

        using FileStream stream = File.OpenWrite(FILENAME);

        using StreamWriter writer = new(stream);

        writer.Write(JsonConvert.SerializeObject(Offsets));
    }

    public static double GetOffset(Song song) {
        if (Offsets.TryGetValue(song.MapHash, out double offset))
            return offset;

        return 0;
    }

    public static void SetOffset(Song song, double offset) {
        offset = Math.Clamp(offset, -200, 200);

        Offsets[song.MapHash] = offset;
    }
}
