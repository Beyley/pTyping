using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace pTyping.Songs {
    [JsonObject(MemberSerialization.OptIn)]
    public class Replay {
        public Replay(string hash, string username) {
            this.SongHash = hash;
            this.Username = username;
            this.Frames   = Array.Empty<ReplayFrame>();
            this.Time     = DateTime.Now;
        }

        public Replay() {}
        
        [JsonProperty]
        public string SongHash;
        [JsonProperty]
        public ReplayFrame[] Frames;
        [JsonProperty]
        public string Username;
        [JsonProperty]
        public DateTime Time;

        public static Replay LoadUTypingReplay(string path) {
            FileStream   stream = File.OpenRead(path);
            BinaryReader reader = new(stream);

            Replay replay = new(pTypingGame.CurrentSong.Value.MapHash, "UTyping");

            List<ReplayFrame> frames = new();
            
            while (stream.Position < stream.Length) {
                ReplayFrame frame = new();

                frame.Character = reader.ReadChar();
                frame.Time      = reader.ReadDouble() * 1000d;
                frame.Used      = false;

                frames.Add(frame);
            }

            stream.Close();

            replay.Frames = frames.ToArray();

            return replay;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public struct ReplayFrame {
        [JsonProperty]
        public char   Character;
        [JsonProperty]
        public double Time;

        public bool Used;
    }
}
