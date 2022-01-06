using Newtonsoft.Json;

namespace pTyping.Songs {
    [JsonObject(MemberSerialization.OptIn)]
    public class TimingPoint {
        [JsonProperty]
        public double Time { get; set; }
        [JsonProperty]
        public double Tempo { get; set; }
        [JsonProperty]
        public double ApproachMultiplier { get; set; } = 1d;
    }
}
