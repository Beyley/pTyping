using System.IO;
using Newtonsoft.Json;
using pTyping.Online;

namespace pTyping.Player {
    [JsonObject(MemberSerialization.OptIn)]
    public class PlayerScore {
        [JsonProperty]
        public long   Score;
        [JsonProperty]
        public double Accuracy = 1d;
        [JsonProperty]
        public int Combo;
        [JsonProperty]
        public int MaxCombo = 0;
        
        [JsonProperty]
        public int ExcellentHits = 0;
        [JsonProperty]
        public int GoodHits      = 0;
        [JsonProperty]
        public int FairHits      = 0;
        [JsonProperty]
        public int PoorHits      = 0;
        [JsonProperty]
        public int MissHits      = 0;

        [JsonProperty]
        public string MapHash;

        [JsonProperty]
        public string Username;
        
        public PlayerScore(string mapHash, string username) {
            this.MapHash  = mapHash;
            this.Username = username;
        }

        private static short _TaikoRsScoreVersion = 1;
        
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
            writer.Write((short)this.PoorHits); //50
            writer.Write((short)this.FairHits); //100
            writer.Write((short)this.GoodHits); //300
            writer.Write((short)this.ExcellentHits); //geki
            writer.Write((short)0); //katu
            writer.Write((short)this.MissHits);
            writer.Write(this.Accuracy);
            
            writer.Flush();

            return stream.ToArray();
        }
    }
}
