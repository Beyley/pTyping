using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using pTyping.Graphics.Player.Mods;

namespace pTyping.Scores;

public class ScoreManager {
    private List<PlayerScore> _scores = new();

    public string ScoreDatabaseFilePath = "scores.json";

    public List<PlayerScore> GetScores(string mapHash) => this._scores.Where(x => x.MapHash == mapHash).ToList();

    public void AddScore(PlayerScore score) {
        this._scores.Add(score);

        this.Save();
    }

    public void Load() {
        if (!File.Exists(this.ScoreDatabaseFilePath))
            this.Save();

        FileStream stream = File.OpenRead(this.ScoreDatabaseFilePath);

        StreamReader reader = new(stream);

        string json = reader.ReadToEnd();
        this._scores = JsonConvert.DeserializeObject<List<PlayerScore>>(json);

        reader.Close();
        stream.Close();

        foreach (PlayerScore score in this._scores) {
            if (score.ModsString is "") continue;

            string[] splitMods = score.ModsString.Split(',');
            foreach (string mod in splitMods)//this is dumb shit
                score.Mods.Add(Activator.CreateInstance(Type.GetType(mod)) as PlayerMod);
        }
    }

    public void Save() {
        FileStream stream = File.Create(this.ScoreDatabaseFilePath);

        StreamWriter writer = new(stream);

        writer.Write(JsonConvert.SerializeObject(this._scores));

        writer.Close();
        stream.Close();
    }
}