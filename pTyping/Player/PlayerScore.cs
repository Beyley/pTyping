using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using pTyping.Online;
using pTyping.Player.Mods;

namespace pTyping.Player {
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
        public List<PlayerMod> Mods = new();

        [JsonProperty]
        public string ModsString => PlayerMod.GetModString(this.Mods);

        [JsonProperty]
        public string MapHash;
        [JsonProperty]
        public int MaxCombo = 0;
        [JsonProperty]
        public int MissHits = 0;
        [JsonProperty]
        public int PoorHits = 0;
        [JsonProperty]
        public long Score {
            get;
            protected set;
        }
        
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
            reader.ReadInt16();//katu
            score.MissHits = reader.ReadInt16();
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
            writer.Write((short)this.PoorHits);     //50
            writer.Write((short)this.FairHits);     //100
            writer.Write((short)this.GoodHits);     //300
            writer.Write((short)this.ExcellentHits);//geki
            writer.Write((short)0);                 //katu
            writer.Write((short)this.MissHits);
            writer.Write(this.Accuracy);

            writer.Flush();

            return stream.ToArray();
        }
    }
}
