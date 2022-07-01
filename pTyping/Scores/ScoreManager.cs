using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Furball.Engine.Engine.Helpers;
using JetBrains.Annotations;
using Newtonsoft.Json;
using pTyping.Graphics.Player.Mods;

namespace pTyping.Scores;

public class ScoreManager {
    private List<PlayerScore> _scores = new();

    public const    string SCORES_FOLDER         = "scores";
    public readonly string ScoreDatabaseFilePath = Path.Combine(SCORES_FOLDER, "database.json");

    public bool WantCreateFreshDatabase = false;

    [Pure, NotNull]
    public List<PlayerScore> GetScores(string mapHash) => this._scores.Where(x => x.MapHash == mapHash).ToList();

    public void AddScore(PlayerScore score) {
        this._scores.Add(score);

        #region Find first open file

        int c = this._scores.Count;

        string path = GetPathForId(c);
        while (File.Exists(path)) {
            ++c;
            path = GetPathForId(c);
        }

        #endregion

        score.ReplayId = c;

        #region Write to the files

        using FileStream   stream = File.Create(path);
        using StreamWriter writer = new(stream);

        writer.Write(JsonConvert.SerializeObject(score));

        #endregion

        this.SaveDatabase();
    }

    [Pure, NotNull]
    private static string GetPathForId(int id) => $"{Path.Combine(SCORES_FOLDER, id.ToString())}.json";

    /// <summary>
    ///     Gets the replay for a score
    /// </summary>
    /// <param name="score">The score to get the replay for</param>
    /// <returns>Whether getting the replay was successful</returns>
    public bool GetReplayForScore(PlayerScore score) {
        string path = GetPathForId(score.ReplayId);

        if (!File.Exists(path))
            return false;

        try {
            PlayerScore gotscore = JsonConvert.DeserializeObject<PlayerScore>(File.ReadAllText(path))!;

            score.ReplayFrames = gotscore.ReplayFrames;
        }
        catch {
            return false;
        }

        return true;
    }

    public void Load() {
        //Make sure the internal scores folder exists
        if (!Directory.Exists(SCORES_FOLDER))
            Directory.CreateDirectory(SCORES_FOLDER);

        //If we dont have a database on disk or we are requested to recreate the database, create a new database
        if (!File.Exists(this.ScoreDatabaseFilePath) || this.WantCreateFreshDatabase)
            this.CreateFreshDatabase();

        //Get the scores from the database
        List<PlayerScore> scores = JsonConvert.DeserializeObject<List<PlayerScore>>(File.ReadAllText(this.ScoreDatabaseFilePath))!;

        this._scores = scores;

        //Add the mods
        //TODO: rework mods to not require *this*
        foreach (PlayerScore score in this._scores) {
            if (score.ModsString is "") continue;
        
            string[] splitMods = score.ModsString.Split(',');
            foreach (string mod in splitMods)//this is dumb shit;
                score.Mods.Add(Activator.CreateInstance(Type.GetType(mod)) as PlayerMod);
        }
    }
    private void CreateFreshDatabase() {
        this._scores = new();

        #region Scan for existing replays

        DirectoryInfo dirInfo = new(SCORES_FOLDER);

        foreach (FileInfo file in dirInfo.GetFiles("*.json", SearchOption.TopDirectoryOnly)) {
            if (file.Name.Contains("database"))
                continue;

            using FileStream   stream = file.Open(FileMode.Open);
            using StreamReader reader = new(stream);

            PlayerScore tempScore = JsonConvert.DeserializeObject<PlayerScore>(reader.ReadToEnd());

            if (tempScore == null) {
                //delete any bad scores
                File.Delete(file.FullName);
                continue;
            }

            this._scores.Add(tempScore);
        }

        #endregion

        this.SaveDatabase();
    }

    public void SaveDatabase() {
        using FileStream   stream = File.Create(this.ScoreDatabaseFilePath);
        using StreamWriter writer = new(stream);

        List<PlayerScore> noReplays = new();

        foreach (PlayerScore score in this._scores) {
            PlayerScore toAdd = score.Copy();
            toAdd.ReplayFrames = Array.Empty<ReplayFrame>();
            noReplays.Add(toAdd);
        }

        writer.Write(JsonConvert.SerializeObject(noReplays));
    }
}