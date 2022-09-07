using Newtonsoft.Json;
using pTyping.Scores;

namespace pTyping.Online.Tataku;

public class ScoreSubmit {
    [JsonProperty("username")]
    public string Username;
    [JsonProperty("password")]
    public string Password;
    [JsonProperty("game")]
    public string Game = "pTyping";
    [JsonProperty("replay")]
    public TatakuReplay Replay;
    [JsonProperty("map_info")]
    public MapInfo MapInfo;
}

public class GameInfo {
    [JsonProperty("Other")]
    public string Other = "pTyping";
}

public class MapInfo {
    [JsonProperty("game")]
    public GameInfo Game;
    [JsonProperty("map_hash")]
    public string MapHash;
    [JsonProperty("playmode")]
    public string PlayMode = "pTyping";
}
