using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace pTyping.Songs {
    [JsonObject(MemberSerialization.OptIn)]
    public class Replay {
        [JsonProperty]
        public string SongHash;
        [JsonProperty]
        public List<ReplayFrame> Frames = new();
        [JsonProperty]
        public string Username;
        [JsonProperty]
        public DateTime Time = DateTime.Now;

        public static Replay LoadUTypingReplay(string path) {
            FileStream   stream = File.OpenRead(path);
            BinaryReader reader = new(stream);

            Replay replay = new();

            while (stream.Position < stream.Length) {

                ReplayFrame frame = new();

                frame.Character = reader.ReadChar();
                frame.Time      = reader.ReadDouble() * 1000d;

                replay.Frames.Add(frame);
            }

            stream.Close();

            return replay;
        }
    }

    [JsonObject(MemberSerialization.Fields)]
    public class ReplayFrame {
        public char   Character;
        public double Time;

        public bool Used = false;
    }
}
