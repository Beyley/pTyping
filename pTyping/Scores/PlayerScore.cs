using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using pTyping.Online;
using pTyping.Online.Taiko_rs;
using pTyping.Screens.Player.Mods;

namespace pTyping.Scores {
    [JsonObject(MemberSerialization.OptIn)]
    public class PlayerScore {

        private static short _TaikoRsScoreVersion = 1;
        [JsonProperty]
        public double Accuracy = 1d;
        [JsonProperty]
        public int Combo;

        [JsonProperty]
        public int ExcellentHits = 0;
        [JsonProperty]
        public int FairHits = 0;
        [JsonProperty]
        public int GoodHits = 0;
        [JsonProperty]
        public int PoorHits = 0;
        
        public List<PlayerMod> Mods = new();

        [JsonProperty]
        public string ModsString = "";
        [JsonProperty]
        public ReplayFrame[] ReplayFrames = Array.Empty<ReplayFrame>();
        
        [JsonProperty]
        public string MapHash;
        [JsonProperty]
        public int MaxCombo = 0;
        [JsonProperty]
        public long Score {
            get;
            protected set;
        }

        [JsonProperty]
        public DateTime Time;
        
        public void AddScore(int score) {
            this.Score += (int)(score * PlayerMod.ScoreMultiplier(this.Mods));
        }

        [JsonProperty]
        public string Username;

        public PlayerScore(string mapHash, string username) {
            this.MapHash  = mapHash;
            this.Username = username;
        }

        public PlayerScore() {}

        public static PlayerScore LoadUTypingReplay(string path) {
            FileStream   stream = File.OpenRead(path);
            BinaryReader reader = new(stream);

            PlayerScore replay = new(pTypingGame.CurrentSong.Value.MapHash, "UTyping");

            List<ReplayFrame> frames = new();

            while (stream.Position < stream.Length)
                frames.Add(ReplayFrame.UTypingDeserialize(reader));

            stream.Close();

            replay.ReplayFrames = frames.ToArray();

            return replay;
        }

        public void SaveUTypingReplay(string path) {
            FileStream   stream = File.Create(path);
            BinaryWriter writer = new(stream);

            foreach (ReplayFrame frame in this.ReplayFrames)
                frame.UTypingSerialize(writer);

            writer.Flush();
            stream.Close();
        }

        public static PlayerScore TaikoRsDeserialize(TaikoRsReader reader) {
            PlayerScore score = new();

            reader.ReadUInt16();// Version (we ignore rn)

            score.Username = reader.ReadString();
            score.MapHash  = reader.ReadString();
            reader.ReadByte();// mode
            score.Score         = reader.ReadInt64();
            score.Combo         = reader.ReadInt16();
            score.MaxCombo      = reader.ReadInt16();
            score.PoorHits      = reader.ReadInt16();
            score.FairHits      = reader.ReadInt16();
            score.GoodHits      = reader.ReadInt16();
            score.ExcellentHits = reader.ReadInt16();
            reader.ReadInt16();// ignore katu
            reader.ReadInt16();// ignore miss
            score.Accuracy = reader.ReadDouble();

            return score;
        }

        public byte[] TaikoRsSerialize() {
            MemoryStream  stream = new();
            TaikoRsWriter writer = new(stream);

            writer.Write(_TaikoRsScoreVersion);
            writer.Write(this.Username);
            writer.Write(this.MapHash);
            writer.Write((byte)PlayMode.pTyping);
            writer.Write(this.Score);
            writer.Write((short)this.Combo);
            writer.Write((short)this.MaxCombo);
            writer.Write((short)this.PoorHits);     // 50
            writer.Write((short)this.FairHits);     // 100
            writer.Write((short)this.GoodHits);     // 300
            writer.Write((short)this.ExcellentHits);// geki
            writer.Write((short)0);                 // katu
            writer.Write((short)0);                 // miss
            writer.Write(this.Accuracy);

            writer.Flush();

            return stream.ToArray();
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public struct ReplayFrame {
        [JsonProperty]
        public char Character;
        [JsonProperty]
        public double Time;

        public bool Used;

        public static ReplayFrame UTypingDeserialize(BinaryReader reader) {
            ReplayFrame frame = new();

            frame.Character = reader.ReadChar();
            frame.Time      = reader.ReadDouble() * 1000d;
            frame.Used      = false;

            return frame;
        }

        public void UTypingSerialize(BinaryWriter writer) {
            writer.Write(this.Character);
            writer.Write(this.Time / 1000d);
        }
    }
}
