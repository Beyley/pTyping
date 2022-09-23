using System.ComponentModel;
using pTyping.Shared.ObjectModel;
using Realms;

namespace pTyping.Shared.Scores;

public class Score : RealmObject, IClonable<Score> {
    public Score() {
        this.User = new DatabaseUser();
        this.Id   = Guid.NewGuid();
    }

    [Description("A unique ID for the score"), PrimaryKey]
    public Guid Id { get; set; }

    [Description("The user that made the score")]
    public DatabaseUser User { get; set; }

    [MapTo("Score"), Description("The score the user achieved")]
    public long AchievedScore { get; set; }

    [Description("The accuracy the user achieved")]
    public double Accuracy { get; set; }

    [Description("The maximum combo the user achieved")]
    public int MaxCombo { get; set; }

    [Ignored]
    public int CurrentCombo { get; set; }

    [Description("The amount of excellent hits the user achieved")]
    public int ExcellentHits { get; set; }
    [Description("The amount of fair hits the user achieved")]
    public int FairHits { get; set; }
    [Description("The amount of good hits the user achieved")]
    public int GoodHits { get; set; }
    [Description("The amount of poor hits the user achieved")]
    public int PoorHits { get; set; }

    [Description("The mods the user used for the play")]
    public string Mods { get; set; }//This is stored as a JSON string

    [Description("A flag telling whether the score originated from another user online.")]
    public bool OnlineScore { get; set; }

    [Description("A list of replay frames associated with this score")]
    public IList<ReplayFrame> ReplayFrames { get; }

    [Description("The unique ID of the beatmap")]
    public string BeatmapId { get; set; }

    [Description("The time the score was set")]
    public DateTimeOffset Time { get; set; }

    public void AddScore(long i) {
        this.AchievedScore += i;//TODO: mod multipliers
    }
    public Score Clone() {
        Score score = new() {
            Time          = this.Time,
            Accuracy      = this.Accuracy,
            Mods          = this.Mods,
            User          = this.User.Clone(),
            AchievedScore = this.AchievedScore,
            BeatmapId     = this.BeatmapId,
            CurrentCombo  = this.CurrentCombo,
            ExcellentHits = this.ExcellentHits,
            FairHits      = this.FairHits,
            GoodHits      = this.GoodHits,
            MaxCombo      = this.MaxCombo,
            OnlineScore   = this.OnlineScore,
            PoorHits      = this.PoorHits,
            Id            = this.Id
        };

        foreach (ReplayFrame frame in this.ReplayFrames)
            score.ReplayFrames.Add(frame.Clone());

        return score;
    }
}
