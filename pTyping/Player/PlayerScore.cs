using Newtonsoft.Json;

namespace pTyping.Player {
    [JsonObject(MemberSerialization.OptIn)]
    public class PlayerScore {
        [JsonProperty]
        public long   Score;
        [JsonProperty]
        public double Accuracy = 1d;
        [JsonProperty]
        public int    Combo;
        
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
    }
}
